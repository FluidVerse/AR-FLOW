using System;

namespace Quests {
    /// <summary>
    /// A target of a <see cref="Quest"/>.
    ///
    /// Each quest target reacts to a specific tuple of <see cref="QuestObject"/> and
    /// <see cref="QuestInteractionType{T}"/>. If the target receives an interaction of exactly that scene object and
    /// interaction type, it will check if the interaction value is accepted by the target (see
    /// <see cref="isValueAccepted"/>). If the value is accepted, the target is marked as completed. However, if the
    /// target receives an interaction afterwards where <see cref="isValueAccepted"/> evaluates to <c>false</c>, the
    /// target completion can be revoked again.
    ///
    /// If the target does not care about the exact interaction value or if the specified
    /// <see cref="QuestInteractionType{T}"/> does not pass any values, <see cref="isValueAccepted"/> can be set so
    /// that it always returns <c>true</c>, i.e. that every value is accepted.
    /// </summary>
    /// 
    /// <typeparam name="T">
    /// Type of the <see cref="QuestInteraction{T}.value"/> of the desired <see cref="QuestInteraction{T}"/> type.
    /// If this interaction does not have any value, use<c>object</c>.
    /// </typeparam>
    public class QuestTarget<T> : IQuestTarget {

        private static readonly Predicate<T> alwaysTrue = _ => true;

        public bool IsCompleted { get; private set; }

        private readonly QuestObject questObject;
        private readonly QuestInteractionType<T> interactionType;
        private readonly Predicate<T> isValueAccepted;

        /// <param name="questObject">Object in the scene that this target reacts to</param>
        /// <param name="interactionType">Interaction type that this target reacts to</param>
        /// <param name="isValueAccepted">
        /// Predicate to determine whether the value passed in an interaction is accepted by this target.
        /// Can be left out if the target does not care about the exact value.
        /// </param>
        public QuestTarget(QuestObject questObject, QuestInteractionType<T> interactionType,
            Predicate<T> isValueAccepted = null) {
            this.questObject = questObject;
            this.interactionType = interactionType;
            this.isValueAccepted = isValueAccepted ?? alwaysTrue;
        }

        public void OfferInteraction(IQuestInteraction interaction) {
            if (interaction.IsObjectAndType(questObject, interactionType, out T value)) {
                IsCompleted = isValueAccepted(value);
            }
        }
    }
}