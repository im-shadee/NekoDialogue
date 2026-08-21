using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace NekoDialogue.Core.UI
{
    /// <summary>
    /// Displays text gradually character by character to create a typewriter text animation.
    /// </summary>
    public class TypewriterEffect : MonoBehaviour
    {
        // Shade: Indicates whether text is currently being typed out.
        public bool IsTyping => m_TypeRoutine != null;

        // Shade: Active multiplier applied to typing delay (default: 1.0f).
        private float m_CurrentSpeedMultiplier = 1f;

        // Shade: Cached delay yield instruction for normal character timing.
        private WaitForSeconds m_WaitForBaseDelayHandle = null;

        // Shade: Cached delay yield instruction for punctuation pauses.
        private WaitForSeconds m_WaitForPunctuationDelayHandle = null;

        // Shade: Reference to the active coroutine animating text.
        private Coroutine m_TypeRoutine = null;

        // Shade: Typing events
        private Action m_OnTypingComplete = null;
        public event Action OnTypingStopped = null;

        // Shade: Fired every time a printable character is revealed on screen.
        public event Action<char> OnCharacterTyped = null;

        [Header("UI References")]
        [SerializeField, Tooltip("The TextMeshPro UI component where text is displayed. Drag your dialogue text object here.")]
        private TextMeshProUGUI m_TextComponent = null;

        [Header("Dialogue Pacing")]
        [SerializeField, Tooltip("Typing speed measured in characters revealed per second. Adjust this to make text appear faster or slower.")]
        private float m_CharactersPerSecond = 30f;

        [SerializeField, Tooltip("Extra delay multiplier applied when encountering punctuation (., !?...) to mimic natural speech cadence.")]
        private float m_PunctuationDelayMultiplier = 2.5f;

        #region Unity Lifecycle
        private void Awake()
        {
            if (m_TextComponent == null)
            {
                NekoDialogueDebug.LogError("TypewriterEffect: m_TextComponent not assigned.");
            }
        }
        #endregion

        #region Public API
        /// <summary>
        /// Begins typing out the provided text string from the start.
        /// </summary>
        /// <param name="text">The full text message to animate on screen.</param>
        /// <param name="onComplete">Optional callback function to run when typing finishes.</param>
        public void Play(string text, Action onComplete = null)
        {
            Stop();

            RecalculateDelays();

            m_OnTypingComplete = onComplete;
            m_TypeRoutine = StartCoroutine(TypeRoutine(text));
        }

        /// <summary>
        /// Immediately displays the complete text and finishes the animation.
        /// </summary>
        /// <param name="playCachedEvent">If true, triggers the completion callback stored during Play.</param>
        public void Skip(bool playCachedEvent = true)
        {
            if (!IsTyping) return;

            Stop();

            if (m_TextComponent != null)
            {
                m_TextComponent.maxVisibleCharacters = m_TextComponent.textInfo?.characterCount ?? 0;
            }
            
            if (playCachedEvent) InvokeAndClearCompleted();
        }

        /// <summary>
        /// Halts the typing animation immediately without revealing remaining text.
        /// </summary>
        public void Stop()
        {
            if (m_TypeRoutine != null)
            {
                StopCoroutine(m_TypeRoutine);
                m_TypeRoutine = null;
            }

            // Shade: Reset the speed multiplier back to default
            m_CurrentSpeedMultiplier = 1f;

            OnTypingStopped?.Invoke();
        }

        /// <summary>
        /// Sets a speed multiplier applied to the base typewriter delay.
        /// Lower values increase reveal speed; higher values decrease reveal speed.
        /// </summary>
        /// <param name="multiplier">The delay multiplier (e.g., 0.5f for double speed, 1.0f for normal speed).</param>
        public void SetSpeedMultiplier(float multiplier)
        {
            m_CurrentSpeedMultiplier = Mathf.Max(0.01f, multiplier);
            RecalculateDelays();
        }
        #endregion

        #region Private Helpers
        /// <summary>
        /// Recalculates delay handles based on base characters-per-second and the active speed multiplier.
        /// </summary>
        private void RecalculateDelays()
        {
            float baseDelay = (1f / Mathf.Max(m_CharactersPerSecond, 1f)) * m_CurrentSpeedMultiplier;

            m_WaitForBaseDelayHandle = new WaitForSeconds(baseDelay);
            m_WaitForPunctuationDelayHandle = new WaitForSeconds(baseDelay * m_PunctuationDelayMultiplier);
        }

        /// <summary>
        /// Runs the stored completion callback and clears its reference.
        /// </summary>
        private void InvokeAndClearCompleted()
        {
            m_OnTypingComplete?.Invoke();
            m_OnTypingComplete = null;
        }

        /// <summary>
        /// Controls the step-by-step character reveal timing over multiple frames.
        /// </summary>
        /// <param name="text">The string being typed out.</param>
        private IEnumerator TypeRoutine(string text)
        {
            if (m_TextComponent == null) yield break;

            m_TextComponent.text = text;

            // Shade: Temporarily allow all characters so TMPro correctly calculates charInfo.isVisible
            m_TextComponent.maxVisibleCharacters = int.MaxValue;
            m_TextComponent.ForceMeshUpdate();

            TMP_TextInfo textInfo = m_TextComponent.textInfo;

            // Shade: Guard against uninitialized TMPro text info or character buffers
            if (textInfo == null || textInfo.characterInfo == null) yield break;

            int totalCharacters = textInfo.characterCount;
            int maxAvailableCharacters = textInfo.characterInfo.Length;

            // Shade: Hide all characters to start the typewriter reveal animation
            m_TextComponent.maxVisibleCharacters = 0;

            for (int i = 0; i <= totalCharacters; i++)
            {
                m_TextComponent.maxVisibleCharacters = i;
                int charIndex = i - 1;

                // Shade: Explicitly check bounds against the internal characterInfo array buffer
                if (charIndex >= 0 && charIndex < maxAvailableCharacters)
                {
                    TMP_CharacterInfo charInfo = textInfo.characterInfo[charIndex];
                    char c = charInfo.character;

                    // Shade: Notify listeners (e.g., TypewriterAudioHandler) of printable characters
                    if (!char.IsWhiteSpace(c) && !char.IsControl(c))
                    {
                        OnCharacterTyped?.Invoke(c);
                    }

                    if (char.IsPunctuation(c)) yield return m_WaitForPunctuationDelayHandle;
                    else yield return m_WaitForBaseDelayHandle;
                }
                else
                {
                    yield return m_WaitForBaseDelayHandle;
                }
            }

            m_TypeRoutine = null;
            OnTypingStopped?.Invoke();
            InvokeAndClearCompleted();
        }
        #endregion
    }
}
