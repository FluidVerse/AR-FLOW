using System;
using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable ParameterHidesMember
namespace MainMenu {
    /// <summary>
    /// List item for displaying a level in the level menu.
    ///
    /// Primarily used and managed inside the <see cref="MainMenuManager"/>.
    /// </summary>
    [UxmlElement]
    public partial class LevelMenuItem : VisualElement {

        private const string styleResource = "LevelMenuStyleSheet";
        private const string ussLevelButton = "button_level";
        private const string ussHintLabel = "label_hint";

        private readonly VisualElement window;
        private readonly Button button;
        private readonly Label hintLabel;

        private Action onButtonClick;

        public LevelMenuItem() {
            window = new VisualElement();
            InitializeWindow();

            button = new Button {
                text = "Levelname"
            };
            button.clicked += () => onButtonClick?.Invoke();
            button.AddToClassList(ussLevelButton);
            window.Add(button);

            hintLabel = new Label {
                text = "Level 1"
            };
            hintLabel.AddToClassList(ussHintLabel);
            window.Add(hintLabel);
        }

        /// <summary>
        /// Fills the newly created level menu item with the given data.
        /// </summary>
        /// <param name="levelName">Display name of the level</param>
        /// <param name="hintText">Hint text in the top left corner</param>
        /// <param name="onButtonClick">Button click callback</param>
        public void Create(string levelName, string hintText, Action onButtonClick) {
            this.onButtonClick = onButtonClick;
            button.text = levelName;
            hintLabel.text = hintText;
        }

        private void InitializeWindow() {
            hierarchy.Clear();
            styleSheets.Add(Resources.Load<StyleSheet>(styleResource));
            window.name = "Window";
            window.transform.position = Vector3.zero;
            hierarchy.Add(window);
        }
    }
}