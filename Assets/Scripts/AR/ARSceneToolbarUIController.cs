using System;
using MainMenu;
using Toolbar;
using UnityEngine;
using UnityEngine.UIElements;

namespace AR {
    /// <summary>
    /// Controller for the AR scene UI elements in the toolbar.
    /// </summary>
    public class ARSceneToolbarUIController : MonoBehaviour {

        private readonly string[] modelInSceneText = {
            "Model has not yet been placed.",
            "Model already exists in the scene."
        };

        private readonly string[] fixatePositionButtonText = { "Lock position", "Release position" };

        private readonly string[] fixatePositionExplanationText = {
            "The position of the AR model is fixed. This means that its position no longer adjusts to the anchor point in the real world (e.g., QR code).",
            "The position of the AR model is released. This means that its position adapts to the anchor point in the real world (e.g., QR code) and moves with it."
        };

        private ARSceneHandler sceneHandler;
        private ARAnchorAlignedSpawner spawner;
        private ToolbarManager toolbarManager;

        private VisualElement settingsMenu, modelSettingsContainer, fixatePositionContainer;
        private Label selectedModelLabel, modelInSceneLabel, fixatePositionLabel;
        private Button fixatePositionButton, resetModelButton;

        private void Awake() {
            sceneHandler = FindAnyObjectByType<ARSceneHandler>();
            if (sceneHandler == null) {
                Debug.LogError("ARSceneHandler not found", this);
            }

            spawner = FindAnyObjectByType<ARAnchorAlignedSpawner>();
            if (spawner == null) {
                Debug.LogError("ARTrackedImageSpawner not found", this);
            }

            toolbarManager = FindAnyObjectByType<ToolbarManager>();
            if (toolbarManager == null) {
                Debug.LogError("ToolbarManager not found", this);
            }
        }

        private void Start() {
            VisualElement root = toolbarManager.root;
            settingsMenu = root.Q<VisualElement>("ARMenu");
            modelSettingsContainer = settingsMenu.Q<VisualElement>("Container_ModelSettings");
            fixatePositionContainer = settingsMenu.Q<VisualElement>("Element_FixPosition");
            selectedModelLabel = settingsMenu.Q<Label>("Label_SelectedModel");
            modelInSceneLabel = settingsMenu.Q<Label>("Label_ModelInScene");
            fixatePositionLabel = settingsMenu.Q<Label>("Label_FixatePosition");
            fixatePositionButton = settingsMenu.Q<Button>("Button_FixatePosition");
            resetModelButton = settingsMenu.Q<Button>("Button_ResetModel");

            Button toolbarArButton = toolbarManager.Toolbar_ButtonAR;
            toolbarArButton.clicked += ToggleSettingsMenu;

            toolbarManager.hideAllButtons();
            toolbarManager.showButtonMainMenu();
            toolbarManager.ShowButtonAR();
            toolbarManager.showButtonBack();

            toolbarManager.Toolbar_ButtonBack.clicked += () => settingsMenu.style.display = DisplayStyle.None;

            fixatePositionButton.clicked += OnClickFixatePositionButton;
            resetModelButton.clicked += OnClickResetModelButton;
            selectedModelLabel.text = "(no model selected)";
            sceneHandler.OnNewModelChosen.AddListener(OnNewModelChosen);
            sceneHandler.OnStateChanged.AddListener(OnARStateChanged);

            fixatePositionButton.text = fixatePositionButtonText[0];
            fixatePositionLabel.text = fixatePositionExplanationText[0];

            // TODO test whether this option is actually useful (and whether the texts are set correctly)
            // until then, hide the option
            fixatePositionContainer.style.display = DisplayStyle.None;

            SetFixatePositionTexts();
            if (sceneHandler.ActiveModelExists(out ARModelProperties m)) {
                OnNewModelChosen(m);
            }
            if (sceneHandler.State != ARState.Inactive) {
                // if state has already changed before we could register, update UI now
                OnARStateChanged(sceneHandler.State);
            }
            MainMenuManager.Instance?.HideMainMenu();
        }

        private void OnDisable() {
            settingsMenu.style.display = DisplayStyle.None;
            sceneHandler.OnNewModelChosen.RemoveListener(OnNewModelChosen);
            sceneHandler.OnStateChanged.RemoveListener(OnARStateChanged);
        }

        private void ToggleSettingsMenu() {
            settingsMenu.style.display =
                settingsMenu.style.display == DisplayStyle.Flex ? DisplayStyle.None : DisplayStyle.Flex;
        }

        private void OnNewModelChosen(ARModelProperties model) {
            selectedModelLabel.text = model.modelName;
        }

        private void OnARStateChanged(ARState newState) {
            modelInSceneLabel.text = modelInSceneText[newState == ARState.ModelPlaced ? 1 : 0];
            modelSettingsContainer.style.display =
                newState == ARState.ModelPlaced ? DisplayStyle.Flex : DisplayStyle.None;
            SetFixatePositionTexts();
        }

        private void OnClickFixatePositionButton() {
            bool success = spawner.SetAnchorAsParent(!spawner.ModelParentExists);
            if (!success) {
                Debug.LogWarning("Could not (un)fixate AR model position: No anchor image or model exists.", this);
                return;
            }
            SetFixatePositionTexts();
        }

        private void SetFixatePositionTexts() {
            if (spawner.ModelParentExists) {
                fixatePositionButton.text = fixatePositionButtonText[1];
                fixatePositionLabel.text = fixatePositionExplanationText[1];
            } else {
                fixatePositionButton.text = fixatePositionButtonText[0];
                fixatePositionLabel.text = fixatePositionExplanationText[0];
            }
        }

        private void OnClickResetModelButton() {
            spawner.ResetTracking();
        }
    }
}