using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Utils {
    public class HelpFunctions {
        
        List<GameObject> meshObjects;
        List<bool> meshObjectsActive;

        private List<VisualElement> uiElements;

        public HelpFunctions() {
            meshObjects = new List<GameObject>();
            meshObjectsActive = new List<bool>();

            GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (GameObject obj in allObjects) {
                if (obj.GetComponent<MeshRenderer>() || obj.GetComponent<MeshFilter>()) {
                    meshObjects.Add(obj);
                    meshObjectsActive.Add(obj.activeSelf);
                }
            }

            uiElements = new List<VisualElement>();
            FindAllUIDocuments();
        }

        public void ScaleToFitCamera(GameObject obj, Camera camera) {
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer == null) return;

            Collider collider = obj.GetComponent<Collider>();
            if (collider == null) return;

            // Berechne die Bounding-Kugel des Objekts
            Bounds bounds = collider.bounds;
            float objectRadius = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z) / 1.5f;


            // Berechne den Abstand zur Kamera
            float distance = Vector3.Distance(camera.transform.position, bounds.center);

            // Berechne die maximale H�he und Breite des Viewports in diesem Abstand
            float frustumHeight = 2.0f * distance * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float frustumWidth = frustumHeight * camera.aspect;

            // Bestimme den Skalierungsfaktor basierend auf dem Bounding-Kugelradius und der Viewport-Gr��e
            float scale = Mathf.Min(frustumWidth, frustumHeight) / (2 * objectRadius);
            obj.transform.localScale = Vector3.one * scale; // Skalierung gleichm��ig anwenden
        }

        public void DisableAllGameObjectsBut(GameObject pObj) {
            for (int i = 0; i < meshObjects.Count; i++) {
                meshObjectsActive[i] = meshObjects[i].activeSelf;
                // disable all objects except pObj AND all its children
                if (meshObjects[i] != pObj && !meshObjects[i].transform.IsChildOf(pObj.transform)) {
                    meshObjects[i].SetActive(false); // Objekte deaktivieren
                }
            }
        }

        public void ResetAllGameObjects() {
            for (int i = 0; i < meshObjects.Count; i++) {
                meshObjects[i].SetActive(meshObjectsActive[i]);
            }
        }

        // **********
        void FindAllUIDocuments() {
            // Suche alle UIDocuments in der Szene
            UIDocument[] uiDocuments = Object.FindObjectsOfType<UIDocument>();

            foreach (UIDocument uiDocument in uiDocuments) {
                if (uiDocument != null && uiDocument.rootVisualElement != null) {
                    // F�ge das rootVisualElement des UIDocuments hinzu
                    uiElements.Add(uiDocument.rootVisualElement);
                }
            }

            Debug.Log($"Gefundene UI-Dokumente: {uiElements.Count}");
        }

        public bool IsPointerOverUI(Vector2 pointerPosition) {
            // Iteriere �ber alle rootVisualElements
            foreach (var rootElement in uiElements) {
                // Pr�fe die Kinder des rootElements rekursiv
                if (IsPointOverUIElement(rootElement, pointerPosition)) {
                    return true; // Ein sichtbares UI-Element wurde getroffen
                }
            }

            return false; // Kein UI-Element wurde getroffen
        }

        private bool IsPointOverUIElement(VisualElement element, Vector2 pointerPosition) {
            // Pr�fe, ob das Element sichtbar und interaktiv ist
            if (!IsElementVisible(element)) {
                return false;
            }

            // Pr�fe, ob der Punkt innerhalb der Bounding Box liegt
            if (element.worldBound.Contains(pointerPosition)) {
                return true;
            }

            // Rekursiv die Kinder pr�fen
            foreach (var child in element.Children()) {
                if (IsPointOverUIElement(child, pointerPosition)) {
                    return true;
                }
            }

            return false;
        }

        private bool IsElementVisible(VisualElement element) {
            // Pr�fe, ob das Element sichtbar ist
            return element.resolvedStyle.display != DisplayStyle.None &&
                   element.resolvedStyle.visibility == Visibility.Visible;
        }
    }
}