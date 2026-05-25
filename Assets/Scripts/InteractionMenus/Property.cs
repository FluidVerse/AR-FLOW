using System;

// ReSharper disable MemberCanBePrivate.Global
namespace InteractionMenus {
    /// <summary>
    /// Property class that holds a value of type <typeparamref name="T"/> and notifies when the value changes.
    ///
    /// This class is used for properties of menu elements (see <see cref="MenuElement"/>). Whenever a property value
    /// of a menu element changes, if this menu element is currently open, then the menu will be recreated to reflect
    /// the new value.
    ///
    /// When changing a value does not require a menu recreation, <see cref="SetSilently"/> can be used.
    /// </summary>
    public class Property<T> : IProperty {

        /// <summary>
        /// Value of the property.
        /// </summary>
        public T Value { get; private set; }

        public event Action<IProperty> OnValueChanged;

        /// <param name="initialValue">Initial value of this property</param>
        public Property(T initialValue) {
            Value = initialValue;
        }

        /// <summary>
        /// Sets the value of the property and notifies listeners.
        /// </summary>
        /// <param name="newValue">New value</param>
        public void Set(T newValue) {
            Value = newValue;
            OnValueChanged?.Invoke(this);
        }

        /// <summary>
        /// Sets the value of the property silently without notifying listeners.
        /// </summary>
        /// <param name="newValue">New value</param>
        public void SetSilently(T newValue) {
            Value = newValue;
        }

        // implicit cast to allow short notations like Property<int> prop = 5;
        public static implicit operator Property<T>(T value) {
            return new Property<T>(value);
        }
    }
}