#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor.Scripts {
    public class IsAssetUnused : EditorWindow {

        private Object checkObject;
        private readonly List<string> references = new();
        private bool hasScanned;
        private Vector2 scrollPos;
        private bool includeIndirect = true;

        [MenuItem("Tools/Is Asset Unused Check")]
        public static void ShowWindow() {
            GetWindow<IsAssetUnused>("Is Asset Unused?");
        }

        private void OnGUI() {
            GUILayout.Label("Check if an Asset is Referenced", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Select an asset to check if it is used by any other asset (Scenes, Prefabs, Materials, etc.) in the project.",
                MessageType.Info);

            EditorGUILayout.Space();

            checkObject = EditorGUILayout.ObjectField("Asset to Check", checkObject, typeof(Object), false);

            // Toggle for recursive check
            includeIndirect =
                EditorGUILayout.Toggle(
                    new GUIContent("Include Indirect Usage",
                        "If checked, finds scenes/prefabs that use this asset indirectly (e.g., a Scene using a Prefab that uses this Material)."),
                    includeIndirect);

            if (GUILayout.Button("Check References")) {
                if (checkObject != null) {
                    FindReferences(checkObject);
                    hasScanned = true;
                } else {
                    Debug.LogWarning("Please assign an asset first.");
                }
            }

            EditorGUILayout.Space();

            if (hasScanned) {
                if (references.Count == 0) {
                    EditorGUILayout.HelpBox(
                        "No references found. This asset appears to be unused directly by OTHER assets.",
                        MessageType.Info);
                    if (GUILayout.Button("Delete Asset")) {
                        if (EditorUtility.DisplayDialog("Delete Asset",
                                "Are you sure you want to delete this asset? This cannot be undone.", "Yes", "No")) {
                            string path = AssetDatabase.GetAssetPath(checkObject);
                            AssetDatabase.DeleteAsset(path);
                            checkObject = null;
                            hasScanned = false;
                            references.Clear();
                        }
                    }
                } else {
                    GUILayout.Label($"Found {references.Count} direct references:", EditorStyles.boldLabel);
                    scrollPos = GUILayout.BeginScrollView(scrollPos);
                    foreach (var refPath in references) {
                        if (GUILayout.Button(refPath, EditorStyles.miniButtonLeft)) {
                            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(refPath);
                            EditorGUIUtility.PingObject(Selection.activeObject);
                        }
                    }
                    GUILayout.EndScrollView();
                }
            }
        }

        private void FindReferences(Object target) {
            references.Clear();
            string targetPath = AssetDatabase.GetAssetPath(target);

            if (string.IsNullOrEmpty(targetPath)) return;

            // Get all Asset GUIDs in the project
            string[] allGuids = AssetDatabase.FindAssets(""); // Empty filter gets everything

            int total = allGuids.Length;
            for (int i = 0; i < total; i++) {
                string guid = allGuids[i];
                string path = AssetDatabase.GUIDToAssetPath(guid);

                // Skip the asset itself
                if (path == targetPath) continue;

                // Skip directories
                if (AssetDatabase.IsValidFolder(path)) continue;

                // Updating progress bar occasionally
                if (i % 50 == 0) {
                    if (EditorUtility.DisplayCancelableProgressBar("Checking Asset Usage", $"Scanning {path}",
                            (float)i / total)) {
                        break;
                    }
                }

                // Check dependencies (Direct dependencies only)
                string[] dependencies = AssetDatabase.GetDependencies(path, includeIndirect);

                if (dependencies.Contains(targetPath)) {
                    references.Add(path);
                }
            }

            EditorUtility.ClearProgressBar();
        }
    }
}
#else
namespace Utils {
    public class IsAssetUnused {}
}
#endif