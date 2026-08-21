using System.Collections.Generic;
using UnityEngine;

namespace NekoDialogue.Core
{
    /// <summary>
    /// ScriptableObject container that executes a sequential chain of text filters on raw dialogue strings.
    /// </summary>
    [CreateAssetMenu(fileName = "DialogueProcessor", menuName = "NekoDialogue/Processing/DialogueProcessor")]
    public sealed class DialogueProcessorSO : ScriptableObject
    {
        [SerializeField, Tooltip("Ordered collection of text filters applied sequentially to raw dialogue text.")]
        private List<DialogueTextFilterSO> m_Filters = new List<DialogueTextFilterSO>();

        /// <summary>
        /// Runs a raw dialogue string sequentially through all assigned non-null text filters.
        /// </summary>
        /// <param name="rawLine">The unformatted raw dialogue string.</param>
        /// <returns>The fully processed and formatted dialogue string.</returns>
        public string ProcessDialogue(string rawLine)
        {
            if (string.IsNullOrEmpty(rawLine)) return rawLine;

            string text = rawLine;

            foreach (DialogueTextFilterSO filter in m_Filters)
            {
                if (filter == null) continue;
                text = filter.ProcessText(text);
            }

            return text;
        }
    }
}