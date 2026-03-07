using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

// ReSharper disable MemberCanBePrivate.Global
namespace InteractionMenus.MenuElements {
    /// <summary>
    /// A <see cref="MenuElements.Slider"/> instance with a button below it.
    ///
    /// Callback for the button click passes the current slider value as an argument.
    /// </summary>
    public class SliderWithButton : MenuElement {

        /// <summary>
        /// Slider.
        /// </summary>
        public Slider Slider { get; }

        /// <summary>
        /// Button below the slider.
        /// </summary>
        public Button Button { get; }

        /// <summary>
        /// If <c>true</c>, the default value is updated to the slider value when the button is clicked.
        ///
        /// This way, the slider automatically remembers the last value set by the user and displays it again when the
        /// menu is recreated. If set to <c>true</c>, the underlying <see cref="Slider.UpdateDefaultValue"/> is
        /// automatically set to <c>false</c> to prevent the slider from updating its default value when the slider is
        /// just moving without clicking the button.
        /// </summary>
        public Property<bool> UpdateDefaultValue { get; }

        /// <summary>
        /// Whether it is possible to click on and interact with the slider.
        ///
        /// If <c>false</c>, the slider appears disabled. This property overrides both <see cref="Slider.IsClickable"/>
        /// and <see cref="Button.IsClickable"/>, so these two sub-elements cannot be disabled independently.
        /// </summary>
        public Property<bool> IsClickable { get; }

        private readonly Action<int> onButtonClick;

        /// <param name="slider">See <see cref="Slider"/></param>
        /// <param name="button">See <see cref="Button"/></param>
        /// <param name="onButtonClick">
        /// Callback when the button is clicked, passes the current slider value as an argument, can be <c>null</c>
        /// </param>
        /// <param name="updateDefaultValue">See <see cref="UpdateDefaultValue"/>. Default value: <c>true</c></param>
        /// <param name="isClickable">See <see cref="IsClickable"/>. Default value: <c>true</c></param>
        public SliderWithButton(Slider slider, Button button, Action<int> onButtonClick = null,
            bool updateDefaultValue = true, bool isClickable = true) {
            Slider = slider;
            Button = button;
            this.onButtonClick = onButtonClick ?? (_ => { });
            UpdateDefaultValue = updateDefaultValue;
            IsClickable = isClickable;
        }

        protected override IProperty[] ChildProperties {
            get {
                List<IProperty> properties = new();
                properties.AddRange(Slider.Properties);
                properties.AddRange(Button.Properties);
                properties.Add(UpdateDefaultValue);
                properties.Add(IsClickable);
                return properties.ToArray();
            }
        }

        public override VisualElement CreateElement() {
            VisualElement container = new VisualElement();

            if (UpdateDefaultValue.Value) {
                Slider.UpdateDefaultValue.SetSilently(false);
            }
            Slider.IsClickable.SetSilently(IsClickable.Value);
            Button.IsClickable.SetSilently(IsClickable.Value);

            VisualElement sliderContainer = Slider.CreateElement();
            var sliderElement = sliderContainer.Q<SliderInt>();
            container.Add(sliderContainer);

            VisualElement buttonContainer = Button.CreateElement();
            var buttonElement = buttonContainer.Q<UnityEngine.UIElements.Button>();
            buttonElement.clicked += () => {
                int actualValue = sliderElement.value * Slider.Step.Value;
                if (UpdateDefaultValue.Value) {
                    Slider.DefaultValue.SetSilently(actualValue);
                }
                onButtonClick(actualValue);
            };
            container.Add(buttonContainer);
            return container;
        }
    }
}