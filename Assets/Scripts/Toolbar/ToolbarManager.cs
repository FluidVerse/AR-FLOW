using System;
using System.Collections.Generic;
using Controls;
using Graphs;
using Quests;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using Utils;

namespace Toolbar {
    [RequireComponent(typeof(UIDocument))]
    public class ToolbarManager : MonoBehaviour {

        private const float refScreenWidth = 2640f;
        private const float refScreenHeight = 1200f;
        private float uiScaleFactor = 1.0f;

        /// <summary>
        /// Callback when the quest menu is opened or closed (<c>true</c> = opened, <c>false</c> = closed).
        /// </summary>
        public UnityEvent<bool> onQuestMenuToggle;

        /// <summary>
        /// Callback when the graph button is clicked.
        /// </summary>
        public UnityEvent onGraphButtonClick;

        /// <summary>
        /// Root element of the toolbar UI.
        /// </summary>
        public VisualElement RootElement => root;

        /// <summary>
        /// Whether the position in the LevelObjects area is currently visible.
        /// </summary>
        public bool IsLevelObjectsPositionVisible => LevelObjects_Position.style.display == DisplayStyle.Flex;

        /// <summary>
        /// Whether the function box is currently visible.
        /// </summary>
        public bool IsFunctionBoxVisible => functionBox.style.display == DisplayStyle.Flex;

        // General
        private VisualElement root;
        private VisualElement Toolbar;
        private VisualElement QuestMenu;
        private VisualElement LevelObjects;
        private VisualElement FieldProperties;
        private VisualElement CameraButtonContainer;

        // Toolbar
        private Button Toolbar_ButtonBack;
        private Button Toolbar_ButtonQuestMenu;
        private Button Toolbar_ButtonMainMenu;
        private Button Toolbar_ButtonCheck;
        private Button Toolbar_ButtonAR;
        private Button Toolbar_ButtonInfo;
        private Button Toolbar_ButtonGraph;
        private Button Toolbar_ButtonFunction;
        private Button Toolbar_ButtonCamera;

        // LevelObjects
        private readonly Button[] LevelObjects_Buttons = new Button[12];

        // LevelObjects - Properties
        private VisualElement LevelObjects_Position;
        private Label LevelObjects_Position_Value;

        private readonly VisualElement[] LevelObjects_Properties = new VisualElement[2];

        // exception: sliders are public because levels need extra granular controls for them
        public readonly Slider[] LevelObjects_PSliders = new Slider[2];
        public readonly Label[] LevelObjects_PValues = new Label[2];

        // FieldProperties
        private readonly Button[] FieldProperties_ButtonFields = new Button[4];
        private Label FieldProperties_LabelDim;
        private Label FieldProperties_LabelMaxVal;
        private Label FieldProperties_LabelMinVal;
        private VisualElement FieldProperties_ColorbarContainer;
        private VisualElement FieldProperties_Legend;

        /// <summary>
        /// Can be <c>null</c> if no <see cref="CameraManager"/> is present in the scene.
        /// </summary>
        private CameraManager cameraManager;

        // Level-Info
        private VisualElement levelInfoBox;
        private VisualElement functionBox;
        private Label levelInfoTitle;
        private Label levelInfoText;

        // Hinweise
        private VisualElement hintBox;
        private Label hintLabel;
        private VisualElement functionImage;
        private Label functionLabel;
        private Label functionLabel2;
        private Label functionLabel3;
        private Label functionLabel4;

        private readonly Dictionary<ToolbarButton, Button> buttonDict = new();
        private readonly Dictionary<ToolbarArea, VisualElement> areaDict = new();

