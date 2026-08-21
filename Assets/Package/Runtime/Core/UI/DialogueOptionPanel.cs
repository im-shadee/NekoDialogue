using NekoDialogue.Core.Conversation;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace NekoDialogue.Core.UI
{
    /// <summary>
    /// UI Panel responsible for rendering, managing, and navigating branching dialogue choices using event-driven inputs.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class DialogueOptionPanel : MonoBehaviour
    {
        private int m_SelectedOption = 0;
        private bool m_bAllowSubmit = false;

        private Coroutine m_EnableSubmitRoutine = null;
        private Coroutine m_CursorMoveRoutine = null;

        private readonly Vector3[] m_SharedCornerBuffer = new Vector3[4];
        private readonly List<TextMeshProUGUI> m_SpawnedTextFields = new List<TextMeshProUGUI>(3);

        private int MaxOptions => m_SpawnedTextFields.Count;

        /// <summary>
        /// Gets the resolved input provider driving panel selection.
        /// </summary>
        public IDialogueInput InputProvider => m_InputProvider as IDialogueInput;

        private DialogueOptionsEntry[] m_CurrentBranchingEntries;
        private Action<DialogueOptionsEntry> m_OnOptionSelected = null;

        [Header("Canvas & Layout References")]
        [SerializeField, Tooltip("Reference to the Canvas parenting this panel element.")]
        private Canvas m_Canvas = null;

        [SerializeField, Tooltip("CanvasGroup component controlling visibility and raycasting for option choices.")]
        private CanvasGroup m_CanvasGroup = null;

        [Header("Font Settings")]
        [SerializeField, Tooltip("The min/max bounds applied to option text font auto-sizing.")]
        private Vector2 m_FontBounds = new Vector2(8f, 52f);

        [Header("Cursor Settings")]
        [SerializeField, Tooltip("The UI cursor object pointing to the selected option.")]
        private RectTransform m_MenuCursor = null;

        [SerializeField, Tooltip("Local position offset applied to the cursor relative to target option edges.")]
        private Vector2 m_CursorOffset = Vector2.zero;

        [SerializeField, Tooltip("Duration (in seconds) for the cursor to smoothly transition between options.")]
        private float m_CursorMoveDuration = 0.15f;

        [Header("Input Provider")]
        [SerializeField, Tooltip("MonoBehaviour implementing IDialogueInput to handle menu navigation.")]
        private MonoBehaviour m_InputProvider;

        #region Unity Lifecycle
        private void Awake()
        {
            if (m_Canvas == null)
            {
                NekoDialogueDebug.LogError($"DialogueOptionPanel: m_Canvas is not assigned on {name}.", this);
            }

            if (m_CanvasGroup == null)
            {
                NekoDialogueDebug.LogError($"DialogueOptionPanel: m_CanvasGroup is not assigned on {name}.", this);
            }
        }
        #endregion

        #region Visibility API
        /// <summary>
        /// Opens the panel, resets input selections, and subscribes to input navigation events.
        /// </summary>
        public void OnOpen()
        {
            gameObject.SetActive(true);

            // Shade: Reset selected option index to default
            m_SelectedOption = 0;

            _SubscribeInputEvents();

            if (m_CanvasGroup != null)
            {
                m_CanvasGroup.alpha = 1f;
                m_CanvasGroup.blocksRaycasts = true;
                m_CanvasGroup.interactable = true;
            }
        }

        /// <summary>
        /// Closes the panel, unsubscribes from events, stops active animations, and tears down text components.
        /// </summary>
        public void OnClose()
        {
            m_SelectedOption = 0;
            AllowSubmit(false);

            // Shade: Clear callback references and input event subscriptions
            m_OnOptionSelected = null;
            _UnsubscribeInputEvents();

            if (m_CursorMoveRoutine != null)
            {
                StopCoroutine(m_CursorMoveRoutine);
                m_CursorMoveRoutine = null;
            }

            // Shade: Destroy dynamic options and clear cached lists
            _Cleanup();

            if (m_CanvasGroup != null)
            {
                m_CanvasGroup.alpha = 0f;
                m_CanvasGroup.blocksRaycasts = false;
                m_CanvasGroup.interactable = false;
            }

            gameObject.SetActive(false);
        }
        #endregion

        #region Event Handlers
        private void _SubscribeInputEvents()
        {
            if (!_InputProviderNotNull()) return;

            InputProvider.OnNavigateLeft += _HandleNavigateLeft;
            InputProvider.OnNavigateRight += _HandleNavigateRight;
            InputProvider.OnSubmit += _HandleSubmit;
        }

        private void _UnsubscribeInputEvents()
        {
            if (!_InputProviderNotNull()) return;

            InputProvider.OnNavigateLeft -= _HandleNavigateLeft;
            InputProvider.OnNavigateRight -= _HandleNavigateRight;
            InputProvider.OnSubmit -= _HandleSubmit;
        }

        private void _HandleNavigateLeft()
        {
            _ChooseOption(-1);
            _UpdateCursor();
        }

        private void _HandleNavigateRight()
        {
            _ChooseOption(1);
            _UpdateCursor();
        }

        private void _HandleSubmit()
        {
            if (m_bAllowSubmit)
            {
                _ResumeDialogue();
            }
        }
        #endregion

        #region Public API
        /// <summary>
        /// Opens the panel and populates choices based on a branching dialogue line.
        /// </summary>
        /// <param name="line">Target branching line containing conversation choices.</param>
        /// <param name="onOptionSelected">Callback fired upon confirming an option selection.</param>
        public void ShowOptions(BranchingDialogueLine line, Action<DialogueOptionsEntry> onOptionSelected = null)
        {
            if (line == null)
            {
                NekoDialogueDebug.LogError("DialogueOptionPanel: Passed BranchingDialogueLine was null.", this);
                return;
            }

            m_CurrentBranchingEntries = line.ConversationOptions;
            OnOpen();

            Populate(
                m_CurrentBranchingEntries,
                line.BoxStyle?.TextColor,
                line.FontSettings?.FontSize,
                line.FontSettings?.Font
            );

            // Shade: Store decision selection callback
            m_OnOptionSelected = onOptionSelected;
        }

        /// <summary>
        /// Dynamically instantiates text elements for each provided option entry.
        /// </summary>
        /// <param name="options">Array of conversation options to display.</param>
        /// <param name="color">Optional text color override.</param>
        /// <param name="fontSize">Optional font size override.</param>
        /// <param name="font">Optional font asset override.</param>
        public void Populate(DialogueOptionsEntry[] options, Color? color = null, int? fontSize = null, TMP_FontAsset font = null)
        {
            if (options == null || options.Length == 0) return;

            int idx = 0;
            foreach (DialogueOptionsEntry option in options)
            {
                string optionName = option.OptionName;

                // Shade: Instantiate container GameObject for UI text styling
                GameObject textObject = new GameObject($"Option_{idx + 1:D2}_{optionName}");
                textObject.transform.SetParent(transform, worldPositionStays: false);

                // Shade: Attach and initialize TextMeshPro text component
                TextMeshProUGUI optionText = textObject.AddComponent<TextMeshProUGUI>();
                _InitializeText(optionText, optionName, color, fontSize, font);

                m_SpawnedTextFields.Add(optionText);
                idx++;
            }

            StartCoroutine(_InitialCursorPositionRoutine());
        }

        /// <summary>
        /// Controls whether submission input is allowed.
        /// </summary>
        /// <param name="value">State determining submit permission.</param>
        public void AllowSubmit(bool value)
        {
            if (m_EnableSubmitRoutine != null)
            {
                StopCoroutine(m_EnableSubmitRoutine);
                m_EnableSubmitRoutine = null;
            }

            if (value)
            {
                m_EnableSubmitRoutine = StartCoroutine(_EnableSubmitRoutine());
            }
            else
            {
                m_bAllowSubmit = false;
            }
        }
        #endregion

        #region Cursor Update
        private void _UpdateCursor(bool immediate = false)
        {
            if (m_CursorMoveRoutine != null)
            {
                StopCoroutine(m_CursorMoveRoutine);
                m_CursorMoveRoutine = null;
            }

            RectTransform targetTransform = _GetSelectedRect();
            if (targetTransform == null || m_MenuCursor == null) return;

            Vector3 targetLocalPoint = _CalculateLeftEdgeLocalPoint(targetTransform, m_MenuCursor.parent as RectTransform);

            if (immediate || m_CursorMoveDuration <= 0f)
            {
                // Shade: Snap cursor position directly without interpolation
                m_MenuCursor.localPosition = targetLocalPoint;
            }
            else
            {
                // Shade: Smoothly transition cursor without external tween dependencies
                m_CursorMoveRoutine = StartCoroutine(_AnimateCursorRoutine(targetLocalPoint));
            }
        }

        private IEnumerator _AnimateCursorRoutine(Vector3 targetLocalPoint)
        {
            Vector3 startPosition = m_MenuCursor.localPosition;
            float elapsed = 0f;

            while (elapsed < m_CursorMoveDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / m_CursorMoveDuration);
                m_MenuCursor.localPosition = Vector3.Lerp(startPosition, targetLocalPoint, t);
                yield return null;
            }

            m_MenuCursor.localPosition = targetLocalPoint;
            m_CursorMoveRoutine = null;
        }

        private IEnumerator _InitialCursorPositionRoutine()
        {
            // Shade: Wait for layout group calculation frame completion before positioning cursor
            yield return new WaitForEndOfFrame();
            _UpdateCursor(immediate: true);
        }
        #endregion

        #region Option Selection Internal Logic
        private void _ChooseOption(int direction)
        {
            int targetIndex = m_SelectedOption + direction;

            if (_IndexOutOfBounds(targetIndex)) return;

            m_SelectedOption = targetIndex;
        }

        private void _ResumeDialogue()
        {
            if (_TryGetChosenEntry(out DialogueOptionsEntry conversationOptions))
            {
                Action<DialogueOptionsEntry> callback = m_OnOptionSelected;

                OnClose();
                callback?.Invoke(conversationOptions);
            }
            else
            {
                NekoDialogueDebug.LogWarning($"DialogueOptionPanel: Submit attempted at invalid index '{m_SelectedOption}'.", this);
                OnClose();
            }
        }
        #endregion

        #region Helpers & Teardown
        private void _Cleanup()
        {
            if (m_SpawnedTextFields == null || m_SpawnedTextFields.Count == 0) return;

            for (int i = MaxOptions - 1; i >= 0; i--)
            {
                TextMeshProUGUI optionText = m_SpawnedTextFields[i];
                if (optionText == null) continue;

                Destroy(optionText.gameObject);
            }

            m_SpawnedTextFields.Clear();
        }

        private RectTransform _GetSelectedRect()
        {
            if (m_SpawnedTextFields == null || m_SpawnedTextFields.Count == 0) return null;
            if (_IndexOutOfBounds(m_SelectedOption)) return null;

            TextMeshProUGUI selectedOption = m_SpawnedTextFields[m_SelectedOption];
            return selectedOption == null ? null : selectedOption.rectTransform;
        }

        private bool _InputProviderNotNull() => InputProvider is UnityEngine.Object obj && obj != null;

        private bool _IndexOutOfBounds(int index) => index < 0 || index >= MaxOptions;

        private void _InitializeText(TextMeshProUGUI text, string line, Color? color = null, int? fontSize = null, TMP_FontAsset font = null)
        {
            text.color = color ?? Color.white;

            if (fontSize.HasValue) text.fontSize = fontSize.Value;
            if (font != null) text.font = font;

            text.enableAutoSizing = true;
            text.fontSizeMin = m_FontBounds.x;
            text.fontSizeMax = m_FontBounds.y;

            text.horizontalAlignment = HorizontalAlignmentOptions.Center;
            text.verticalAlignment = VerticalAlignmentOptions.Middle;

            text.text = line;
        }

        private IEnumerator _EnableSubmitRoutine()
        {
            yield return null;
            m_bAllowSubmit = true;
            m_EnableSubmitRoutine = null;
        }

        private bool _TryGetChosenEntry(out DialogueOptionsEntry conversationOptions)
        {
            conversationOptions = default;

            if (m_CurrentBranchingEntries == null || m_CurrentBranchingEntries.Length == 0) return false;
            if (_IndexOutOfBounds(m_SelectedOption)) return false;
            if (m_SelectedOption >= m_CurrentBranchingEntries.Length) return false;

            conversationOptions = m_CurrentBranchingEntries[m_SelectedOption];
            return true;
        }

        private Vector3 _CalculateLeftEdgeLocalPoint(RectTransform target, RectTransform pointerParent)
        {
            if (target == null || m_Canvas == null || pointerParent == null) return Vector3.zero;

            target.GetWorldCorners(m_SharedCornerBuffer);
            Camera uiCamera = m_Canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : m_Canvas.worldCamera;

            Vector3 worldLeft = (m_SharedCornerBuffer[0] + m_SharedCornerBuffer[1]) * 0.5f;
            Vector3 worldCenter = (m_SharedCornerBuffer[0] + m_SharedCornerBuffer[2]) * 0.5f;

            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, new Vector3(worldLeft.x, worldCenter.y, 0f));

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                pointerParent,
                screenPoint,
                uiCamera,
                out Vector2 localPoint
            );

            return new Vector3(localPoint.x + m_CursorOffset.x, localPoint.y + m_CursorOffset.y, 0f);
        }
        #endregion
    }
}
