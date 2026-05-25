using static Toolbar.ToolbarButton;

namespace Toolbar {
    /// <summary>
    /// Collection of frequently used groups of <see cref="ToolbarButton"/>s that belong together.
    ///
    /// Can be used for methods like <see cref="ToolbarManager.SetButtonVisibility(ToolbarButton[], bool)"/> or
    /// <see cref="ToolbarManager.SetButtonEnabled(ToolbarButton[], bool, bool)"/> to show/hide a list of buttons that
    /// belong together at once.
    /// </summary>
    public static class ToolbarButtonGroups {

        /// <summary>
        /// All buttons in the actual toolbar in the bottom right corner.
        /// </summary>
        public static readonly ToolbarButton[] BottomRow = {
            QuestMenu,
            ToolbarButton.MainMenu,
            Back,
            Check,
            ToolbarButton.AR,
            Graph,
            Info,
            Function,
        };

        /// <summary>
        /// All LevelObjects buttons in the upper left corner.
        /// </summary>
        public static readonly ToolbarButton[] LevelObjects = {
            LevelObjects1,
            LevelObjects2,
            LevelObjects3,
            LevelObjects4,
            LevelObjects5,
            LevelObjects6,
            LevelObjects7,
            LevelObjects8,
            LevelObjects9,
            LevelObjects10,
            LevelObjects11,
            LevelObjects12
        };

        /// <summary>
        /// All field buttons.
        /// </summary>
        public static readonly ToolbarButton[] ButtonFields = {
            ButtonField,
            ButtonFieldA,
            ButtonFieldB,
            ButtonFieldC
        };
        
        /// <summary>
        /// All field buttons except the one that is behind the camera button in the top right corner (ButtonField).
        /// </summary>
        public static readonly ToolbarButton[] ButtonFieldABC = {
            ButtonFieldA,
            ButtonFieldB,
            ButtonFieldC
        };
    }
}