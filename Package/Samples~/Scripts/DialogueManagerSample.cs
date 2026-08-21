using NekoDialogue;
using NekoDialogue.Core;
using NekoDialogue.Core.Audio;
using NekoDialogue.Core.Conversation;
using NekoDialogue.Core.Interaction;
using NekoDialogue.Core.UI;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

/// <summary>
/// Singleton manager class responsible for controlling dialogue flow, processing text formatting,
/// handling localization, typewriter effects, audio blips, and branching paths in conversations.
/// </summary>
public class DialogueManagerSample : MonoBehaviour, IDialogueService
{
    // Shade: Tracks the current conversation line displayed within the playing conversation asset
    private int m_CurrentLineIndex = 0;

    // Shade: Cached speaking entity transform to position the box on it if this dialogue is a speech bubble
    // or to assign the new dialogue asset for branching paths
    private Transform m_CurrentEntity = null;

    // Shade: Caches the box animation routine to prevent early A presses from messing up the flow
    private Coroutine m_BoxAnimationRoutine;

    // Shade: Last localized string displayed on screen. Cached to correctly subscribe/unsubscribe to
    // line changed events
    private LocalizedString m_ActiveLocalizedString;

    // Shade: Cached current conversation asset being played
    private ConversationAsset m_CurrentConversation = null;

    // Shade: Events for conversation start and end
    public event Action OnConversationStarted;
    public event Action OnConversationEnded;

    [Header("Dialogue Components")]
    [SerializeField, Tooltip("The DialogueBoxUI to populate. Contains box elements (outline, background) and speech bubble tail.")]
    private DialogueBoxUI m_DialogueBoxUI = null;

    [SerializeField, Tooltip("A reference to the text asset displaying the dialogue.")]
    private TextMeshProUGUI m_DialogueText = null;

    [SerializeField, Tooltip("A reference to the option panel displaying options for branching dialogue paths.")]
    private DialogueOptionPanel m_OptionPanel = null;

    [SerializeField, Tooltip("A small confirmation prompt icon. Shown only for regular dialogues, not lines with choices.")]
    private Image m_ConfirmationPromptIcon = null;

    [SerializeField, Tooltip("Reference to the processor used for dialogue text formatting and filtering.")]
    private DialogueProcessorSO m_Processor = null;

    [Header("Dialogue Effects")]
    [SerializeField, Tooltip("Typewriter effect component responsible for typing out text letter by letter.")]
    private TypewriterEffect m_Typewriter = null;

    [SerializeField, Tooltip("Audio handler managing sound effects and voice blips during typing.")]
    private TypewriterAudioHandler m_TypewriterAudioHandler = null;

    [Header("Audio Settings")]
    [SerializeField, Tooltip("AudioSource component used to play UI and dialogue transition sound effects.")]
    private AudioSource m_SFXAudioSource = null;

