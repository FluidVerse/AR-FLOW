using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace AR {
    /// <summary>
    /// Uses AR image tracking to spawn an AR model when an image anchor is detected.
    ///
    /// Currently, the image anchor is one instance of an QR code, surrounded by black borders on a white background.
    /// After the QR code is detected by the image tracking system, the user is prompted to align the phone with the
    /// printed black borders. After manual user confirmation, the AR model is spawned on the QR code and aligned
    /// between the borders.
    ///
    /// The model to instantiate is provided by <see cref="ARSceneHandler"/>.
    /// </summary>
    public class ARAnchorAlignedSpawner : MonoBehaviour {

        /// <summary>
        /// Delay in seconds after which the AR model is spawned.
        ///
        /// Used to prevent the model from spawning immediately after the anchor image is detected and thus having
        /// a weird rotation.
        /// </summary>
        private const float spawnObjectDelay = 1f;

        /// <summary>
        /// If <c>true</c>, assumes that the anchor image lies on a flat surface when spawning the AR model.
        ///
        /// This simplifies the alignment process and produces better alignment results on flat surfaces by locking the
        /// x- and z-rotation of the model.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public bool AssumeFlatSurface { get; set; } = true;

        /// <summary>
        /// If <c>true</c>, the y-rotation of the model is calculated based on right vector of the camera.
        /// 
        /// Otherwise, the y-rotation is calculated based on the rotation of the anchor image.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public bool RotationBasedOnCamera { get; set; } = false;

        /// <summary>
        /// Returns <c>true</c> if an instantiated model exists and has a parent transform (i.e., is attached to the
        /// anchor image).
        /// </summary>
        public bool ModelParentExists => instantiatedModel != null && instantiatedModel.transform.parent != null;

        /// <summary>
        /// Public only for debug purposes.
        /// </summary>
        public ARTrackedImage AnchorImage => anchorImage;

        /// <summary>
        /// Public only for debug purposes.
        /// </summary>
        // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
        public GameObject InstantiatedModel => instantiatedModel;

        private new Camera camera;
        private ARSceneHandler sceneHandler;
        private ARTrackedImageManager trackedImageManager;

        private ARTrackedImage anchorImage;
        private GameObject instantiatedModel;

        private void Awake() {
            camera = Camera.main;
            if (camera == null) {
                Debug.LogError("Main camera not found", this);
            }

            trackedImageManager = FindAnyObjectByType<ARTrackedImageManager>();
            if (trackedImageManager == null) {
                Debug.LogError("ARTrackedImageManager not found", this);
            }
        }

        private void Start() {
            sceneHandler = ARSceneHandler.Instance;
            if (sceneHandler == null) {
                Debug.LogError("ARSceneHandler not found", this);
            }

            sceneHandler.OnNewModelChosen.AddListener(OnNewModelChosen);
            trackedImageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);

            ResetTracking();
        }

        private void OnDisable() {
            sceneHandler.OnNewModelChosen.RemoveListener(OnNewModelChosen);
            sceneHandler.State = ARState.Inactive;
            trackedImageManager.trackablesChanged.RemoveAllListeners();
        }

        private void Update() {
            if (AssumeFlatSurface) {
                FixateModelRotation();
            }
        }

        /// <summary>
        /// Spawns the chosen model (if it exists) on the anchor image.
        /// </summary>
        public void TrySpawnObject() {
            ARModelProperties chosenModel = sceneHandler.ActiveModel;
            if (chosenModel == null) {
                Debug.Log("Trying to spawn model, but no model chosen", this);
                return;
            }
            if (anchorImage == null) {
                Debug.Log("Trying to spawn model, but no anchor image found", this);
                return;
            }

            instantiatedModel = Instantiate(chosenModel.prefab, anchorImage.transform, true);
            ARModelAnchor modelAnchor = instantiatedModel.GetComponentInChildren<ARModelAnchor>();
            if (modelAnchor == null) {
                Debug.LogError("Instantiated AR model prefab does not contain an ARModelAnchor component", this);
                return;
            }

            Debug.Log($"Anchor image at position {anchorImage.transform.position} with " +
                      $"tracking state {anchorImage.trackingState}", this);
            Debug.Log($"Camera rotation is {camera.transform.eulerAngles}", this);

            instantiatedModel.transform.localScale = chosenModel.scale;
            Vector3 anchorPositionOffset = anchorImage.transform.position - modelAnchor.transform.position;
            instantiatedModel.transform.position += anchorPositionOffset + chosenModel.positionOffset;
            Vector3 newRotation = camera.transform.eulerAngles + chosenModel.rotationOffset;
            if (AssumeFlatSurface) {
                newRotation.x = 0;
                newRotation.z = 0;
            }

            // calculate y rotation
            if (RotationBasedOnCamera) {
                Vector3 cameraRight = camera.transform.right;
                cameraRight.y = 0; // project to horizontal plane
                float signedAngle = Vector3.SignedAngle(Vector3.right, cameraRight, Vector3.up);
                float angle = (signedAngle + 360) % 360;
                newRotation.y = angle + chosenModel.rotationOffset.y;
            } else {
                newRotation.y = anchorImage.transform.eulerAngles.y + chosenModel.rotationOffset.y;
            }
            instantiatedModel.transform.rotation = Quaternion.Euler(newRotation);

            Debug.Log($"Spawned AR model '{chosenModel.modelName}' at position " +
                      $"{instantiatedModel.transform.position} with rotation " +
                      $"{instantiatedModel.transform.eulerAngles}", this);
            Debug.Log($"Model anchor is at position {modelAnchor.transform.position}", this);

            sceneHandler.State = ARState.ModelPlaced;
        }

        /// <summary>
        /// Resets the AR image tracking by forgetting the tracked image and destroying the instantiated AR model.
        /// </summary>
        public void ResetTracking() {
            // reset ARTrackedImageManager to forget tracked images
            trackedImageManager.enabled = false;
            if (sceneHandler.ActiveModel == null) {
                return;
            }

            trackedImageManager.referenceLibrary = sceneHandler.ActiveModel.anchorImageLibrary;
            trackedImageManager.enabled = true;

            // reset our own variables
            anchorImage = null;
            Destroy(instantiatedModel);
            instantiatedModel = null;
            sceneHandler.State = ARState.SearchingReference;
        }

        /// <summary>
        /// Sets the anchor image as the parent of the instantiated AR model.
        /// </summary>
        /// <param name="imageAsParent">
        /// Whether to set the parent. If <c>false</c>, the model is not attached to the
        /// tracked image and thus does not follow its position.
        /// </param>
        /// <returns><c>true</c> on success, <c>false</c> if no anchor image or model exists.</returns>
        public bool SetAnchorAsParent(bool imageAsParent) {
            if (anchorImage == null || instantiatedModel == null) {
                return false;
            }

            instantiatedModel.transform.parent = imageAsParent ? anchorImage.gameObject.transform : null;
            return true;
        }

        /// <summary>
        /// Callback for <see cref="ARTrackedImageManager.trackablesChanged"/>.
        /// </summary>
        private void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args) {
            if (anchorImage != null) {
                return; // already have an anchor image
            }

            Debug.Log($"Tracked images changed (added: {args.added.Count}, updated: {args.updated.Count}, removed: " +
                      $"{args.removed.Count})", this);

            /*if (args.added.Count == 0 && args.removed.Count == 0) {
                return; // no interesting changes
            }*/

            FindAnchor();
        }

        /// <summary>
        /// Callback for <see cref="ARSceneHandler.OnNewModelChosen"/>.
        /// </summary>
        private void OnNewModelChosen(ARModelProperties model) {
            ResetTracking();
        }

        /// <summary>
        /// Tries to find a suitable image anchor from all <see cref="ARTrackedImageManager.trackables"/>.
        ///
        /// On success, the found anchor is stored in <see cref="anchorImage"/> and <see cref="ARSceneHandler.State"/>
        /// is progressed to <see cref="ARState.AligningBorders"/>.
        /// </summary>
        private void FindAnchor() {
            anchorImage = null;
            Debug.Log($"Finding anchors from {trackedImageManager.trackables.count} tracked images", this);
            foreach (ARTrackedImage trackedImage in trackedImageManager.trackables) {
                Debug.Log($"Image '{trackedImage.referenceImage.name}' with tracking state " +
                          $"{trackedImage.trackingState} at position {trackedImage.transform.position}", this);

                if (trackedImage.trackingState != TrackingState.None) {
                    anchorImage = trackedImage;
                }
            }

            if (anchorImage != null) {
                // skip AligningBorders phase because RotationBasedOnCamera = false, spawn model immediately 
                // sceneHandler.State = ARState.AligningBorders;
                StartCoroutine(TrySpawnObjectCoroutine());
            } else {
                Debug.Log("No suitable anchor image found", this);
            }
        }

        /// <summary>
        /// Fixates the rotation of the instantiated model by setting the x- and z-rotation to zero.
        /// </summary>
        private void FixateModelRotation() {
            if (instantiatedModel == null) {
                return;
            }

            Vector3 rotation = instantiatedModel.transform.eulerAngles;
            rotation.x = 0;
            rotation.z = 0;
            instantiatedModel.transform.eulerAngles = rotation;
        }

        private IEnumerator TrySpawnObjectCoroutine() {
            yield return new WaitForSeconds(spawnObjectDelay);
            TrySpawnObject();
        }
    }
}