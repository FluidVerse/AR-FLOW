using UnityEngine;
using Utils;

#pragma warning disable CS0162 // Unreachable code detected
// ReSharper disable HeuristicUnreachableCode

namespace Controls {
    /// <summary>
    /// A camera mode that allows the player to fly around the scene.
    ///
    /// The camera is locked to a certain height and can only move horizontally.
    /// </summary>
    public class AerialCameraMode : CameraMode {

        /// <summary>
        /// Movement speed of the camera in aerial mode.
        ///
        /// This is later multiplied by the joystick input and the global sensitivity.
        /// </summary>
        private const float movementSpeed = 30f;

        /// <summary>
        /// Offset how far the aerial camera should spawn behind the dummy character model (behind = z-axis). 
        /// </summary>
        private const float characterModelOffset = 2f;

        /// <summary>
        /// If <c>true</c>, the camera position is saved when leaving aerial mode and restored when re-entering it.
        ///
        /// Otherwise, the camera always spawns above and behind the dummy character model.
        /// </summary>
        private const bool restoreOldCameraPosition = false;

        [SerializeField] private DummyCharacterController characterController;

        /// <summary>
        /// Constant camera rotation for the aerial camera mode.
        ///
        /// The camera is tilted downwards to give a better view of the scene, i.e. not fully looking down 90 degrees.
        /// </summary>
        [SerializeField] private Vector3 cameraRotation = new(40f, 0, 0);

        /// <summary>
        /// Bounds within which the camera can move.
        ///
        /// Use the y coordinate of the bounds center to set the height of the camera. The camera is then locked to
        /// this height, so the y extent should stay unchanged. The x and z extents can be adjusted per scene to limit
        /// the camera's horizontal movement.
        /// </summary>
        [SerializeField] private Bounds bounds = new(Vector3.zero, new Vector3(20, 2e7f, 20));

        private Vector3? oldCameraPosition;

        protected override void OnCameraUpdate() {
            if (!cameraParams.MouseDown || joystick.IsNeutral) {
                return;
            }

            // move camera based on joystick input, but clamp it to the bounds
            Vector3 dt = new(joystick.Horizontal, 0, joystick.Vertical);
            float dtFactor = GlobalSettings.Instance.sensitivity * movementSpeed * Time.deltaTime;
            Vector3 newPosition = camera.transform.position + dt * dtFactor;
            camera.transform.position = bounds.ClosestPoint(newPosition);
        }

        protected override void OnActivate() {
            // assume that the player was in spline mode before, so that spline pos/rot in cameraParams is already set
            SetCameraPosition();
            joystick.SetAxisMode(JoystickWrapper.Axis.Both);
            characterController.ShowModel(cameraParams.CurrentSplinePosition, cameraParams.CurrentSplineRotation);
        }

        protected override void OnDeactivate() {
            oldCameraPosition = camera.transform.position;
            characterController.HideModel();
        }

        private void SetCameraPosition() {
            if (restoreOldCameraPosition && oldCameraPosition.HasValue) {
                camera.transform.position = oldCameraPosition.Value;
                camera.transform.eulerAngles = cameraRotation;
                return;
            }

            Vector3 cameraPos = cameraParams.CurrentSplinePosition;
            cameraPos += Vector3.back * characterModelOffset; // move back from the model
            cameraPos.y = bounds.center.y; // lock height

            camera.transform.position = bounds.ClosestPoint(cameraPos); // bound just be 100% safe 
            camera.transform.eulerAngles = cameraRotation;
        }
    }
}