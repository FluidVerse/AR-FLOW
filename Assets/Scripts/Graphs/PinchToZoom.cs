using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Graphs {
    /// <summary>
    /// Handles pinch to zoom gestures on touch screens.
    /// </summary>
    public class PinchToZoom : MonoBehaviour {

        /// <summary>
        /// Callback when zooming in using a pinch gesture.
        ///
        /// First parameter is the center point between the two touches in screen space, i.e. the zoom point. Second
        /// parameter is the zoom delta, i.e. the distance the fingers moved apart since the last frame.
        /// </summary>
        public UnityEvent<Vector2, float> onZoomIn;

        /// <summary>
        /// Callback when zooming out using a pinch gesture.
        ///
        /// First parameter is the center point between the two touches in screen space, i.e. the zoom point. Second
        /// parameter is the zoom delta, i.e. the distance the fingers moved closer since the last frame.
        /// </summary>
        public UnityEvent<Vector2, float> onZoomOut;

        /// <summary>
        /// Callback when starting zooming, i.e. when the pinch gesture starts.
        /// </summary>
        public UnityEvent onZoomStart;

        /// <summary>
        /// Callback when stopping zooming, i.e. when the pinch gesture ends.
        /// </summary>
        public UnityEvent onZoomEnd;

        /// <summary>
        /// Whether this script is actively checking for user input.
        /// </summary>
        public bool IsCheckingForInput { get; set; }

        /// <summary>
        /// Whether a pinch gesture is currently active.
        /// </summary>
        public bool IsZooming { get; private set; }

        private InputAction click0Action, point0Action, point1Action, contact1Action;

        private void Awake() {
            click0Action = InputSystem.actions.FindAction("UI/Click", true);
            point0Action = InputSystem.actions.FindAction("UI/Point0", true);
            point1Action = InputSystem.actions.FindAction("UI/Point1", true);
            contact1Action = InputSystem.actions.FindAction("UI/TouchContact1", true);
        }

        private void OnEnable() {
            click0Action.performed += OnZoomEnd;
            contact1Action.performed += OnContact1Performed;
        }

        private void OnDisable() {
            click0Action.performed -= OnZoomEnd;
            contact1Action.performed -= OnContact1Performed;
        }

        private void OnZoomEnd(InputAction.CallbackContext ctx) {
            if (!ctx.ReadValueAsButton() && IsZooming) {
                IsZooming = false;
                onZoomEnd?.Invoke();
            }
        }

        private void OnContact1Performed(InputAction.CallbackContext ctx) {
            OnZoomEnd(ctx);
            if (!IsCheckingForInput || IsZooming) {
                return;
            }
            IsZooming = true;
            onZoomStart?.Invoke();
            StartCoroutine(ZoomCoroutine());
        }

        private IEnumerator ZoomCoroutine() {
            float previousDistance = -1f;

            while (IsZooming) {
                Vector2 pos0 = point0Action.ReadValue<Vector2>();
                Vector2 pos1 = point1Action.ReadValue<Vector2>();
                Vector2 touchCenter = (pos0 + pos1) / 2f;

                float distance = Vector2.Distance(pos0, pos1);
                if (previousDistance < 0f) {
                    // skip first frame to first calculate proper previousDistance to avoid large jump
                    previousDistance = distance;
                    yield return null;
                }

                float diff = Mathf.Abs(distance - previousDistance);
                if (distance > previousDistance) {
                    onZoomIn?.Invoke(touchCenter, diff);
                } else if (distance < previousDistance) {
                    onZoomOut?.Invoke(touchCenter, diff);
                }

                previousDistance = distance;
                yield return null;
            }
        }
    }
}