namespace Quests {
    /// <summary>
    /// Represents the state of a <see cref="Quest"/>.
    /// </summary>
    public enum QuestState {
        /// <summary>
        /// Quest is not active yet and cannot receive any interactions.
        /// </summary>
        Pending,

        /// <summary>
        /// Quest is active and can receive interactions.
        /// </summary>
        Active,

        /// <summary>
        /// Quest is completed because all of its targets (see <see cref="QuestTarget{T}"/>) are completed.
        /// </summary>
        Completed
    }
}