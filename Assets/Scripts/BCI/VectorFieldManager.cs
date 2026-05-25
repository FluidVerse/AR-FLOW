using UnityEngine;

namespace BCI {
    public class VectorFieldManager : MonoBehaviour
    {
        public GateState[] gates; // Die vier Tore
        public GameObject[] forceFieldObjects = new GameObject[4]; // Vier Quadranten als Force Fields
        private string lastState = "0000"; // Speichert den letzten bekannten Zustand

        void Update()
        {
            string currentState = GetGateState();

            if (currentState != lastState) // Nur wenn sich der Zustand ändert, aktualisieren
            {
                lastState = currentState;
                LoadForceFields();
            }
        }

        public void LoadForceFields()
        {
            string state = GetGateState();
            Debug.Log($"Lade Force Fields für Zustand: {state}");

            if (state == "0000")
            {
                Debug.Log("Alle Tore geschlossen - Force Fields werden deaktiviert.");
                ClearForceFields();
                return;
            }

            bool foundField = false;
            for (int i = 0; i < 4; i++)
            {
                string filePath = $"Vektorfelder/{state}_3D_{i + 1}";
                Debug.Log($"Prüfe Datei: {filePath}");

                GameObject forceFieldPrefab = Resources.Load<GameObject>(filePath);
                if (forceFieldPrefab == null)
                {
                    Debug.LogError($"FEHLER: Konnte Datei nicht laden: {filePath}");
                    continue;
                }

                Debug.Log($"Erstelle Force Field für Quadrant {i + 1}");
                if (forceFieldObjects[i] != null)
                {
                    Destroy(forceFieldObjects[i]);
                }

                forceFieldObjects[i] = Instantiate(forceFieldPrefab, GetQuadrantPosition(i), Quaternion.identity);
                foundField = true;
            }

            if (!foundField)
            {
                Debug.LogWarning($"Kein gültiges Force Field gefunden für Zustand: {state}");
                ClearForceFields();
            }
            else
            {
                Debug.Log($"Force Fields für Zustand {state} geladen.");
            }



            if (!foundField)
            {
                Debug.LogWarning($"Kein Force Field gefunden für Zustand: {state}");
                ClearForceFields();
            }
            else
            {
                Debug.Log($"Force Fields für Zustand {state} aktiviert.");
            }
        }

        private void ClearForceFields()
        {
            for (int i = 0; i < 4; i++)
            {
                if (forceFieldObjects[i] != null)
                {
                    Destroy(forceFieldObjects[i]);
                    forceFieldObjects[i] = null;
                }
            }
        }

        private string GetGateState()
        {
            return $"{gates[0].GetState()}{gates[1].GetState()}{gates[2].GetState()}{gates[3].GetState()}";
        }

        private Vector3 GetQuadrantPosition(int index)
        {
            float offset = 5f; // Falls dein Kreuz größer ist, anpassen!

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
