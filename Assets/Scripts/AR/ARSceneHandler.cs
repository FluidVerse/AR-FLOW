using System.Collections.Generic;
using System.Linq;
using MainMenu;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace AR {
    /// <summary>
    /// Handles the AR scene and provides the chosen AR model.
    ///
    /// In addition, provides <see cref="State"/> management and publishing of <see cref="AREvent"/>s. This way,
    /// different scripts in the AR scene can communicate with each other.
    /// </summary>
    public class ARSceneHandler : MonoBehaviour {

        /// <summary>
        /// Path to the directory inside the <c>Assets/Resources</c> folder where <see cref="ARModelProperties"/>s are stored.
        /// </summary>
        private const string modelDirectoryPath = "ARModels";

        public static ARSceneHandler Instance { get; private set; }

        /// <summary>
        /// Current state of the AR scene.
        /// </summary>
        public ARState State {
            get => state;
            set {
                state = value;
                Debug.Log($"AR state changed to {state}", this);
                OnStateChanged.Invoke(state);
            }
        }

        /// <summary>
        /// Callback when the AR state is changed (parameter is the new state).
        ///
        /// Get-only property so that it is hidden in the Unity inspector, because as a singleton, subscribing in the
        /// Unity inspector can cause issues. However, it can still be subscribed to in code.
        /// </summary>
        public UnityEvent<ARState> OnStateChanged { get; } = new();

        /// <summary>
        /// Callback when a new model is chosen, see <see cref="ActiveModel"/>.
        ///
        /// Get-only property so that it is hidden in the Unity inspector, because as a singleton, subscribing in the
        /// Unity inspector can cause issues. However, it can still be subscribed to in code.
        /// </summary>
        public UnityEvent<ARModelProperties> OnNewModelChosen { get; } = new();

        /// <summary>
        /// Callback when a sender object publishes an <see cref="AREvent"/> (first parameter is the sender).
        ///
        /// If a script is both sending and receiving events, make sure to not respond to events that it sent itself.
        ///
        /// Get-only property so that it is hidden in the Unity inspector, because as a singleton, subscribing in the
        /// Unity inspector can cause issues. However, it can still be subscribed to in code.
        /// </summary>
        public UnityEvent<object, AREvent> OnEventPublished { get; } = new();

        /// <summary>
        /// List of all models that can be spawned in the AR scene.
        ///
        /// Every model needs to have a unique key.
        /// </summary>
        private readonly List<ARModelProperties> models = new();

        private ARState state = ARState.Inactive;
        private const string arSceneName = "ARScene";
        private string chosenModelKey;

        private void Awake() {
            if (Instance == null) {
                Instance = this;
                // DontDestroyOnLoad handled by GlobalSingleton class in parent object
                Setup();
            } else {
                Destroy(gameObject);
            }
        }

        private void Setup() {
            CreateModelList();
            if (chosenModelKey == null && models.Count > 0) {
                TrySetNewModel("testsetup1"); // TODO remove later, choose test model by default
            }
        }

        private void CreateModelList() {
            models.Clear();
            models.AddRange(Resources.LoadAll<ARModelProperties>(modelDirectoryPath));
            Debug.Log($"ARSceneHandler loaded {models.Count} AR models from Resources/{modelDirectoryPath}", this);
        }

        /// <summary>
        /// Returns the active <see cref="ARModelProperties"/> entry or <c>null</c> if none is chosen.
        /// </summary>
        public ARModelProperties ActiveModel => GetModelByKey(chosenModelKey);

        /// <summary>
        /// Returns the <see cref="ARModelProperties"/> entry with the given <see cref="key"/> or <c>null</c> if an entry with
        /// this key does not exist.
        /// </summary>
        public ARModelProperties GetModelByKey(string key) {
            return models.FirstOrDefault(model => model.key == key);
        }

        /// <summary>
        /// Returns whether a model is active and outputs the active model.
        /// </summary>
        /// <param name="model">The chosen model if one is active, otherwise <see langword="default"/></param>
        /// <returns>Whether a model is currently active</returns>
        public bool ActiveModelExists(out ARModelProperties model) {
            model = ActiveModel;
            return ActiveModel != null;
        }

        /// <summary>
        /// Callback function that is registered in <see cref="DeepLink.DeepLinkManager"/>.
        /// </summary>
        public void OnDeepLinkReceived(Dictionary<string, string> args) {
            if (!args.TryGetValue("ar", out var key)) {
                Debug.Log("Deep link received, but no AR model specified", this);
                return;
            }
            OpenARSceneWithModel(key);
        }
        
        /// <summary>
        /// Opens the AR scene with the specified model.
        /// </summary>
        /// <param name="modelKey">Key of the AR model, see <see cref="ARModelProperties.key"/></param>
        public void OpenARSceneWithModel(string modelKey) {
            if (!TrySetNewModel(modelKey)) {
                Debug.Log("Tried to open AR scene with model, but AR model with specified key does not exist", this);
                return;
            }

            MainMenuManager.Instance?.HideMainMenu();
            SceneManager.LoadScene(arSceneName);
        }

        /// <summary>
        /// Publishes an <see cref="AREvent"/> to all subscribers of <see cref="OnEventPublished"/>.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="arEvent">Event to publish</param>
        public void PublishEvent(object sender, AREvent arEvent) {
            OnEventPublished?.Invoke(sender, arEvent);
        }

        /// <summary>
        /// Tries to set a new AR model as the chosen model.
        /// </summary>
        /// <param name="key">Key of the model as in <see cref="ARModelProperties"/></param>
        /// <returns><c>true</c> if successfully set, <c>false</c> if model with such key does not exist</returns>
        private bool TrySetNewModel(string key) {
            if (models.All(model => model.key != key)) {
                Debug.LogWarning($"Trying to set model with key {key}, but no model with this key exists", this);
                return false;
            }

            chosenModelKey = key;
            OnNewModelChosen.Invoke(ActiveModel);

            Debug.Log($"Successfully set new AR model with key {key}", this);
            return true;
        }
    }
}