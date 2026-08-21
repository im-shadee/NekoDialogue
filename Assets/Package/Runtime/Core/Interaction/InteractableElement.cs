using NekoDialogue.Core.Conversation;
using System;
using UnityEngine;

namespace NekoDialogue.Core.Interaction
{
    /// <summary>
    /// Serves as the base abstract implementation for objects in the scene that trigger dialogue interactions.
    /// </summary>
    public abstract class InteractableElement : MonoBehaviour, IDialogueTarget
    {
        // Shade: Holds reference to the actively assigned conversation asset
        private ConversationAsset m_ConversationToUse = null;
        protected ConversationAsset ConversationToUse => m_ConversationToUse;

        [Header("Main Interaction Settings")]
        [SerializeField, Tooltip("ScriptableObject that contains the conversation data for the entity.")]
        private ConversationAsset m_BaseConversation;

        [SerializeField, Tooltip("Sets interaction status. If false, no interaction will happen.")]
        private bool m_bAllowInteraction = true;

        #region Unity Lifecycle
        protected virtual void Awake()
        {
            if (m_BaseConversation == null)
            {
                NekoDialogueErrorLogger.LogError($"InteractableElement: No ConversationAsset assigned on {name}.");
                return;
            }

            // Shade: Default active conversation to base assignment on initialization
            m_ConversationToUse = m_BaseConversation;
        }
        #endregion

        #region IDialogueTarget Implementation
        /// <summary>
        /// Triggers interaction behavior using the assigned dialogue initiator.
        /// </summary>
        /// <param name="interactor">The entity initiating dialogue.</param>
        public void Interact(IDialogueInitiator interactor)
        {
            if (!CanInteract(interactor)) return;
            _InstantiateDialogue(m_ConversationToUse, _HandleConversationEnded, interactor);
        }
        #endregion

        #region Abstract Methods
        /// <summary>
        /// Spawns or triggers the dialogue UI instance with specified completion callbacks.
        /// </summary>
        /// <param name="conversation">Target conversation payload to present.</param>
        /// <param name="onConversationEnded">Callback method invoked when the conversation sequence completes.</param>
        /// <param name="interactor">Optional reference to initiating interactor entity.</param>
        protected abstract void _InstantiateDialogue(ConversationAsset conversation, Action onConversationEnded, IDialogueInitiator interactor = null);

        /// <summary>
        /// Handles teardown logic, state cleanup, or narrative triggers when conversation finishes.
        /// </summary>
        protected abstract void _HandleConversationEnded();
        #endregion

        #region Virtual Methods
        /// <summary>
        /// Validates whether the current entity state and initiator permit interaction execution.
        /// </summary>
        /// <param name="interactor">The entity attempting interaction.</param>
        /// <returns>True if interaction conditions pass; otherwise, false.</returns>
        protected virtual bool CanInteract(IDialogueInitiator interactor)
        {
            // Shade: Validate that interactor is non-null and underlying Unity Object reference is alive
            if (interactor == null || interactor as UnityEngine.Object == null)
            {
                NekoDialogueErrorLogger.LogError("InteractableElement: Interactor passed in Interact() was null or destroyed. Cannot start conversation.");
                return false;
            }

            return m_bAllowInteraction;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Replaces the active conversation asset with a new instance for conditional dialogue paths.
        /// </summary>
        /// <param name="conversation">The target conversation asset to assign.</param>
        public void SetConversationAsset(ConversationAsset conversation)
        {
            if (conversation == null)
            {
                NekoDialogueErrorLogger.LogError("InteractableElement: ConversationAsset passed in SetConversationAsset() was null.");
                return;
            }

            m_ConversationToUse = conversation;
        }

        /// <summary>
        /// Configures whether interaction attempts are currently allowed.
        /// </summary>
        /// <param name="value">State determining if interactions can proceed.</param>
        public void SetInteraction(bool value) => m_bAllowInteraction = value;
        #endregion
    }
}
