using System.Collections.Generic;
using Graphs.Content;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Graphs {
    /// <summary>
    /// Handles touch and mouse input for interacting with <see cref="GraphController"/> and
    /// <see cref="GraphContent"/>.
    /// </summary>
    public class GraphInput : MonoBehaviour {

        /// <summary>
        /// Callback when a new raycast result is available after the user starts a click/touch.
        /// </summary>
        public UnityEvent<List<RaycastResult>> onNewRaycastResult;

        /// <summary>
        /// Callback for a valid click on the graph background that has been successfully differentiated from a drag.
        ///
        /// Argument is the screen position where the user clicked.
        /// </summary>
        public UnityEvent<Vector2> OnValidGraphClick;

        /// <summary>
        /// Whether this script is actively checking for user input.
        /// </summary>
        public bool IsCheckingForInput { get; set; }

        [SerializeField] private GraphicRaycaster raycaster;
        [SerializeField] private GraphController controller;
        [SerializeField] private GraphContent content;
        [SerializeField] private Image graphBackground;
        [SerializeField] private PinchToZoom pinchToZoom;

        /// <summary>
        /// Whether movement (dragging) of the graph background is enabled.
        /// </summary>
        [SerializeField] private bool enableMovement = true;

        /// <summary>
        /// Whether zooming of the graph is enabled.
        /// </summary>
        [SerializeField] private bool enableZoom = true;

        /// <summary>
        /// Factor that determines how much zooming in is applied per scroll step.
        /// </summary>
        [SerializeField] private float scrollZoomFactor = 1.25f;

        /// <summary>
        /// Distance (in px) on the screen during the pinch gesture that corresponds to a single zoom step as in
        /// <see cref="scrollZoomFactor"/>.
        /// </summary>
        [SerializeField] private float pinchToZoomDistance = 50f;

        /// <summary>
        /// Minimum cumulative drag distance (in px) during a drag operation before the drag is considered valid.
        ///
        /// If <see cref="cumulativeDragDistance"/> is below, the action is counted as a click instead of a drag.
        /// </summary>
        [SerializeField] private float dragThresholdDistance = 30f;

        /// <summary>
        /// Minimum time (in seconds) the user must be touching the graph background before a drag is considered valid
        /// if <see cref="cumulativeDragDistance"/> is below <see cref="dragThresholdDistance"/>.
        /// </summary>
        [SerializeField] private float dragThresholdTime = 0.2f;

        private InputAction point0Action, clickAction, scrollAction;

        private readonly List<RaycastResult> raycastResults = new();
        private bool isTouchingGraph;
        private Vector2? lastTouchPosition; // can be null
        private float cumulativeDragDistance; // during a single drag operation
        private float touchDuration;

        private void Awake() {
            point0Action = InputSystem.actions.FindAction("UI/Point0", true);
            clickAction = InputSystem.actions.FindAction("UI/Click", true);
            scrollAction = InputSystem.actions.FindAction("UI/ScrollWheel", true);
        }

        private void Update() {
            if (isTouchingGraph) {
                touchDuration += Time.deltaTime;
            }
        }

        private void OnEnable() {
            point0Action.performed += OnPoint0Performed;
            clickAction.performed += OnClickPerformed;
            scrollAction.performed += OnScrollPerformed;
        }

        private void OnDisable() {
            point0Action.performed -= OnPoint0Performed;
            clickAction.performed -= OnClickPerformed;
            scrollAction.performed -= OnScrollPerformed;
        }

        private void OnPoint0Performed(InputAction.CallbackContext ctx) {
            Vector2 currentTouchPosition = ctx.ReadValue<Vector2>();
            if (IsCheckingForInput && isTouchingGraph && lastTouchPosition.HasValue) {
                bool isValidDrag = cumulativeDragDistance >= dragThresholdDistance ||
                                   touchDuration >= dragThresholdTime;

                if (isValidDrag) {
                    HandleDrag(lastTouchPosition.Value, currentTouchPosition);
                }

                cumulativeDragDistance += Vector2.Distance(lastTouchPosition.Value, currentTouchPosition);
            }
            lastTouchPosition = currentTouchPosition;
        }

        private void OnClickPerformed(InputAction.CallbackContext ctx) {
            bool isButton = ctx.ReadValueAsButton();
            Debug.Log($"[GraphInput] OnClickPerformed: isButton={isButton}, IsCheckingForInput={IsCheckingForInput}, IsZooming={pinchToZoom?.IsZooming}");

            if (isButton && IsCheckingForInput && !pinchToZoom.IsZooming) {
                // click started
                isTouchingGraph = IsTouchingBackground();
                // Position beim Klick-Start speichern (falls OnPoint0Performed nicht feuert)
                lastTouchPosition = GetCurrentPointerPosition();
                Debug.Log($"[GraphInput] Click started, isTouchingGraph={isTouchingGraph}, lastTouchPosition={lastTouchPosition}");
            } else if (!isButton) {
                // click released
                bool isValidClick = cumulativeDragDistance < dragThresholdDistance &&
                                    touchDuration < dragThresholdTime;
                Debug.Log($"[GraphInput] Click released: isTouchingGraph={isTouchingGraph}, isValidClick={isValidClick}, lastTouchPosition={lastTouchPosition}, dragDist={cumulativeDragDistance}, touchDur={touchDuration}");
                if (lastTouchPosition.HasValue && isValidClick) {
                    OnValidGraphClickPerformed(lastTouchPosition.Value);
                }
                isTouchingGraph = false;
                lastTouchPosition = null;
                cumulativeDragDistance = 0;
                touchDuration = 0;
            }
        }

        private void OnScrollPerformed(InputAction.CallbackContext ctx) {
            if (!IsCheckingForInput || !enableZoom) {
                return;
            }

            Vector2 scrollDelta = ctx.ReadValue<Vector2>();
            if (Mathf.Approximately(scrollDelta.y, 0f)) {
                return; // no vertical scroll, ignore
            }

            Vector2 touchPosition = point0Action.ReadValue<Vector2>();
            float factor = scrollDelta.y > 0 ? scrollZoomFactor : 1f / scrollZoomFactor;
            controller.Zoom(touchPosition, factor);
        }

        /// <summary>
        /// Callback for a valid click on the graph background that has been successfully differentiated from a drag.
        /// </summary>
        /// <param name="touchPosition">Screen position where the user clicked</param>
        private void OnValidGraphClickPerformed(Vector2 touchPosition) {
            Debug.Log($"[GraphInput] OnValidGraphClickPerformed at {touchPosition}, IsCheckingForInput={IsCheckingForInput}");
            if (!IsCheckingForInput) {
                return;
            }
            Debug.Log($"[GraphInput] Invoking OnValidGraphClick with {OnValidGraphClick?.GetPersistentEventCount()} listeners");
            OnValidGraphClick?.Invoke(touchPosition);
        }

        /// <summary>
        /// Callback for <see cref="PinchToZoom.onZoomIn"/>
        /// </summary>
        public void OnZoomInPerformed(Vector2 touchCenter, float distance) {
            if (!enableZoom) {
                return;
            }

            float factor = Mathf.LerpUnclamped(1f, scrollZoomFactor, distance / pinchToZoomDistance);
            controller.Zoom(touchCenter, factor);
        }

        /// <summary>
        /// Callback for <see cref="PinchToZoom.onZoomOut"/>
        /// </summary>
        public void OnZoomOutPerformed(Vector2 touchCenter, float distance) {
            if (!enableZoom) {
                return;
            }

            float factor = Mathf.LerpUnclamped(1f, scrollZoomFactor, distance / pinchToZoomDistance);
            controller.Zoom(touchCenter, 1f / factor);
        }

        /// <summary>
        /// Returns <c>true</c> if the user is touching any element within the graph area.
        /// Uses RectTransform bounds check instead of raycast (more reliable when Raycast Target is disabled).
        /// </summary>
        private bool IsTouchingBackground() {
            // Pointer-Position aus verschiedenen Quellen holen (point0Action ist manchmal (0,0) beim Klick-Start)
            Vector2 touchPosition = GetCurrentPointerPosition();

            // Bounds-Check: Ist der Touch innerhalb des graphBackground RectTransform?
            RectTransform rect = graphBackground.rectTransform;
            bool result = RectTransformUtility.RectangleContainsScreenPoint(rect, touchPosition, null);

            Debug.Log($"[GraphInput] IsTouchingBackground: touchPos={touchPosition}, result={result}");

            // Raycast für andere Listener beibehalten (z.B. Close-Button)
            PointerEventData eventData = new(EventSystem.current) {
                position = touchPosition
            };
            raycaster.Raycast(eventData, raycastResults);
            onNewRaycastResult?.Invoke(raycastResults);
            raycastResults.Clear();

            return result;
        }

        /// <summary>
        /// Gets the current pointer position from the best available source.
        /// </summary>
        private Vector2 GetCurrentPointerPosition() {
            // Erst aus point0Action versuchen
            Vector2 pos = point0Action.ReadValue<Vector2>();
            if (pos != Vector2.zero) {
                return pos;
            }

            // Fallback: Mouse position
            if (UnityEngine.InputSystem.Mouse.current != null) {
                pos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
                if (pos != Vector2.zero) {
                    return pos;
                }
            }

            // Fallback: Touchscreen
            if (UnityEngine.InputSystem.Touchscreen.current != null &&
                UnityEngine.InputSystem.Touchscreen.current.primaryTouch.press.isPressed) {
                return UnityEngine.InputSystem.Touchscreen.current.primaryTouch.position.ReadValue();
            }

            return pos;
        }

        /// <summary>
        /// Handles the drag input when the user moves their finger on the graph background.
        /// </summary>
        /// <param name="oldTouchPosition">Touch position in the last frame</param>
        /// <param name="newTouchPosition">Touch position in the current frame</param>
        private void HandleDrag(Vector2 oldTouchPosition, Vector2 newTouchPosition) {
            if (!enableMovement) {
                return;
            }

            Vector2 offset = oldTouchPosition - newTouchPosition;
            controller.MoveBackground(offset / controller.ZoomLevel);
        }
    }
}