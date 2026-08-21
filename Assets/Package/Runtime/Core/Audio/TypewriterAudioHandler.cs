using NekoDialogue.Core.UI;
using UnityEngine;

namespace NekoDialogue.Core.Audio
{
    /// <summary>
    /// Plays character voice blips in sync with a TypewriterEffect text display.
    /// </summary>
    public class TypewriterAudioHandler : MonoBehaviour
    {
        // Shade: Active voice profile asset containing audio clips and pitch settings.
        private VoiceProfileSO m_CurrentProfile = null;

        // Shade: Currently active dialogue emotion driving audio configuration.
        private eDialogueEmotion m_CurrentEmotion = eDialogueEmotion.Neutral;

        // Shade: Cached emotion audio settings fetched from the active voice profile.
        private EmotionAudioSettings m_CurrentSettings = default;

        // Shade: Indicates whether valid emotion audio settings were loaded successfully.
        private bool m_bHasValidSettings = true;

        // Shade: Tracks total printable characters processed to control blip frequency.
        private int m_CharacterBlipCounter = 0;

        [SerializeField, Tooltip("Reference to the TypewriterEffect component driving text animation. Subscribes to typing events automatically.")]
        private TypewriterEffect m_Typewriter = null;

        [SerializeField, Tooltip("AudioSource component used to trigger dialogue voice blip audio clips.")]
        private AudioSource m_BlipAudioSource = null;

        #region Unity Lifecycle
        private void Awake()
        {
            if (m_Typewriter == null)
            {
                NekoDialogueDebug.LogError($"TypewriterAudioHandler: m_Typewriter not assigned on {name}.");
            }

            if (m_BlipAudioSource == null)
            {
                NekoDialogueDebug.LogError($"TypewriterAudioHandler: m_BlipAudioSource not assigned on {name}.");
            }
        }

        private void OnEnable()
        {
            if (m_Typewriter != null)
            {
                m_Typewriter.OnCharacterTyped += OnCharacterTyped;
                m_Typewriter.OnTypingStopped += StopAudio;
            }
        }

        private void OnDisable()
        {
            if (m_Typewriter != null)
            {
                m_Typewriter.OnCharacterTyped -= OnCharacterTyped;
                m_Typewriter.OnTypingStopped -= StopAudio;
            }
        }
        #endregion

        #region Public API
        /// <summary>
        /// Prepares the audio handler with a character's voice profile and active emotion.
        /// </summary>
        /// <param name="profile">The voice profile asset containing character audio settings.</param>
        /// <param name="emotion">The dialogue emotion state to evaluate for audio playback settings.</param>
        public void PrepareAudio(VoiceProfileSO profile, eDialogueEmotion emotion)
        {
            StopAudio();

            m_CurrentProfile = profile;
            m_CurrentEmotion = emotion;
            m_CharacterBlipCounter = 0;

            m_bHasValidSettings = m_CurrentProfile != null 
                && m_CurrentProfile.TryGetSettings(m_CurrentEmotion, out m_CurrentSettings);

            if (m_Typewriter != null && m_bHasValidSettings)
            {
                // Shade: Feed the speed multiplier to the typewriter
                m_Typewriter.SetSpeedMultiplier(m_CurrentSettings.SpeedMultiplier);
            }  
        }

        /// <summary>
        /// Manually plays a single voice blip for a given character.
        /// </summary>
        /// <param name="c">The character typed out by the typewriter effect.</param>
        public void PlayBlip(char c)
        {
            if (!m_bHasValidSettings)
            {
                NekoDialogueDebug.LogWarning("TypewriterAudioHandler: PlayBlip blocked - m_HasValidSettings is false.");
                return;
            }

            if (m_BlipAudioSource == null || m_CurrentSettings.SoundClips == null || m_CurrentSettings.SoundClips.Count == 0) return;
            if (char.IsWhiteSpace(c) || char.IsPunctuation(c)) return;

            // Shade; Increment printable character counter
            m_CharacterBlipCounter++;

            // Shade: Check playback frequency constraint
            // (e.g., if CharacterFrequency = 2, only play sound every 2nd character)
            int frequency = Mathf.Max(1, m_CurrentSettings.CharacterFrequency);

            if (m_CharacterBlipCounter % frequency != 0) return;

            // Shade: Select a random audio sample from the available voice clips
            AudioClip clip = m_CurrentSettings.SoundClips[Random.Range(0, m_CurrentSettings.SoundClips.Count)];

            // Shade: Calculate pitch variance
            // Offsets the base pitch by a random float between -PitchVariance and +PitchVariance
            float pitchJitter = Random.Range(-m_CurrentSettings.PitchVariance, m_CurrentSettings.PitchVariance);

            // Shade: Apply pitch to the audio source, clamping between 0.1 (ultra low) and 3.0 (high pitch) for safety
            m_BlipAudioSource.pitch = Mathf.Clamp(m_CurrentSettings.BasePitch + pitchJitter, 0.1f, 3.0f);
            m_BlipAudioSource.PlayOneShot(clip, m_CurrentSettings.Volume);
        }

        /// <summary>
        /// Immediately halts any playing audio and resets state as well as audio settings.
        /// </summary>
        public void StopAudio()
        {
            if (m_BlipAudioSource != null && m_BlipAudioSource.isPlaying)
            {
                m_BlipAudioSource.Stop();
            }

            m_CharacterBlipCounter = 0;
            ResetAudioSource();
        }

        /// <summary>
        /// Resets audio source volume and pitch parameters to default values.
        /// </summary>
        public void ResetAudioSource()
        {
            if (m_BlipAudioSource == null) return;

            m_BlipAudioSource.pitch = 1f;
            m_BlipAudioSource.volume = 1f;
        }
        #endregion

        #region Private Region
        /// <summary>
        /// Callback triggered when a character is typed out by the typewriter effect.
        /// </summary>
        private void OnCharacterTyped(char c) => PlayBlip(c);
        #endregion
    }
}
