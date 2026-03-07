using UnityEngine;

namespace Controls {
    /// <summary>
    /// Controls the dummy character model that is shown in aerial camera mode.
    /// </summary>
    public class DummyCharacterController : MonoBehaviour {

        [SerializeField] private GameObject model;
        [SerializeField] private GameObject head;
        [SerializeField] private GameObject visors;
        [SerializeField] private float modelYPosition;  // = 0 by default

        private void Awake() {
            model.SetActive(false);
            visors.transform.parent = head.transform;   // make visors a child of the head so they rotate together
        }

        /// <summary>
        /// Shows the dummy character model at the specified position and rotation.
        ///
        /// The y position is set to a fixed value to keep the model on the ground.
        /// </summary>
        /// <param name="position">New position of the model</param>
        /// <param name="rotation">New rotation of the model</param>
        public void ShowModel(Vector3 position, Quaternion rotation) {
            model.SetActive(true);
            position.y = modelYPosition;
            model.transform.position = position;
            model.transform.rotation = rotation;
        }

        /// <summary>
        /// Hides the dummy character model.
        /// </summary>
        public void HideModel() {
            model.SetActive(false);
        }
    }
}