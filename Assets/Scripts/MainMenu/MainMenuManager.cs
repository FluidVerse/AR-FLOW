using System.Collections.Generic;
using System.Linq;
using AR;
using Toolbar;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Utils;

namespace MainMenu {
    [RequireComponent(typeof(UIDocument))]
    public class MainMenuManager : MonoBehaviour {

        /// <summary>
        /// Name of the main menu scene.
        /// </summary>
        private const string mainMenuSceneName = "MainMenu";

        /// <summary>
        /// Path to the directory inside the <c>Assets/Resources</c> folder where level properties are stored.
        /// </summary>
        private const string levelDirectoryPath = "Levels";

        /// <summary>
        /// Callback when the main menu is shown or hidden. If the main menu is shown, then <c>true</c> is passed,
        /// otherwise <c>false</c>.
        ///
        /// Get-only property so that it is hidden in the Unity inspector, because as a singleton, subscribing in the
        /// Unity inspector can cause issues. However, it can still be subscribed to in code.
        /// </summary>
        public UnityEvent<bool> OnMainMenuToggle { get; } = new();

        public static MainMenuManager Instance { get; private set; }

        private VisualElement ui;
        private VisualElement MainMenu;
        private VisualElement OptionsMenu;
        private VisualElement LevelMenu;

        private Button MainMenu_ButtonContinue;
        private Button MainMenu_ButtonOption;
        private Button MainMenu_ButtonLevel;
        private Button MainMenu_ButtonQuit;

        private Button OptionsMenu_ButtonBack;
        private Slider OptionsMenu_SliderSensitivity; // Empfindlichkeit des Touchs
        private Toggle OptionsMenu_60fpsToggle;
        private Button OptionsMenu_CreditsButton;

        private ScrollView CreditsScrollView;
        private Button Credits_ButtonBack;

        private Label LevelMenu_LabelLevel;
        private VisualElement LevelMenu_ImageLevel;
        private Button LevelMenu_ButtonBack;
        private Button LevelMenu_ButtonPlay;

        private readonly List<LevelProperties> levelList = new();
        public LevelProperties selectedLevel; // can be null

        public bool isActive;

        // MainMenu-Verknüpfung
        private ToolbarManager toolbarManager;
        
