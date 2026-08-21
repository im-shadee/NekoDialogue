using NekoDialogue.Core.Audio;
using NekoDialogue.Core.UI;
using NekoDialogue.Core.UI.Themes;
using System;
using UnityEngine;

namespace NekoDialogue.Core.Conversation
{
    /// <summary>
    /// Abstract base data container representing a single line of dialogue, including text, layout, visual themes, animations, and voice audio settings.
    /// </summary>
    [Serializable]
    public abstract class DialogueLine
    {
        [Header("Dialogue Text Settings")]
#if UNITY_EDITOR
        [SerializeField, Tooltip("Tag displayed in inspector for this line. Used in editor to identify this line.")]
        private string m_DialogueTag;
#endif

        [SerializeField, Tooltip("The localized or plain text payload displayed in this dialogue line.")]
        private DialogueText m_DialogueText = null;

        [Header("Dialogue Box Customization")]
        [SerializeField, Tooltip("The screen placement, offset, and sizing metrics for the dialogue box.")]
        private DialogueBoxLayout m_BoxLayout = DialogueBoxLayout.Default;

        [SerializeField, Tooltip("The visual theme asset defining background graphics, border colors, and text colors.")]
        private DialogueBoxStyle m_BoxStyle = null;

        [SerializeField, Tooltip("An optional animation asset to play on the dialogue box container.")]
        private BoxAnimationSO m_BoxAnimation = null;

        [Header("Speech Bubble Settings")]
        [SerializeField, Tooltip("The position, vertical edge, and auto-alignment settings for the speech bubble tail.")]
        private SpeechBubbleTailSettings m_TailSettings = default;

        [Header("Text Settings")]
        [SerializeField, Tooltip("Font asset and size overrides applied to the dialogue text component.")]
        private FontSettings m_FontSettings = null;

        [SerializeField, Tooltip("Toggles whether the typewriter text reveal effect is active for this line.")]
        private bool m_bUseTypeWriter = true;

        [Header("Audio Settings")]
        [SerializeField, Tooltip("The emotional state associated with this line, dictating voice audio pitches and typewriter speeds.")]
        private eDialogueEmotion m_DialogueEmotion = eDialogueEmotion.Neutral;

        [SerializeField, Tooltip("The character voice profile asset supplying typewriter blip audio clips.")]
        private VoiceProfileSO m_VoiceProfile = null;

        // Shade: Public read-only properties
        public DialogueText DialogueText => m_DialogueText;
        public DialogueBoxLayout BoxLayout => m_BoxLayout;
        public DialogueBoxStyle BoxStyle => m_BoxStyle;
        public BoxAnimationSO BoxAnimation => m_BoxAnimation;
        public SpeechBubbleTailSettings TailSettings => m_TailSettings;
        public FontSettings FontSettings => m_FontSettings;
        public bool UseTypeWriter => m_bUseTypeWriter;
        public eDialogueEmotion DialogueEmotion => m_DialogueEmotion;
        public VoiceProfileSO VoiceProfile => m_VoiceProfile;
    }

    /// <summary>
    /// Represents a standard linear dialogue line without choice branching.
    /// </summary>
    [Serializable]
    public class StandardDialogueLine : DialogueLine { }

    /// <summary>
    /// Represents a branching dialogue line that presents choice options leading to alternate conversation assets.
    /// </summary>
    [Serializable]
    public class BranchingDialogueLine : DialogueLine
    {
        [Header("Branching Dialogue Settings")]
        [SerializeField, Tooltip("The list of options and their associated conversations. Choosing an option branches into the specified conversation.")]
        private DialogueOptionsEntry[] m_ConversationOptions = new DialogueOptionsEntry[0];
        public DialogueOptionsEntry[] ConversationOptions => m_ConversationOptions;
    }
}
