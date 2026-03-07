namespace Quests {
    /// <summary>
    /// Interface for an interaction between scene objects and quests.
    ///
    /// See <see cref="QuestInteraction{T}"/> for the specific implementation.
    /// </summary>
    public interface IQuestInteraction {
        
        /// <summary>
        /// Checks whether this interaction refers to the same scene object and interaction type as passed in the
        /// parameters. 
        /// </summary>
        /// <param name="obj">Scene object to compare to</param>
        /// <param name="type">Interaction type to compare to</param>
        /// <returns>
        /// <c>true</c> if the scene object and interaction type are both equal, otherwise <c>false</c>
        /// </returns>
        public bool IsObjectAndType<U>(QuestObject obj, QuestInteractionType<U> type);
        
        /// <summary>
        /// Checks whether this interaction refers to the same scene object and interaction type as passed in the
        /// parameters. If yes, then <paramref name="outValue"/> is set to the value of this interaction.
        /// </summary>
        /// <param name="obj">Scene object to compare to</param>
        /// <param name="type">Interaction type to compare to</param>
        /// <param name="outValue">Value of this interaction if the check is successful</param>
        /// <returns>
        /// <c>true</c> if the scene object and interaction type are both equal, otherwise <c>false</c>
        /// </returns>
        public bool IsObjectAndType<U>(QuestObject obj, QuestInteractionType<U> type, out U outValue);
    }
}