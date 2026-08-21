using System.Text.RegularExpressions;
using UnityEngine;

namespace NekoDialogue.Core
{
    /// <summary>
    /// Dialogue text filter that converts Markdown elements into TextMeshPro formatting tags.
    /// </summary>
    [CreateAssetMenu(fileName = "NewMarkdownFilter", menuName = "NekoDialogue/Processing/Filters/MarkdownFilter")]
    public sealed class MarkdownFilterSO : DialogueTextFilterSO
    {
        // Shade: Regex Patterns
        // Matches start of line OR <br> tag before, and stops at next <br>, newline, or end of string
        private const string m_kPrefix = @"(?<=^|<br\s*/?>)\s*";
        private const string m_kSuffix = @"(?=<br\s*/?>|[\r\n]|$)";

        // Shade: Cache compiled Regex objects as static readonly fields to prevent allocations
        // Shade: Subtext (-# text) -> <size=75%>text</size>
        private static readonly Regex m_SubtextRegex = new Regex(
            $@"{m_kPrefix}-#\s*(.*?)\s*{m_kSuffix}",
            RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase
        );

        // Shade: Headings (## evaluated before # to prevent collision)
        private static readonly Regex m_Heading2Regex = new Regex(
            $@"{m_kPrefix}##\s*(.*?)\s*{m_kSuffix}",
            RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase
        );

        private static readonly Regex m_Heading1Regex = new Regex(
            $@"{m_kPrefix}#\s*(.*?)\s*{m_kSuffix}",
            RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase
        );

        // Shade: Bold: **myText** or __myText__ -> <b>myText</b>
        private static readonly Regex m_BoldRegex = new Regex(
            @"(\*\*|__)\s*(.*?)\s*\1",
            RegexOptions.Compiled
        );

        // Shade: Italic: _myText_ or *myText* -> <i>myText</i>
        private static readonly Regex m_ItalicRegex = new Regex(
            @"([_*])\s*(.*?)\s*\1",
            RegexOptions.Compiled
        );

        /// <summary>
        /// Processes the provided dialogue text and replaces Markdown markup with equivalent TextMeshPro tags.
        /// </summary>
        /// <param name="text">The raw dialogue string containing Markdown tags.</param>
        /// <returns>The formatted string containing rich text markup.</returns>
        public override string ProcessText(string text) => ProcessMarkdown(text);

        /// <summary>
        /// Converts Markdown elements (-# Subtext, # Header, ## Subheader, **Bold**, *Italic*) into TextMeshPro formatting tags.
        /// Enforces that header and subtext symbols occur at line starts or immediately following a &lt;br&gt; tag.
        /// </summary>
        /// <param name="text">The raw string to evaluate.</param>
        /// <returns>The string updated with TextMeshPro size, bold, and italic tags.</returns>
        private static string ProcessMarkdown(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            text = m_SubtextRegex.Replace(text, "<size=75%>$1</size>");
            text = m_Heading2Regex.Replace(text, "<size=125%><b>$1</b></size>");
            text = m_Heading1Regex.Replace(text, "<size=150%><b>$1</b></size>");
            text = m_BoldRegex.Replace(text, "<b>$2</b>");
            text = m_ItalicRegex.Replace(text, "<i>$2</i>");

            return text;
        }
    }
}
