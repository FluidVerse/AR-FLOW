namespace ActionLog {
    /// <summary>
    /// Message to be displayed in the action log.
    /// </summary>
    public struct LogMessage {

        /// <summary>
        /// Icon emoji representing the message type (e.g., info, checkmark, ...).
        /// </summary>
        public readonly string icon;

        /// <summary>
        /// Text content of the message.
        /// </summary>
        public readonly string text;

        /// <param name="icon">See <see cref="icon"/></param>
        /// <param name="text">See <see cref="text"/></param>
        public LogMessage(string icon, string text) {
            this.icon = icon;
            this.text = text;
        }
    }
}