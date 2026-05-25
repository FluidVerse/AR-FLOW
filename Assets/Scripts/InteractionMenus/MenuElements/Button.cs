using System;
using UnityEngine.UIElements;

// ReSharper disable MemberCanBePrivate.Global
namespace InteractionMenus.MenuElements {
    /// <summary>
    /// Button with a text inside it.
    /// </summary>
    public class Button : MenuElement {

        private const string ussMenuButton = "MenuButton";

        /// <summary>
        /// Text inside the button.
        /// </summary>
        public Property<string> Text { get; }

        /// <summary>
        /// Whether it is possible to click on the button.
        ///
        /// If <c>false</c>, the button appears disabled.
        /// </summary>
        public Property<bool> IsClickable { get; }

        private readonly Action onButtonClick;

        /// <param name="text">See <see cref="Text"/></param>
        /// <param name="onButtonClick">Callback when the button is clicked, can be <c>null</c></param>
        /// <param name="isClickable">See <see cref="IsClickable"/>. Default value: <c>true</c></param>
        public Button(string text, Action onButtonClick = null, bool isClickable = true) {
            Text = text;
            this.onButtonClick = onButtonClick ?? (() => { });
            IsClickable = isClickable;
        }

        protected override IProperty[] ChildProperties => new IProperty[] { Text, IsClickable };

        public override VisualElement CreateElement() {
            UnityEngine.UIElements.Button button = new() {
                text = Text.Value
            };
            button.SetEnabled(IsClickable.Value);
            button.AddToClassList(ussMenuElementGeneral);
            button.AddToClassList(ussMenuButton);
            button.clicked += onButtonClick;
            return button;
        }
    }
}