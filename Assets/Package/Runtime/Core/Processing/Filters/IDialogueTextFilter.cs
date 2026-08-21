namespace NekoDialogue.Core
{
    /// <summary>
    /// Contract for objects that parse, modify, or format dialogue text.
    /// </summary>
    public interface IDialogueTextFilter
    {
        /// <summary>
        /// Parses and transforms the input text.
        /// </summary>
        /// <param name="text">The text to process.</param>
        /// <returns>The transformed text.</returns>
        public string ProcessText(string text);
    }
}
