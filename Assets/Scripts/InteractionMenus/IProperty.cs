using System;

namespace InteractionMenus {
    /// <summary>
    /// Interface for properties that can notify when their value changes.
    /// </summary>
    public interface IProperty {

        /// <summary>
        /// Callback when the value of the property changes.
        /// </summary>
        public event Action<IProperty> OnValueChanged;
    }
}