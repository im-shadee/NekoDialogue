using NekoDialogue.Core.Conversation;
using NekoDialogue.Core.UI.Themes;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace NekoDialogue.Core.UI
{
    public class DialogueBoxUI : MonoBehaviour
    {
        // Shade: Define matching pivot points alongside anchor presets for all 9 standard positions
        private readonly Vector2 m_TopLeftAnchor = new Vector2(0f, 1f);
        private readonly Vector2 m_TopCenterAnchor = new Vector2(0.5f, 1f);
        private readonly Vector2 m_TopRightAnchor = new Vector2(1f, 1f);

        private readonly Vector2 m_MiddleLeftAnchor = new Vector2(0f, 0.5f);
        private readonly Vector2 m_MiddleCenterAnchor = new Vector2(0.5f, 0.5f);
        private readonly Vector2 m_MiddleRightAnchor = new Vector2(1f, 0.5f);

        private readonly Vector2 m_BottomLeftAnchor = new Vector2(0f, 0f);
        private readonly Vector2 m_BottomCenterAnchor = new Vector2(0.5f, 0f);
        private readonly Vector2 m_BottomRightAnchor = new Vector2(1f, 0f);

        private RectTransform m_CanvasRect = null;
        private eSpeechBubbleVerticalEdge m_CurrentTailEdge;

        private struct DialogueBoxLayoutParams
        {
            // Shade: Positioning
            public Vector2 Position;

            // Shade: Anchors
            public Vector2 Anchor;
            public Vector2 Pivot;

            public DialogueBoxLayoutParams(Vector2 anchor, Vector2 pivot, Vector2 position)
            {
                Anchor = anchor;
                Pivot = pivot;
                Position = position;
            }
        }

        [Header("Global References")]
        [SerializeField]
        private Canvas m_ParentCanvas;

        [SerializeField, Tooltip("Manages the visibility and interaction of the dialogue box.")]
        private CanvasGroup m_CanvasGroup = null;

        [SerializeField, Tooltip("Main camera following the players for transform to screen-space conversions.")]
        private Camera m_MainCamera = null;

        [Header("Box UI References")]
        [SerializeField, Tooltip("The parent transform to move around on the canvas. Contains the box's UI and text.")]
        private RectTransform m_DialogueBox = null;

        [SerializeField, Tooltip("The background of this box.")]
        private Image m_BoxBackground = null;

        [Header("Tail UI References")]
        [SerializeField]
        private RectTransform m_TailRectTransform = null;

        [SerializeField, Tooltip("The background of the speech bubble tail.")]
        private Image m_TailBackground = null;

        [Header("Tail Layout Configuration")]
        [SerializeField, Range(0.0f, 0.5f), Tooltip("Shade: Normalized X position ratio threshold below which the tail uses the Left sprite zone.")]
        private float m_LeftZoneThreshold = 0.35f;

        [SerializeField, Range(0.5f, 1.0f), Tooltip("Shade: Normalized X position ratio threshold above which the tail reuses the Left sprite zone flipped horizontally.")]
        private float m_RightZoneThreshold = 0.65f;

        #region Unity Lifecycle
        private void Awake()
        {
            if (m_ParentCanvas == null)
            {
                NekoDialogueDebug.LogError($"DialogueBoxUI: m_ParentCanvas not assigned on {name}.");
            }
            else
            {
                m_CanvasRect = m_ParentCanvas.GetComponent<RectTransform>();
            }

            if (m_CanvasGroup == null)
            {
                NekoDialogueDebug.LogError($"DialogueManager: m_CanvasGroup not assigned on {name}.");
            }

            if (m_DialogueBox == null)
            {
                NekoDialogueDebug.LogError($"DialogueBoxUI: m_DialogueBox not assigned on {name}.");
            }

            if (m_BoxBackground == null)
            {
                NekoDialogueDebug.LogError($"DialogueBoxUI: m_BoxBackground not assigned on {name}.");
            }

            if (m_TailRectTransform == null)
            {
                NekoDialogueDebug.LogError($"DialogueBoxUI: m_TailRectTransform not assigned on {name}.");
            }

            if (m_TailBackground == null)
            {
                NekoDialogueDebug.LogError($"DialogueBoxUI: m_TailBackground not assigned on {name}.");
            }

            if (m_MainCamera == null)
            {
                NekoDialogueDebug.LogError($"DialogueManager: m_MainCamera not assigned on {name}.");
            }
        }
        #endregion

        #region Public API
        public void ApplyBoxStyle(DialogueBoxStyle boxStyle)
        {
            if (boxStyle == null) return;

            if (m_BoxBackground != null)
            {
                m_BoxBackground.sprite = boxStyle.Background;
                m_BoxBackground.color = boxStyle.BackgroundColor;
            }

            if (m_TailBackground != null)
            {
                m_TailBackground.color = boxStyle.BackgroundColor;
            }
        }

        /// <summary>
        /// Shade: Calculates and applies the anchor, pivot, position, offset, and size for the dialogue box.
        /// </summary>
        public void ApplyBoxLayout(DialogueBoxLayout boxLayout, Vector3 speakerPosition, Camera camera = null)
        {
            if (camera == null) camera = m_MainCamera;

            DialogueBoxLayoutParams layoutParams = _ResolveLayoutParams(boxLayout, speakerPosition, camera);
            _SetDialogueBoxLayout(layoutParams, boxLayout.Size);
        }

        /// <summary>
        /// Shade: Positions, flips, and sets sprites for the speech bubble tail based on line settings and speaker location.
        /// </summary>
        public void ResolveTailPlacement(DialogueLine line, Vector3 speakerWorldPosition, Camera worldCamera = null)
        {
            if (m_TailRectTransform == null) return;
            if (worldCamera == null) worldCamera = m_MainCamera;

            SpeechBubbleTailSettings settings = line.TailSettings;

            if (!settings.EnableTail)
            {
                // Shade: Render the tail object invisible and early return
                m_TailRectTransform.gameObject.SetActive(false);
                return;
            }

            m_TailRectTransform.gameObject.SetActive(true);

            // Shade: Get the screen point position of the speaking entity from its world position
            Vector3 speakerScreenPoint = worldCamera.WorldToScreenPoint(speakerWorldPosition);

            // Shade: Resolve the vertical alignment of the tail (display at the top or bottom of the box)
            eSpeechBubbleVerticalEdge resolvedEdge = _ResolveVerticalEdge(settings, speakerScreenPoint);
            m_CurrentTailEdge = resolvedEdge;

            // Shade: Resolve the tail's horizontal position along the box's width
            float resolvedNormX = _ResolveHorizontalPosition(settings, speakerScreenPoint);

            // Shade: Apply style and determine the correct sprite to use.
            // Since we only use a 'middle' and a 'left' sprite for the tail, we flip the left one if we position it on the right.
            // This is determined and returned by _ApplyTailStyle.
            bool isFlippedX = _ApplyTailStyle(line.BoxStyle, resolvedNormX);

            // Shade: Update tail anchors, position, and scale
            _ApplyTailTransform(resolvedNormX, resolvedEdge, isFlippedX);
        }

        public IEnumerator PlayAnimation(BoxAnimationSO boxAnimationSO, Action onComplete = null)
        {
            if (boxAnimationSO == null) yield break;

            yield return boxAnimationSO.Execute(m_DialogueBox);
            onComplete?.Invoke();
        }

        public void ClampDialogueBox() => StartCoroutine(_ResolveAndClamp());
        #endregion

        #region Visibility API
        public void Show()
        {
            // Shade: Safety check in case this is called internally/externally while the canvas group is null
            if (m_CanvasGroup == null) return;

            m_CanvasGroup.alpha = 1;
            m_CanvasGroup.interactable = true;
        }

        public void Hide()
        {
            // Shade: Safety check in case this is called internally/externally while the canvas group is null
            if (m_CanvasGroup == null) return;

            m_CanvasGroup.alpha = 0;
            m_CanvasGroup.interactable = false;
        }
        #endregion

        #region Layout Helpers
        private DialogueBoxLayoutParams _ResolveLayoutParams(DialogueBoxLayout boxLayout, Vector3 speakerPosition, Camera camera)
        {
            // Shade: Calculate the base position based on placement type
            Vector2 basePosition = boxLayout.BoxPlacement switch
            {
                eDialogueBoxPlacement.Custom => boxLayout.Position,
                eDialogueBoxPlacement.Speaker => _GetSpeakerLocalPosition(speakerPosition, camera),
                _ => Vector2.zero // Shade: Standard anchors start at zero base position
            };

            // Shade: Apply the layout offset to the base position
            Vector2 finalPosition = basePosition + boxLayout.Offset;

            // Shade: Return the populated layout parameters with matching anchor/pivot mappings
            return boxLayout.BoxPlacement switch
            {
                eDialogueBoxPlacement.TopLeft => new DialogueBoxLayoutParams(m_TopLeftAnchor, m_TopLeftAnchor, finalPosition),
                eDialogueBoxPlacement.TopCenter => new DialogueBoxLayoutParams(m_TopCenterAnchor, m_TopCenterAnchor, finalPosition),
                eDialogueBoxPlacement.TopRight => new DialogueBoxLayoutParams(m_TopRightAnchor, m_TopRightAnchor, finalPosition),

                eDialogueBoxPlacement.MiddleLeft => new DialogueBoxLayoutParams(m_MiddleLeftAnchor, m_MiddleLeftAnchor, finalPosition),
                eDialogueBoxPlacement.MiddleCenter => new DialogueBoxLayoutParams(m_MiddleCenterAnchor, m_MiddleCenterAnchor, finalPosition),
                eDialogueBoxPlacement.MiddleRight => new DialogueBoxLayoutParams(m_MiddleRightAnchor, m_MiddleRightAnchor, finalPosition),

                eDialogueBoxPlacement.BottomLeft => new DialogueBoxLayoutParams(m_BottomLeftAnchor, m_BottomLeftAnchor, finalPosition),
                eDialogueBoxPlacement.BottomCenter => new DialogueBoxLayoutParams(m_BottomCenterAnchor, m_BottomCenterAnchor, finalPosition),
                eDialogueBoxPlacement.BottomRight => new DialogueBoxLayoutParams(m_BottomRightAnchor, m_BottomRightAnchor, finalPosition),

                // Shade: Custom and Speaker default to MiddleCenter as their baseline anchor/pivot
                eDialogueBoxPlacement.Custom => new DialogueBoxLayoutParams(m_MiddleCenterAnchor, m_MiddleCenterAnchor, finalPosition),
                eDialogueBoxPlacement.Speaker => new DialogueBoxLayoutParams(m_MiddleCenterAnchor, m_MiddleCenterAnchor, finalPosition),

                _ => new DialogueBoxLayoutParams(m_MiddleCenterAnchor, m_MiddleCenterAnchor, finalPosition)
            };
        }

        /// <summary>
        /// Shade: Projects a 3D entity position in world space to 2D UI local space,
        /// handling behind-camera inversion.
        /// </summary>
        private Vector2 _GetSpeakerLocalPosition(Vector3 speakerPosition, Camera worldCamera)
        {
            if (worldCamera == null) return Vector2.zero;

            // Shade: Convert speaker world space coordinate into screen pixel coordinates
            Vector3 screenPoint = worldCamera.WorldToScreenPoint(speakerPosition);

            // Shade: Handle object being behind the camera (Z < 0)
            if (screenPoint.z < 0)
            {
                screenPoint.x = Screen.width - screenPoint.x;
                screenPoint.y = Screen.height - screenPoint.y;
            }

            // Shade: UI Projections need the Canvas's world camera for Non-Overlay canvases
            Camera uiCamera = _GetUICamera();

            RectTransform parentRect = _GetBoxParent();

            // Shade: Convert screen coordinates into local point coordinates within the parent's Rect
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPoint, uiCamera, out Vector2 localPoint))
            {
                return localPoint;
            }

            return Vector2.zero;
        }

        private void _SetDialogueBoxLayout(DialogueBoxLayoutParams boxLayoutParams, Vector2 size)
        {
            if (m_DialogueBox == null) return;

            Vector2 anchor = boxLayoutParams.Anchor;
            Vector2 pivot = boxLayoutParams.Pivot;

            // Shade: Anchor setup
            m_DialogueBox.anchorMin = anchor;
            m_DialogueBox.anchorMax = anchor;
            m_DialogueBox.pivot = pivot;
            m_DialogueBox.sizeDelta = size;

            // Shade: Actually apply the calculated position to the RectTransform
            m_DialogueBox.anchoredPosition = boxLayoutParams.Position;
        }
        #endregion

        #region Clamping Helpers
        private IEnumerator _ResolveAndClamp()
        {
            if (m_DialogueBox == null) yield break;

            // Shade: Wait for layout rebuild
            yield return null;

            float overflow = _GetTailOverflow();

            _ClampToCanvas(
                m_DialogueBox.anchorMin,
                m_DialogueBox.pivot,
                m_DialogueBox.sizeDelta,
                m_DialogueBox.anchoredPosition,
                m_CurrentTailEdge == eSpeechBubbleVerticalEdge.Top ? overflow : 0f,
                m_CurrentTailEdge == eSpeechBubbleVerticalEdge.Bottom ? overflow : 0f);
        }

        private void _ClampToCanvas(
            Vector2 anchor,
            Vector2 pivot,
            Vector2 size,
            Vector2 targetPosition,
            float topMargin = 0f,
            float bottomMargin = 0f)
        {
            // Shade: Resolve parent rect transform container
            RectTransform parentRect = _GetBoxParent();
            Vector2 parentSize = parentRect.rect.size;

            // Shade: Calculate positional bounds in local anchored space based on anchors, pivot, and container
            // sizes, including whether the tail is present or not
            float minX = (pivot.x * size.x) - (anchor.x * parentSize.x);
            float maxX = (parentSize.x * (1f - anchor.x)) - ((1f - pivot.x) * size.x);

            float minY = (pivot.y * size.y) - (anchor.y * parentSize.y) + bottomMargin;
            float maxY = (parentSize.y * (1f - anchor.y)) - ((1f - pivot.y) * size.y) - topMargin;

            targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);

            m_DialogueBox.anchoredPosition = targetPosition;
        }
        #endregion

        #region Speech Bubble Tail Helpers
        private eSpeechBubbleVerticalEdge _ResolveVerticalEdge(SpeechBubbleTailSettings settings, Vector3 speakerScreenPoint)
        {
            // Shade: Just return the vertical edge value if we don't auto-detect it
            if (settings.VerticalEdge != eSpeechBubbleVerticalEdge.AutoDetect) return settings.VerticalEdge;

            // Shade: Else, determine whether to show the tail under or above the dialogue box depending on
            // the current speaking entity position
            bool isBoxAboveSpeaker = m_DialogueBox.position.y > speakerScreenPoint.y;

            return isBoxAboveSpeaker ? eSpeechBubbleVerticalEdge.Bottom : eSpeechBubbleVerticalEdge.Top;
        }

        private float _ResolveHorizontalPosition(SpeechBubbleTailSettings settings, Vector3 speakerScreenPoint)
        {
            // Shade: If the designer chose to manually assign a position for the tail, simply return this position
            // NB: We only return a normalized X position since the height is dependent on the vertical edge.
            // X position is calculated as a percentage of the box's width (5%-95%)
            if (!settings.AutoPositionX) return settings.NormalizedXPosition;

            // Shade: Determine the correct camera to use depending on render mode.
            // Overlay requires no camera, while the other two modes do.
            Camera uiCamera = _GetUICamera();

            // Shade: Project the speaker's screen coordinates into local 2D space relative to m_DialogueBox.
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                m_DialogueBox,
                speakerScreenPoint,
                uiCamera,
                out Vector2 localPoint))
            {
                // Shade: Retrieve the local bounding rect dimensions of the dialogue box container (xMin to xMax).
                Rect boxRect = m_DialogueBox.rect;

                // Shade: Calculate where localPoint.x sits across the box width as a normalized ratio (0.0 = left edge, 1.0 = right edge).
                float rawNormX = Mathf.InverseLerp(boxRect.xMin, boxRect.xMax, localPoint.x);

                // Shade: Clamp normalized X between 0.05 (5%) and 0.95 (95%) so the tail sprite remains visually
                // connected to the box without overflowing or floating past rounded corners.
                return Mathf.Clamp(rawNormX, 0.05f, 0.95f);
            }

            // Shade: Safety fallback if the translation failed
            return settings.NormalizedXPosition;
        }

        private bool _ApplyTailStyle(DialogueBoxStyle style, float resolvedNormX)
        {
            if (style == null) return false;

            bool isFlippedX = false;

            // Shade: Left Zone — Tail is positioned near the left edge (< LeftZoneThreshold).
            // Use standard left sprite.
            if (resolvedNormX < m_LeftZoneThreshold)
            {
                if (m_TailBackground != null) m_TailBackground.sprite = style.TailLeftSprite;
            }

            // Shade: Right Zone — Tail is positioned near the right edge (> RightZoneThreshold).
            // Reuses the Left sprite asset and flips it horizontally on the X axis.
            else if (resolvedNormX > m_RightZoneThreshold)
            {
                if (m_TailBackground != null) m_TailBackground.sprite = style.TailLeftSprite;
                isFlippedX = true;
            }

            // Shade: Middle Zone — Tail is positioned in the center zone between left and right thresholds.
            else
            {
                if (m_TailBackground != null) m_TailBackground.sprite = style.TailMiddleSprite;
            }

            return isFlippedX;
        }

        private void _ApplyTailTransform(float resolvedNormX, eSpeechBubbleVerticalEdge resolvedEdge, bool isFlippedX)
        {
            // Shade: Determine if the tail attaches to the top or bottom edge of the dialogue box container.
            bool isTop = resolvedEdge == eSpeechBubbleVerticalEdge.Top;

            // Shade: Pin anchors to the resolved X percentage along top (Y = 1) or bottom (Y = 0).
            m_TailRectTransform.anchorMin = new Vector2(resolvedNormX, isTop ? 1f : 0f);
            m_TailRectTransform.anchorMax = new Vector2(resolvedNormX, isTop ? 1f : 0f);

            // Shade: Keep vertical pivot at 1.0 (base of tail) for both top and bottom edges.
            m_TailRectTransform.pivot = new Vector2(0.5f, 1f);

            // Shade: Reset anchored position (if adding a custom Y offset here, multiply by -1f when isTop is true)
            m_TailRectTransform.anchoredPosition = Vector2.zero;

            // Shade: Invert local scale axes:
            // - Scale X = -1 flips horizontally for right placement.
            // - Scale Y = -1 flips vertically (multiplies direction by -1) when positioned at the top edge.
            float scaleX = isFlippedX ? -1f : 1f;
            float scaleY = isTop ? -1f : 1f;
            m_TailRectTransform.localScale = new Vector3(scaleX, scaleY, 1f);
        }

        private float _GetTailOverflow()
        {
            // Shade: Ensure we only measure if the tail reference exists and is active
            if (m_TailRectTransform == null || !m_TailRectTransform.gameObject.activeSelf) return 0f;

            // Shade: Grab the local height directly from the RectTransform component
            float tailHeight = m_TailRectTransform.rect.height;

            // Shade: Apply the local scale of the tail (since scaleY becomes -1 when the tail is at the top edge)
            float effectiveHeight = tailHeight * Mathf.Abs(m_TailRectTransform.localScale.y);

            return effectiveHeight;
        }
        #endregion

        #region Global Helpers
        private RectTransform _GetBoxParent()
        {
            if (m_DialogueBox == null || m_CanvasRect == null) return null;

            RectTransform parent = m_DialogueBox.parent as RectTransform;
            return parent == null ? m_CanvasRect : parent;
        }

        private Camera _GetUICamera()
        {
            if (m_ParentCanvas == null) return null;
            return m_ParentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : m_ParentCanvas.worldCamera;
        }
        #endregion

    }
}
