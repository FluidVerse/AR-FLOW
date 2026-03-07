using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;

namespace AR {
    /// <summary>
    /// Sends an AR raycast upon a click action and instantiates a prefab at the hit point.
    ///
    /// The prefab to instantiate is provided by <see cref="ARSceneHandler"/>.
    /// </summary>
    public class ARRaycastSpawner : MonoBehaviour {

        private ARSceneHandler sceneHandler;
        private ARRaycastManager raycastManager;
        private Vector2 pointPosition;

        private void Awake() {
            sceneHandler = ARSceneHandler.Instance;
            raycastManager = FindAnyObjectByType<ARRaycastManager>();

            if (sceneHandler == null) {
                Debug.LogError("ARSceneHandler not found");
            }
            if (raycastManager == null) {
                Debug.LogError("ARRaycastManager not found");
            }
        }

        public void onPoint(InputAction.CallbackContext context) {
            pointPosition = context.ReadValue<Vector2>();
        }

        public void onClick(InputAction.CallbackContext context) {
            if (raycastManager == null || context.ReadValueAsButton()) {
                return; // only handle the click when the button is released
            }
            Raycast();
        }

        private void Raycast() {
            List<ARRaycastHit> hits = new();
            bool raycastHit = raycastManager.Raycast(pointPosition, hits);
            if (!raycastHit) {
                return;
            }

            ARModelProperties chosenModel = sceneHandler.ActiveModel;
            if (chosenModel == null) {
                Debug.LogError("Trying to raycast, but no model chosen");
                return;
            }

            ARRaycastHit hit = hits[0];
            GameObject obj = Instantiate(chosenModel.prefab, hit.pose.position, Quaternion.identity);
            obj.transform.localScale = chosenModel.scale * hit.distance;
        }
    }
}