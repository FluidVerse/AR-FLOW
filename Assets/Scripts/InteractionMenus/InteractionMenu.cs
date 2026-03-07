using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace InteractionMenus {
    /// <summary>
    /// Menu for displaying a list of interactions, i.e. a list of <see cref="MenuElement"/>.
    ///
    /// The lifecycle of such a menu is managed by the <see cref="InteractionMenuManager"/>.
    /// 
    /// To display this menu in Unity UI Builder for development purposes, comment out the marked line in
    /// <see cref="InitializeWindow"/>.
    /// </summary>
    public class InteractionMenu : VisualElement {
        [UnityEngine.Scripting.Preserve]
        public new class UxmlFactory : UxmlFactory<InteractionMenu> {
        }

        /// <summary>
        /// Smoothing factor for the y position change, i.e. how quickly the menu falls down from the top.
        /// </summary>
        private const float yPositionSmoothFactor = 20f;

        /// <summary>
        /// Bottom margin (px) of the menu to prevent it being hidden behind the toolbar.
        /// </summary>
        private const int bottomMargin = 205;

        private const string styleResource = "InteractionMenuStyleSheet";
        private const string ussInteractionMenu = "InteractionMenu";

        /// <summary>
        /// Whether the menu is currently displayed.
        /// </summary>
        public bool IsDisplayed { get; private set; }

        private VisualElement window;

        public InteractionMenu() {
            styleSheets.Add(Resources.Load<StyleSheet>(styleResource));

            InitializeWindow();

            // example elements for testing purposes (displayed in the unity editor), not used inside the game
            AddElementToWindow(new MenuElements.Label("TestLabel"));
            AddElementToWindow(new MenuElements.Button("TestButton", () => { }));
            AddElementToWindow(new MenuElements.Slider("TestSlider", _ => { }, 500000));
        }

        /// <summary>
        /// Checks whether the mouse is currently over the menu.
        /// </summary>
        /// <param name="mousePosition">Current mouse position</param>
        public bool MouseOver(Vector2 mousePosition) {
            if (window == null) {
                return false;
            }

            // Lokale Koordinaten des window in globale Koordinaten transformieren
            Rect globalPos = window.LocalToWorld(window.contentRect);

            // y-Achse invertieren und die H�he von window verschieben
            globalPos.y = Screen.height - globalPos.y - globalPos.height;

            //Debug.Log("globalPos = " + globalPos.x.ToString() + "," + globalPos.y.ToString() + "," + globalPos.width.ToString() + "," + globalPos.height.ToString());
            //Debug.Log("mousePosition = " + mousePosition.x.ToString() + "," + mousePosition.y.ToString());
            return globalPos.Contains(mousePosition);
        }

        /// <summary>
        /// Creates and displays the menu with the given title and elements.
        ///
        /// The menu title is displayed as a normal <see cref="MenuElements.Label"/> at the top of the menu. Below this
        /// label, the menu elements are displayed in the order they are provided.
        /// </summary>
        /// <param name="menuTitle">Menu title</param>
        /// <param name="menuElements">List of menu elements</param>
        /// <param name="isAnimated">
        /// If <c>true</c>, the menu creation will be animated, i.e. it will fall down from the top of the screen
        /// </param>
        public void Create(string menuTitle, List<MenuElement> menuElements, bool isAnimated) {
            window.Clear();
            AddElementToWindow(new MenuElements.Label(menuTitle)); // add title as first element
            foreach (var element in menuElements) {
                if (element.IsActive.Value) {
                    AddElementToWindow(element); // add all other elements, but only active elements
                }
            }

            // Put window to the top to prepare fall down animation due to InteractionMenuManager.UpdateMenuPosition()
            if (isAnimated) {
                Vector3 windowPos = window.transform.position;
                windowPos.y = 0;
                window.transform.position = windowPos;
            }

            style.display = DisplayStyle.Flex;
            IsDisplayed = true;
            Debug.Log($"InteractionMenu created with {menuElements.Count} elements");
        }

        /// <summary>
        /// Deletes the menu and clears all elements.
        /// </summary>
        public void Delete() {
            if (!IsDisplayed) {
                return;
            }

            window.Clear();
            style.display = DisplayStyle.None;
            IsDisplayed = false;
            Debug.Log("InteractionMenu deleted");
        }

        /// <summary>
        /// Sets the position of the menu window.
        /// </summary>
        /// <param name="x">X coordinate on the screen</param>
        /// <param name="y">Y coordinate on the screen (handled as <c>Screen.height - y</c>)</param>
        /// <param name="smoothY">
        /// If <c>true</c>, the Y position change is smoothed by <see cref="yPositionSmoothFactor"/>
        /// </param>
        public void SetPosition(float x, float y, bool smoothY = false) {
            // ensure the menu is fully visible on the screen (push it left and/or down if too tall)
            float xPosition = Mathf.Clamp(x, 0, Screen.width - window.resolvedStyle.width);
            float yPosition = Mathf.Clamp(Screen.height - y, window.resolvedStyle.height, Screen.height - bottomMargin);
            if (smoothY) {
                // smooth the y position to avoid flickering
                yPosition = Mathf.Lerp(window.transform.position.y, yPosition, yPositionSmoothFactor * Time.deltaTime);
            }
            window.transform.position = new Vector3(xPosition, yPosition, 0);
        }

        private void InitializeWindow() {
            hierarchy.Clear();
            window = new VisualElement();
            window.AddToClassList(ussInteractionMenu);
            window.name = "Window";
            window.transform.position = Vector3.zero;
            hierarchy.Add(window);
            style.display = DisplayStyle.None; // comment this to display in Unity editor (for editing and testing)
        }

        private void AddElementToWindow(MenuElement menuElement) {
            window.Add(menuElement.CreateElement());
        }
    }
}