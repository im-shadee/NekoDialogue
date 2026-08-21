using NekoDialogue.Core.Conversation;
using System;

namespace NekoDialogue
{
    /// <summary>
    /// Provides utility extension methods for arrays of <see cref="DialogueOptionsEntry"/>.
    /// </summary>
    public static class DialogueOptionsEntryExtensions
    {
        /// <summary>
        /// Extracts all evaluated option text strings from an array of dialogue option entries.
        /// </summary>
        /// <param name="entries">The source array of dialogue option entries.</param>
        /// <returns>An array of evaluated string options, or an empty array if <paramref name="entries"/> is null.</returns>
        public static string[] GetOptions(this DialogueOptionsEntry[] entries)
        {
            if (entries == null) return Array.Empty<string>();

            string[] options = new string[entries.Length];

            for (int i = 0; i < entries.Length; i++)
            {
                options[i] = entries[i].OptionName;
            }

            return options;
        }

        /// <summary>
        /// Extracts all target conversation assets from an array of dialogue option entries.
        /// </summary>
        /// <param name="entries">The source array of dialogue option entries.</param>
        /// <returns>An array of target <see cref="ConversationAsset"/> instances, or an empty array if <paramref name="entries"/> is null.</returns>
        public static ConversationAsset[] GetConversations(this DialogueOptionsEntry[] entries)
        {
            if (entries == null) return Array.Empty<ConversationAsset>();

            ConversationAsset[] conversations = new ConversationAsset[entries.Length];

            for (int i = 0; i < entries.Length; i++)
            {
                conversations[i] = entries[i].Conversation;
            }

            return conversations;
        }
    }
}
