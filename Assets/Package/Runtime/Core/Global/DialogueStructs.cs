using NekoDialogue.Core.Conversation;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NekoDialogue
{
    /// <summary>
    /// Encapsulates position, offset, size, and anchor placement configurations for a dialogue UI box.
    /// </summary>
    [Serializable]
    public struct DialogueBoxLayout
    {
        [SerializeField, Tooltip("The base position coordinates for the dialogue box.")]
        private Vector2 m_Position;

        [SerializeField, Tooltip("The positional offset applied to the base position.")]
        private Vector2 m_Offset;

        [SerializeField, Tooltip("The width and height dimensions of the dialogue box.")]
        private Vector2 m_Size;

        [SerializeField, Tooltip("Determines how the dialogue box is anchored or placed on screen.")]
        private eDialogueBoxPlacement m_BoxPlacement;

        // Shade: Public properties
        public readonly Vector2 Position => m_Position;
        public readonly Vector2 Offset => m_Offset;
        public readonly Vector2 Size => m_Size;
        public readonly eDialogueBoxPlacement BoxPlacement => m_BoxPlacement;

        /// <summary>
        /// Gets a default <see cref="DialogueBoxLayout"/> configuration with zeroed position/offset and standard 1600x200 dimensions.
        /// </summary>
        public static DialogueBoxLayout Default => new DialogueBoxLayout
        {
            m_Position = Vector2.zero,
            m_Offset = Vector2.zero,
            m_Size = new Vector2(1600f, 200f),
            m_BoxPlacement = eDialogueBoxPlacement.BottomCenter
        };
    }

    /// <summary>
    /// Configuration settings for rendering and positioning a speech bubble tail on a dialogue box.
    /// </summary>
    [Serializable]
    public struct SpeechBubbleTailSettings
    {
        [SerializeField, Tooltip("Toggle whether this line shows a speech bubble tail.")]
        private bool m_EnableTail;

        [SerializeField, Tooltip("Controls top vs bottom edge placement.")]
        private eSpeechBubbleVerticalEdge m_VerticalEdge;

        [SerializeField, Tooltip("Automatically align the tail horizontally with the speaker's screen position.")]
        private bool m_AutoPositionX;

        [SerializeField, Range(0f, 1f), Tooltip("Manual normalized position along the box edge (used if AutoPositionX is false).")]
        private float m_NormalizedXPosition;

        // Shade: Public read-only properties
        public readonly bool EnableTail => m_EnableTail;
        public readonly eSpeechBubbleVerticalEdge VerticalEdge => m_VerticalEdge;
        public readonly bool AutoPositionX => m_AutoPositionX;
        public readonly float NormalizedXPosition => Mathf.Clamp(m_NormalizedXPosition, 0f, 1f);
    }

    /// <summary>
    /// Audio and timing configurations for typewriter blip sound effects associated with a specific dialogue emotion.
    /// </summary>
    [Serializable]
    public class EmotionAudioSettings
    {
        [SerializeField, Tooltip("The dialogue emotion state associated with these audio settings.")]
        private eDialogueEmotion m_Emotion = eDialogueEmotion.Neutral;

        [SerializeField, Tooltip("Audio clips used for blips. If multiple, one is randomly selected per blip.")]
        private AudioClip[] m_SoundClips = new AudioClip[0];

        [SerializeField, Range(0f, 3f), Tooltip("The base playback pitch for audio blips.")]
        private float m_BasePitch;

        [SerializeField, Range(0f, 0.5f), Tooltip("Random pitch jitter added to each blip for natural variance.")]
        private float m_PitchVariance;

        [SerializeField, Range(0f, 1f), Tooltip("The output playback volume for audio blips.")]
        private float m_Volume;

        [SerializeField, Tooltip("Plays a blip every N printable characters (e.g., 2 = every second character).")]
        private int m_CharacterFrequency;

        [SerializeField, Min(0f), Tooltip("Multiplier applied to typewriter delay (e.g., 0.6 = faster text for angry/excited).")]
        private float m_SpeedMultiplier;

        // Shade: Public read-only properties
        public readonly eDialogueEmotion Emotion => m_Emotion;
        public readonly IReadOnlyList<AudioClip> SoundClips => m_SoundClips;
        public readonly float BasePitch => Mathf.Max(0.1f, m_BasePitch);
        public readonly float PitchVariance => m_PitchVariance;
        public readonly float Volume => m_Volume;
        public readonly int CharacterFrequency => m_CharacterFrequency;
        public readonly float SpeedMultiplier => m_SpeedMultiplier;
    }

    /// <summary>
    /// Represents a single branching dialogue option, mapping a choice string to a target conversation asset.
    /// </summary>
    [Serializable]
    public struct DialogueOptionsEntry
    {
        [SerializeField, Tooltip("The option name for the associated branching conversation asset, e.g. 'Yes', 'No', etc.")]
        private DialogueText m_Option;

        [SerializeField, Tooltip("The conversation asset to play when the associated option is chosen.")]
        private ConversationAsset m_Conversation;

        [SerializeField, Tooltip("Whether to override the NPC dialogue with the conversation associated with the chosen dialogue.")]
        private bool m_bOverrideNPCDialogue;

        // Shade: Public read-only properties
        public readonly DialogueText Option => m_Option;
        public readonly string OptionName => m_Option != null ? m_Option.GetText() : string.Empty;
        public readonly ConversationAsset Conversation => m_Conversation;
        public readonly bool OverrideNPCDialogue => m_bOverrideNPCDialogue;
    }
}
