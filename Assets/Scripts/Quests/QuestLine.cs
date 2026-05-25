using System.Collections.Generic;

namespace Quests {
    /// <summary>
    /// A quest line (or level) that consists of one or multiple <see cref="QuestStage"/>s.
    ///
    /// If a quest line has zero stages, it is considered an empty quest line that only exists to display 
    /// <see cref="Name"/> and <see cref="Description"/> in the UI. In this case, it is highly advised to hide the
    /// quests button in the toolbar.
    /// </summary>
    public class QuestLine {

        /// <summary>
        /// Name of the quest line which is displayed in the UI.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Description of the quest line which is displayed in the level info box.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// List of quest stages that are part of this quest line in exactly this order.
        /// </summary>
        public List<QuestStage> Stages { get; }

        /// <summary>
        /// Index of the currently active quest stage in the <see cref="Stages"/> list.
        /// </summary>
        public int CurrentStageIndex { get; set; }

        /// <summary>
        /// Currently active quest stage.
        /// </summary>
        public QuestStage CurrentStage => Stages[CurrentStageIndex];

        /// <summary>
        /// Whether this quest line is empty, i.e. has zero stages.
        /// </summary>
        public bool IsEmpty => Stages.Count == 0;

        /// <param name="name">See <see cref="Name"/></param>
        /// <param name="description">See <see cref="Description"/></param>
        /// <param name="stages">See <see cref="Stages"/></param>
        public QuestLine(string name, string description, List<QuestStage> stages) {
            Name = name;
            Description = description;
            Stages = stages;
        }
    }
}