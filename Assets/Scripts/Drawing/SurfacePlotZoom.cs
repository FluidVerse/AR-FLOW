using System.Collections;
using Graphs;
using MainMenu;
using Toolbar;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Drawing {
    public class SurfacePlotZoom : MonoBehaviour {

        [SerializeField] private GameObject plotObject;
        [SerializeField] private PinchToZoom pinchToZoom;
        [SerializeField] private float pinchToZoomScaleFactor = 0.005f;

        private new Camera camera;
        private InputAction clickAction, pointAction;
        private ToolbarManager tm;
        private Vector3 basePosition;
        private Vector3 baseScale;

        private bool isZoomed; // is currently zoomed in/out?
        private Vector3 lastPivot;

        public bool ZoomEnabled {
            set {
                pinchToZoom.IsCheckingForInput = value;
                if (!value && isZoomed) {
                    ResetZoom();
                }
            }
        }

        private void Awake() {
            clickAction = InputSystem.actions.FindAction("UI/Click", true);
            pointAction = InputSystem.actions.FindAction("UI/Point", true);
            tm = FindAnyObjectByType<ToolbarManager>();
            camera = Camera.main;
        }

        private void Start() {
            clickAction.performed += OnClickPerformed;
            pinchToZoom.IsCheckingForInput = true;
            MainMenuManager.Instance.OnMainMenuToggle.AddListener(OnMainMenuToggle);
        }

        private void OnDisable() {
            clickAction.performed -= OnClickPerformed;
            MainMenuManager.Instance.OnMainMenuToggle.RemoveListener(OnMainMenuToggle);
        }

        private void OnClickPerformed(InputAction.CallbackContext ctx) {
            Vector3 mousePos = pointAction.ReadValue<Vector2>();
            Vector2 panelPos = new Vector2(mousePos.x, Screen.height - mousePos.y); // flip y coordinate
            if (!isZoomed || tm.Pick(panelPos) != null) {
                return; // not zooming or clicking on some toolbar UI element, return
            }

            // reset zoom after clicking on the screen
            ResetZoom();
        }

        /// <summary>
        /// Callback for <see cref="PinchToZoom.onZoomIn"/>.
        /// </summary>
        public void OnZoomIn(Vector2 touchCenter, float distance) {
            Vector3 newScale = plotObject.transform.localScale + Vector3.one * pinchToZoomScaleFactor * distance;
            PerformZoom(touchCenter, newScale);
        }

        /// <summary>
        /// Callback for <see cref="PinchToZoom.onZoomOut"/>.
        /// </summary>
        public void OnZoomOut(Vector2 touchCenter, float distance) {
            Vector3 newScale = plotObject.transform.localScale - Vector3.one * pinchToZoomScaleFactor * distance;
            if (newScale.x < baseScale.x) {
                newScale = baseScale;
            }
            PerformZoom(touchCenter, newScale);
        }

        /// <summary>
        /// Callback for <see cref="PinchToZoom.onZoomStart"/>.
        /// </summary>
        public void OnZoomStart() {
            basePosition = plotObject.transform.localPosition;
            baseScale = plotObject.transform.localScale;
        }

        /// <summary>
        /// Callback for <see cref="PinchToZoom.onZoomEnd"/>.
        /// </summary>
        public void OnZoomEnd() {
            // don't snap back immediately after letting go 
            // ResetZoom();
            StartCoroutine(SetIsZoomedCoroutine());
        }

        /// <summary>
        /// Sets <see cref="isZoomed"/> to <c>true</c> in the next frames.
        /// </summary>
        private IEnumerator SetIsZoomedCoroutine() {
            yield return null;
            yield return null;
            yield return null;
            isZoomed = true;
        }

        /// <summary>
        /// Resets the zoom level of the main object to its initial position and scale.
        /// </summary>
        private void ResetZoom() {
            plotObject.transform.localPosition = basePosition;
            plotObject.transform.localScale = baseScale;
            isZoomed = false;
        }

        /// <summary>
        /// Callback for <see cref="MainMenuManager.OnMainMenuToggle"/>.
        /// </summary>
        private void OnMainMenuToggle(bool isOpen) {
            pinchToZoom.IsCheckingForInput = !isOpen;
        }

        private void PerformZoom(Vector2 touchCenter, Vector3 newScale) {
            // 1. raycast on plane (surface plot, but infinitely large)
            Plane plane = new(-plotObject.transform.forward, plotObject.transform.position);
            Ray ray = camera.ScreenPointToRay(touchCenter);
            Vector3 pivot = plane.Raycast(ray, out float distance)
                ? ray.GetPoint(distance)
                : lastPivot; // zoomed "too far in" so that plane is behind camera, use last pivot instead
            lastPivot = pivot;

            // 2. perform translation and scaling
            Vector3 oldScale = plotObject.transform.localScale;
            Vector3 scaleRatio = new(newScale.x / oldScale.x, newScale.y / oldScale.y, newScale.z / oldScale.z);
            Vector3 dir = plotObject.transform.position - pivot;
            dir = Vector3.Scale(dir, scaleRatio);
            plotObject.transform.position = pivot + dir;
            plotObject.transform.localScale = newScale;
        }
    }
}