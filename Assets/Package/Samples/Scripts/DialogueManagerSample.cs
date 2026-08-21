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
    private RectTransform m_ConfirmationPromptIcon = null;

    [SerializeField]
    private DialogueProcessorSO m_Processor = null;

    [Header("Dialogue Effects")]
    [SerializeField]
    private TypewriterEffect m_Typewriter = null;

    [SerializeField]
    private TypewriterAudioHandler m_TypewriterAudioHandler = null;

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

        m_CurrentLineIndex++;

        if (m_CurrentLineIndex >= m_CurrentConversation.ConversationLines.Count)
        {
            EndConversation();
            return;
        }

        ShowCurrentLine();
    }

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

    public string FormatDialogueLine(string rawText)
    {
        // Shade: Formatting through processor with custom filters
        return m_Processor == null ? string.Empty : m_Processor.ProcessDialogue(rawText);
    }
    #endregion

    #region Branching System
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

    private void UnsubscribeCurrentLocalization()
    {
        if (m_ActiveLocalizedString != null)
        {
            m_ActiveLocalizedString.StringChanged -= DisplayDialogueText;
            m_ActiveLocalizedString = null;
        }
    }

    private DialogueLine GetCurrentLine()
    {
        // Shade: Fetch the active line to check its typewriter preferences
        return (m_CurrentConversation != null && m_CurrentLineIndex < m_CurrentConversation.ConversationLines.Count)
            ? m_CurrentConversation.ConversationLines[m_CurrentLineIndex]
            : null;
    }

    private void ResetDialogueCache()
    {
        m_DialogueText.text = "";
        m_CurrentConversation = null;
        m_CurrentEntity = null;
    }

    private void ResetAudioSettings()
    {
        if (m_TypewriterAudioHandler != null) m_TypewriterAudioHandler.ResetAudioSource();
    }

    private void SetBoxSettings(DialogueLine line)
    {
        if (m_DialogueBoxUI == null) return;

        Vector3 speakerWorldPos = m_CurrentEntity != null ? m_CurrentEntity.position : Vector3.zero;

        // Shade: Change the text box's style and layout depending on the options set in the current conversation line
        m_DialogueBoxUI.ApplyBoxStyle(line.BoxStyle);
        m_DialogueBoxUI.ApplyBoxLayout(line.BoxLayout, speakerWorldPos);
        m_DialogueBoxUI.ResolveTailPlacement(line, speakerWorldPos);
        m_DialogueBoxUI.ClampDialogueBox();
    }

    private void PrepareForBlip(VoiceProfileSO voiceProfile, eDialogueEmotion dialogueEmotion)
    {
        if (m_TypewriterAudioHandler == null)
        {
            Debug.LogError("DialogueManager: m_AudioHandler is not assigned.");
            return;
        }

        if (voiceProfile == null) return;

        m_TypewriterAudioHandler.PrepareAudio(voiceProfile, dialogueEmotion);
    }

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

    public static DialogueManagerSample GetInstanceSafe() => HasInstance ? m_Instance : null;

    public static bool TryGetInstance(out DialogueManagerSample instance)
    {
        instance = m_Instance;
        return HasInstance;
    }

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
