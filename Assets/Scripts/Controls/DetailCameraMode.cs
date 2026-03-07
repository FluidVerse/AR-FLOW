using InteractionMenus;
using Toolbar;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Utils;

namespace Controls {
    /// <summary>
    /// Camera mode for showing a detailed view of an object.
    ///
    /// The camera will focus on the object and allow the user to rotate it by dragging. The rest of the scene will be
    /// hidden to avoid distractions. 
    /// </summary>
    public class DetailCameraMode : CameraMode {

        /// <summary>
        /// Multiplier for the distance from the detail object to the camera.
        /// </summary>
        private const float DistanceMultiplier = 1.25f;

        /// <summary>
        /// Callback when the detail view for objects is shown or hidden.
        ///
        /// If the detail view is shown, then <c>true</c> is passed, otherwise <c>false</c>.
        /// </summary>
        public UnityEvent<bool> onDetailViewToggle = new();

        /// <summary>
        /// Object to focus on in detail view.
        ///
        /// This should be set before activating the detail camera mode.
        /// </summary>
        public GameObject DetailObject { get; set; }

        /// <summary>
        /// Midpoint of the detail object, used as the pivot point for rotation and camera focus.
        /// </summary>
        public Vector3 MidPoint { get; set; }

        /// <summary>
        /// Canvas containing the movement controls for the detail view.
        /// 
        /// This will be hidden when the detail view is active.
        /// </summary>
        [SerializeField] private Canvas movementCanvas;

        /// <summary>
        /// Background image for the detail view, used to hide the rest of the scene and avoid distractions.
        /// </summary>
        [SerializeField] private RawImage detailBackground;

        private InputAction pointAction;
        private InteractionMenuManager interactionMenuManager;
        private ToolbarManager toolbarManager;
        private HelpFunctions helpFunctions;

        private Vector3 oldCameraPosition;
        private Quaternion oldCameraRotation;

        private void Awake() {
            pointAction = InputSystem.actions.FindAction("UI/Point", true);
            interactionMenuManager = FindAnyObjectByType<InteractionMenuManager>();
            toolbarManager = FindAnyObjectByType<ToolbarManager>();
            helpFunctions = new HelpFunctions();
            detailBackground.enabled = false;
        }

        protected override void OnCameraUpdate() {
            camera.transform.LookAt(MidPoint);
            if (!cameraParams.MouseDown || !cameraParams.IsCameraMovementAllowed) {
                return;
            }

            Vector3 mousePos = pointAction.ReadValue<Vector2>();
            Vector2 panelPos = new Vector2(mousePos.x, Screen.height - mousePos.y); // flip y coordinate
            if (toolbarManager.Pick(panelPos) != null) {
                return; // we clicked on some toolbar UI element, return
            }

            mousePos /= movementCanvas.scaleFactor; // ???, relic from the past?
            float y_rot = -(mousePos.x - Screen.width / 2f) * GlobalSettings.Instance.sensitivity *
                          Time.deltaTime;
            float x_rot = (mousePos.y - Screen.height / 2f) * GlobalSettings.Instance.sensitivity *
                          Time.deltaTime;

            camera.transform.RotateAround(MidPoint, Vector3.up, -y_rot);
            camera.transform.RotateAround(MidPoint, camera.transform.right, -x_rot);
        }

        protected override void OnActivate() {
            if (DetailObject == null) {
                return;
            }
            SaveCameraState();
            InitCameraPosition();
            interactionMenuManager.enabled = false;
            detailBackground.enabled = true;
            movementCanvas.enabled = false;
            helpFunctions.DisableAllGameObjectsBut(DetailObject);
            toolbarManager.HideCameraButtonContainer();
            onDetailViewToggle.Invoke(true);
        }

        protected override void OnDeactivate() {
            if (DetailObject == null) {
                return;
            }
            RestoreCameraState();
            helpFunctions.ResetAllGameObjects();
            interactionMenuManager.enabled = true;
            detailBackground.enabled = false;
            movementCanvas.enabled = true;
            toolbarManager.ShowCameraButtonContainer();
            onDetailViewToggle.Invoke(false);
        }


        private void SaveCameraState() {
            oldCameraPosition = camera.transform.position;
            oldCameraRotation = camera.transform.rotation;
        }

        private void RestoreCameraState() {
            camera.transform.position = oldCameraPosition;
            camera.transform.rotation = oldCameraRotation;
        }
        
        private void InitCameraPosition() {
            // Calculate MidPoint and Bounds
            Bounds bounds;
            Renderer r = DetailObject.GetComponent<Renderer>();
            if (r != null) {
                bounds = r.bounds;
                MidPoint = bounds.center;
            } else {
                Collider c = DetailObject.GetComponent<Collider>();
                if (c != null) {
                    bounds = c.bounds;
                    MidPoint = bounds.center;
                } else {
                    Debug.LogError("No renderer or collider found on the detail object.");
                    return;
                }
            }
            
            // Move camera to fit object
            float maxDimension = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            float objectRadius = maxDimension / 1.5f;
            float distance = objectRadius / Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);

            // Calculate dynamic multiplier based on object size (Logarithmic fit)
            float multiplier = (1.0f - 1.5f * Mathf.Log10(maxDimension)) * DistanceMultiplier;
            distance *= multiplier;

            // Adjust for aspect ratio 
            if (camera.aspect < 1) {
                distance /= camera.aspect;
            }

            // Keep the direction from object to camera, but adjust distance
            Vector3 direction = (camera.transform.position - MidPoint).normalized;
            if (direction == Vector3.zero) direction = -Vector3.forward; // fallback

            camera.transform.position = MidPoint + direction * distance;
            camera.transform.LookAt(MidPoint);
        }
    }
}