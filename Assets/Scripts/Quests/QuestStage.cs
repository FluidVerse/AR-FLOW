using System.Collections.Generic;

namespace Quests {
    /// <summary>
    /// A quest stage within a <see cref="QuestLine"/> which contains a list of <see cref="Quest"/>s.
    /// </summary>
    public class QuestStage {

        /// <summary>
        /// Name of the quest stage which is displayed in the UI.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Description of the quest stage which is displayed in the UI.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// List of quests to be completed in order to complete the quest stage.
        ///
        /// Only the non-optional quests must be completed to complete the quest stage (see
        /// <see cref="Quest.IsOptional"/>). However, at least one non-optional quest is required in each quest stage.
        /// </summary>
        public List<Quest> Quests { get; }

        /// <summary>
        /// List of interactions to be sent by the quest stage when the quest stage is activated.
        ///
        /// Can be used to notify other objects in the scene to do something when this quest stage is activated. For
        /// that, other scene objects can listen to the <see cref="QuestManager.onInteractionFromQuest"/> event.
        /// List can be empty if no objects in the scene are affected by the activation of this quest stage.
        ///
        /// <b><i>Note that this list should not be used inside the first quest stage of a quest line.</i></b> If a new
        /// quest line is loaded immediately, the interactions may be sent before the subscribed interaction objects
        /// properly initialized. Therefore, it is likely that the subscribed interaction objects will simply miss the
        /// interaction.
        /// </summary>
        public List<IQuestInteraction> SendOnActivation { get; }

        /// <summary>
        /// List of interactions to be sent by the quest stage when the quest stage is completed.
        ///
        /// Can be used to notify other objects in the scene to do something when this quest stage is completed. For
        /// that, other scene objects can listen to the <see cref="QuestManager.onInteractionFromQuest"/> event.
        /// List can be empty if no objects in the scene are affected by the completion of this quest stage.
        /// </summary>
        public List<IQuestInteraction> SendOnCompletion { get; }

        /// <param name="name">See <see cref="Name"/></param>
        /// <param name="description">See <see cref="Description"/></param>
        /// <param name="quests">See <see cref="Quests"/></param>
        /// <param name="sendOnActivation">See <see cref="SendOnActivation"/>, is <c>null</c>/empty by default</param>
        /// <param name="sendOnCompletion">See <see cref="SendOnCompletion"/>, is <c>null</c>/empty by default</param>
        public QuestStage(string name, string description, List<Quest> quests,
            List<IQuestInteraction> sendOnActivation = null, List<IQuestInteraction> sendOnCompletion = null) {
            Name = name;
            Description = description;
            Quests = quests;
            SendOnActivation = sendOnActivation ?? new List<IQuestInteraction>();
            SendOnCompletion = sendOnCompletion ?? new List<IQuestInteraction>();
        }
    }
}