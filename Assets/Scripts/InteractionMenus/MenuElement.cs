using System;
using UnityEngine.UIElements;

namespace InteractionMenus {
    /// <summary>
    /// Menu element that can be displayed in an <see cref="InteractionMenu"/>.
    /// </summary>
    public abstract class MenuElement {

        /// <summary>
        /// USS class name for the general menu element style.
        /// </summary>
        protected const string ussMenuElementGeneral = "MenuElementGeneral";

        /// <summary>
        /// Whether this menu element is active and should be displayed in the menu.
        /// </summary>
        // ReSharper disable once ReplaceAutoPropertyWithComputedProperty
        public Property<bool> IsActive { get; } = true;

        /// <summary>
        /// Returns an array of all properties (see <see cref="Property{T}"/>) that this menu element contains.
        /// </summary>
        public IProperty[] Properties {
            get {
                // join all properties in this abstract class with all properties in the child class
                IProperty[] childProperties = ChildProperties;
                IProperty[] allProperties = new IProperty[childProperties.Length + 1];
                allProperties[0] = IsActive;
                Array.Copy(childProperties, 0, allProperties, 1, childProperties.Length);
                return allProperties;
            }
        }

        /// <summary>
        /// Returns an array of all properties (see <see cref="Property{T}"/>) that the child class (which extends
        /// <see cref="MenuElement"/>) contains.
        /// </summary>
        protected abstract IProperty[] ChildProperties { get; }

        /// <summary>
        /// Creates and returns a <see cref="VisualElement"/> instance that represents this menu element.
        /// </summary>
        public abstract VisualElement CreateElement();
    }
}