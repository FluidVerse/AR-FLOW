using UnityEngine;

namespace Models {
    /// <summary>
    /// Animation script for the valve object in
    /// <c>Assets/Modelle/Rohre/EKugelhahn DN80 separat.blend</c>.
    ///
    /// Animates the handwheel to imitate a turning motion when setting the valve position.
    /// </summary>
    public class ValveAnimator : SingleMotionAnimator {

        /// <summary>
        /// Hand wheel object to animate.
        /// </summary>
        [SerializeField] private GameObject handWheel;

        /// <summary>
        /// Angle in degrees the hand wheel turns during the animation.
        ///
        /// Recommended to be a multiple of 90° so that the animation reset is basically invisible.
        /// </summary>
        [SerializeField] private float turnAngle = 90f;

        protected override GameObject[] AllAnimatedObjects => new[] { handWheel };

        private MeshRenderer meshRenderer;

        protected override void InitializeModel() {
            meshRenderer = handWheel.GetComponent<MeshRenderer>();
            if (meshRenderer == null) {
                Debug.LogError("MeshRenderer not found", this);
            }
        }

        protected override void Animate() { 
            Vector3 centerPoint = meshRenderer.bounds.center;
            float angle = turnAngle / duration * Time.deltaTime;
            handWheel.transform.RotateAround(centerPoint, Vector3.up, angle);

            if (transform.localEulerAngles.z >= turnAngle) {
                // quick fix so that the last animation frame looks less janky
                ResetAnimation();
                handWheel.transform.RotateAround(centerPoint, Vector3.up, turnAngle);
            }
        }
    }
}