namespace ActionLog {
    /// <summary>
    /// Collection of predefined log messages.
    ///
    /// If possible, always define log messages here to ensure consistency, either using a method (if parameters are
    /// needed) or as a <c>readonly</c> constant.
    /// </summary>
    public static class LogMessages {

        public static readonly LogMessage GraphZoomReset = new("🔍", "reset Graph-Zsom");
        public static readonly LogMessage MotorStarted = new("🔧", "Motor started");
        public static readonly LogMessage MotorStopped = new("🔧", "Motor stopped");
        public static readonly LogMessage WashingMachineProgram1Started = new("🔧", "Program 1 started");
        public static readonly LogMessage WashingMachineProgram2Started = new("🔧", "Program 2 started");
        public static readonly LogMessage WashingMachineStopped = new("🔧", "Washing machine stopped");
        public static readonly LogMessage MoodyDiagramClickedCorrectly = new("🎯", $"Richtig!");
        public static readonly LogMessage MoodyDiagramClickedIncorrectly = new("❌", "Leider falsch...");

        public static readonly LogMessage ChoseToolUniformFlow =
            new("🔧", "Please set x and y velocity component and place object by tapping.");

        public static readonly LogMessage ChoseToolHeatSink =
            new("🔧", $"Please set source intensity and place object by tapping.");

        public static readonly LogMessage ChoseToolDipole =
            new("🔧", "Please set dipole intensity and place object by tapping.");

        public static readonly LogMessage ChoseToolVortex =
            new("🔧", "Please set vortex intensity and place object by tapping.");

        public static readonly LogMessage ChoseToolMoveElement = new("🔧", "Tap on object to move object.");
        public static readonly LogMessage ChoseToolRemoveElement = new("🔧", "Tap on object to delete object.");

        public static readonly LogMessage ChoseToolCylinder =
            new("🔧", "Please set cylinder radius and place object by tapping.");

        public static readonly LogMessage
            ChangeTo2DView = new("ℹ️", "Changed to 2D view. You can edit the flow field.");

        public static readonly LogMessage ChangeTo3DView =
            new("ℹ️", "Changed to 3D view. Tap and drag to rotate the view. Pinch to zoom.");

        public static readonly LogMessage ChoseProbe = new("ℹ️", "Probe Mode: Measure Bernoulli-Number");

        public static LogMessage QuestCompleted(string questName) {
            return new LogMessage("✅", $"Quest completed: {questName}");
        }

        public static LogMessage QuestRevoked(string questName) {
            return new LogMessage("⚠️", $"Quest reset: {questName}");
        }

        public static LogMessage QuestStageCompleted(string stageName) {
            return new LogMessage("🏆", $"Quest line completed: {stageName}");
        }

        public static LogMessage AnimationActivated(string objectName) {
            return new LogMessage("▶️", $"Animation activated: {objectName}");
        }

        public static LogMessage AnimationDeactivated(string objectName) {
            return new LogMessage("⏸️", $"Animation deactivated: {objectName}");
        }

        public static LogMessage AnimationReset(string objectName) {
            return new LogMessage("🔄", $"Animation reset: {objectName}");
        }

        public static LogMessage TransparencyEnabled(string objectName) {
            return new LogMessage("👓", $"Transparency activated: {objectName}");
        }

        public static LogMessage TransparencyDisabled(string objectName) {
            return new LogMessage("👓", $"Transparency deactivated: {objectName}");
        }

        public static LogMessage MotorSpeedSet(int speed) {
            return new LogMessage("🔧", $"Changed motor speed: {speed} 1/min");
        }

        public static LogMessage ValvePositionSet(int position) {
            return new LogMessage("🔧", $"Changed valve position: {position} %");
        }

        public static LogMessage ChangeCameraView(string viewName) {
            return new LogMessage("📷", $"Changed camera view to {viewName}");
        }
    }
}