using UnityEngine;

namespace NekoDialogue.Core.UI.Themes
{
    /// <summary>
    /// ScriptableObject data container that defines visual theme assets and colors for dialogue boxes and speech bubble tails.
    /// </summary>
    [CreateAssetMenu(fileName = "DialogueBoxStyle", menuName = "NekoDialogue/UI/Box Theme")]
    public class DialogueBoxStyle : ScriptableObject
    {
        [Header("Background Settings")]
        [SerializeField, Tooltip("The background sprite assigned to the dialogue box.")]
        private Sprite m_Background = null;

        [SerializeField, Tooltip("The tint color applied to the background sprite.")]
        private Color m_BackgroundColor = Color.white;

        [Header("Theme Font Settings")]
        [SerializeField, Tooltip("The default text color associated with this theme style.")]
        private Color m_TextColor = Color.white;

        [Header("Speech Bubble Tail Sprites")]
        [SerializeField, Tooltip("Left-aligned tail background sprite for speech bubble setups.")]
        private Sprite m_TailLeftSprite = null;

        [SerializeField, Tooltip("Middle-aligned tail background sprite for speech bubble setups.")]
        private Sprite m_TailMiddleSprite = null;

        // Shade: Public read-only properties
        public Sprite Background => m_Background;
        public Color BackgroundColor => m_BackgroundColor;
        public Color TextColor => m_TextColor;
        public Sprite TailLeftSprite => m_TailLeftSprite;
        public Sprite TailMiddleSprite => m_TailMiddleSprite;
    }
}
