using System.Collections.Generic;
using ActionLog;
using UnityEngine;
using UnityEngine.Events;

namespace Controls {
    /// <summary>
    /// Manages the camera modes in the application, allowing switching between detail view, spline path view, and
    /// aerial view.
    ///
    /// Important: this class does not implement the actual camera movement logic for each mode. Instead, it only
    /// handles switching between the different implementations (see <see cref="CameraMode"/>).
    /// </summary>
    public class CameraManager : MonoBehaviour {

        /// <summary>
        /// Callback when the camera mode is changed.
        ///
        /// The new camera mode is passed as a parameter to the callback.
        /// </summary>
        public UnityEvent<CameraType> onCameraModeChanged;

        /// <summary>
        /// If <c>true</c>, the camera mode changes back to the default mode when the back button is clicked in detail
        /// view.
        /// </summary>s
        public bool ChangeCameraOnBackButton { get; set; } = true;

        /// <summary>
        /// Currently active camera mode.
        /// </summary>
        public CameraType camType;

        private CameraType? oldCamType;
        private bool _isJoystickPressed;

        private ActionLogManager log;

        private DetailCameraMode detailCameraMode;
        private SplineCameraMode splineCameraMode;
        private AerialCameraMode aerialCameraMode;

        private Dictionary<CameraType, CameraMode> cameraModeMap;

        private void Awake() {
            log = FindAnyObjectByType<ActionLogManager>();
            detailCameraMode = FindAnyObjectByType<DetailCameraMode>();
            splineCameraMode = FindAnyObjectByType<SplineCameraMode>();
            aerialCameraMode = FindAnyObjectByType<AerialCameraMode>();
            cameraModeMap = new Dictionary<CameraType, CameraMode> {
                { CameraType.DetailView, detailCameraMode },
                { CameraType.SplinePath, splineCameraMode },
                { CameraType.AerialView, aerialCameraMode }
            };
        }

        private void Start() {
            if (camType == CameraType.SplinePath) {
                splineCameraMode.Activate();
            }
        }

        /// <summary>
        /// Toggles between aerial view and spline path view. 
        /// </summary>
        public void ToggleAerialMode() {
            if (camType == CameraType.SplinePath) {
                ChangeToAerialView();
            } else if (camType == CameraType.AerialView) {
                ChangeToSplineView();
            }
        }

        /// <summary>
        /// Switches to aerial view mode. 
        /// </summary>
        public void ChangeToAerialView() {
            ChangeCameraMode(CameraType.AerialView);
            log.Write(LogMessages.ChangeCameraView("aerial view"));
        }

        /// <summary>
        /// Switches to detail view mode.
        /// </summary>
        /// <param name="detailObject">Object to focus on in detail view</param>
        /// <param name="silently">If <c>true</c>, the camera view change is not logged in the action log</param>
        public void ChangeToDetailView(GameObject detailObject, bool silently = false) {
            detailCameraMode.DetailObject = detailObject;
            ChangeCameraMode(CameraType.DetailView);
            if (!silently) {
                log.Write(LogMessages.ChangeCameraView("detail view"));
            }
        }

        /// <summary>
        /// Switches to spline path view.
        /// </summary>
        public void ChangeToSplineView() {
            ChangeCameraMode(CameraType.SplinePath);
            log.Write(LogMessages.ChangeCameraView("spline path"));
        }

        /// <summary>
        /// Handles the camera mode change by deactivating the current mode and activating the new mode.
        ///
        /// Also stores the current mode as the old mode before switching to the new mode.
        /// </summary>
        /// <param name="newType">New camera mode to switch to</param>
        private void ChangeCameraMode(CameraType newType) {
            cameraModeMap[camType]?.Deactivate(); // deactivate current mode
            cameraModeMap[newType].Activate(); // activate new mode
            oldCamType = camType; // store current mode as old mode
            camType = newType; // set new mode as current mode
            onCameraModeChanged.Invoke(newType);
        }

        public void OnToolbarBackClicked() {
            if (camType != CameraType.DetailView || !ChangeCameraOnBackButton) {
                return;
            }

            // change depending on the previous camera mode 
            if (oldCamType == CameraType.AerialView) {
                ChangeToAerialView();
            } else if (oldCamType == CameraType.SplinePath) {
                ChangeToSplineView();
            }
        }
    }
}