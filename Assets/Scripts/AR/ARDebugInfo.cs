using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace AR {
    /// <summary>
    /// Shows debug information for the AR scene.
    /// </summary>
    public class ARDebugInfo : MonoBehaviour {

        private ARAnchorAlignedSpawner spawner;

        private void Awake() {
            spawner = FindAnyObjectByType<ARAnchorAlignedSpawner>();
        }

        private void OnGUI() {
            GUIStyle style = new GUIStyle {
                fontSize = 24,
                normal = { textColor = Color.white }
            };

            ARSceneHandler sceneHandler = ARSceneHandler.Instance;
            GameObject model = spawner.InstantiatedModel;

            GUI.Label(new Rect(10, 10, 500, 30),
                sceneHandler != null ? $"AR State: {sceneHandler.State}" : "ARSceneHandler not found!", style);

            Camera cam = Camera.main;
            GUI.Label(new Rect(10, 40, 500, 30),
                cam != null ? $"Cam pos: {cam.transform.position}" : "Main Camera not found!", style);
            GUI.Label(new Rect(10, 70, 500, 30),
                cam != null ? $"Cam rot: {cam.transform.eulerAngles}" : "Main Camera not found!", style);

            ARTrackedImage anchorImg = spawner.AnchorImage;
            GUI.Label(new Rect(10, 100, 500, 30),
                anchorImg != null ? $"Anchor pos: {anchorImg.transform.position}" : "Anchor not found!", style);
            GUI.Label(new Rect(10, 130, 500, 30),
                anchorImg != null ? $"Anchor rot: {anchorImg.transform.eulerAngles}" : "Anchor not found!", style);

            GUI.Label(new Rect(10, 160, 500, 30),
                model != null ? $"Model pos: {model.transform.position}" : "Model not found!", style);
            GUI.Label(new Rect(10, 190, 500, 30),
                model != null ? $"Model rot: {model.transform.eulerAngles}" : "Model not found!", style);

            string hasParentText = model != null
                ? model.transform.parent != null
                    ? $"Model has a parent: ${model.transform.parent.name}."
                    : "Model has no parent."
                : "Model not found!";
            GUI.Label(new Rect(10, 220, 500, 30), hasParentText, style);
        }
    }
}