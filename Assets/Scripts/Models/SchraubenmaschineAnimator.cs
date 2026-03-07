using UnityEngine;

namespace Models {
    /// <summary>
    /// Animation script for the Schraubenmaschine model prefabs in
    /// <c>Assets/Modelle/Schraubenmaschine/Schraubenmaschine.prefab</c> and
    /// <c>Assets/Modelle/Cycloid-Schraubenmaschine/Cycloid-Schraubenmaschine.prefab</c>.
    /// </summary>
    public class SchraubenmaschineAnimator : ModelAnimator {

        [SerializeField] private GameObject hauptRotor, nebenRotor, gehaeuse, axisAnchor;
        [SerializeField] private float axisDistance;
        [SerializeField] private float nebenRotorFactor;

        private MeshRenderer hauptRotorRenderer, nebenRotorRenderer, gehaeuseRenderer;

        protected override GameObject[] AllAnimatedObjects => new[] { hauptRotor, nebenRotor };

        protected override void InitializeModel() {
            hauptRotorRenderer = hauptRotor.GetComponent<MeshRenderer>();
            nebenRotorRenderer = nebenRotor.GetComponent<MeshRenderer>();
            gehaeuseRenderer = gehaeuse.GetComponent<MeshRenderer>();
            if (hauptRotorRenderer == null) {
                Debug.LogError("MeshRenderer of HauptRotor not found");
            }
            if (nebenRotorRenderer == null) {
                Debug.LogError("MeshRenderer of NebenRotor not found");
            }
            if (gehaeuseRenderer == null) {
                Debug.LogError("MeshRenderer of Gehaeuse not found");
            }

            // save original rotation to apply positioning with identity rotation, reapply afterwards
            Quaternion parentRotation = transform.rotation;
            transform.rotation = Quaternion.identity;
            
            Vector3 hauptRotorPos = hauptRotor.transform.localPosition;
            Vector3 nebenRotorPos = nebenRotor.transform.localPosition;

            // put both rotors on same y position (based on mesh bounds)
            nebenRotorPos.y += hauptRotorRenderer.bounds.center.y - nebenRotorRenderer.bounds.center.y;

            // align the main rotor with the case on the z-axis and place the secondary rotor on the same z position
            if (Physics.Raycast(hauptRotorPos, hauptRotor.transform.forward, out RaycastHit hit,
                    hauptRotorRenderer.bounds.extents.z + 1f) && hit.collider.gameObject == gehaeuse) {
                hauptRotorPos.z += hit.distance - hauptRotorRenderer.bounds.extents.z;
            }
            if (Physics.Raycast(hauptRotorPos, -hauptRotor.transform.forward, out RaycastHit hit2,
                    hauptRotorRenderer.bounds.extents.z + 1f) && hit.collider.gameObject == gehaeuse) {
                hauptRotorPos.z -= hit2.distance - hauptRotorRenderer.bounds.extents.z;
            }
            nebenRotorPos.z = hauptRotorPos.z;

            // place rotors exactly in the center on the x-axis
            Vector3 caseCenter = gehaeuseRenderer.bounds.center;
            hauptRotorPos.x = caseCenter.x - (hauptRotorRenderer.bounds.center.x - hauptRotorPos.x);
            nebenRotorPos.x = caseCenter.x - (nebenRotorRenderer.bounds.center.x - nebenRotorPos.x);

            // apply axis distance
            hauptRotorPos.x += axisDistance * 0.5f;
            nebenRotorPos.x -= axisDistance * 0.5f;

            // place rotors on the same height as the case
            hauptRotorPos.y += axisAnchor.transform.position.y - hauptRotorRenderer.bounds.center.y;
            nebenRotorPos.y += axisAnchor.transform.position.y - nebenRotorRenderer.bounds.center.y;

            // positioning done, apply to transform
            hauptRotor.transform.localPosition = hauptRotorPos;
            nebenRotor.transform.localPosition = nebenRotorPos;
            transform.rotation = parentRotation;
        }

        protected override void Animate() {
            hauptRotor.transform.RotateAround(hauptRotorRenderer.bounds.center, hauptRotor.transform.forward,
                AnimationSpeed * Time.deltaTime);
            nebenRotor.transform.RotateAround(nebenRotorRenderer.bounds.center, nebenRotor.transform.forward,
                -AnimationSpeed * nebenRotorFactor * Time.deltaTime);
        }
    }
}