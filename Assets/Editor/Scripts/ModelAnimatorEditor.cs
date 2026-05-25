using Models;
using UnityEditor;
using UnityEngine;

namespace Editor.Scripts {
    /// <summary>
    /// Custom inspector for the <see cref="ModelAnimator"/> script.
    /// </summary>
    [CustomEditor(typeof(ModelAnimator), true)]
    public class ModelAnimatorEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            ModelAnimator animator = (ModelAnimator)target;
            if (!Application.isPlaying) {
                EditorGUILayout.HelpBox("This script provides additional inspector controls in play mode.",
                    MessageType.Info);
                return;
            }
            
            if (GUILayout.Button("Toggle animation on/off")) {
                animator.AnimationEnabled = !animator.AnimationEnabled;
            }
            if (GUILayout.Button("Reset animation")) {
                animator.ResetAnimation();
            }
            if (GUILayout.Button("Increase animation speed")) {
                animator.AnimationSpeed += 10f;
            }
            if (GUILayout.Button("Decrease animation speed")) {
                animator.AnimationSpeed -= 10f;
            }
        }
    }
}