        private void Awake() {
            InitGeneral();
            InitToolbar();
            InitLevelObjects();
            InitFieldProperties();
            InitLevelInfo();
            InitFunctionBox();
            InitHints();

            SetButtonVisibility(ToolbarButton.AR, false); // only relevant for AR scene
            SetButtonVisibility(ToolbarButton.Function, false); // only relevant for potential flow level
            SetButtonVisibility(ToolbarButton.Graph, false);
            SetButtonVisibility(ToolbarButton.Check, false);
            SetButtonVisibility(ToolbarButtonGroups.LevelObjects, false); // hide all by default
            SetAreaVisibility(ToolbarArea.FunctionBox, false); // only relevant for potential flow level
            SetAreaVisibility(ToolbarArea.LevelObjects, false);
            SetAreaVisibility(ToolbarArea.FieldProperties, false);
            SetAreaVisibility(ToolbarArea.CameraButtonContainer, false);
            SetAreaVisibility(ToolbarArea.LevelInfoBox, false);
            HideHint();
        }

        private void Start() {
            cameraManager = FindAnyObjectByType<CameraManager>();

            ShowToolbar();
            ScaleUi();
        }

        private void Update() {
            // fix for input disabled bug after leaving AR scene
            // (put in ToolbarManager because it is expected that every non-AR level has a ToolbarManager)
            InputHelper.TryEnableInput();
        }

        /// <summary>
        /// Sets up a button with the given icon and onClick action.
        /// </summary>
        /// <param name="toolbarButton">Enum value corresponding to the button to set up</param>
        /// <param name="icon">Icon texture</param>
        /// <param name="onClick">On click action, can be <c>null</c></param>
        public void SetupButton(ToolbarButton toolbarButton, Texture2D icon, Action onClick) {
            Button b = buttonDict[toolbarButton];
            b.style.backgroundImage = new StyleBackground(icon);
            b.style.display = DisplayStyle.Flex; // make visible by default 
            if (onClick != null) {
                b.clicked += onClick;
            }
        }

        /// <summary>
        /// Sets the icon of a button.
        /// </summary>
        /// <param name="toolbarButton">Enum value corresponding to the button</param>
        /// <param name="icon">Icon texture</param>
        public void SetButtonIcon(ToolbarButton toolbarButton, Texture2D icon) {
            buttonDict[toolbarButton].style.backgroundImage = new StyleBackground(icon);
        }

        /// <summary>
        /// Sets the visibility of a button.
        ///
        /// Hidden buttons are not visible and do not take up space in the layout.
        /// </summary>
        /// <param name="toolbarButton">Enum value corresponding to the button</param>
        /// <param name="isVisible"><c>true</c> to make the button visible, <c>false</c> to hide it</param>
        /// <param name="keepInLayout">
        /// If <c>true</c>, the button will keep its space in the layout even when hidden
        /// </param>
        public void SetButtonVisibility(ToolbarButton toolbarButton, bool isVisible, bool keepInLayout = false) {
            Button b = buttonDict[toolbarButton];
            b.style.display = isVisible || keepInLayout ? DisplayStyle.Flex : DisplayStyle.None;
            b.style.visibility = !isVisible && keepInLayout ? Visibility.Hidden : Visibility.Visible;
        }

        /// <summary>
        /// Sets the visibility of a button group, see <see cref="ToolbarButtonGroups"/>.
        ///
        /// Hidden buttons are not visible and do not take up space in the layout.
        /// </summary>
        /// <param name="toolbarButtons">Enum values corresponding to the button</param>
        /// <param name="isVisible"><c>true</c> to make the button visible, <c>false</c> to hide it</param>
        public void SetButtonVisibility(ToolbarButton[] toolbarButtons, bool isVisible) {
            foreach (ToolbarButton toolbarButton in toolbarButtons) {
                SetButtonVisibility(toolbarButton, isVisible);
            }
        }

        /// <summary>
        /// Enables/disables a button.
        ///
        /// Disabled buttons visually still exist in the UI, but are slightly transparent and cannot be clicked on.
        /// </summary>
        /// <param name="toolbarButton">Enum value corresponding to the button</param>
        /// <param name="isEnabled"><c>true</c> to enable the button, <c>false</c> to disable it</param>
        /// <param name="keepOnClick">
        /// If <c>true</c>, the button will still trigger its onClick action when clicked, even if it is disabled.
        /// </param>
        public void SetButtonEnabled(ToolbarButton toolbarButton, bool isEnabled, bool keepOnClick = false) {
            Button b = buttonDict[toolbarButton];
            b.SetEnabled(isEnabled || keepOnClick);
            b.style.opacity = isEnabled ? 1f : 0.5f;
        }

