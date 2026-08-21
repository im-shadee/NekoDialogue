namespace NekoDialogue.Core
{
    /// <summary>
    /// Global registry allowing interactable elements to resolve the active IDialogueService instance.
    /// </summary>
    public static class DialogueServiceLocator
    {
        private static IDialogueService m_ActiveService;
        public static IDialogueService Service => m_ActiveService;

        /// <summary>
        /// Registers a custom or sample IDialogueService implementation as the global active instance.
        /// </summary>
        public static void RegisterService(IDialogueService service)
        {
            m_ActiveService = service;
        }

        /// <summary>
        /// Unregisters the current service if it matches the active instance.
        /// </summary>
        public static void UnregisterService(IDialogueService service)
        {
            if (m_ActiveService == service)
            {
                m_ActiveService = null;
            }
        }
    }
}
