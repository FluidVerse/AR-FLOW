using UnityEngine;
using UnityEngine.XR.ARSubsystems;

namespace AR {
    /// <summary>
    /// Information about an AR model that can be spawned in the AR scene.
    ///
    /// Each AR model in this game needs to have an instance of this class in the <c>Resources/ARModels</c> directory.
    /// </summary>
    [CreateAssetMenu(fileName = "ARModelProperties", menuName = "AR Flow/ARModelProperties")]
    public class ARModelProperties : ScriptableObject {

        /// <summary>
        /// Unique key to identify the model, used e.g. in deep links.
        /// </summary>
        public string key;

        /// <summary>
        /// Name of the model to display in UI.
        /// </summary>
        public string modelName;

        /// <summary>
        /// Prefab of the model.
        /// </summary>
        public GameObject prefab;

        /// <summary>
        /// Reference image library that contains the anchor image(s) for this model.
        /// </summary>
        public XRReferenceImageLibrary anchorImageLibrary;

        /// <summary>
        /// Position offset of the instantiated prefab.
        /// </summary>
        public Vector3 positionOffset;

        /// <summary>
        /// Rotation offset of the instantiated prefab.
        /// </summary>
        public Vector3 rotationOffset;

        /// <summary>
        /// Scale of the instantiated prefab.
        /// </summary>
        public Vector3 scale;
    }
}