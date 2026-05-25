using System.Collections.Generic;
using System.IO;
using InteractionObjects;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Utils {
    public class GlobalSettings : MonoBehaviour {
    
        private const string TargetFrameRateKey = "GraphicsSettings.TargetFrameRate";
    
        public static GlobalSettings Instance { get; private set; }

        public string savePath;

        public float sensitivity = 0.1f;

        /// <summary>
        /// Gets or sets the target frame rate saved in PlayerPrefs.
        /// </summary>
        public static int TargetFrameRate {
            get => PlayerPrefs.GetInt(TargetFrameRateKey, 60);   // 60 fps as default
            set => PlayerPrefs.SetInt(TargetFrameRateKey, value);
        }

        void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject); // Verhindert doppelte Instanzen
            } else {
                Instance = this;
                // DontDestroyOnLoad handled by GlobalSingleton class in parent object

                savePath = Path.Combine(Application.persistentDataPath, "globalsettings.json");
            }
        }

        private void Start() {
            ApplyGraphicsSettings();
        }

        public void SaveGame() {
            string json = JsonUtility.ToJson(this, true); // Konvertiere die Daten zu JSON
            File.WriteAllText(savePath, json); // Speichere in eine Datei
            Debug.Log("Spiel gespeichert: " + savePath);
        }

        public void LoadGame() {
            if (File.Exists(savePath)) {
                string json = File.ReadAllText(savePath); // Lade die Datei
                JsonUtility.FromJsonOverwrite(json, this); // Schreibe die Daten zur�ck ins Scriptable Object
                Debug.Log("Spiel geladen.");
            } else {
                Debug.LogWarning("Kein Spielstand gefunden.");
            }
        }

        public void SaveScene() {
            Scene scene = SceneManager.GetActiveScene();
            GameObject[] allObjects = scene.GetRootGameObjects();
            List<GameObject> interactableObjects = new List<GameObject>();
            foreach (GameObject obj in allObjects) {
                InteractionObject interactionComponent = obj.GetComponent<InteractionObject>();
                if (interactionComponent != null) {
                    string file = Path.Combine(Application.persistentDataPath, scene.name + "_" + obj.name + ".json");
                    string json = JsonUtility.ToJson(interactionComponent, true); // Konvertiere die Daten zu JSON
                    File.WriteAllText(file, json); // Speichere in eine Datei
                    Debug.Log("Spiel gespeichert: " + file);
                }
            }
        }

        /// <summary>
        /// Applies graphics settings based on PlayerPrefs.
        /// </summary>
        public static void ApplyGraphicsSettings() {
            Application.targetFrameRate = TargetFrameRate;
            QualitySettings.vSyncCount = 0; // deactivate vSync to allow target frame rate to work
            Debug.Log($"Successfully applied graphics settings: TargetFrameRate={TargetFrameRate}");
        }
    }
}