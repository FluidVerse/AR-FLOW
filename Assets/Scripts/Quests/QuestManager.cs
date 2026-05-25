using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Quests {
    /// <summary>
    /// Manages the active <see cref="QuestLine"/> and enables the interaction between scene objects and quests.
    /// </summary>
    public class QuestManager : MonoBehaviour {

        /// <summary>
        /// Callback when the currently active <see cref="QuestLine"/> changes.
        ///
        /// The new quest line is passed as a parameter or can be accessed via <see cref="ActiveQuestLine"/>.
        /// </summary>
        public UnityEvent<QuestLine> onQuestLineChanged = new();

        /// <summary>
        /// Callback when a <see cref="Quest"/> is completed.
        ///
        /// The completed quest is passed as a parameter.
        /// </summary>
        public UnityEvent<Quest> onQuestCompleted = new();

        /// <summary>
        /// Callback when a <see cref="Quest"/> is revoked, i.e. when it is set to not completed again after it was
        /// completed before.
        ///
        /// The revoked quest is passed as a parameter.
        /// </summary>
        public UnityEvent<Quest> onQuestRevoked = new();

        /// <summary>
        /// Callback when a <see cref="QuestStage"/> is completed.
        ///
        /// The completed stage is passed as a parameter. At the time of this callback,
        /// <see cref="QuestLine.CurrentStage"/> of <see cref="ActiveQuestLine"/> already points to the next stage
        /// (if a next stage exists).
        /// </summary>
        public UnityEvent<QuestStage> onQuestStageCompleted = new();

        /// <summary>
        /// Callback when a <see cref="IQuestInteraction"/> is sent by a <see cref="Quest"/> or
        /// <see cref="QuestStage"/>.
        ///
        /// A quest (stage) can define interactions in <see cref="Quest.SendOnCompletion"/> and
        /// <see cref="Quest.SendOnRevocation"/>. Scene objects can listen to this event if they should do something
        /// upon completion of certain quests or quest stages.
        /// </summary>
        public UnityEvent<IQuestInteraction> onInteractionFromQuest = new();

        /// <summary>
        /// Currently active quest line.
        /// </summary>
        public QuestLine ActiveQuestLine { get; private set; }

        private void Start() {
            // TODO only for testing, remove this and initialize the level somewhere else later
            StartCoroutine(PostStartCoroutine());
        }

        /// <summary>
        /// Coroutine that runs after Start to set a test quest line if no quest line was set before.
        /// </summary>
        private IEnumerator PostStartCoroutine() {
            yield return null;
            if (ActiveQuestLine != null) {
                yield break;
            }

            SetQuestLine(QuestLines.TestLevel);
            Debug.Log(
                $"No quest line was set in the QuestManager. For testing purposes, {QuestLines.TestLevel.Name} has been set automatically.",
                this);
        }

        /// <summary>
        /// Changes the currently active <see cref="QuestLine"/> to <paramref name="newQuestLine"/>.
        /// </summary>
        /// <param name="newQuestLine">New quest line</param>
        public void SetQuestLine(QuestLine newQuestLine) {
            ActiveQuestLine = newQuestLine;
            ActiveQuestLine.CurrentStageIndex = 0;
            if (!ActiveQuestLine.IsEmpty) {
                ActivateQuestStage(ActiveQuestLine.CurrentStage);
            }
            onQuestLineChanged.Invoke(ActiveQuestLine);
        }

        /// <summary>
        /// Sends an interaction from a scene object to all quests in the currently active <see cref="QuestStage"/>.
        /// </summary>
        /// <param name="interaction">Interaction to send (see <see cref="QuestInteraction{T}"/>)</param>
        public void SendInteraction(IQuestInteraction interaction) {
            Debug.Log($"Sending interaction to QuestManager: {interaction}", this);
            if (ActiveQuestLine == null) {
                Debug.LogWarning("Interaction sent to QuestManager, but no QuestLine is active.", this);
                return;
            }
            if (ActiveQuestLine.IsEmpty) {
                Debug.LogWarning("Interaction sent to QuestManager, but the active QuestLine is empty.", this);
                return;
            }

            // task here: make lists of all offer targets, i.e. all quests that should receive this interaction
            List<Quest> currentQuests = ActiveQuestLine.CurrentStage.Quests;
            // active quests across ALL stages to allow completing optional quests in previous stages afterwards
            List<Quest> allActiveQuests = ActiveQuestLine.Stages
                .SelectMany(s => s.Quests)
                .Where(q => q.State == QuestState.Active)
                .ToList();
            // quests that were already completed before this interaction in THIS stage to allow revoking them
            List<Quest> previouslyCompletedQuests = currentQuests
                .Where(q => q.State == QuestState.Completed)
                .ToList();

            // for all previously active quests that have now been completed thanks to this interaction
            foreach (var quest in allActiveQuests.Where(quest => OfferInteractionToQuest(quest, interaction))) {
                Debug.Log($"Quest completed: {quest.Name}", this);
                onQuestCompleted.Invoke(quest);
                foreach (var interactionFromQuest in quest.SendOnCompletion) {
                    LogInteraction(quest, interactionFromQuest);
                    onInteractionFromQuest.Invoke(interactionFromQuest);
                }
            }

            // for all previously completed quests that have now been revoked thanks to this interaction
            foreach (var quest in
                     previouslyCompletedQuests.Where(quest => OfferInteractionToQuest(quest, interaction))) {
                Debug.Log($"Quest revoked: {quest.Name}", this);
                onQuestRevoked.Invoke(quest);
                foreach (var interactionFromQuest in quest.SendOnRevocation) {
                    LogInteraction(quest, interactionFromQuest);
                    onInteractionFromQuest.Invoke(interactionFromQuest);
                }
            }

            // complete quest stage if all non-optional quests are completed
            if (currentQuests.TrueForAll(q => q.IsOptional || q.State == QuestState.Completed)) {
                CompleteCurrentStage();
            }
        }

        /// <summary>
        /// Activates the given quest stage by setting all quests to <see cref="QuestState.Active"/> and sending all
        /// <see cref="QuestStage.SendOnActivation"/> interactions.
        /// </summary>
        /// <param name="questStage">Quest stage to activate</param>
        private void ActivateQuestStage(QuestStage questStage) {
            foreach (var quest in questStage.Quests) {
                quest.State = QuestState.Active; // activate all quests of the new stage 
            }
            foreach (var interactionFromStage in questStage.SendOnActivation) {
                LogInteraction(questStage, interactionFromStage);
                onInteractionFromQuest.Invoke(interactionFromStage);
            }
        }

        /// <summary>
        /// Offers <paramref name="interaction"/> to all <see cref="QuestTarget{T}"/> instances of
        /// <paramref name="quest"/>.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the quest state has changed after this interaction, i.e. there are two cases:
        /// 1. <see cref="QuestState.Active"/> before and <see cref="QuestState.Completed"/> after (quest completed)
        /// 2. <see cref="QuestState.Completed"/> before and <see cref="QuestState.Active"/> after (quest revoked)
        /// </returns>
        private bool OfferInteractionToQuest(Quest quest, IQuestInteraction interaction) {
            QuestState oldState = quest.State;
            if (oldState == QuestState.Pending) {
                return false; // only offer interaction to active or completed quests
            }

            foreach (var target in quest.Targets) {
                target.OfferInteraction(interaction);
            }

            quest.State = quest.Targets.TrueForAll(t => t.IsCompleted) ? QuestState.Completed : QuestState.Active;
            return quest.State != oldState; // return true if the state has changed
        }

        /// <summary>
        /// Completes the current stage and increments the stage index.
        /// </summary>
        private void CompleteCurrentStage() {
            QuestStage completedStage = ActiveQuestLine.CurrentStage;
            Debug.Log($"Quest stage completed: {completedStage.Name}", this);

            // increment stage index only if there is a next stage
            if (ActiveQuestLine.CurrentStageIndex < ActiveQuestLine.Stages.Count - 1) {
                ActiveQuestLine.CurrentStageIndex++;
                ActivateQuestStage(ActiveQuestLine.CurrentStage);
            }

            foreach (var interactionFromStage in completedStage.SendOnCompletion) {
                LogInteraction(completedStage, interactionFromStage);
                onInteractionFromQuest.Invoke(interactionFromStage);
            }
            onQuestStageCompleted.Invoke(completedStage);
        }

        private void LogInteraction(Quest quest, IQuestInteraction interaction) {
            Debug.Log($"Interaction returned from quest \"{quest.Name}\": {interaction}", this);
        }

        private void LogInteraction(QuestStage stage, IQuestInteraction interaction) {
            Debug.Log($"Interaction returned from quest stage \"{stage.Name}\": {interaction}", this);
        }
    }
}