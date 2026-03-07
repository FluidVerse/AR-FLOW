using UnityEngine;

namespace Utils {
    /// <summary>
    /// Helper script for the <c>Pipemaster_Global</c> prefab to ensure that all children are persistent across scenes.
    ///
    /// This way, if the parent object is persistent, all children will also stay grouped under the same parent across
    /// scenes. Thus, its children do not need to call <c>DontDestroyOnLoad</c> individually.
    /// </summary>
    public class GlobalSingletonParent : MonoBehaviour {

        private static GlobalSingletonParent Instance { get; set; }

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            transform.parent = null; // DontDestroyOnLoad only works on root objects
            DontDestroyOnLoad(gameObject);
        }
    }
}