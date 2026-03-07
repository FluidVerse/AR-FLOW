using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;

namespace AR {
    /// <summary>
    /// Keeps track of all AR planes in the scene that get created by the Unity <see cref="ARPlaneManager"/>.
    /// </summary>
    public class ARPlaneTracker : MonoBehaviour {

        /// <summary>
        /// Callback when the list of tracked planes changes.
        /// </summary>
        [SerializeField] private UnityEvent<List<ARPlane>> onListChanged;
        
        private readonly List<ARPlane> trackedPlanes = new();
        private ARPlaneManager planeManager;

        private void Awake() {
            planeManager = FindAnyObjectByType<ARPlaneManager>();
            if (planeManager == null) {
                Debug.LogError("ARPlaneManager not found");
            }

            planeManager.trackablesChanged.AddListener(OnTrackablesChanged);
        }

        private void OnTrackablesChanged(ARTrackablesChangedEventArgs<ARPlane> changes) {
            trackedPlanes.AddRange(changes.added);
            foreach (var plane in changes.removed) {
                trackedPlanes.Remove(plane.Value);
            }

            trackedPlanes.RemoveAll(plane => plane == null);

            if (changes.added.Count > 0 || changes.removed.Count > 0) {
                onListChanged.Invoke(trackedPlanes);
            }
        }
    }
}