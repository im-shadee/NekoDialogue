using System;

namespace NekoDialogue.Core
{
    /// <summary>
    /// Contract defining input events required for dialogue navigation and selection.
    /// Implement this interface on a custom component to bridge input backends (e.g., Unity Input System or Legacy Input Manager) to dialogue UI panels.
    /// </summary>
    public interface IDialogueInput
    {
        /// <summary>
        /// Fired when a left navigation input is performed.
        /// </summary>
        public event Action OnNavigateLeft;

        /// <summary>
        /// Fired when a right navigation input is performed.
        /// </summary>
        public event Action OnNavigateRight;

        /// <summary>
        /// Fired when a submission or confirmation input is performed.
        /// </summary>
        public event Action OnSubmit;
    }
}