        /// <summary>
        /// Enables/disables a button group, see <see cref="ToolbarButtonGroups"/>.
        ///
        /// Disabled buttons visually still exist in the UI, but are slightly transparent and cannot be clicked on.
        /// </summary>
        /// <param name="toolbarButtons">Enum values corresponding to the buttons</param>
        /// <param name="isEnabled"><c>true</c> to enable the button, <c>false</c> to disable it</param>
        /// <param name="keepOnClick">
        /// If <c>true</c>, the button will still trigger its onClick action when clicked, even if it is disabled.
        /// </param>
        public void SetButtonEnabled(ToolbarButton[] toolbarButtons, bool isEnabled, bool keepOnClick = false) {
            foreach (ToolbarButton toolbarButton in toolbarButtons) {
                SetButtonEnabled(toolbarButton, isEnabled, keepOnClick);
            }
        }

        /// <summary>
        /// Adds an onClick action to a button.
        /// </summary>
        /// <param name="toolbarButton">Enum value corresponding to the button to set up</param>
        /// <param name="onClick">On click action to add</param>
        public void AddButtonAction(ToolbarButton toolbarButton, Action onClick) {
            buttonDict[toolbarButton].clicked += onClick;
        }

        /// <summary>
        /// Removes an onClick action from a button.
        /// </summary>
        /// <param name="toolbarButton">Enum value corresponding to the button to set up</param>
        /// <param name="onClick">
        /// On click action that was previous added using <see cref="SetupButton"/>
        /// </param>
        public void RemoveButtonAction(ToolbarButton toolbarButton, Action onClick) {
            buttonDict[toolbarButton].clicked -= onClick;
        }

        /// <summary>
        /// Sets the text of the dimension label in the FieldProperties menu.
        /// </summary>
        /// <param name="text">New text</param>
        public void SetFieldPropertiesLabelDimText(string text) {
            FieldProperties_LabelDim.text = text;
        }

        /// <summary>
        /// Sets the text of the min and max value labels in the FieldProperties menu.
        /// </summary>
        /// <param name="minVal">Minimum value text</param>
        /// <param name="maxVal">Maximum value text</param>
        public void SetFieldPropertiesMinMax(string minVal, string maxVal) {
            FieldProperties_LabelMinVal.text = minVal;
            FieldProperties_LabelMaxVal.text = maxVal;
        }

