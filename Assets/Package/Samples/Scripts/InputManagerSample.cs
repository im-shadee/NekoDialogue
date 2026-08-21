using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManagerSample : MonoBehaviour
{
    // Singleton Instance
    public static InputManagerSample Instance { get; private set; }

    // Navigation and Interaction Events
    public event Action OnSubmit;

    // Input Actions
    private InputAction m_SubmitAction;
    private InputAction m_NavigateLeftAction;
    private InputAction m_NavigateRightAction;

    public bool SubmitPressed => m_SubmitAction.WasPressedThisFrame();
    public bool NavLeftPressed => m_NavigateLeftAction.WasPressedThisFrame();
    public bool NavRightPressed => m_NavigateRightAction.WasPressedThisFrame();

    private void Awake()
    {
        // Singleton Setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetupInputActions();
    }

    private void SetupInputActions()
    {
        // Initialize Submit Action (E, Enter, South Button / A)
        m_SubmitAction = new InputAction("Submit", type: InputActionType.Button);
        m_SubmitAction.AddBinding("<Keyboard>/e");
        m_SubmitAction.AddBinding("<Keyboard>/enter");
        m_SubmitAction.AddBinding("<Gamepad>/buttonSouth");

        // Initialize Left Navigation (A, Left Arrow, D-Pad Left)
        m_NavigateLeftAction = new InputAction("NavigateLeft", type: InputActionType.Button);
        m_NavigateLeftAction.AddBinding("<Keyboard>/a");
        m_NavigateLeftAction.AddBinding("<Keyboard>/leftArrow");
        m_NavigateLeftAction.AddBinding("<Gamepad>/dpad/left");

        // Initialize Right Navigation (D, Right Arrow, D-Pad Right)
        m_NavigateRightAction = new InputAction("NavigateRight", type: InputActionType.Button);
        m_NavigateRightAction.AddBinding("<Keyboard>/d");
        m_NavigateRightAction.AddBinding("<Keyboard>/rightArrow");
        m_NavigateRightAction.AddBinding("<Gamepad>/dpad/right");

        // Bind C# Events to Input Performed Contexts
        m_SubmitAction.performed += ctx => OnSubmit?.Invoke();
    }

    private void OnEnable()
    {
        m_SubmitAction?.Enable();
        m_NavigateLeftAction?.Enable();
        m_NavigateRightAction?.Enable();
    }

    private void OnDisable()
    {
        m_SubmitAction?.Disable();
        m_NavigateLeftAction?.Disable();
        m_NavigateRightAction?.Disable();
    }

    private void OnDestroy()
    {
        m_SubmitAction?.Dispose();
        m_NavigateLeftAction?.Dispose();
        m_NavigateRightAction?.Dispose();
    }
}
