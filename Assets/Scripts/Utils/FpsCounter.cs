using UnityEngine;

namespace Utils {
    public class FpsCounter : MonoBehaviour {

        private readonly GUIStyle style = new();
        private float fps;

        private void Start() {
            style.fontSize = 36;
            //Application.targetFrameRate = 30;
            //QualitySettings.vSyncCount = 0;
        }

        private void Update() {
            fps = 1f / Time.unscaledDeltaTime;
        }

        private void OnGUI() {
            GUI.Label(new Rect(10, 40, 100, 25), ((int)fps).ToString(), style);
        }
    }
}