        /// <summary>
        /// Sets the visibility of the legend in the FieldProperties menu.
        /// </summary>
        /// <param name="isVisible"><c>true</c> to make the legend visible, <c>false</c> to hide it</param>
        public void SetFieldPropertiesLegendVisibility(bool isVisible) {
            FieldProperties_Legend.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        /// <summary>
        /// Sets the visibility of an area, see <see cref="ToolbarArea"/>.
        ///
        /// Hidden areas are not visible and do not take up space in the layout.
        /// </summary>
        /// <param name="toolbarArea">Enum value corresponding to the area</param>
        /// <param name="isVisible"><c>true</c> to make the area visible, <c>false</c> to hide it</param>
        public void SetAreaVisibility(ToolbarArea toolbarArea, bool isVisible) {
            areaDict[toolbarArea].style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }
        
        /// <summary>
        /// Sets the visibility of an area group, see <see cref="ToolbarAreaGroups"/>.
        ///
        /// Hidden areas are not visible and do not take up space in the layout.
        /// </summary>
        /// <param name="toolbarArea">Enum values corresponding to the area</param>
        /// <param name="isVisible"><c>true</c> to make the area visible, <c>false</c> to hide it</param>
        public void SetAreaVisibility(ToolbarArea[] toolbarArea, bool isVisible) {
            foreach (ToolbarArea area in toolbarArea) {
                SetAreaVisibility(area, isVisible);
            }
        }

        private void InitGeneral() {
            UIDocument uiDocument = GetComponent<UIDocument>();
            root = uiDocument.rootVisualElement;
            Toolbar = InitElement<VisualElement>(root, "Toolbar");
            QuestMenu = InitElement<VisualElement>(root, "QuestMenu");
            LevelObjects = InitElement<VisualElement>(root, "LevelObjects");
            areaDict.Add(ToolbarArea.LevelObjects, LevelObjects);
            FieldProperties = InitElement<VisualElement>(root, "FieldProperties");
            areaDict.Add(ToolbarArea.FieldProperties, FieldProperties);
            CameraButtonContainer = InitElement<VisualElement>(root, "CameraButtonContainer");
            areaDict.Add(ToolbarArea.CameraButtonContainer, CameraButtonContainer);
        }

        private void InitToolbar() {
            Toolbar_ButtonQuestMenu = InitButton(Toolbar, "ButtonQuestMenu", ToolbarButton.QuestMenu,
                Toolbar_ClickButtonQuestMenu);
            Toolbar_ButtonMainMenu = InitButton(Toolbar, "ButtonMainMenu", ToolbarButton.MainMenu,
                Toolbar_ClickButtonMainMenu);
            Toolbar_ButtonBack = InitButton(Toolbar, "ButtonBack", ToolbarButton.Back, Toolbar_ClickButtonBack);
            Toolbar_ButtonCheck = InitButton(Toolbar, "ButtonCheck", ToolbarButton.Check, null);
            Toolbar_ButtonAR = InitButton(Toolbar, "ButtonARMenu", ToolbarButton.AR, null);
            Toolbar_ButtonGraph = InitButton(Toolbar, "ButtonGraph", ToolbarButton.Graph, Toolbar_ClickButtonGraph);
            Toolbar_ButtonInfo = InitButton(Toolbar, "ButtonInfo", ToolbarButton.Info, Toolbar_ClickButtonInfo);
            Toolbar_ButtonCamera = InitButton(CameraButtonContainer, "ButtonCamera", ToolbarButton.Camera,
                Toolbar_ClickButtonCamera);
            Toolbar_ButtonFunction = InitButton(Toolbar, "ButtonFunction", ToolbarButton.Function, null);
        }

        private void InitLevelObjects() {
            for (int i = 0; i < LevelObjects_Buttons.Length; i++) {
                string buttonName = "ButtonItem" + (i + 1); // ButtonItem1, ButtonItem2, ..., ButtonItem12
                // onClick actions are assigned by levels themselves using SetupLevelObjectButton()
                LevelObjects_Buttons[i] =
                    InitButton(LevelObjects, buttonName, ToolbarButtonGroups.LevelObjects[i], null);
            }

            LevelObjects_Position = InitElement<VisualElement>(LevelObjects, "Position");
            areaDict.Add(ToolbarArea.Position, LevelObjects_Position);
            LevelObjects_Position_Value = InitElement<Label>(LevelObjects, "Line2");
            LevelObjects_Properties[0] = InitElement<VisualElement>(LevelObjects, "Property1");
            areaDict.Add(ToolbarArea.Property1, LevelObjects_Properties[0]);
            LevelObjects_Properties[1] = InitElement<VisualElement>(LevelObjects, "Property2");
            areaDict.Add(ToolbarArea.Property2, LevelObjects_Properties[1]);
            LevelObjects_PSliders[0] = InitElement<Slider>(LevelObjects, "SliderProperty1");
            LevelObjects_PSliders[1] = InitElement<Slider>(LevelObjects, "SliderProperty2");
            LevelObjects_PValues[0] = InitElement<Label>(LevelObjects, "ValueProperty1");
            LevelObjects_PValues[1] = InitElement<Label>(LevelObjects, "ValueProperty2");
        }

        private void InitFieldProperties() {
            FieldProperties_LabelDim = InitElement<Label>(FieldProperties, "LabelDim");
            FieldProperties_LabelMaxVal = InitElement<Label>(FieldProperties, "LabelMaxVal");
            FieldProperties_LabelMinVal = InitElement<Label>(FieldProperties, "LabelMinVal");
            FieldProperties_ColorbarContainer = InitElement<VisualElement>(FieldProperties, "ColorbarContainer");
            FieldProperties_Legend = InitElement<VisualElement>(FieldProperties, "Legend");

            FieldProperties_ButtonFields[0] =
                InitButton(FieldProperties, "ButtonField", ToolbarButton.ButtonField, null);
            FieldProperties_ButtonFields[1] =
                InitButton(FieldProperties, "ButtonFieldA", ToolbarButton.ButtonFieldA, null);
            FieldProperties_ButtonFields[2] =
                InitButton(FieldProperties, "ButtonFieldB", ToolbarButton.ButtonFieldB, null);
            FieldProperties_ButtonFields[3] =
                InitButton(FieldProperties, "ButtonFieldC", ToolbarButton.ButtonFieldC, null);
        }

        private void InitLevelInfo() {
            levelInfoBox = InitElement<VisualElement>(root, "HintBoxBigBackground");
            areaDict.Add(ToolbarArea.LevelInfoBox, levelInfoBox);
            levelInfoTitle = InitElement<Label>(levelInfoBox, "BigHintTitle");
            levelInfoText = InitElement<Label>(levelInfoBox, "BigHintText");
        }

        private void InitFunctionBox() {
            functionBox = InitElement<VisualElement>(root, "FunctionBox");
            areaDict.Add(ToolbarArea.FunctionBox, functionBox);
            functionLabel = InitElement<Label>(functionBox, "FunctionLabel");
            functionLabel2 = InitElement<Label>(functionBox, "FunctionLabel2");
            functionLabel3 = InitElement<Label>(functionBox, "FunctionLabel3");
            functionLabel4 = InitElement<Label>(functionBox, "FunctionLabel4");
            functionImage = InitElement<VisualElement>(functionBox, "functionImage");
        }

        private void InitHints() {
            hintBox = InitElement<VisualElement>(root, "HintBox");
            hintLabel = InitElement<Label>(hintBox, "HintText");
        }

        /// <summary>
        /// Helper function to find a <see cref="VisualElement"/> and log an error if the element is not found.
        /// </summary>
        /// <param name="baseElement">Base element to query the element from</param>
        /// <param name="elementName">Name of the element to query (as set in the UI Builder)</param>
        /// <typeparam name="T">Element type</typeparam>
        /// <returns>
        /// The found element, or <c>null</c> if no element with the given name was found in the base element
        /// </returns>
        private static T InitElement<T>(VisualElement baseElement, string elementName) where T : VisualElement {
            T element = baseElement.Q<T>(elementName);
            if (element == null) {
                Debug.Log($"Element \"{elementName}\" not found");
                return null;
            }
            return element;
        }

        /// <summary>
        /// Helper function to initialize a button with an onClick action and log an error if the button is not found.
        /// </summary>
        /// <param name="baseElement">Base element to query the button from</param>
        /// <param name="buttonName">Name of the button to query (as set in the UI Builder)</param>
        /// <param name="enumValue">
        /// Corresponding enum value for this button, can be <c>null</c> if this button should not be added to
        /// <see cref="buttonDict"/>
        /// </param>
        /// <param name="onClick">On click action, can be <c>null</c></param>
        /// <returns>
        /// The initialized button, or <c>null</c> if no button with the given name was found in the base element
        /// </returns>
        private Button InitButton(VisualElement baseElement, string buttonName, ToolbarButton? enumValue,
            Action onClick) {
            Button b = InitElement<Button>(baseElement, buttonName);
            if (b == null) {
                return null;
            }
            if (enumValue != null) {
                if (!buttonDict.TryAdd(enumValue.Value, b)) {
                    Debug.LogError($"Duplicate enum value {enumValue.Value} for button {buttonName}", this);
                }
            }
            if (onClick != null) {
                b.clicked += onClick;
            }
            return b;
        }

        /// <summary>
        /// Scales the UI elements based on the current screen resolution compared to a reference resolution.
        /// </summary>
        private void ScaleUi() {
            float actScreenWidth = Screen.width;
            float actScreenHeight = Screen.height;

            float facWidth = actScreenWidth / refScreenWidth;
            float facHeight = actScreenHeight / refScreenHeight;

            uiScaleFactor = Mathf.Clamp(Mathf.Min(facWidth, facHeight), 0.5f, 1f);

            float button150Width = 150 * uiScaleFactor;
            float button120Width = 120 * uiScaleFactor;

            foreach (Button button in LevelObjects_Buttons) {
                button.style.width = button150Width;
            }

            foreach (Button button in FieldProperties_ButtonFields) {
                button.style.width = button120Width;
            }
            FieldProperties_ColorbarContainer.style.height = 300 * facHeight;

            Toolbar_ButtonQuestMenu.style.width = button150Width;
            Toolbar_ButtonMainMenu.style.width = button150Width;
            Toolbar_ButtonBack.style.width = button150Width;
            Toolbar_ButtonCheck.style.width = button150Width;
            Toolbar_ButtonAR.style.width = button150Width;
            Toolbar_ButtonGraph.style.width = button150Width;
            Toolbar_ButtonInfo.style.width = button150Width;
            Toolbar_ButtonFunction.style.width = button150Width;
            Toolbar_ButtonCamera.style.width = button150Width;

            Debug.Log(
                $"[UI SCALE] Screen: {Screen.width}x{Screen.height} | facW: {facWidth:F2}, facH: {facHeight:F2} | scale: {uiScaleFactor:F2} | Button150Width: {button150Width:F1}px"
            );
        }

        private void ShowQuestMenu() {
            onQuestMenuToggle.Invoke(true);
            Toolbar.style.display = DisplayStyle.Flex;
            QuestMenu.style.display = DisplayStyle.Flex;
        }

        private void HideQuestMenu() {
            onQuestMenuToggle.Invoke(false);
            Toolbar.style.display = DisplayStyle.Flex;
            QuestMenu.style.display = DisplayStyle.None;
        }

        public void ShowToolbar() {
            Toolbar.style.display = DisplayStyle.Flex;
            QuestMenu.style.display = DisplayStyle.None;
            root.style.display = DisplayStyle.Flex;
        }

        private void HideToolbar() {
            Toolbar.style.display = DisplayStyle.None;
            QuestMenu.style.display = DisplayStyle.None;
            root.style.display = DisplayStyle.None;
        }

        public void SetPositionText(float berKonst, float x, float y) {
            LevelObjects_Position_Value.text = $"Bernoulli-Konst. = {berKonst:F0}\nan Ort ({x:F2} | {y:F2})";
            //LevelObjects_PValue2.text = $"Bernoulli-Konst. = 4.3\nan Ort ({x:F2} | {y:F2})";
        }

        private void Toolbar_ClickButtonQuestMenu() {
            if (QuestMenu.style.display == DisplayStyle.None) {
                ShowQuestMenu();
            } else {
                HideQuestMenu();
            }
        }

        private void Toolbar_ClickButtonMainMenu() {
            HideToolbar();
        }

        private void Toolbar_ClickButtonBack() {
            var graphApi = FindAnyObjectByType<GraphApi>();
            if (graphApi != null && graphApi.IsGraphEnabled) {
                graphApi.DisableGraph();
                return; // only close graph
            }

            if (cameraManager != null) {
                cameraManager.OnToolbarBackClicked();
            }
            HideQuestMenu(); // close quest menu if it is open
            SetAreaVisibility(ToolbarArea.LevelInfoBox, false); // hide level info box if it is open
        }

        private void Toolbar_ClickButtonGraph() {
            onGraphButtonClick?.Invoke();
            HideQuestMenu();
        }

        private void Toolbar_ClickButtonInfo() {
            SetAreaVisibility(ToolbarArea.LevelInfoBox, levelInfoBox.style.display == DisplayStyle.None);
        }

        private void Toolbar_ClickButtonCamera() {
            if (cameraManager == null) {
                return;
            }
            cameraManager.ToggleAerialMode();
        }

        /// <summary>
        /// Callback for <see cref="Quests.QuestManager.onQuestLineChanged"/>.
        /// </summary>
        public void OnQuestLineChanged(QuestLine questLine) {
            levelInfoTitle.text = questLine.Name;
            // add newlines to prevent text hiding behind the toolbar + invisible text to stop TMP from cutting them off
            levelInfoText.text = questLine.Description + "\n\n<color=#00000000>.</color>";
        }

        public void ShowHintWithText(string text) {
            hintLabel.text = text;
            hintBox.style.display = DisplayStyle.Flex;
        }

        public void HideHint() {
            hintBox.style.display = DisplayStyle.None;
        }

        public void ShowFunctionCyl(float cylXpos, float cylYpos) {
            //TBD Alex: load correct image and display it
            Texture2D potentialTexture = Resources.Load<Texture2D>("Images/Potential/PotentialZylinderParallel_bold");
            Debug.Log(potentialTexture != null); // should be true
            Debug.Log(functionImage.resolvedStyle.width);
            functionImage.style.backgroundImage = potentialTexture;

            functionLabel.style.position = Position.Absolute;
            functionLabel.style.top = 78;
            functionLabel.style.left = 53;

            functionLabel2.style.position = Position.Absolute;
            functionLabel2.style.top = 78;
            functionLabel2.style.left = 225;

            functionLabel3.style.position = Position.Absolute;
            functionLabel3.style.top = 253;
            functionLabel3.style.left = 53;

            functionLabel4.style.position = Position.Absolute;
            functionLabel4.style.top = 253;
            functionLabel4.style.left = 225;

            functionLabel.text = $"{cylXpos:F2}";
            functionLabel2.text = $"{cylYpos:F2}";
            functionLabel3.text = $"{cylXpos:F2}";
            functionLabel4.text = $"{cylYpos:F2}";
        }

        public void ShowFunctionVort(float cylXpos, float cylYpos) {
            //TBD Alex: load correct image and display it
            Texture2D potentialTexture = Resources.Load<Texture2D>("Images/Potential/PotentialVortex_bold");
            Debug.Log(potentialTexture != null); // should be true
            Debug.Log(functionImage.resolvedStyle.width);
            functionImage.style.backgroundImage = potentialTexture;

            functionLabel.style.position = Position.Absolute;
            functionLabel.style.top = 87;
            functionLabel.style.left = 130;
            functionLabel.style.fontSize = 32;

            functionLabel2.style.position = Position.Absolute;
            functionLabel2.style.top = 44;
            functionLabel2.style.left = 130;
            functionLabel2.style.fontSize = 32;

            functionLabel3.style.position = Position.Absolute;
            functionLabel3.style.top = 245;
            functionLabel3.style.left = 92;
            functionLabel3.style.fontSize = 32;

            functionLabel4.style.position = Position.Absolute;
            functionLabel4.style.top = 245;
            functionLabel4.style.left = 330;
            functionLabel4.style.fontSize = 32;

            functionLabel.text = $"{cylXpos:F2}";
            functionLabel2.text = $"{cylYpos:F2}";
            functionLabel3.text = $"{cylXpos:F2}";
            functionLabel4.text = $"{cylYpos:F2}";
        }

        /// <summary>
        /// Picks the topmost VisualElement at the given screen position.
        /// </summary>
        /// <param name="screenPosition">Screen position in px</param>
        /// <returns>
        /// The topmost VisualElement at the given position, or <c>null</c> if none was found.
        /// </returns>
        public VisualElement Pick(Vector2 screenPosition) {
            return root.panel.Pick(screenPosition);
        }
    }
}