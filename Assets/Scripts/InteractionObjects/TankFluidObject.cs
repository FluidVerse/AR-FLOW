using InteractionMenus.MenuElements;
using UnityEngine;

namespace InteractionObjects {
    public class TankFluidObject : InteractionObject {

        public float diameter = 0.08f;
        public float V = 1000.0f;

        public float dVdt = 0f;
        public float Vmax = 1000.0f;

        private GameObject fluidObject;

        void Start() {
            AddMenuElement(new Button("Detailansicht", OpenDetailView));
            AddMenuElement(new ParameterLabel<float>("Volumen [l]", V));
            AddMenuElement(new Button("Öl ablassen", () => Outflow()));

            fluidObject = gameObject.transform.Find("Fluid").gameObject;
        }

        void Update() {
            if (V > 0f) {
                V += dVdt * Time.deltaTime;
                Vector3 fscale = fluidObject.transform.localScale;
                fscale.z = V / Vmax * 100f;
                fluidObject.transform.localScale = fscale;
            }
        }

        void Outflow() {
            dVdt = -10;
            Debug.Log("Öl wird abgelassen!");
        }
    }
}