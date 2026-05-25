namespace Quests {
    /// <summary>
    /// Type of quest interaction (e.g. "UseDetailView").
    /// </summary>
    /// 
    /// <typeparam name="T">
    /// Type of the <see cref="QuestInteraction{T}.value"/> of an <see cref="QuestInteraction{T}"/> of this type.
    /// If this interaction does not have any value, use<c>object</c>.
    /// </typeparam>
    // ReSharper disable once UnusedTypeParameter
    public readonly struct QuestInteractionType<T> {

        /// <summary>
        /// Name of the quest interaction type (e.g. "UseDetailView").
        ///
        /// Must be unique for each interaction type in the game.
        /// </summary>
        public string Name { get; }

        /// <param name="name">See <see cref="Name"/></param>
        public QuestInteractionType(string name) {
            Name = name;
        }
    }
}