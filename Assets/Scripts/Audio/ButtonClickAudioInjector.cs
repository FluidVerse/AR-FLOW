using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Audio {
    /// <summary>
    /// Provides methods to inject a click sound into all <see cref="Button"/>s from the Unity UI Toolkit.
    ///
    /// As a singleton, only one instance of this class exists globally. Whenever a new scene is loaded, it
    /// automatically hooks into all existing <see cref="UIDocument"/>s and adds the click sound to all buttons that do
    /// not already have it. If a document root is not available at scene creation or gets replaced later, the click
    /// sound has to be injected manually using <see cref="InjectAudio"/>.
    ///
    /// In addition, <see cref="PlayOnce"/> plays the click sound once without the need to inject it into a button.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class ButtonClickAudioInjector : MonoBehaviour {

        public static ButtonClickAudioInjector Instance { get; private set; }

        /// <summary>
        /// USS class name to mark buttons that already have the click sound attached.
        /// </summary>
        private const string attachedMarker = "click-sound-attached";

        private AudioSource audioSource;

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            // DontDestroyOnLoad handled by GlobalSingleton class in parent object

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) {
                Debug.LogError("AudioSource not found", this);
            }

            audioSource.playOnAwake = false;
            audioSource.loop = false;
            SceneManager.sceneLoaded += (_, _) => HookIntoAllUIDocuments();
        }

        /// <summary>
        /// Injects the click sound into all buttons of the given <see cref="UIDocument"/>.
        /// </summary>
        /// <param name="uiDocument">UIDocument to find all buttons in</param>
        public void InjectAudio(UIDocument uiDocument) {
            VisualElement root = uiDocument.rootVisualElement;
            root?.RegisterCallback<AttachToPanelEvent>(_ => AddToAllButtons(root));
            AddToAllButtons(uiDocument.rootVisualElement);
        }

        /// <summary>
        /// Plays the click sound once.
        /// </summary>
        public void PlayOnce() {
            audioSource.Play();
        }

        private void HookIntoAllUIDocuments() {
            UIDocument[] uiDocuments =
                FindObjectsByType<UIDocument>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var uiDocument in uiDocuments) {
                InjectAudio(uiDocument);
            }
        }

        private void AddToAllButtons(VisualElement root) {
            if (root == null) {
                return;
            }
            foreach (var button in root.Query<Button>().Build()) {
                if (!button.ClassListContains(attachedMarker)) {
                    AddAndMark(button);
                }
            }
        }

        private void AddAndMark(Button button) {
            button.clicked -= PlayOnce;
            button.clicked += PlayOnce;
            button.AddToClassList(attachedMarker);
        }
    }
}