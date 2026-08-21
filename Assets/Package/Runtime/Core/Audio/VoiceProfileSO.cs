using System.Collections.Generic;
using UnityEngine;

namespace NekoDialogue.Core.Audio
{
    /// <summary>
    /// Stores voice audio configurations mapped to different dialogue emotions for a character.
    /// </summary>
    [CreateAssetMenu(fileName = "NewVoiceProfile", menuName = "NekoDialogue/Voice Profile")]
    public class VoiceProfileSO : ScriptableObject
    {
        // Shade: Dictionary cache for O(1) runtime lookups
        private Dictionary<eDialogueEmotion, EmotionAudioSettings> m_dSettingsCache;

#if UNITY_EDITOR
        [SerializeField, Tooltip("Internal character name used for editor identification. Type the character's name here to easily identify this asset in the inspector.")]
        private string m_EditorCharacterName;
#endif

        [SerializeField, Tooltip("List of audio settings associated with each dialogue emotion. Add audio profiles and assign corresponding emotions here.")]
        private EmotionAudioSettings[] m_EmotionSettings;

        private void OnEnable()
        {
            InitializeCache();
        }

        private void OnValidate()
        {
            InitializeCache();
        }

        /// <summary>
        /// Builds the dictionary lookup cache from the serialized array.
        /// </summary>
        private void InitializeCache()
        {
            if (m_EmotionSettings == null) return;

            if (m_dSettingsCache == null)
            {
                m_dSettingsCache = new Dictionary<eDialogueEmotion, EmotionAudioSettings>(m_EmotionSettings.Length);
            }
            else
            {
                m_dSettingsCache.Clear();
            }

            for (int i = 0; i < m_EmotionSettings.Length; i++)
            {
                // Shade: Prevents duplicate keys from breaking dictionary creation
                m_dSettingsCache.TryAdd(m_EmotionSettings[i].Emotion, m_EmotionSettings[i]);
            }
        }

        /// <summary>
        /// Retrieves audio settings matching a specific dialogue emotion, falling back to Neutral or the first entry if not found.
        /// </summary>
        /// <param name="emotion">The dialogue emotion state to search for.</param>
        /// <param name="settings">Outputs the matching or fallback emotion audio settings struct.</param>
        /// <returns>True if any valid settings entry was found; otherwise, false.</returns>
        public bool TryGetSettings(eDialogueEmotion emotion, out EmotionAudioSettings settings)
        {
            if (m_dSettingsCache == null)
            {
                InitializeCache();
            }

            // Shade: O(1) Direct Lookup
            if (m_dSettingsCache != null && m_dSettingsCache.TryGetValue(emotion, out settings))
            {
                return true;
            }

            // Shade: Fallback 1 - Check if explicit Neutral settings exist
            if (m_dSettingsCache != null && m_dSettingsCache.TryGetValue(eDialogueEmotion.Neutral, out settings))
            {
                return true;
            }

            // Shade: Fallback 2 - Default to first array element if Neutral is missing
            if (m_EmotionSettings != null && m_EmotionSettings.Length > 0)
            {
                settings = m_EmotionSettings[0];
                return true;
            }

            settings = default;
            return false;
        }
    }
}
