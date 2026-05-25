namespace Toolbar {
    /// <summary>
    /// Collection of frequently used groups of <see cref="ToolbarArea"/>s that belong together.
    ///
    /// CCan be used for methods like <see cref="ToolbarManager.SetAreaVisibility(ToolbarArea[], bool)"/>  to show/hide
    /// a list of areas that belong together at once.
    /// </summary>
    public static class ToolbarAreaGroups {

        /// <summary>
        /// <see cref="ToolbarArea.Property1"/> and <see cref="ToolbarArea.Property2"/>.
        /// </summary>
        public static readonly ToolbarArea[] Properties = {
            ToolbarArea.Property1,
            ToolbarArea.Property2,
        };
    }
}