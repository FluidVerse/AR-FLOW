namespace Quests {
    /// <summary>
    /// Static class to hold all the <see cref="QuestInteractionType{T}"/> instances in the game.
    ///
    /// New interaction types should be added here to keep them centralized. When adding new interaction types,
    /// <see cref="QuestInteractionType{T}.Name"/> must be unique.
    /// </summary>
    public static class QuestInteractionTypes {

        public static QuestInteractionType<object> UseDetailView = new("UseDetailView");
        public static QuestInteractionType<int> SetValvePosition = new("SetValvePosition");
        public static QuestInteractionType<int> SetMotorSpeed = new("SetMotorSpeed");
        public static QuestInteractionType<object> StartMotor = new("StartMotor");
        public static QuestInteractionType<object> OpenInteractionMenu = new("OpenInteractionMenu");
        public static QuestInteractionType<object> BlockMotorSpeed = new("BlockMotorSpeed");
        public static QuestInteractionType<object> BlockValvePosition = new("BlockValvePosition");
        public static QuestInteractionType<object> UnblockMotorSpeed = new("UnblockMotorSpeed");
        public static QuestInteractionType<object> UnblockValvePosition = new("UnblockValvePosition");
        public static QuestInteractionType<object> StartProgram1 = new("StartProgram1");
        public static QuestInteractionType<object> StartProgram2 = new("StartProgram2");
        public static QuestInteractionType<object> ClickedCorrectly = new("ClickedCorrectly");
        public static QuestInteractionType<float> SetReynoldsNumber = new("SetReynoldsNumber");
        public static QuestInteractionType<string> AnsweredCorrectly = new("AnsweredCorrectly");
    }
}