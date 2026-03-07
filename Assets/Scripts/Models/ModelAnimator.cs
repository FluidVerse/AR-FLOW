using System;
using UnityEngine;

namespace Models {
    /// <summary>
    /// Base class for model animations that can be accessed in a uniform way using the AR Flow in-game UI.
    /// </summary>
    public abstract class ModelAnimator : MonoBehaviour {

        /// <summary>
        /// Speed of the implemented animation.
        ///
        /// The exact meaning of this value (e.g. °/s, m/s, etc.) depends on the implementation of the derived class.
        /// </summary>
        public float AnimationSpeed { get; set; } = 20f;

        /// <summary>
        /// Enables or disables the animation.
        /// </summary>
        public bool AnimationEnabled { get; set; }

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

        private void Awake() {
            InitializeModel();
            objectBasePositions = Array.ConvertAll(AllAnimatedObjects, obj => obj.transform.localPosition);
            objectBaseRotations = Array.ConvertAll(AllAnimatedObjects, obj => obj.transform.localRotation);
        }

        private void Update() {
            if (AnimationEnabled) {
                Animate();
            }
        }

        /// <summary>
        /// Initializes the model and its components.
        ///
        /// Is called once in the <see cref="Awake"/> method.
        /// </summary>
        protected abstract void InitializeModel();

        /// <summary>
        /// Resets the animation to its initial state.
        ///
        /// Is called upon user interaction with the according UI element.
        /// </summary>
        public virtual void ResetAnimation() {
            for (int i = 0; i < AllAnimatedObjects.Length; i++) {
                AllAnimatedObjects[i].transform.localPosition = objectBasePositions[i];
                AllAnimatedObjects[i].transform.localRotation = objectBaseRotations[i];
            }
        }

        /// <summary>
        /// Animates the model.
        ///
        /// Is called once per frame in <see cref="Update"/> if <see cref="AnimationEnabled"/> is <c>true</c>.
        /// </summary>
        protected abstract void Animate();
    }
}