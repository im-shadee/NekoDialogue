using NekoDialogue.Core;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// Dialogue text filter that strips newline characters and collapses multi-line text into a single continuous line.
/// </summary>
[CreateAssetMenu(fileName = "NewNewlineFilter", menuName = "NekoDialogue/Processing/Filters/NewlineFilter")]
public class NewlineFilterSO : DialogueTextFilterSO
{
    // Shade: Cached regex matching line breaks (\r, \n) and surrounding whitespace.
    private static readonly Regex m_NewlineRegex = new Regex(
        @"\s*[\r\n]+\s*",
        RegexOptions.Compiled
    );

    /// <summary>
    /// Processes the provided dialogue text by removing newlines and consolidating spacing.
    /// </summary>
    /// <param name="text">The raw dialogue string containing newlines.</param>
    /// <returns>A single-line string with normalized spacing.</returns>
    public override string ProcessText(string text) => ProcessNewlines(text);

    /// <summary>
    /// Strips line breaks (\r, \n) and collapses any surrounding whitespace down to a single space.
    /// </summary>
    /// <param name="text">The target dialogue string.</param>
    /// <returns>A clean, single-line dialogue string.</returns>
    private static string ProcessNewlines(string text) => string.IsNullOrEmpty(text) ? text : m_NewlineRegex.Replace(text, " ");
}
