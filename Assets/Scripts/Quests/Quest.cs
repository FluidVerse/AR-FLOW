using System.Collections.Generic;

namespace Quests {
    /// <summary>
    /// A quest within a <see cref="QuestStage"/>.
    /// </summary>
    public class Quest {

        /// <summary>
        /// Name of the quest which is displayed in the UI.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Description of the quest which is displayed in the UI.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Whether the quest is optional or not in order to complete the <see cref="QuestStage"/>.
        ///
        /// Only the non-optional quests must be completed to complete the quest stage. However, at least one
        /// non-optional quest is required in each quest stage.
        /// </summary>
        public bool IsOptional { get; }

        /// <summary>
        /// Whether the quest requires a text input field in the UI.
        /// Used for questions that require the user to type an answer.
        /// </summary>
        public bool RequiresInput { get; }

        /// <summary>
        /// Current state of the quest.
        /// </summary>
        public QuestState State { get; set; }

        /// <summary>
        /// List of quest targets to be completed in order to complete the whole quest.
        /// </summary>
        public List<IQuestTarget> Targets { get; }

        /// <summary>
        /// List of interactions to be sent by the quest when the quest is completed.
        ///
        /// Can be used to notify other objects in the scene to do something when this quest is completed. For that,
        /// other scene objects can listen to the <see cref="QuestManager.onInteractionFromQuest"/> event.
        /// List can be empty if no objects in the scene are affected by the completion of this quest.
        /// </summary>
        public List<IQuestInteraction> SendOnCompletion { get; }

        /// <summary>
        /// List of interactions to be sent by the quest when the quest is revoked, i.e. when it is set to not
        /// completed again after it was completed before.
        ///
        /// Can be used to notify other objects in the scene to do something when this quest is revoked. For that,
        /// other scene objects can listen to the <see cref="QuestManager.onInteractionFromQuest"/> event.
        /// List can be empty if no objects in the scene are affected by the completion of this quest.
        /// </summary>
        public List<IQuestInteraction> SendOnRevocation { get; }

        /// <summary>
        /// Optional parameters which have a default value can be omitted when not desired to be set.
        /// </summary>
        /// <param name="name">See <see cref="Name"/></param>
        /// <param name="description">See <see cref="Description"/></param>
        /// <param name="targets">See <see cref="Targets"/></param>
        /// <param name="sendOnCompletion">See <see cref="SendOnCompletion"/>, is <c>null</c>/empty by default</param>
        /// <param name="sendOnRevocation">See <see cref="SendOnRevocation"/>, is <c>null</c>/empty by default</param>
        /// <param name="isOptional">See <see cref="IsOptional"/>, is <c>false</c> by default</param>
        /// <param name="requiresInput">See <see cref="RequiresInput"/>, is <c>false</c> by default</param>
        public Quest(string name, string description, List<IQuestTarget> targets,
            List<IQuestInteraction> sendOnCompletion = null, List<IQuestInteraction> sendOnRevocation = null,
            bool isOptional = false, bool requiresInput = false) {
            Name = name;
            Description = description;
            Targets = targets;
            IsOptional = isOptional;
            RequiresInput = requiresInput;
            SendOnCompletion = sendOnCompletion ?? new List<IQuestInteraction>();
            SendOnRevocation = sendOnRevocation ?? new List<IQuestInteraction>();
            State = QuestState.Pending;
        }
    }
}