using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;
using Utils;

namespace Controls {
    /// <summary>
    /// Camera mode that moves the camera along a spline path.
    ///
    /// The camera's position and view direction are determined by the spline's position and tangent at a given
    /// parameter value. 
    /// </summary>
    public class SplineCameraMode : CameraMode {

        /// <summary>
        /// Callback when the joystick state, i.e. whether it is pressed or not, changes.
        ///
        /// If the joystick is pressed, then <c>true</c> is passed once, otherwise <c>false</c> is passed once when the
        /// joystick is released.
        /// </summary>
        public UnityEvent<bool> onJoystickPressed = new();

        [SerializeField] private SplineContainer cameraPath;
        [SerializeField] private float position = 0.8f;

        public bool IsJoystickPressed {
            set => onJoystickPressed.Invoke(value);
        }

        private Quaternion oldCameraRotation;

        protected override void OnCameraUpdate() {
            position = Mathf.Clamp(position, 0f, 1f);
            float3 cameraPosition = cameraPath.EvaluatePosition(position);
            float3 cameraView = cameraPath.EvaluateTangent(position);

            Vector3 upVector = new Vector3(0, 1f, 0);
            Vector3 view = Vector3.Cross(cameraView, upVector);

            camera.transform.position = cameraPosition;
            camera.transform.forward = view;

            if (cameraParams.MouseDown && Mathf.Abs(joystick.Horizontal) > 0f) {
                float dt = joystick.Horizontal;
                position += dt * GlobalSettings.Instance.sensitivity * Time.deltaTime;
                IsJoystickPressed = true;
            } else {
                IsJoystickPressed = false;
            }
        }

        protected override void OnActivate() {
            camera.transform.rotation = oldCameraRotation;
            joystick.SetAxisMode(JoystickWrapper.Axis.Horizontal);
        }

        protected override void OnDeactivate() {
            oldCameraRotation = camera.transform.rotation;
            cameraParams.CurrentSplinePosition = camera.transform.position;
            cameraParams.CurrentSplineRotation = camera.transform.rotation;
        }
    }
}