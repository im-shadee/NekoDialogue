using NekoDialogue;
using NekoDialogue.Core;
using NekoDialogue.Core.Conversation;
using NekoDialogue.Core.Interaction;
using System;

/// <summary>
/// Represents an interactable NPC that initiates and manages dialogue sequences.
/// Binds input events to advance conversations and handles proper cleanup upon dialogue completion.
/// </summary>
/// <remarks>(For testing purposes)</remarks>
public class InteractableNPC : DialogueSource
{
    private IDialogueService m_CurrentDialogueManager = null;

    /// <summary>
    /// Initiates the dialogue sequence using the assigned conversation asset.
    /// </summary>
    public void StartConversation()
    {
        if (ConversationToUse != null)
        {
            InstantiateDialogue(ConversationToUse, HandleConversationEnded);
        }
        else
        {
            NekoDialogueDebug.LogError($"InteractableNPC: ConversationToUse not assigned on {name}. Cannot start dialogue.");
        }
    }

    /// <inheritdoc/>
    protected override void InstantiateDialogue(ConversationAsset conversation, Action onConversationEnded, IDialogueInitiator interactor = null)
    {
        m_CurrentDialogueManager = DialogueServiceLocator.Service;

        // Shade: Perform standard null check and Unity Object validity check if applicable
        if (m_CurrentDialogueManager == null || (m_CurrentDialogueManager is UnityEngine.Object unityObj && unityObj == null))
        {
            NekoDialogueDebug.LogError("InteractableNPC: No valid IDialogueService registered in DialogueServiceLocator.");
            return;
        }

        // Shade: Cache and bind the dialogue input bridge
        if (InputManagerSample.Instance != null)
        {
            InputManagerSample.Instance.OnSubmit += m_CurrentDialogueManager.AdvanceConversation;
        }
        else
        {
            NekoDialogueDebug.LogWarning("InteractableNPC: No InputManagerSample component found in scene to bind submit events.");
        }

        m_CurrentDialogueManager.OnConversationEnded += HandleConversationEnded;
        m_CurrentDialogueManager.StartConversation(conversation, transform);
    }

    /// <inheritdoc/>
    protected override void HandleConversationEnded()
    {
        // Shade: Unsubscribe submit input from advancing the conversation
        if (InputManagerSample.Instance != null && m_CurrentDialogueManager != null)
        {
            InputManagerSample.Instance.OnSubmit -= m_CurrentDialogueManager.AdvanceConversation;
        }

        // Shade: Unsubscribe conversation ended handler and clean reference
        if (m_CurrentDialogueManager != null && (m_CurrentDialogueManager as UnityEngine.Object) != null)
        {
            m_CurrentDialogueManager.OnConversationEnded -= HandleConversationEnded;
        }

        m_CurrentDialogueManager = null;
    }
}