    #region Unity Lifecycle
    private void Awake()
    {
        // Shade: Register this instance as the primary active dialogue service
        DialogueServiceLocator.RegisterService(this);

        _InitializeSingleton();

        // Shade: Validate all references have been assigned
        if (m_DialogueText == null)
        {
            NekoDialogueDebug.LogError($"DialogueManager: m_DialogueText not assigned on {name}.");
            enabled = false;
            return;
        }

        if (m_DialogueBoxUI == null)
        {
            NekoDialogueDebug.LogError($"DialogueManager: m_DialogueBoxUI not assigned on {name}.");
            enabled = false;
            return;
        }

        if (m_OptionPanel == null)
        {
            NekoDialogueDebug.LogError($"DialogueManager: m_OptionPanel not assigned on {name}.");
            enabled = false;
            return;
        }

        if (m_Typewriter == null)
        {
            NekoDialogueDebug.LogError($"DialogueManager: m_Typewriter not assigned on {name}.");
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        if (m_DialogueBoxUI != null)
        {
            m_DialogueBoxUI.Hide();
        }
    }

    private void OnDestroy()
    {
        // Shade: Clean up service reference when destroyed
        DialogueServiceLocator.UnregisterService(this);
    }
    #endregion

    #region IDialogueService Implementation
    /// <summary>
    /// Starts playing a conversation sequence using the provided conversation asset.
    /// </summary>
    /// <param name="conversation">The conversation asset containing the lines to display.</param>
    /// <param name="speakingEntity">Optional transform reference for positioning speech bubbles or tracking speaker world position.</param>
    public void StartConversation(ConversationAsset conversation, Transform speakingEntity = null)
    {
        if (!enabled) return;

        if (conversation == null || conversation.ConversationLines == null || conversation.ConversationLines.Count == 0)
        {
            NekoDialogueDebug.LogError("DialogueManager: Conversation is empty or not set. Dialogue will not start.");
            return;
        }

        m_DialogueText.text = "";
        m_CurrentLineIndex = 0;
        m_CurrentConversation = conversation;

        // Shade: Clean up previous string handlers
        UnsubscribeCurrentLocalization();

        // Shade: Cache the speaking entity's interactable
        m_CurrentEntity = speakingEntity;

        // Shade: Notify listeners that a conversation started
        OnConversationStarted?.Invoke();

        ShowCurrentLine();

        if (m_DialogueBoxUI != null)
        {
            m_DialogueBoxUI.Show();
        }
    }

    /// <summary>
    /// Displays the current active dialogue line based on the internal line index.
    /// </summary>
    public void ShowCurrentLine()
    {
        if (!enabled) return;
        if (m_CurrentConversation == null) return;

        // Shade: End the conversation if we've run out of lines
        if (m_CurrentLineIndex >= m_CurrentConversation.ConversationLines.Count)
        {
            EndConversation();
            return;
        }

        // Shade: Unsubscribe from previous line's localization event to avoid leaks/stale updates
        UnsubscribeCurrentLocalization();

        DialogueLine line = m_CurrentConversation.ConversationLines[m_CurrentLineIndex];
        PrepareForBlip(line.VoiceProfile, line.DialogueEmotion);

        if (m_DialogueBoxUI != null)
        {
            SetBoxSettings(line);

            // Shade: Only update with the new text once the box animation has ended
            if (line.BoxAnimation != null)
            {
                // Shade: Play an animation on the box if provided
                m_BoxAnimationRoutine = StartCoroutine(m_DialogueBoxUI.PlayAnimation(
                    line.BoxAnimation,
                    onComplete: () =>
                    {
                        // Shade: Reset the reference to the routine
                        m_BoxAnimationRoutine = null;

                        // Shade: Defer subscription until animation completes.
                        // Subscribing here automatically triggers BindAndDisplayText with the current text
                        BindAndDisplayText(line);
                    }
                ));
            }
            else
            {
                // Shade: Display immediately if there is no animation
                BindAndDisplayText(line);
            }
        }

        // Shade: Change the text's font style and sizing depending on the options set
        ApplyFontStyle(line);

        // Shade: Process branching path logic for this line
        ProcessBranchingPaths(line);
    }

    /// <summary>
    /// Advances the conversation to the next line or completes typewriter typing if currently active.
    /// </summary>
    public void AdvanceConversation()
    {
        if (!enabled) return;

        // Shade: Automatically reject inputs during the box animation sequence
        if (m_BoxAnimationRoutine != null) return;

        // Shade: If typewriter is currently active, skip to full line reveal instead of next line
        // Otherwise, handle standard line advancement logic
        if (m_Typewriter != null && m_Typewriter.IsTyping)
        {
            m_Typewriter.Skip(playCachedEvent: true);
            return;
        }

        if (m_CurrentConversation == null)
        {
            EndConversation();
            return;
        }

        // Shade: Check whether the current line is a branching path first,
        // to ignore the 'index reaching end count' logic
        if (m_CurrentConversation.ConversationLines[m_CurrentLineIndex] is BranchingDialogueLine)
        {
            // Shade: Only return and delegate the logic to _HandleOptionSelected to avoid race conditions.
            return;
        }

        // Shade: Play the advance sound effect (uses custom line override if present, else fallback)
        PlayAdvanceSound();

        m_CurrentLineIndex++;

        if (m_CurrentLineIndex >= m_CurrentConversation.ConversationLines.Count)
        {
            EndConversation();
            return;
        }

        ShowCurrentLine();
    }

    /// <summary>
    /// Ends the active conversation, cleans up handlers, and hides dialogue UI.
    /// </summary>
    public void EndConversation()
    {
        // Shade: Clean up event subscriptions
        UnsubscribeCurrentLocalization();

        // Shade: Stop typewriter routine if active
        if (m_Typewriter != null)
        {
            m_Typewriter.Stop();
        }

        if (m_DialogueBoxUI != null)
        {
            m_DialogueBoxUI.Hide();
        }

        ResetDialogueCache();
        ResetAudioSettings();

        OnConversationEnded?.Invoke();
    }

    /// <summary>
    /// Formats a raw dialogue string using the configured dialogue processor filters.
    /// </summary>
    /// <param name="rawText">The unformatted text string.</param>
    /// <returns>The processed and formatted text string.</returns>
    public string FormatDialogueLine(string rawText)
    {
        // Shade: Formatting through processor with custom filters
        return m_Processor == null ? string.Empty : m_Processor.ProcessDialogue(rawText);
    }
    #endregion

    #region Branching System
    /// <summary>
    /// Processes branching choices for a line and displays options if present.
    /// </summary>
    /// <param name="line">The dialogue line to check for branching options.</param>
    private void ProcessBranchingPaths(DialogueLine line)
    {
        if (m_OptionPanel == null) return;

        // Shade: If this line is a branching conversation path, display all options on screen
        if (line is BranchingDialogueLine branchingLine)
        {
            if (m_ConfirmationPromptIcon != null) m_ConfirmationPromptIcon.gameObject.SetActive(false);
            m_OptionPanel.ShowOptions(branchingLine, HandleOptionSelected);
        }
        else
        {
            if (m_ConfirmationPromptIcon != null) m_ConfirmationPromptIcon.gameObject.SetActive(true);
            m_OptionPanel.OnClose();
        }
    }

    /// <summary>
    /// Handles option selection events in branching conversations and transitions to the chosen conversation asset.
    /// </summary>
    /// <param name="selectedOptionEntry">The selected option entry containing the target conversation asset.</param>
    private void HandleOptionSelected(DialogueOptionsEntry selectedOptionEntry)
    {
        ConversationAsset branchingAsset = selectedOptionEntry.Conversation;
        if (branchingAsset == null) return;

        // Shade: We can successfully assign the conversation asset to the interactable if desired without issues
        if (m_CurrentEntity != null && selectedOptionEntry.OverrideNPCDialogue)
        {
            if (m_CurrentEntity.TryGetComponent(out IDialogueTarget interactable))
            {
                interactable.SetConversationAsset(branchingAsset);
            }
        }

        // Shade: Reset the index so we can restart the conversation from 0 (conversation ends when the index
        // reaches the end of the current asset count)
        m_CurrentLineIndex = 0;

        // Shade: Assign the branching conversation asset as the new current conversation, and "restart" a
        // dialogue with the new asset.
        // (We basically just show the next line, which will be picked from branchingAsset)
        m_CurrentConversation = branchingAsset;
        ShowCurrentLine();
    }
    #endregion

    #region Private Helpers
    /// <summary>
    /// Resolves Plain vs Localized mode after animations complete.
    /// </summary>
    private void BindAndDisplayText(DialogueLine line)
    {
        DialogueText dialogueText = line.DialogueText;
        if (dialogueText == null) return;

        bool localizedLineEmptyOrNull = dialogueText.LocalizedString == null || dialogueText.LocalizedString.IsEmpty;

        if (dialogueText.TextMode == eDialogueTextMode.Localized && !localizedLineEmptyOrNull)
        {
            m_ActiveLocalizedString = dialogueText.LocalizedString;

            // Shade: Subscribing automatically fires StringChanged with the current string value
            m_ActiveLocalizedString.StringChanged += DisplayDialogueText;
        }
        else
        {
            // Shade: Plain mode: Execute display directly
            DisplayDialogueText(dialogueText.GetText());
        }
    }

    /// <summary>
    /// Single entry point for formatting, typing, and updating dialogue UI text.
    /// </summary>
    private void DisplayDialogueText(string rawText)
    {
        string processedText = FormatDialogueLine(rawText);

        // Shade: Fetch current line configuration
        DialogueLine currentLine = GetCurrentLine();
        bool shouldType = currentLine != null && currentLine.UseTypeWriter && m_Typewriter != null;

        if (shouldType)
        {
            // Shade: Play a typewriter effect. If the branching options panel is open, the submit input
            // will stop the typewriter effect instead of submitting the option. Once the typewriter is
            // done typing, the submit button will be enabled again
            m_Typewriter.Play(processedText, onComplete: () =>
            {
                if (m_OptionPanel != null && m_OptionPanel.isActiveAndEnabled)
                {
                    m_OptionPanel.AllowSubmit(true);
                }
            });
        }
        else
        {
            if (m_Typewriter != null) m_Typewriter.Stop();
            if (m_OptionPanel != null && m_OptionPanel.isActiveAndEnabled)
            {
                m_OptionPanel.AllowSubmit(true);
            }

            if (m_DialogueText != null)
            {
                m_DialogueText.text = processedText;
            }
        }
    }

    /// <summary>
    /// Removes subscription from the active localized string event to prevent memory leaks.
    /// </summary>
    private void UnsubscribeCurrentLocalization()
    {
        if (m_ActiveLocalizedString != null)
        {
            m_ActiveLocalizedString.StringChanged -= DisplayDialogueText;
            m_ActiveLocalizedString = null;
        }
    }

    /// <summary>
    /// Retrieves the current dialogue line based on the active conversation asset and index.
    /// </summary>
    /// <returns>The active DialogueLine instance, or null if out of bounds.</returns>
    private DialogueLine GetCurrentLine()
    {
        // Shade: Fetch the active line to check its typewriter preferences
        return (m_CurrentConversation != null && m_CurrentLineIndex < m_CurrentConversation.ConversationLines.Count)
            ? m_CurrentConversation.ConversationLines[m_CurrentLineIndex]
            : null;
    }

    /// <summary>
    /// Resets cached active dialogue references to default values.
    /// </summary>
    private void ResetDialogueCache()
    {
        m_DialogueText.text = "";
        m_CurrentConversation = null;
        m_CurrentEntity = null;
    }

    /// <summary>
    /// Resets audio source settings on the audio handler.
    /// </summary>
    private void ResetAudioSettings()
    {
        if (m_TypewriterAudioHandler != null) m_TypewriterAudioHandler.ResetAudioSource();
    }

    /// <summary>
    /// Configures UI box style, layout, and tail placement for the given dialogue line.
    /// </summary>
    /// <param name="line">The line data specifying style and layout preferences.</param>
    private void SetBoxSettings(DialogueLine line)
    {
        if (m_DialogueBoxUI == null) return;

        Vector3 speakerWorldPos = m_CurrentEntity != null ? m_CurrentEntity.position : Vector3.zero;

        // Shade: Change the text box's style and layout depending on the options set in the current conversation line
        m_DialogueBoxUI.ApplyBoxStyle(line.BoxStyle);
        m_DialogueBoxUI.ApplyBoxLayout(line.BoxLayout, speakerWorldPos);
        m_DialogueBoxUI.ResolveTailPlacement(line, speakerWorldPos);
        m_DialogueBoxUI.ClampDialogueBox();

        if (m_ConfirmationPromptIcon != null) m_ConfirmationPromptIcon.color = line.BoxStyle.ConfirmationIconColor;
    }

    /// <summary>
    /// Prepares typewriter audio blips using the speaker's voice profile and current emotion.
    /// </summary>
    /// <param name="voiceProfile">Voice profile parameters for character audio blips.</param>
    /// <param name="dialogueEmotion">Current emotion modifier for audio playback.</param>
    private void PrepareForBlip(VoiceProfileSO voiceProfile, eDialogueEmotion dialogueEmotion)
    {
        if (m_TypewriterAudioHandler == null)
        {
            NekoDialogueDebug.LogError("DialogueManager: m_AudioHandler is not assigned.");
            return;
        }

        if (voiceProfile == null) return;

        m_TypewriterAudioHandler.PrepareAudio(voiceProfile, dialogueEmotion);
    }

    /// <summary>
    /// Plays the advance sound effect for the current line or falls back to the default UI clip.
    /// </summary>
    private void PlayAdvanceSound()
    {
        if (m_SFXAudioSource == null)
        {
            NekoDialogueDebug.LogError("DialogueManager: m_SFXAudioSource is not assigned.");
            return;
        }

        DialogueLine currentLine = GetCurrentLine();
        if (currentLine == null) return;

        AudioClip advanceSound = currentLine.AdvanceSound;
        if (advanceSound == null) return;

        m_SFXAudioSource.PlayOneShot(advanceSound);
    }

    /// <summary>
    /// Applies font size, font asset, and text color configurations specified in the line.
    /// </summary>
    /// <param name="line">The line data defining visual font properties.</param>
    private void ApplyFontStyle(DialogueLine line)
    {
        if (m_DialogueText == null) return;

        // Shade: Text customization
        if (line.BoxStyle != null)
        {
            m_DialogueText.color = line.BoxStyle.TextColor;
        }

        m_DialogueText.font = line.FontSettings?.Font;

        // Shade: Clamp the font size between min/max auto-size values
        m_DialogueText.fontSize =
            Mathf.Clamp(line.FontSettings.FontSize, m_DialogueText.fontSizeMin, m_DialogueText.fontSizeMax);
    }
    #endregion

    #region Instance Management
    private static DialogueManagerSample m_Instance = null;
    public static DialogueManagerSample Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = FindAnyObjectByType<DialogueManagerSample>();

                if (m_Instance == null)
                {
                    GameObject go = new(name: "[" + typeof(DialogueManagerSample).Name + "]");
                    m_Instance = go.AddComponent<DialogueManagerSample>();
                }
            }
            return m_Instance;
        }
    }

    public static bool HasInstance => m_Instance != null;

    /// <summary>
    /// Safely retrieves the active singleton instance if it exists.
    /// </summary>
    /// <returns>The active instance of DialogueManagerSample, or null if unassigned.</returns>
    public static DialogueManagerSample GetInstanceSafe() => HasInstance ? m_Instance : null;

    /// <summary>
    /// Tries to get the current singleton instance.
    /// </summary>
    /// <param name="instance">Outputs the instance if assigned.</param>
    /// <returns>True if the instance exists; otherwise, false.</returns>
    public static bool TryGetInstance(out DialogueManagerSample instance)
    {
        instance = m_Instance;
        return HasInstance;
    }

    /// <summary>
    /// Handles singleton initialization and ensures only one instance persists across scene loads.
    /// </summary>
    private void _InitializeSingleton()
    {
        if (!Application.isPlaying) return;

        if (m_Instance == null)
        {
            m_Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        else if (m_Instance != this)
        {
            Destroy(gameObject);
        }
    }
    #endregion
}
