namespace AR {
    /// <summary>
    /// State of the AR scene.
    /// </summary>
    public enum ARState {
        /// <summary>
        /// AR scene is not active.
        /// </summary>
        Inactive,

        /// <summary>
        /// AR subsystem is searching for the reference/anchor image (e.g. a QR code).
        /// </summary>
        SearchingReference,

        /// <summary>
        /// User is prompted to align the borders of the AR model with the printed reference borders.
        /// </summary>
        AligningBorders,

        /// <summary>
        /// Reference image was found and AR model is placed.
        /// </summary>
        ModelPlaced
    }
}