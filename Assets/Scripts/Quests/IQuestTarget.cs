namespace Quests {
    /// <summary>
    /// Interface for a target of a quest.
    ///
    /// See <see cref="QuestTarget{T}"/> for the specific implementation.
    /// </summary>
    public interface IQuestTarget {

        /// <summary>
        /// Whether the target is completed.
        /// </summary>
        public bool IsCompleted { get; }

        /// <summary>
        /// Offers an interaction to this target and completes it if the interaction value is accepted, or revokes it
        /// if the interaction value is not denied.
        /// </summary>
        public void OfferInteraction(IQuestInteraction interaction);
    }
}