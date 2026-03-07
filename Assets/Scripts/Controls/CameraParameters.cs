using UnityEngine;
using UnityEngine.InputSystem;

namespace Controls {
    /// <summary>
    /// Holds parameters related to camera control that can be shared across different camera modes. 
    /// </summary>
    public class CameraParameters : MonoBehaviour {

        /// <summary>
        /// Whether the mouse is currently clicked. 
        /// </summary>
        public bool MouseDown => clickAction.ReadValue<float>() > 0f;

        /// <summary>
        /// If <c>true</c>, camera movement is allowed in detail view.
        /// </summary>
        public bool IsCameraMovementAllowed { get; set; } = true;
        
        /// <summary>
        /// Current position of the camera in spline mode.
        ///
        /// Set by <see cref="SplineCameraMode"/> and then used by <see cref="AerialCameraMode"/> to let it position
        /// a human character dummy there while in aerial mode.
        /// </summary>
        public Vector3 CurrentSplinePosition { get; set; }
        
        /// <summary>
        /// Current rotation of the camera in spline mode, see <see cref="CurrentSplinePosition"/>.
        /// </summary>
        public Quaternion CurrentSplineRotation { get; set; }

        private InputAction pointAction, clickAction;

        private void Awake() {
            pointAction = InputSystem.actions.FindAction("UI/Point", true);
            clickAction = InputSystem.actions.FindAction("UI/Click", true);
        }
    }
}