using UnityEngine;

namespace BCI {
    public class VectorFieldVisualizer : MonoBehaviour
    {
        public VectorFieldManager vectorFieldManager; // Referenz zum Manager
        public int stepSize = 2; // Abstand der Pfeile
        public float scale = 2.0f; // Länge der Pfeile

        void OnDrawGizmos()
        {
            if (vectorFieldManager == null)
            {
                Debug.LogError("FEHLER: `vectorFieldManager` ist nicht gesetzt!");
                return;
            }

            for (int q = 0; q < 4; q++)
            {
                GameObject forceFieldObject = vectorFieldManager.forceFieldObjects[q];
                if (forceFieldObject == null) continue; // Falls kein Force Field existiert, weiter

                // Debug-Ausgabe
                Debug.Log($"Zeichne Force Field für Quadrant {q + 1}");

                // Zeichne einen Würfel zur Visualisierung des Force Fields
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(GetQuadrantPosition(q), Vector3.one * 4);
            }
        }

        private Vector3 GetQuadrantPosition(int index)
        {
            float offset = 390f; // Falls dein Kreuz größer ist, anpassen!

            switch (index)
            {
                case 0: return new Vector3(-offset, 0, -offset); // Q1 (unten links)
                case 1: return new Vector3(offset, 0, -offset);  // Q2 (unten rechts)
                case 2: return new Vector3(-offset, 0, offset);  // Q3 (oben links)
                case 3: return new Vector3(offset, 0, offset);   // Q4 (oben rechts)
                default: return Vector3.zero;
            }
        }
    }
}
