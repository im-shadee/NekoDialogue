using NekoDialogue.Core;
using UnityEngine;

/// <summary>
/// Bridges events from <see cref="InputManagerSample"/> to the <see cref="IDialogueInput"/> interface.
/// </summary>
public class DialogueInputBridge : MonoBehaviour, IDialogueInput
{
    public bool NavigateLeft => InputManagerSample.Instance != null && InputManagerSample.Instance.NavLeftPressed;
    public bool NavigateRight => InputManagerSample.Instance != null && InputManagerSample.Instance.NavRightPressed;
    public bool Submit => InputManagerSample.Instance != null && InputManagerSample.Instance.SubmitPressed;
}
