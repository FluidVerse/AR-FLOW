using System;
using UnityEngine;

namespace Models {
    /// <summary>
    /// Base class for a "one-time" (not continuous), i.e. single motion animation.
    ///
    /// After the animation <see cref="duration"/> has passed, the default implementation of
    /// <see cref="ResetAnimation"/> resets all objects in <see cref="AllAnimatedObjects"/> to their initial position
    /// and rotation. Therefore, the animation should ideally end in a state that is visually indistinguishable from
    /// the initial state.
    /// </summary>
    public abstract class SingleMotionAnimator : MonoBehaviour {

        /// <summary>
        /// Duration of the animation in seconds.
        /// </summary>
        [SerializeField] protected float duration = 1f;

        /// <summary>
        /// If <c>true</c>, the animation will be reset to its initial state when <see cref="AnimateOnce"/> is called
        /// while the animation is already playing, i.e. while <see cref="isAnimating"/> is <c>true</c>.
        ///
        /// If <c>false</c>, the animation will not be interrupted and the second <see cref="AnimateOnce"/> call will
        /// be ignored.
        /// </summary>
        [SerializeField] private bool resetOnInterrupt = true;

        /// <summary>
        /// List all animated objects in the model.
        ///
        /// Can be implemented to make use of the default implementation of <see cref="ResetAnimation"/> which resets
        /// all objects in this list to their initial position and rotation. If this property is not implemented, it is
        /// expected to provide a custom implementation of <see cref="ResetAnimation"/> in the derived class.
        /// </summary>
        protected virtual GameObject[] AllAnimatedObjects { get; } = Array.Empty<GameObject>();

        // base positions and rotations for the default implementation of ResetAnimation
        private Vector3[] objectBasePositions;
        private Quaternion[] objectBaseRotations;

        private bool isAnimating;
        private float elapsedTime;

        private void Awake() {
            InitializeModel();
            objectBasePositions = Array.ConvertAll(AllAnimatedObjects, obj => obj.transform.localPosition);
            objectBaseRotations = Array.ConvertAll(AllAnimatedObjects, obj => obj.transform.localRotation);
        }

        private void Update() {
            if (!isAnimating) {
                return;
            }
            if (elapsedTime >= duration) {
                ResetAll();
                return;
            }

            Animate();
            elapsedTime += Time.deltaTime;
        }

        /// <summary>
        /// Plays the animation once.
        /// </summary>
        public void AnimateOnce() {
            if (isAnimating) {
                if (resetOnInterrupt) {
                    return;
                }
                ResetAll();
            }
            isAnimating = true;
        }

        /// <summary>
        /// Initializes the model and its components.
        ///
        /// Is called once in the <see cref="Awake"/> method.
        /// </summary>
        protected abstract void InitializeModel();

        /// <summary>
        /// Animates the model.
        ///
        /// Is called once per frame in <see cref="Update"/> for the time specified in <see cref="duration"/> after
        /// <see cref="AnimateOnce"/> has been called.
        /// </summary>
        protected abstract void Animate();

        /// <summary>
        /// Resets the animation to its initial state.
        /// </summary>
        protected virtual void ResetAnimation() {
            for (int i = 0; i < AllAnimatedObjects.Length; i++) {
                AllAnimatedObjects[i].transform.localPosition = objectBasePositions[i];
                AllAnimatedObjects[i].transform.localRotation = objectBaseRotations[i];
            }
        }

        /// <summary>
        /// Resets the animation and all related state variables.
        /// </summary>
        private void ResetAll() {
            ResetAnimation();
            isAnimating = false;
            elapsedTime = 0;
        }
    }
}