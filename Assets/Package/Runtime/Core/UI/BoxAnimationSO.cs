using System.Collections;
using UnityEngine;

namespace NekoDialogue.Core.UI
{
    /// <summary>
    /// Shade: Abstract base ScriptableObject for UI dialogue box animations.
    /// </summary>
    public abstract class BoxAnimationSO : ScriptableObject
    {
        [Header("Global Settings")]
        [SerializeField, Tooltip("Duration of the animation in seconds. Adjust this to speed up or slow down the animation.")]
        private float m_Duration = 1.0f;
        protected float Duration => m_Duration;

        /// <summary>
        /// Shade: Executes the UI box animation coroutine on the target transform.
        /// </summary>
        /// <param name="boxTransform">The RectTransform of the UI element being animated.</param>
        public abstract IEnumerator Execute(RectTransform boxTransform);
    }
}
