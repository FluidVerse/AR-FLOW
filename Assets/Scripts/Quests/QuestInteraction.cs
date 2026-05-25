using System;

namespace Quests {
    /// <summary>
    /// An interaction between objects in the scene and <see cref="Quest"/>s.
    ///
    /// Interactions can work in both ways. Scene objects can send interactions to quests to complete matching
    /// <see cref="QuestTarget"/>s. Vice versa, quests can send interactions to scene objects to notify them to do
    /// something upon completion of a quest.
    /// </summary>
    /// 
    /// <typeparam name="T">
    /// Type of the optional <see cref="value"/> of the interaction. If this interaction does not have any value, use
    /// <c>object</c>.
    /// </typeparam>
    // ReSharper disable ParameterHidesMember
    public readonly struct QuestInteraction<T> : IQuestInteraction {

        private readonly QuestObject obj;
        private readonly QuestInteractionType<T> type;
        private readonly T value;

        /// <param name="obj">Object in the scene to which this interaction refers to</param>
        /// <param name="type">Type of interaction</param>
        /// <param name="value">
        /// Value of this interaction. Can be omitted if this interaction does not have any value.
        /// </param>
        public QuestInteraction(QuestObject obj, QuestInteractionType<T> type, T value = default) {
            this.obj = obj;
            this.type = type;
            this.value = value;
        }

        public bool IsObjectAndType<U>(QuestObject obj, QuestInteractionType<U> type) {
            return this.obj == obj && this.type.Name == type.Name;
        }

        public bool IsObjectAndType<U>(QuestObject obj, QuestInteractionType<U> type, out U outValue) {
            if (!IsObjectAndType(obj, type)) {
                outValue = default;
                return false;
            }

            if (value is null) {
                outValue = default;
            } else if (value is U castValue) {
                outValue = castValue;
            } else {
                throw new InvalidOperationException(
                    $"Value {value} is not of type {typeof(T)}. Actual type: {value.GetType()}");
            }
            return true;
        }

        public override string ToString() {
            string valueString = value is null ? "no value" : value.ToString();
            return $"QuestInteraction<{typeof(T).Name}>: {obj} - {type.Name} - {valueString}";
        }
    }
}