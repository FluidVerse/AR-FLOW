using System.Collections.Generic;
using UnityEngine;

namespace Models {
    /// <summary>
    /// Animation script for the Etanorm Kreiselpumpe model prefab in
    /// <c>Assets/Modelle/Kreiselpumpe/Etanorm/Etanorm 80-65-200.prefab</c>.
    /// </summary>
    public class EtanormAnimator : ModelAnimator {

        // all objects that rotate around their own x-axis like Laufrad, Welle etc.
        [SerializeField] private GameObject[] xAxisRotationObjects;

        [SerializeField] private GameObject passfeder;

        // object that represents the axis of rotation of Passfeder (in this case: Welle)
        [SerializeField] private GameObject passfederRotationAxis;

        private MeshRenderer[] xAxisRotationObjectRenderers;
        private MeshRenderer passfederRotationAxisRenderer;

        protected override GameObject[] AllAnimatedObjects {
            get {
                // all objects in xAxisRotationObjects + Passfeder in addition
                List<GameObject> list = new List<GameObject>(xAxisRotationObjects) { passfeder };
                return list.ToArray();
            }
        }

        protected override void InitializeModel() {
            // assign MeshRenderers of all objects 
            xAxisRotationObjectRenderers = new MeshRenderer[xAxisRotationObjects.Length];
            for (int i = 0; i < xAxisRotationObjects.Length; i++) {
                xAxisRotationObjectRenderers[i] = xAxisRotationObjects[i].GetComponent<MeshRenderer>();
                if (xAxisRotationObjectRenderers[i] == null) {
                    Debug.LogError($"MeshRenderer of {xAxisRotationObjects[i].name} not found");
                }
            }
            
            // assign passfederRotationPoint to the center of Welle
            passfederRotationAxisRenderer = passfederRotationAxis.GetComponent<MeshRenderer>();
            if (passfederRotationAxisRenderer == null) {
                Debug.LogError("MeshRenderer of PassfederRotationAxis not found");
            }
        }

        protected override void Animate() {
            for (int i = 0; i < xAxisRotationObjects.Length; i++) {
                xAxisRotationObjects[i].transform.RotateAround(xAxisRotationObjectRenderers[i].bounds.center,
                    xAxisRotationObjects[i].transform.right, AnimationSpeed * Time.deltaTime);
            }
            // rotate Passfeder around the x-axis of Welle
            Vector3 passfederRotationPoint = passfederRotationAxisRenderer.bounds.center;
            passfeder.transform.RotateAround(passfederRotationPoint, passfederRotationAxis.transform.right,
                AnimationSpeed * Time.deltaTime);
        }
    }
}