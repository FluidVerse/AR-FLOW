using UnityEngine;
using UnityEngine.UIElements;

namespace ActionLog {
    /// <summary>
    /// Log item for displaying an action in the action log.
    ///
    /// Primarily used and managed inside the <see cref="ActionLogManager"/>.
    /// </summary>
    [UxmlElement]
    public partial class ActionLogItem : VisualElement {

        /// <summary>
        /// Duration of the slide-in/-out animation in seconds.
        /// </summary>
        public const float AnimationDuration = 0.5f;

        private const string styleResource = "ActionLogItemStyleSheet";
        private const string ussBackground = "action_log_background";
        private const string ussLabelIcon = "action_log_label_icon";
        private const string ussLabelText = "action_log_label_text";
        private const string ussItemVisible = "action_log_item_visible";
        private const string ussItemHidden = "action_log_item_hidden";

        private readonly VisualElement window;
        private readonly Label labelIcon, labelText;

        public ActionLogItem() {
            window = new VisualElement();
            InitializeWindow();

            labelIcon = new Label {
                text = "✅"
            };
            labelIcon.AddToClassList(ussLabelIcon);
            labelIcon.pickingMode = PickingMode.Ignore;
            window.Add(labelIcon);

            labelText = new Label {
                text = "Message..."
            };
            labelText.AddToClassList(ussLabelText);
            labelText.pickingMode = PickingMode.Ignore;
            window.Add(labelText);
        }

        /// <summary>
        /// Fills the newly created action log item with the given data.
        /// </summary>
        /// <param name="message">See <see cref="LogMessage"/></param>
        public void Create(LogMessage message) {
            labelIcon.text = message.icon;
            labelText.text = message.text;
        }

        /// <summary>
        /// Slides the action log item into view.
        /// </summary>
        public void AnimateIn() {
            window.AddToClassList(ussItemVisible);
        }

        /// <summary>
        /// Slides the action log item out of view.
        /// </summary>
        public void AnimateOut() {
            window.RemoveFromClassList(ussItemVisible);
        }

        private void InitializeWindow() {
            hierarchy.Clear();
            styleSheets.Add(Resources.Load<StyleSheet>(styleResource));
            pickingMode = PickingMode.Ignore;
            window.name = "Window";
            window.AddToClassList(ussBackground);
            window.AddToClassList(ussItemHidden); // comment this out to view and edit in editor
            window.pickingMode = PickingMode.Ignore;
            hierarchy.Add(window);
        }
    }
}