using System;
using UnityEngine.UIElements;

// ReSharper disable MemberCanBePrivate.Global
namespace InteractionMenus.MenuElements {
    /// <summary>
    /// Label that displays a prefixed text and a value of a parameter with type <typeparamref name="T"/> after it.
    /// </summary>
    /// <typeparam name="T">Type of the displayed parameter value</typeparam>
    public class ParameterLabel<T> : MenuElement {

        /// <summary>
        /// Prefixed text that is displayed before the value of the parameter.
        /// </summary>
        public Property<string> Prefix { get; }
        
        /// <summary>
        /// Displayed parameter value.
        /// </summary>
        public Property<T> Value { get; }

        private readonly Func<T, string> valueFormatter;

        /// <param name="prefix">See <see cref="Prefix"/></param>
        /// <param name="value">See <see cref="Value"/></param>
        /// <param name="valueFormatter">
        /// Formatter to convert the value to a string. Can be used to specify decimal places of <c>float</c>s or how
        /// to display complex types in general. If <c>null</c>, the default <c>ToString()</c> method will be used
        /// </param>
        public ParameterLabel(string prefix, T value, Func<T, string> valueFormatter = null) {
            Prefix = prefix;
            Value = value;
            this.valueFormatter = valueFormatter ?? (v => v?.ToString() ?? "null");
        }

        protected override IProperty[] ChildProperties => new IProperty[] { Prefix, Value };

        public override VisualElement CreateElement() {
            var unityLabel = new UnityEngine.UIElements.Label($"{Prefix.Value}: {valueFormatter(Value.Value)}");
            unityLabel.AddToClassList("MenuLabel");
            unityLabel.AddToClassList("MenuElementGeneral");
            return unityLabel;
        }
    }
}