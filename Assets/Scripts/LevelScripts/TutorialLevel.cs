using Quests;
using Toolbar;
using UnityEngine;
using static Quests.QuestInteractionTypes;
using static Quests.QuestObject;
using static Toolbar.ToolbarArea;
using static Toolbar.ToolbarButton;

namespace LevelScripts {
    public class TutorialLevel : MonoBehaviour {

        [SerializeField] private Texture2D[] buttonIcons = new Texture2D[3];
        [SerializeField] private Texture2D[] fieldButtonIcons = new Texture2D[3];

        private QuestManager questManager;
        private ToolbarManager toolbarManager;

        // visibility states of LevelObject buttons 1-3
        private readonly bool[] buttonStates = { true, false, false };

        // visibility states of Property1 and Property2
        private readonly bool[] propertyStates = { false, false };

        private void Awake() {
            questManager = FindAnyObjectByType<QuestManager>();
            if (questManager == null) {
                Debug.LogError("QuestManager not found", this);
            }
            toolbarManager = FindAnyObjectByType<ToolbarManager>();
            if (toolbarManager == null) {
                Debug.LogError("ToolbarManager not found", this);
            }
        }

        private void Start() {
            // ToolbarArea and ToolbarButton are statically imported on top so that we don't have to type 
            // ToolbarButton.LevelObjects1, ToolbarButton.LevelObjects2 etc. every time
            questManager.SetQuestLine(QuestLines.TutorialLevel);
            questManager.onInteractionFromQuest.AddListener(OnInteractionFromQuest);
            toolbarManager.SetAreaVisibility(CameraButtonContainer, true);

            // set up LevelObject buttons 1-3
            toolbarManager.SetAreaVisibility(LevelObjects, true);
            toolbarManager.SetupButton(LevelObjects1, buttonIcons[0], OnLevelObjectsButton1Click);
            toolbarManager.SetupButton(LevelObjects2, buttonIcons[1], OnLevelObjectsButton2Click);
            toolbarManager.SetupButton(LevelObjects3, buttonIcons[2], OnLevelObjectsButton3Click);
            toolbarManager.SetButtonEnabled(LevelObjects1, buttonStates[0], true);
            toolbarManager.SetButtonEnabled(LevelObjects2, buttonStates[1], true);
            toolbarManager.SetButtonEnabled(LevelObjects3, buttonStates[2], true);

            // set up FieldProperties
            toolbarManager.SetAreaVisibility(FieldProperties, true);
            // ButtonField is behind the camera button in the top right, so it would be overlapping anyway
            toolbarManager.SetButtonVisibility(ButtonField, false, true);
            // onClick argument can be null too...
            toolbarManager.SetupButton(ButtonFieldA, fieldButtonIcons[0], null);
            toolbarManager.SetupButton(ButtonFieldB, fieldButtonIcons[1], null);
            toolbarManager.SetupButton(ButtonFieldC, fieldButtonIcons[2], null);
            // ...and set up later using AddButtonAction if needed
            toolbarManager.AddButtonAction(ButtonFieldA, () => Debug.Log("Clicked on ButtonFieldA"));
            toolbarManager.AddButtonAction(ButtonFieldB, () => Debug.Log("Clicked on ButtonFieldB"));
            toolbarManager.AddButtonAction(ButtonFieldC, () => Debug.Log("Clicked on ButtonFieldC"));
        }

        private void OnLevelObjectsButton1Click() {
            OnLevelObjectsButtonClick(0);
            // if first button clicked: fulfill quest 
            // note: QuestObject and QuestInteractionTypes are statically imported for the same reasons as  
            // ToolbarArea and ToolbarButton 
            questManager.SendInteraction(new QuestInteraction<object>(LevelObjectsButton1, ButtonClicked));
        }

        private void OnLevelObjectsButton2Click() {
            OnLevelObjectsButtonClick(1);
            ToggleProperty(0);
        }

        private void OnLevelObjectsButton3Click() {
            OnLevelObjectsButtonClick(2);
            ToggleProperty(1);
        }

        private void OnLevelObjectsButtonClick(int index) {
            bool newState = !buttonStates[index];
            buttonStates[index] = newState;
            Debug.Log($"LevelObjects button {index + 1} clicked. New state: {buttonStates[index]}");

            // keepOnClick = true so that we can still click on the button to toggle it back on
            toolbarManager.SetButtonEnabled(ToolbarButtonGroups.LevelObjects[index], newState, true);
        }

        private void ToggleProperty(int index) {
            bool newState = !propertyStates[index];
            propertyStates[index] = newState;

            toolbarManager.SetAreaVisibility(ToolbarAreaGroups.Properties[index], newState);
        }

        private void OnInteractionFromQuest(IQuestInteraction interaction) {
            if (interaction.IsObjectAndType(SecretObject, RevealObject)) {
                // this interaction was sent out by the quest "Get an overview"!
                // since we got an overview about the basic objects in the scene, maybe now it's time to reveal
                // a secret object in the scene that was invisible before?
                Debug.Log("Received interaction from quest: SecretObject revealed!");
                // not implemented, just an example for how to utilize the bidirectional quest communication :)
            }
        }
    }
}