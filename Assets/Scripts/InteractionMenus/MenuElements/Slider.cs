using System;
using UnityEngine.UIElements;

// ReSharper disable MemberCanBePrivate.Global
namespace InteractionMenus.MenuElements {
    /// <summary>
    /// Slider with integer values and a <see cref="Label"/> element above it as a title.
    /// </summary>
    public class Slider : MenuElement {

        private const string ussMenuSlider = "MenuSlider";

        /// <summary>
        /// Text for the label above the slider.
        /// </summary>
        public Property<string> Text { get; }

        /// <summary>
        /// Default value of the slider.
        ///
        /// Must be a multiple of <see cref="Step"/>.
        /// </summary>
        public Property<int> DefaultValue { get; }

        /// <summary>
        /// Lowest possible value of the slider.
        ///
        /// Must be a multiple of <see cref="Step"/>.
        /// </summary>
        public Property<int> MinValue { get; }

        /// <summary>
        /// Highest possible value of the slider.
        ///
        /// Must be a multiple of <see cref="Step"/>.
        /// </summary>
        public Property<int> MaxValue { get; }

        /// <summary>
        /// Value to add or remove to the slider value when the slider is moved by one step.
        /// </summary>
        public Property<int> Step { get; }

        /// <summary>
        /// If <c>true</c>, the default value is updated to the slider value when the slider value is changed.
        ///
        /// This way, the slider automatically remembers the last value set by the user and displays it again when the
        /// menu is recreated.
        /// </summary>
        public Property<bool> UpdateDefaultValue { get; }

        /// <summary>
        /// Whether it is possible to click on and interact with the slider.
        ///
        /// If <c>false</c>, the slider appears disabled.
        /// </summary>
        public Property<bool> IsClickable { get; }

        private readonly Action<int> onValueChanged;
        private readonly LabelDigits labelDigits;
        private readonly Func<int, string> valueFormatter;

        /// <summary>
        /// Converts <see cref="labelDigits"/> to the width (px) of the label next to the slider.
        /// </summary>
        private int LabelDigitsPx => labelDigits switch {
            LabelDigits.Two => 64,
            LabelDigits.Three => 96,
            LabelDigits.Four => 128,
            LabelDigits.Five => 160,
            LabelDigits.Six => 192,
            _ => throw new ArgumentOutOfRangeException(nameof(labelDigits), labelDigits, null)
        };

        /// <param name="text">See <see cref="Text"/></param>
        /// <param name="onValueChanged">Callback when the slider value is changed, can be <c>null</c></param>
        /// <param name="defaultValue">See <see cref="DefaultValue"/>. Default value: <c>0</c></param>
        /// <param name="minValue">See <see cref="MinValue"/>. Default value: <c>0</c></param>
        /// <param name="maxValue">See <see cref="MaxValue"/>. Default value: <c>100</c></param>
        /// <param name="step">See <see cref="Step"/>. Default value: <c>1</c></param>
        /// <param name="updateDefaultValue">See <see cref="UpdateDefaultValue"/>. Default value: <c>true</c></param>
        /// <param name="labelDigits">
        /// Sets the label width to a fixed value based on the desired number of digits that should be displayed.
        /// </param>
        /// <param name="isClickable">See <see cref="IsClickable"/>. Default value: <c>true</c></param>
        /// <param name="valueFormatter">
        /// Formatter to convert the slider value to a string. Can be used to modify how the slider value is displayed
        /// in the label next to the slider. If <c>null</c>, the default <c>ToString()</c> method will be used
        /// </param>
        public Slider(
            string text,
            Action<int> onValueChanged = null,
            int defaultValue = 0,
            int minValue = 0,
            int maxValue = 100,
            int step = 1,
            bool updateDefaultValue = true,
            LabelDigits labelDigits = LabelDigits.Six,
            bool isClickable = true,
            Func<int, string> valueFormatter = null
        ) {
            Text = text;
            this.onValueChanged = onValueChanged ?? (_ => { });
            DefaultValue = defaultValue;
            MinValue = minValue;
            MaxValue = maxValue;
            Step = step;
            UpdateDefaultValue = updateDefaultValue;
            this.labelDigits = labelDigits;
            IsClickable = isClickable;
            this.valueFormatter = valueFormatter ?? (v => v.ToString());
        }

        protected override IProperty[] ChildProperties => new IProperty[] {
            Text, DefaultValue, MinValue, MaxValue, Step, UpdateDefaultValue, IsClickable
        };

        public override VisualElement CreateElement() {
            VisualElement container = new();
            Label label = new(Text.Value);
            container.Add(label.CreateElement());

            SliderInt slider = new WideHitboxSliderInt {
                lowValue = MinValue.Value / Step.Value,
                highValue = MaxValue.Value / Step.Value,
                pageSize = 1,
                value = DefaultValue.Value / Step.Value,
                label = valueFormatter(DefaultValue.Value)
            };

            slider.RegisterValueChangedCallback(evt => {
                int actualValue = evt.newValue * Step.Value;
                slider.label = valueFormatter(actualValue);
                if (UpdateDefaultValue.Value) {
                    DefaultValue.SetSilently(actualValue);
                }
                onValueChanged(actualValue);
            });
            slider.SetEnabled(IsClickable.Value);
            slider.AddToClassList(ussMenuElementGeneral);
            slider.AddToClassList(ussMenuSlider);
            // comment out when MenuElements.Slider uses the new WideHitboxSlider
            // slider.Q<VisualElement>("unity-dragger").AddToClassList("MenuSliderDragger");

            var sliderLabel = slider.Q<UnityEngine.UIElements.Label>();
            sliderLabel.style.minWidth = 0;
            sliderLabel.style.width = LabelDigitsPx;

            container.Add(slider);
            return container;
        }
    }
}