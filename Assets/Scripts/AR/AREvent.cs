namespace AR {
    /// <summary>
    /// Collection of events that can occur in the AR scene and are somehow worth publishing between scripts.
    ///
    /// Example: Controller scripts can publish messages towards the user that the UI will display.
    /// </summary>
    public enum AREvent {
        
        /// <summary>
        /// Position tracking of the anchor image appears unstable.
        /// </summary>
        PositionUnstable
    }
}