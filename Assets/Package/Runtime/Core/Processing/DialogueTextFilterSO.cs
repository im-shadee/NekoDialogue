using UnityEngine;

namespace NekoDialogue.Core
{
    /// <summary>
    /// Abstract ScriptableObject base class for custom dialogue text processors.
    /// Provides an extensible framework to modify or format raw dialogue strings before rendering.
    /// </summary>
    public abstract class DialogueTextFilterSO : ScriptableObject, IDialogueTextFilter
    {
        /// <summary>
        /// Processes and transforms the input dialogue string.
        /// </summary>
        /// <param name="text">The raw input text to process.</param>
        /// <returns>The modified or formatted string ready for rendering.</returns>
        public abstract string ProcessText(string text);
    }
}
