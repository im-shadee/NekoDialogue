using System;
using UnityEngine;
using UnityEngine.Localization;

namespace NekoDialogue.Core.Conversation
{
    /// <summary>
    /// Represents a flexible dialogue text entry that supports both raw string values and Unity Localization tables.
    /// </summary>
    [Serializable]
    public class DialogueText
    {
        [SerializeField, Tooltip("Determines whether dialogue resolves via plain text or Unity Localization.")]
        private eDialogueTextMode m_DialogueTextMode = eDialogueTextMode.Localized;

        [SerializeField, Tooltip("The fallback or plain-text content used when text mode is set to Plain.")]
        private string m_DialogueText = "";

        [SerializeField, Tooltip("The localized string reference used when text mode is set to Localized.")]
        private LocalizedString m_LocalizedDialogueText = null;

        /// <summary>
        /// Retrieves the evaluated dialogue string based on the active <see cref="eDialogueTextMode"/>.
        /// </summary>
        /// <returns>The raw or localized text string. Returns <see cref="string.Empty"/> if localized content is unassigned or invalid.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <see cref="m_DialogueTextMode"/> contains an unhandled enum value.</exception>
        public string GetText()
        {
            return m_DialogueTextMode switch
            {
                eDialogueTextMode.Plain => m_DialogueText,
                eDialogueTextMode.Localized => GetSafeLocalizedString(),
                _ => throw NekoDialogueErrorLogger.CreateException<ArgumentOutOfRangeException>
                    ($"DialogueText@GetText: Unsupported dialogue text mode: {m_DialogueTextMode}")
            };
        }

        /// <summary>
        /// Safely fetches the localized string content without throwing missing key exceptions.
        /// </summary>
        private string GetSafeLocalizedString()
        {
            return IsValidLocalizedString()
                ? m_LocalizedDialogueText.GetLocalizedString()
                : string.Empty;
        }

        /// <summary>
        /// Checks if the localized string reference is non-null and linked to a valid table entry.
        /// </summary>
        private bool IsValidLocalizedString()
        {
            // Shade: Ensures reference exists and is actively linked to a table key before reading
            return m_LocalizedDialogueText != null
                && !m_LocalizedDialogueText.IsEmpty;
        }
    }
}
