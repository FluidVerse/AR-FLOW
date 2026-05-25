using Shaders;
using UnityEditor;
using UnityEngine;

namespace Editor.Scripts {
    /// <summary>
    /// Custom inspector for the <see cref="TransparencyController"/> script.
    /// </summary>
    [CustomEditor(typeof(TransparencyController))]
    public class TransparencyControllerEditor : UnityEditor.Editor {

        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            if (!Application.isPlaying) {
                // don't show extra buttons in edit mode avoid material instancing in the editor (we only want that
                // in play mode)
                EditorGUILayout.HelpBox("This script provides additional inspector controls in play mode.",
                    MessageType.Info);
                return;
            }

            TransparencyController controller = (TransparencyController)target;
            if (GUILayout.Button("Set to opaque")) {
                controller.SetMode(TransparencyMode.Opaque);
            }
            if (GUILayout.Button("Set to transparent")) {
                controller.SetMode(TransparencyMode.Transparent);
            }
            if (GUILayout.Button("Increase opaqueness")) {
                controller.Alpha += 0.1f;
            }
            if (GUILayout.Button("Decrease opaqueness")) {
                controller.Alpha -= 0.1f;
            }
        }
    }
}