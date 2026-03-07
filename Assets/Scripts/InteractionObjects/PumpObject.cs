using InteractionMenus.MenuElements;
using UnityEngine;

namespace InteractionObjects {
    public class PumpObject : InteractionObject {

        public int speed = 0;

        void Start() {
            AddMenuElement(new Button("Detail View", OpenDetailView));
            //AddMenuElement(new Button("Pumpe starten", () => StartPump()));
            //AddMenuElement(new Button("Pumpe stoppen", () => StopPump()));
            //AddMenuElement(new Slider("Drehzahl", SetSpeedPump, speed));
        }
        
        void StartPump() {
            //Drehzahl = 
            Debug.Log("Pumpe wird gestartet!");
        }

        void StopPump() {
            Debug.Log("Pumpe wird gestoppt!");
        }

        void SetSpeedPump(int pSpeed) {
            speed = pSpeed;
            Debug.Log("Drehzahl = " + speed.ToString());
        }
    }
}