        /// <summary>
        /// Gibt zurück, ob der "Fortsetzen"-Button im Hauptmenü aktiviert sein soll.
        /// </summary>
        private static bool CanContinue => SceneManager.GetActiveScene().name != mainMenuSceneName;

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject); // Verhindert doppelte Instanzen
                return;
            }

            Instance = this;
            // DontDestroyOnLoad handled by GlobalSingleton class in parent object


            ui = GetComponent<UIDocument>().rootVisualElement;
            MainMenu = ui.Q<VisualElement>("MainMenu");
            OptionsMenu = ui.Q<VisualElement>("OptionsMenu");
            LevelMenu = ui.Q<VisualElement>("LevelMenu");
            CreditsScrollView = ui.Q<ScrollView>("CreditsScrollView");

            // Beim Start immer MainMenu anzeigen (egal was im UI Editor eingestellt ist)
            MainMenu.style.display = DisplayStyle.Flex;
            OptionsMenu.style.display = DisplayStyle.None;
            LevelMenu.style.display = DisplayStyle.None;
            CreditsScrollView.style.display = DisplayStyle.None;

            // Hauptmenü
            MainMenu_ButtonContinue = MainMenu.Q<Button>("ButtonContinue");
            MainMenu_ButtonContinue.clicked += MainMenu_ClickButtonContinue;
            MainMenu_ButtonOption = ui.Q<Button>("ButtonOption");
            MainMenu_ButtonOption.clicked += MainMenu_ClickButtonOption;
            MainMenu_ButtonLevel = ui.Q<Button>("ButtonLevel");
            MainMenu_ButtonLevel.clicked += MainMenu_ClickButtonLevel;
            MainMenu_ButtonQuit = ui.Q<Button>("ButtonQuit");
            MainMenu_ButtonQuit.clicked += MainMenu_ClickButtonQuit;

            // Optionsmenü
            OptionsMenu_ButtonBack = OptionsMenu.Q<Button>("ButtonBack");
            OptionsMenu_ButtonBack.clicked += ClickButtonBackToMainMenu;
            OptionsMenu_SliderSensitivity = OptionsMenu.Q<Slider>("SliderSensitivity");
            OptionsMenu_SliderSensitivity.RegisterValueChangedCallback(evt =>
                OptionsMenu_ChangeSliderSensitivity());
            OptionsMenu_60fpsToggle = OptionsMenu.Q<Toggle>("60fpsToggle");
            OptionsMenu_60fpsToggle.RegisterValueChangedCallback(OptionsMenu_60fpsToggleChanged);
            OptionsMenu_60fpsToggle.value = GlobalSettings.TargetFrameRate == 60;
            OptionsMenu_CreditsButton = OptionsMenu.Q<Button>("ButtonCredits");
            OptionsMenu_CreditsButton.clicked += OptionsMenu_ClickButtonCredits;

            // Credits
            Credits_ButtonBack = CreditsScrollView.Q<Button>("CreditsButtonBack");
            Credits_ButtonBack.clicked += CreditsMenu_ClickButtonBack;
            
            // Levelmenü
            LevelMenu_LabelLevel = LevelMenu.Q<Label>("LabelLevel");
            LevelMenu_ImageLevel = LevelMenu.Q<VisualElement>("ImageLevel");
            LevelMenu_ButtonPlay = LevelMenu.Q<Button>("ButtonPlay");
            LevelMenu_ButtonPlay.clicked += OnClickLevelPlay;
            LevelMenu_ButtonBack = LevelMenu.Q<Button>("ButtonBack");
            LevelMenu_ButtonBack.clicked += ClickButtonBackToMainMenu;

            CreateLevelList();
            SelectLevel(null);
        }

        private void Start() {
            ToggleContinueOpacity();
        }

        private void CreateLevelList() {
            levelList.Clear();
            levelList.AddRange(Resources.LoadAll<LevelProperties>(levelDirectoryPath)
                .Where(level => level.showInMenu)); // only levels with showInMenu == true
            levelList.Sort((a, b) => a.id - b.id); // sort by id 
            ScrollView levelScrollView = LevelMenu.Q<ScrollView>("LevelList");

            foreach (LevelProperties level in levelList) {
                LevelMenuItem item = new();
                item.Create(level.displayName, level.displayHint, () => SelectLevel(level));
                levelScrollView.Add(item);
            }
        }

        // in der aktuellen Szene die Toolbar verknüpfen
        public void linkToolbar() {
            // Toolbar verknüpfen
            toolbarManager = FindAnyObjectByType<ToolbarManager>();
            if (toolbarManager == null) {
                Debug.Log("<color=red>" + this.name + ": No Toolbar object with toolbarManager in Scene!</color>");
            }
        }

        public void ShowMainMenu() {
            //gameObject.SetActive(true);
            ui.style.display = DisplayStyle.Flex;
            MainMenu.style.display = DisplayStyle.Flex;
            OptionsMenu.style.display = DisplayStyle.None;
            LevelMenu.style.display = DisplayStyle.None;
            isActive = true;
            OnMainMenuToggle.Invoke(true);
            ToggleContinueOpacity();
        }

        public void HideMainMenu() {
            //gameObject.SetActive(true);
            MainMenu.style.display = DisplayStyle.None;
            LevelMenu.style.display = DisplayStyle.None;
            OptionsMenu.style.display = DisplayStyle.None;
            ui.style.display = DisplayStyle.None;
            isActive = false;
            OnMainMenuToggle.Invoke(false);
        }

        public void MainMenu_ClickButtonContinue() {
            if (!CanContinue) {
                return; // im Hauptmenü, nichts tun
            }
            
            // wenn eine Toolbar schon vorhanden, dann einblenden
            if (toolbarManager != null) {
                toolbarManager.showToolbar();
            }
            HideMainMenu();
        }

        private void ToggleContinueOpacity() {
            if (CanContinue) {
                MainMenu_ButtonContinue.style.opacity = 1f;
                MainMenu_ButtonContinue.SetEnabled(true);
            } else {
                MainMenu_ButtonContinue.style.opacity = 0.5f;
                MainMenu_ButtonContinue.SetEnabled(false);
            }
        }

        // Event um zurück ins Hauptmenü zu kommen
        public void ClickButtonBackToMainMenu() {
            OptionsMenu.style.display = DisplayStyle.None;
            LevelMenu.style.display = DisplayStyle.None;
            MainMenu.style.display = DisplayStyle.Flex;
        }

        public void MainMenu_ClickButtonOption() {
            //GlobalSettings.Instance.volume = 9f;
            MainMenu.style.display = DisplayStyle.None;
            LevelMenu.style.display = DisplayStyle.None;
            OptionsMenu.style.display = DisplayStyle.Flex;


            OptionsMenu_SliderSensitivity.value = GlobalSettings.Instance.sensitivity * 200f;
        }

        public void MainMenu_ClickButtonLevel() {
            MainMenu.style.display = DisplayStyle.None;
            OptionsMenu.style.display = DisplayStyle.None;
            LevelMenu.style.display = DisplayStyle.Flex;
        }

        public void MainMenu_ClickButtonQuit() {
            //GlobalSettings.Instance.LoadGame();
            Application.Quit();
        }

        public void OptionsMenu_ChangeSliderSensitivity() {
            Debug.Log(OptionsMenu_SliderSensitivity.value.ToString());
            GlobalSettings.Instance.sensitivity = OptionsMenu_SliderSensitivity.value / 200f;
        }
        
        private void OptionsMenu_60fpsToggleChanged(ChangeEvent<bool> evt) {
            GlobalSettings.TargetFrameRate = evt.newValue ? 60 : 30;
            GlobalSettings.ApplyGraphicsSettings();
        }

        private void OptionsMenu_ClickButtonCredits() {
            OptionsMenu.style.display = DisplayStyle.None;
            CreditsScrollView.style.display = DisplayStyle.Flex;
        }

        private void CreditsMenu_ClickButtonBack() {
            CreditsScrollView.style.display = DisplayStyle.None;
            OptionsMenu.style.display = DisplayStyle.Flex;
        }

        private void OnClickLevelPlay() {
            if (selectedLevel == null) {
                Debug.LogWarning("No level selected to play", this);
                return;
            }
            if (string.IsNullOrEmpty(selectedLevel.sceneName)) {
                Debug.LogWarning(
                    $"Selected level {selectedLevel.id} ({selectedLevel.displayName}) has no scene name set.", this);
                return;
            }

            if (selectedLevel.IsArLevel) {
                ARSceneHandler.Instance.OpenARSceneWithModel(selectedLevel.modelKey);
            } else {
                SceneManager.LoadScene(selectedLevel.sceneName);
                HideMainMenu();
            }
        }

        private void SelectLevel(LevelProperties level) {
            bool selected = level != null;
            LevelMenu_ButtonPlay.SetEnabled(selected);
            LevelMenu_ButtonPlay.style.opacity = selected ? 1f : 0.5f;

            if (level == null) {
                return;
            }

            selectedLevel = level;
            LevelMenu_LabelLevel.text = level.displayName;
            LevelMenu_ImageLevel.style.backgroundImage = level.levelImage;
            Debug.Log($"Selected level {selectedLevel.id} ({level.displayName})", this);
        }
    }
}