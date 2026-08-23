using NekoDialogue.Core.Conversation;
using UnityEngine;

namespace NekoDialogue.Core.Interaction
{
    /// <summary>
    /// Defines the contract for any object capable of receiving player or entity interaction within the game world.
    /// </summary>
    public interface IDialogueTarget
    {
        /// <summary>
        /// Executes interaction behavior when triggered by an interactor entity.
        /// </summary>
        /// <param name="initiator">The entity initiating the interaction.</param>
        public void StartDialogue(IDialogueInitiator initiator);

        /// <summary>
        /// Replaces the active conversation asset with a new instance for dynamic dialogue branching.
        /// </summary>
        /// <param name="conversation">The target conversation asset to assign.</param>
        public void SetConversationAsset(ConversationAsset conversation);
    }

    /// <summary>
    /// Defines the contract for an entity capable of initiating interactions with world objects.
    /// </summary>
    public interface IDialogueInitiator
    {
        /// <summary>
        /// Gets the world transform representation of the interactor entity.
        /// </summary>
        public Transform Transform { get; }
    }
}
