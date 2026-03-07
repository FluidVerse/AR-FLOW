using UnityEngine;

namespace AR {
    /// <summary>
    /// Controller that analyzes the user behavior in the AR scene and provides hints to improve the AR experience.
    /// </summary>
    public class ARHintsController : MonoBehaviour {

        /// <summary>
        /// Threshold for a single position delta within one frame to consider the AR model position unstable.
        /// </summary>
        private float singlePositionDeltaThreshold = 2f;

        /// <summary>
        /// Threshold for the accumulated position delta to consider the AR model position unstable.
        /// </summary>
        private float accumulatedPositionDeltaThreshold = 5f;

        private ARSceneHandler sceneHandler;
        private ARAnchorAlignedSpawner spawner;

        private Vector3? lastModelPosition;
        private float accumulatedPositionDelta;

        private void Awake() {
            spawner = FindAnyObjectByType<ARAnchorAlignedSpawner>();
            if (spawner == null) {
                Debug.LogError("ARAnchorAlignedSpawner not found", this);
            }
        }

        private void Start() {
            sceneHandler = ARSceneHandler.Instance;
            if (sceneHandler == null) {
                Debug.LogError("ARSceneHandler not found", this);
            }
        }

        private void Update() {
            TrackModelStability();
        }

        /// <summary>
        /// Tracks the stability of the AR model position and triggers an <see cref="AREvent"/> if it appears unstable.
        /// </summary>
        private void TrackModelStability() {
            if (spawner.InstantiatedModel == null) {
                lastModelPosition = null;
                accumulatedPositionDelta = 0f;
                return;
            }
            if (lastModelPosition == null) {
                lastModelPosition = spawner.InstantiatedModel.transform.position;
                return;
            }

            float positionDelta =
                Vector3.Distance(spawner.InstantiatedModel.transform.position, lastModelPosition.Value);
            accumulatedPositionDelta += positionDelta;
            if (positionDelta > singlePositionDeltaThreshold ||
                accumulatedPositionDelta > accumulatedPositionDeltaThreshold) {
                sceneHandler.PublishEvent(this, AREvent.PositionUnstable);
                accumulatedPositionDelta = 0f;

                // increase thresholds to avoid spamming the hint
                singlePositionDeltaThreshold = float.MaxValue;  // disable for the rest of the session
                accumulatedPositionDeltaThreshold = float.MaxValue;
            }

            lastModelPosition = spawner.InstantiatedModel.transform.position;
        }
    }
}