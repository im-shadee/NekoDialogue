using NekoDialogue.Core.Conversation;
using NekoDialogue.Core.Interaction;
using System;
using UnityEngine;

public class InteractableNPC : InteractableElement
{
    protected override void _HandleConversationEnded()
    {
        throw new NotImplementedException();
    }

    protected override void _InstantiateDialogue(ConversationAsset conversation, Action onConversationEnded, IDialogueInitiator interactor = null)
    {
        throw new NotImplementedException();
    }
}
