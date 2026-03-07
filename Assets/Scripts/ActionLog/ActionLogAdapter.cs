using BCI;
using Graphs;
using Quests;
using UnityEngine;
using static ActionLog.LogMessages;

namespace ActionLog {
    /// <summary>
    /// Adapter that connects the <see cref="ActionLogManager"/> to game events, such as quest completions.
    ///
    /// This adapter is used when subscribing to events from other big systems to keep them independent. If applicable,
    /// directly accessing the <see cref="ActionLogManager"/> within smaller objects is fine too.
    /// </summary>
    public class ActionLogAdapter : MonoBehaviour {

        [SerializeField] private ActionLogManager log;

        /// <summary>
        /// Callback for <see cref="QuestManager.onQuestCompleted"/>.
        /// </summary>
        public void OnQuestCompleted(Quest quest) {
            log.Write(QuestCompleted(quest.Name));
        }

        /// <summary>
        /// Callback for <see cref="QuestManager.onQuestRevoked"/>.
        /// </summary>
        public void OnQuestRevoked(Quest quest) {
            log.Write(QuestRevoked(quest.Name));
        }

        /// <summary>
        /// Callback for <see cref="QuestManager.onQuestStageCompleted"/>.
        /// </summary>
        public void OnQuestStageCompleted(QuestStage stage) {
            log.Write(QuestStageCompleted(stage.Name));
        }

        /// <summary>
        /// Callback for <see cref="GraphUIControls.onClickResetZoomButton"/>.
        /// </summary>
        public void OnGraphZoomReset() {
            log.Write(GraphZoomReset);
        }

        /// <summary>
        /// Callback for <see cref="MoodyDiagramController.onClickOnDiagram"/>
        /// </summary>
        /// <param name="clickedCorrectly"><c>true</c> if the user has clicked on the desired point</param>
        public void OnClickOnMoodyDiagram(bool clickedCorrectly) {
            log.Write(clickedCorrectly ? MoodyDiagramClickedCorrectly : MoodyDiagramClickedIncorrectly);
        }
    }
}