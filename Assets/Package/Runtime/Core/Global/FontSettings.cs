using System;
using TMPro;
using UnityEngine;

namespace NekoDialogue
{
    /// <summary>
    /// Configuration container for specifying typography styling and sizing on dialogue text instances.
    /// </summary>
    [Serializable]
    public class FontSettings
    {
        [SerializeField, Tooltip("The TextMeshPro font asset assigned to this dialogue line.")]
        private TMP_FontAsset m_Font = null;

        [SerializeField, Tooltip("The point size of the dialogue font. Recommended to keep within autosize bounds.")]
        private int m_FontSize = 68;

        // Shade: Public read-only properties
        public TMP_FontAsset Font => m_Font;
        public int FontSize => m_FontSize;
    }
}
