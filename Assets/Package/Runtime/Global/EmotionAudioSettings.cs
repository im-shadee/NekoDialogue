using System;
using System.Collections.Generic;
using UnityEngine;

namespace NekoDialogue
{
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
        private float m_BasePitch = 1f;

        [SerializeField, Range(0f, 0.5f), Tooltip("Random pitch jitter added to each blip for natural variance.")]
        private float m_PitchVariance = 0.06f;

        [SerializeField, Range(0f, 1f), Tooltip("The output playback volume for audio blips.")]
        private float m_Volume = 1f;

        [SerializeField, Tooltip("Plays a blip every N printable characters (e.g., 2 = every second character).")]
        private int m_CharacterFrequency = 2;

        [SerializeField, Min(0f), Tooltip("Multiplier applied to typewriter delay (e.g., 0.6 = faster text for angry/excited).")]
        private float m_SpeedMultiplier = 1f;

        // Shade: Public read-only properties
        public eDialogueEmotion Emotion => m_Emotion;
        public IReadOnlyList<AudioClip> SoundClips => m_SoundClips;
        public float BasePitch => Mathf.Max(0.1f, m_BasePitch);
        public float PitchVariance => m_PitchVariance;
        public float Volume => m_Volume;
        public int CharacterFrequency => m_CharacterFrequency;
        public float SpeedMultiplier => m_SpeedMultiplier;
    }
}
