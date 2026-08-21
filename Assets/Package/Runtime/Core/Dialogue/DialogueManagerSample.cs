using NekoDialogue.Core;
using NekoDialogue.Core.Conversation;
using UnityEngine;

public class DialogueManagerSample : MonoBehaviour, IDialogueService
{

    #region Unity Lifecycle
    private void Awake()
    {
        // Shade: Register this instance as the primary active dialogue service
        DialogueServiceLocator.RegisterService(this);
    }

    private void OnDestroy()
    {
        // Shade: Clean up service reference when destroyed
        DialogueServiceLocator.UnregisterService(this);
    }
    #endregion

    #region IDialogueService Implementation
    public void StartConversation(ConversationAsset conversation, Transform speakingEntity = null)
    {
        throw new System.NotImplementedException();
    }

    public void ShowCurrentLine()
    {
        throw new System.NotImplementedException();
    }

    public void AdvanceConversation()
    {
        throw new System.NotImplementedException();
    }

    public void EndConversation()
    {
        throw new System.NotImplementedException();
    }

    public string FormatDialogueLine(string rawText)
    {
        throw new System.NotImplementedException();
    }
    #endregion
}