using UnityEngine.UIElements;

// ReSharper disable MemberCanBePrivate.Global
namespace InteractionMenus.MenuElements {
    /// <summary>
    /// Label that displays a text.
    /// </summary>
    public class Label : MenuElement {

        private const string ussMenuLabel = "MenuLabel";

        /// <summary>
        /// Displayed text.
        /// </summary>
        public Property<string> Text { get; }

        /// <param name="text">See <see cref="Text"/></param>
        public Label(string text) {
            Text = text;
        }

        protected override IProperty[] ChildProperties => new IProperty[] { Text };

        public override VisualElement CreateElement() {
            UnityEngine.UIElements.Label label = new() {
                text = Text.Value
            };
            label.AddToClassList(ussMenuElementGeneral);
            label.AddToClassList(ussMenuLabel);
            return label;
        }
    }
}