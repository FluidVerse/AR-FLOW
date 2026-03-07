using Toolbar;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Quests {
    /// <summary>
    /// Controller for the quest UI.
    /// </summary>
    public class QuestUIController : MonoBehaviour {

        /// <summary>
        /// Event fired when the user submits an answer in an input quest.
        /// The string parameter is the answer text.
        /// </summary>
        public UnityEvent<string> onAnswerSubmitted;

        private VisualElement root;
        private VisualTreeAsset questItemTemplate;
        private VisualTreeAsset questItemWithInputTemplate;
        private ScrollView questList;
        private Label stageTitle;
        private Button stageBack, stageForward; // for quest stages

        private ToolbarManager toolbarManager;
        private QuestManager questManager;

        private int displayedStage; // currently displayed quest stage
        private bool isMenuOpen; // is the quest menu currently open?

        private void Awake() {
            toolbarManager = FindAnyObjectByType<ToolbarManager>();
            if (toolbarManager == null) {
                Debug.LogError("ToolbarManager not found");
            }
            questManager = FindAnyObjectByType<QuestManager>();
            if (questManager == null) {
                Debug.LogError("QuestManager not found");
            }
        }

        private void Start() {
            root = toolbarManager.RootElement;
            questItemTemplate = Resources.Load<VisualTreeAsset>("QuestItemTemplate");
            if (questItemTemplate == null) {
                Debug.LogError("QuestItemTemplate.uxml could not be loaded from Resources");
            }
            questItemWithInputTemplate = Resources.Load<VisualTreeAsset>("QuestItemWithInputTemplate");
            if (questItemWithInputTemplate == null) {
                Debug.LogError("QuestItemWithInputTemplate.uxml could not be loaded from Resources");
            }

            VisualElement questMenu = root.Q("QuestMenu");
            VisualElement topPanel = questMenu.Q("TopPanel");
            questList = questMenu.Q<ScrollView>("QuestList");
            stageTitle = topPanel.Q<Label>("Title");
            stageBack = topPanel.Q<Button>("QuestStageButtonBack");
            stageForward = topPanel.Q<Button>("QuestStageButtonForward");

            stageBack.clicked += () => ChangeDisplayedStage(-1);
            stageForward.clicked += () => ChangeDisplayedStage(1);
        }

        /// <summary>
        /// Used for <see cref="ToolbarManager.onQuestMenuToggle"/>.
        /// </summary>
        /// <param name="opened">Whether the quest menu was opened or closed</param>
        public void OnQuestMenuToggle(bool opened) {
            isMenuOpen = opened;
            if (!isMenuOpen) {
                return; // we only care about opening the menu
            }
            if (questManager.ActiveQuestLine.IsEmpty) {
                Debug.LogWarning("Trying to open quest menu, but the active quest line is empty");
                return;
            }

            displayedStage = questManager.ActiveQuestLine.CurrentStageIndex;
            FillQuestList();
        }

        /// <summary>
        /// Used for <see cref="QuestManager.onQuestCompleted"/> and <see cref="QuestManager.onQuestStageCompleted"/>.
        /// </summary>
        public void OnQuestStateChange() {
            FillQuestList();
        }

        /// <summary>
        /// Changes the currently displayed quest stage and updates the quest list if it is currently open.
        /// </summary>
        /// <param name="offset">Offset to change <see cref="displayedStage"/></param>
        private void ChangeDisplayedStage(int offset) {
            displayedStage = Mathf.Clamp(0, displayedStage + offset, questManager.ActiveQuestLine.Stages.Count - 1);
            if (isMenuOpen) {
                FillQuestList();
            }
        }

        /// <summary>
        /// Clears the quest list, fills it with the quests of the currently displayed quest stage and updates the stage
        /// buttons.
        /// </summary>
        private void FillQuestList() {
            UpdateTopPanel();
            questList.Clear();

            foreach (var quest in questManager.ActiveQuestLine.Stages[displayedStage].Quests) {
                questList.Add(CreateQuestListItem(quest));
            }
        }

        /// <summary>
        /// Creates a quest list item for the given <see cref="Quest"/> based on <see cref="questItemTemplate"/>
        /// or <see cref="questItemWithInputTemplate"/> if the quest requires input.
        /// </summary>
        private VisualElement CreateQuestListItem(Quest quest) {
            bool useInputTemplate = quest.RequiresInput && quest.State != QuestState.Completed;
            VisualElement item = useInputTemplate
                ? questItemWithInputTemplate.CloneTree()
                : questItemTemplate.CloneTree();

            Label questName = item.Q<Label>("QuestName");
            Label description = item.Q<Label>("Description");
            VisualElement imageQuestOpen = item.Q<VisualElement>("ImageQuestOpen");
            VisualElement imageQuestDone = item.Q<VisualElement>("ImageQuestDone");
            bool isQuestCompleted = quest.State == QuestState.Completed;

            questName.text = quest.Name;
            string descriptionPrefix = quest.IsOptional ? "(optional) " : "";
            description.text = descriptionPrefix + quest.Description;
            imageQuestOpen.style.display = isQuestCompleted ? DisplayStyle.None : DisplayStyle.Flex;
            imageQuestDone.style.display = isQuestCompleted ? DisplayStyle.Flex : DisplayStyle.None;

            // Setup input field if using input template
            if (useInputTemplate) {
                TextField answerInput = item.Q<TextField>("AnswerInput");
                Button submitButton = item.Q<Button>("SubmitButton");
                if (answerInput != null && submitButton != null) {
                    submitButton.clicked += () => {
                        string answer = answerInput.value;
                        if (!string.IsNullOrEmpty(answer)) {
                            onAnswerSubmitted?.Invoke(answer);
                        }
                    };
                }
            }

            return item;
        }

        /// <summary>
        /// Updates the top panel of the quest menu with the current stage name and visibility of the stage buttons
        /// based on <see cref="displayedStage"/>.
        /// </summary>
        private void UpdateTopPanel() {
            stageTitle.text = questManager.ActiveQuestLine.Stages[displayedStage].Name;
            bool backVisible = displayedStage > 0;
            bool forwardVisible = displayedStage < questManager.ActiveQuestLine.Stages.Count - 1;
            stageBack.style.visibility = backVisible ? Visibility.Visible : Visibility.Hidden;
            stageForward.style.visibility = forwardVisible ? Visibility.Visible : Visibility.Hidden;
        }
    }
}