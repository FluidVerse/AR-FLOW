using Quests;
using Toolbar;
using UnityEngine;

namespace LevelScripts {
    public class ARScene : MonoBehaviour {

        private QuestManager questManager;
        private ToolbarManager toolbarManager;

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
            questManager.SetQuestLine(QuestLines.ARScene);
            toolbarManager.hideButtonQuestMenu();
        }
    }
}