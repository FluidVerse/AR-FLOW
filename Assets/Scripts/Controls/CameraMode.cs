using UnityEngine;

namespace Controls {
    /// <summary>
    /// Base class for different camera modes. Each mode can have its own logic for updating the camera's position and
    /// rotation based on player input and game state.
    /// </summary>
    public abstract class CameraMode : MonoBehaviour {

        /// <summary>
        /// Whether this camera mode is currently active.
        ///
        /// Only the active camera mode calls <see cref="OnCameraUpdate"/> each frame.
        ///
        /// In theory, this should only be set internally using <see cref="Activate"/> and <see cref="Deactivate"/>.
        /// However, it is public to allow for the hacky camera implementation of surface plot (PotentialFlowLevel).
        /// </summary>
        public bool IsActive { get; set; }

        [SerializeField] protected new Camera camera;

        [SerializeField] protected CameraParameters cameraParams;

        [SerializeField] protected JoystickWrapper joystick;

        /// <summary>
        /// Called every frame when this camera mode is active.
        /// </summary>
        protected abstract void OnCameraUpdate();

        /// <summary>
        /// Called when this camera mode is activated.
        /// </summary>
        protected abstract void OnActivate();

        /// <summary>
        /// Called when this camera mode is deactivated.
        /// </summary>
        protected abstract void OnDeactivate();

        /// <summary>
        /// Activates this camera mode.
        /// </summary>
        public void Activate() {
            OnActivate();
            IsActive = true;
        }

        /// <summary>
        /// Deactivates this camera mode.
        /// </summary>
        public void Deactivate() {
            OnDeactivate();
            IsActive = false;
        }

        private void Update() {
            if (IsActive) {
                OnCameraUpdate();
            }
        }
    }
}