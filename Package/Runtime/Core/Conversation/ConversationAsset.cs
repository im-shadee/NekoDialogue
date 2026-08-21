using UnityEngine;
using System.Collections.Generic;

namespace NekoDialogue.Core.Conversation
{
    /// <summary>
    /// ScriptableObject for creating and managing conversation assets in the game.
    /// </summary>
    [CreateAssetMenu(fileName = "NewConversation", menuName = "NekoDialogue/Conversation/Conversation Asset")]
    public class ConversationAsset : ScriptableObject
    {
        [SerializeReference, Tooltip("The list of conversation lines to display for this dialogue.")]
        private List<DialogueLine> m_ConversationLines = new List<DialogueLine>();
        public IReadOnlyList<DialogueLine> ConversationLines => m_ConversationLines;
    }
}
