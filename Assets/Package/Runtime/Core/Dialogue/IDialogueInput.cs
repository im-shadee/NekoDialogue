using System;

namespace NekoDialogue.Core
{
    /// <summary>
    /// Contract defining input events required for dialogue navigation and selection.
    /// Implement this interface on a custom component to bridge input backends (e.g., Unity Input System or Legacy Input Manager) to dialogue UI panels.
    /// </summary>
    public interface IDialogueInput
    {
        public bool NavigateLeft { get; }

        public bool NavigateRight { get; }

        public bool Submit { get; }
    }
}
