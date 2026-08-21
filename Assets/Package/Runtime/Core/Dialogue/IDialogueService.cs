using NekoDialogue.Core.Conversation;
using UnityEngine;

namespace NekoDialogue.Core
{
    /// <summary>
    /// Defines the core contract for a service that manages the lifecycle, display, and progression of dialogue sequences.
    /// </summary>
    public interface IDialogueService
    {
        /// <summary>
        /// Initiates a new conversation sequence using the provided asset.
        /// </summary>
        /// <param name="conversation">The conversation asset containing the sequence of dialogue lines.</param>
        /// <param name="speakingEntity">Optional transform of the entity initiating or anchoring the dialogue in the world.</param>
        public void StartConversation(ConversationAsset conversation, Transform speakingEntity = null);

        /// <summary>
        /// Terminates the current active conversation state and handles associated UI teardown or cleanup.
        /// </summary>
        public void EndConversation();

        /// <summary>
        /// Triggers the visual rendering or display of the currently active dialogue line.
        /// </summary>
        public void ShowCurrentLine();

        /// <summary>
        /// Progresses the active conversation to the next available line or decision branch.
        /// </summary>
        public void AdvanceConversation();

        /// <summary>
        /// Prepares raw dialogue for display by running built-in formatting followed by custom preprocessor pipelines.
        /// </summary>
        /// <param name="rawText">The unformatted text payload to process.</param>
        /// <returns>The fully formatted and processed dialogue string ready for UI presentation.</returns>
        public string FormatDialogueLine(string rawText);
    }
}
