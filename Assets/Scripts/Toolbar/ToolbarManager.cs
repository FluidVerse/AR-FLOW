using System;
using Controls;
using Graphs;
using MainMenu;
using Quests;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using Utils;
using Button = UnityEngine.UIElements.Button;

namespace Toolbar {
    public class ToolbarManager : MonoBehaviour {

        public float refScreenWidth = 2640f;
        public float refScreenHeight = 1200f;
        public float scaleUIElements = 1.0f;

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

        // Allgemein
        public VisualElement root;
        public VisualElement Toolbar;
        public VisualElement QuestMenu;
        public VisualElement LevelObjects;
        public VisualElement FieldProperties;
        public VisualElement CameraButtonContainer;

        // Toolbar
        public Button Toolbar_ButtonBack;
        public Button Toolbar_ButtonQuestMenu;
        public Button Toolbar_ButtonMainMenu;
        public Button Toolbar_ButtonCheck;
        public Button Toolbar_ButtonAR;
        public Button Toolbar_ButtonInfo;
        public Button Toolbar_ButtonGraph;
        public Button Toolbar_ButtonFunction;
        public Button Toolbar_ButtonCamera;

        // LevelObjects
        public Button LevelObjects_ButtonItem1;
        public Button LevelObjects_ButtonItem2;
        public Button LevelObjects_ButtonItem3;
        public Button LevelObjects_ButtonItem4;
        public Button LevelObjects_ButtonItem5;
        public Button LevelObjects_ButtonItem6;
        public Button LevelObjects_ButtonItem7;
        public Button LevelObjects_ButtonItem8;
        public Button LevelObjects_ButtonItem9;
        public Button LevelObjects_ButtonItem10;
        public Button LevelObjects_ButtonItem11;
        public Button LevelObjects_ButtonItem12;


        // LevelObjects - Properties
        public VisualElement LevelObjects_Position;
        public Label LevelObjects_Position_Value;
        public VisualElement LevelObjects_Property1;
        public UnityEngine.UIElements.Slider LevelObjects_PSlider1;
        public Label LevelObjects_PValue1;
        public VisualElement LevelObjects_Property2;
        public UnityEngine.UIElements.Slider LevelObjects_PSlider2;
        public Label LevelObjects_PValue2;


        // FieldProperties
        public Button FieldProperties_ButtonField;
        public Label FieldProperties_LabelDim;
        public Label FieldProperties_LabelMaxVal;
        public Label FieldProperties_LabelMinVal;
        public Button FieldProperties_ButtonFieldA;
        public Button FieldProperties_ButtonFieldB;
        public Button FieldProperties_ButtonFieldC;
        public VisualElement FieldProperties_ColorbarContainer;
        public VisualElement FieldProperties_Legend;

        // QuestMenu
        public ScrollView QuestMenu_QuestList;

        public VisualTreeAsset questItemTemplate;

        // MainMenu-Verknüpfung
        private MainMenuManager mainMenuManager;

        /// <summary>
        /// Can be <c>null</c> if no <see cref="CameraManager"/> is present in the scene.
        /// </summary>
        private CameraManager cameraManager;

        // Level-Info
        private VisualElement levelInfoBox;
        public VisualElement functionBox;
        private Label levelInfoTitle;
        private Label levelInfoText;

        // Hinweise
        private VisualElement hintBox;
        private Label hintLabel;
        private VisualElement functionImage;
        public Label functionLabel;
        public Label functionLabel2;
        public Label functionLabel3;
        public Label functionLabel4;


        private void Awake() {
            UIDocument uiDocument = GetComponent<UIDocument>();
            root = uiDocument.rootVisualElement;
            Toolbar = root.Q<VisualElement>("Toolbar");
            QuestMenu = root.Q<VisualElement>("QuestMenu");
            LevelObjects = root.Q<VisualElement>("LevelObjects");
            FieldProperties = root.Q<VisualElement>("FieldProperties");
            CameraButtonContainer = root.Q<VisualElement>("CameraButtonContainer");

            if (root == null) Debug.Log("No root Element found");
            if (Toolbar == null) Debug.Log("No Toolbar found");
            if (QuestMenu == null) Debug.Log("No QuestMenu found");
            if (LevelObjects == null) Debug.Log("No LevelObjects found");

            // Toolbar
            Toolbar_ButtonQuestMenu = Toolbar.Q<Button>("ButtonQuestMenu");
            if (Toolbar_ButtonQuestMenu == null) Debug.Log("No ButtonQuestMenu found");
            Toolbar_ButtonQuestMenu.clicked += Toolbar_ClickButtonQuestMenu;
            Toolbar_ButtonMainMenu = Toolbar.Q<Button>("ButtonMainMenu");
            if (Toolbar_ButtonMainMenu == null) Debug.Log("No ButtonMainMenu found");
            Toolbar_ButtonMainMenu.clicked += Toolbar_ClickButtonMainMenu;
            Toolbar_ButtonBack = Toolbar.Q<Button>("ButtonBack");
            if (Toolbar_ButtonBack == null) Debug.Log("No ButtonBack found");
            Toolbar_ButtonBack.clicked += Toolbar_ClickButtonBack;
            Toolbar_ButtonCheck = Toolbar.Q<Button>("ButtonCheck");
            if (Toolbar_ButtonCheck == null) Debug.Log("No ButtonCheck found");
            Toolbar_ButtonAR = Toolbar.Q<Button>("ButtonARMenu");
            if (Toolbar_ButtonAR == null) Debug.Log("No ButtonAR found");
            Toolbar_ButtonGraph = Toolbar.Q<Button>("ButtonGraph");
            if (Toolbar_ButtonGraph == null) Debug.Log("No ButtonGraph found");
            Toolbar_ButtonGraph.clicked += () => {
                onGraphButtonClick?.Invoke();
                closeQuestMenu();
            };
            Toolbar_ButtonInfo = Toolbar.Q<Button>("ButtonInfo");
            if (Toolbar_ButtonInfo == null) Debug.Log("No ButtonInfo found");
            Toolbar_ButtonInfo.clicked += Toolbar_ClickButtonInfo;
            Toolbar_ButtonCamera = CameraButtonContainer.Q<Button>("ButtonCamera");
            if (Toolbar_ButtonCamera == null) Debug.Log("No ButtonCamera found");
            Toolbar_ButtonCamera.clicked += Toolbar_ClickButtonCamera;

            Toolbar_ButtonFunction = Toolbar.Q<Button>("ButtonFunction");
            if (Toolbar_ButtonFunction == null) Debug.Log("No ButtonFunction found");
            Toolbar_ButtonFunction.clicked += Toolbar_ClickButtonFunction;

            // LevelObjects
            LevelObjects_ButtonItem1 = LevelObjects.Q<Button>("ButtonItem1");
            if (LevelObjects_ButtonItem1 == null) Debug.Log("No ButtonItem1 found");
            LevelObjects_ButtonItem1.clicked += LevelObjects_ClickButtonButtonItem1;
            LevelObjects_ButtonItem2 = LevelObjects.Q<Button>("ButtonItem2");
            if (LevelObjects_ButtonItem2 == null) Debug.Log("No ButtonItem2 found");
            LevelObjects_ButtonItem2.clicked += LevelObjects_ClickButtonButtonItem2;
            LevelObjects_ButtonItem3 = LevelObjects.Q<Button>("ButtonItem3");
            if (LevelObjects_ButtonItem3 == null) Debug.Log("No ButtonItem3 found");
            LevelObjects_ButtonItem3.clicked += LevelObjects_ClickButtonButtonItem3;
            LevelObjects_ButtonItem4 = LevelObjects.Q<Button>("ButtonItem4");
            if (LevelObjects_ButtonItem4 == null) Debug.Log("No ButtonItem4 found");
            LevelObjects_ButtonItem4.clicked += LevelObjects_ClickButtonButtonItem4;
            LevelObjects_ButtonItem5 = LevelObjects.Q<Button>("ButtonItem5");
            if (LevelObjects_ButtonItem5 == null) Debug.Log("No ButtonItem5 found");
            LevelObjects_ButtonItem5.clicked += LevelObjects_ClickButtonButtonItem5;
            LevelObjects_ButtonItem6 = LevelObjects.Q<Button>("ButtonItem6");
            if (LevelObjects_ButtonItem6 == null) Debug.Log("No ButtonItem6 found");
            LevelObjects_ButtonItem6.clicked += LevelObjects_ClickButtonButtonItem6;
            LevelObjects_ButtonItem7 = LevelObjects.Q<Button>("ButtonItem7");
            if (LevelObjects_ButtonItem7 == null) Debug.Log("No ButtonItem7 found");
            LevelObjects_ButtonItem7.clicked += LevelObjects_ClickButtonButtonItem7;
            LevelObjects_ButtonItem8 = LevelObjects.Q<Button>("ButtonItem8");
            if (LevelObjects_ButtonItem8 == null) Debug.Log("No ButtonItem8 found");
            LevelObjects_ButtonItem8.clicked += LevelObjects_ClickButtonButtonItem8;
            LevelObjects_ButtonItem9 = LevelObjects.Q<Button>("ButtonItem9");
            if (LevelObjects_ButtonItem9 == null) Debug.Log("No ButtonItem9 found");
            LevelObjects_ButtonItem9.clicked += LevelObjects_ClickButtonButtonItem9;
            LevelObjects_ButtonItem10 = LevelObjects.Q<Button>("ButtonItem10");
            if (LevelObjects_ButtonItem10 == null) Debug.Log("No ButtonItem10 found");
            LevelObjects_ButtonItem10.clicked += LevelObjects_ClickButtonButtonItem10;
            LevelObjects_ButtonItem11 = LevelObjects.Q<Button>("ButtonItem11");
            if (LevelObjects_ButtonItem11 == null) Debug.Log("No ButtonItem11 found");
            LevelObjects_ButtonItem11.clicked += LevelObjects_ClickButtonButtonItem11;
            LevelObjects_ButtonItem12 = LevelObjects.Q<Button>("ButtonItem12");
            if (LevelObjects_ButtonItem12 == null) Debug.Log("No ButtonItem12 found");
            LevelObjects_ButtonItem12.clicked += LevelObjects_ClickButtonButtonItem12;

            LevelObjects_Position = LevelObjects.Q<VisualElement>("Position");
            if (LevelObjects_Position == null) Debug.LogError("No Position found");
            LevelObjects_Position_Value = LevelObjects.Q<Label>("Line2");
            if (LevelObjects_Position_Value == null) Debug.LogError("No Line2 found");

            LevelObjects_Property1 = LevelObjects.Q<VisualElement>("Property1");
            if (LevelObjects_Property1 == null) Debug.LogError("No Property1 found");
            LevelObjects_Property2 = LevelObjects.Q<VisualElement>("Property2");
            if (LevelObjects_Property2 == null) Debug.LogError("No Property2 found");
            LevelObjects_PSlider1 = LevelObjects.Q<UnityEngine.UIElements.Slider>("SliderProperty1");
            if (LevelObjects_PSlider1 == null) Debug.LogError("No SliderProperty1 found");
            LevelObjects_PSlider2 = LevelObjects.Q<UnityEngine.UIElements.Slider>("SliderProperty2");
            if (LevelObjects_PSlider2 == null) Debug.LogError("No SliderProperty2 found");
            LevelObjects_PValue1 = LevelObjects.Q<Label>("ValueProperty1");
            if (LevelObjects_PValue1 == null) Debug.LogError("No ValueProperty1 found");
            LevelObjects_PValue2 = LevelObjects.Q<Label>("ValueProperty2");
            if (LevelObjects_PValue2 == null) Debug.LogError("No ValueProperty2 found");

            // FieldProperties
            FieldProperties_ButtonField = FieldProperties.Q<Button>("ButtonField");
            if (FieldProperties_ButtonField == null) Debug.Log("No ButtonField found");
            FieldProperties_ButtonField.clicked += FieldProperties_ClickButtonField;
            FieldProperties_LabelDim = FieldProperties.Q<Label>("LabelDim");
            if (FieldProperties_LabelDim == null) Debug.Log("No LabelDim found");
            FieldProperties_LabelMaxVal = FieldProperties.Q<Label>("LabelMaxVal");
            if (FieldProperties_LabelMaxVal == null) Debug.Log("No LabelMaxVal found");
            FieldProperties_LabelMinVal = FieldProperties.Q<Label>("LabelMinVal");
            if (FieldProperties_LabelMinVal == null) Debug.Log("No LabelMinVal found");
            FieldProperties_ButtonFieldA = FieldProperties.Q<Button>("ButtonFieldA");
            if (FieldProperties_ButtonFieldA == null) Debug.Log("No ButtonFieldA found");
            FieldProperties_ButtonFieldA.clicked += FieldProperties_ClickButtonFieldA;
            FieldProperties_ButtonFieldB = FieldProperties.Q<Button>("ButtonFieldB");
            if (FieldProperties_ButtonFieldB == null) Debug.Log("No ButtonFieldB found");
            FieldProperties_ButtonFieldB.clicked += FieldProperties_ClickButtonFieldB;
            FieldProperties_ButtonFieldC = FieldProperties.Q<Button>("ButtonFieldC");
            if (FieldProperties_ButtonFieldC == null) Debug.Log("No ButtonFieldC found");
            FieldProperties_ButtonFieldC.clicked += FieldProperties_ClickButtonFieldC;
            FieldProperties_ColorbarContainer = FieldProperties.Q<VisualElement>("ColorbarContainer");
            if (FieldProperties_ColorbarContainer == null) Debug.Log("No ColorbarContainer found");
            FieldProperties_Legend = FieldProperties.Q<VisualElement>("Legend");
            if (FieldProperties_Legend == null) Debug.Log("No Legend found");

            // QuestMenu
            QuestMenu_QuestList = QuestMenu.Q<ScrollView>("QuestList");
            if (QuestMenu_QuestList == null) Debug.Log("No QuestList found");

            // Level-Info
            levelInfoBox = root.Q<VisualElement>("HintBoxBigBackground");
            levelInfoTitle = levelInfoBox.Q<Label>("BigHintTitle");
            levelInfoText = levelInfoBox.Q<Label>("BigHintText");
            levelInfoBox.style.display = DisplayStyle.None;

            // potential function box
            functionBox = root.Q<VisualElement>("functionBox");
            functionLabel = functionBox.Q<Label>("FunctionLabel");
            functionLabel2 = functionBox.Q<Label>("FunctionLabel2");
            functionLabel3 = functionBox.Q<Label>("FunctionLabel3");
            functionLabel4 = functionBox.Q<Label>("FunctionLabel4");
            functionImage = functionBox.Q<VisualElement>("functionImage");


            // Hinweise
            hintBox = root.Q<VisualElement>("HintBox");
            hintLabel = hintBox.Q<Label>("HintText");

            // Sichtbarkeit beim Start nur die Toolbar
            Toolbar.style.display = DisplayStyle.Flex;
            QuestMenu.style.display = DisplayStyle.None;

            // QuestItem-Template als Resource laden
            questItemTemplate = Resources.Load<VisualTreeAsset>("QuestItemTemplate");
            if (questItemTemplate == null) {
                Debug.LogError("QuestItemTemplate.uxml could not be loaded from Resources!");
            }

            // AR-Button pauschal verstecken (da nur für AR-Scene relevant)
            HideButtonAR();

            // Function-Button pauschal verstecken (da nur für Potential-Scene relevant)
            HideButtonFunction();

            // Function-Textfeld pauschal verstecken (da nur für Potential-Scene relevant)
            HideFunctionBox();

            // Hinweis-Box pauschal verstecken
            HideHint();

            // Graph-Button pauschal verstecken 
            HideGraphButton();

            // Hide Button Check
            Toolbar_ButtonCheck.style.display = DisplayStyle.None;

            // Level-Objekt Leiste ausblenden
            HideLevelObjects();
            HideFieldProperties();
            HideCameraButtonContainer();
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() {
            // Hauptmenü verknüpfen
            mainMenuManager = MainMenuManager.Instance;
            if (mainMenuManager == null) {
                Debug.Log("<color=red>" + this.name + ": No MainMenu object with mainMenuManager in Scene!</color>");
            }


            mainMenuManager.linkToolbar();

            // CameraManager verknüpfen
            cameraManager = FindAnyObjectByType<CameraManager>();

            showToolbar();

            // Scale UIElements depending on Screen size

            float actScreenWidth = Screen.width;
            float actScreenHeight = Screen.height;

            float facWidth = actScreenWidth / refScreenWidth;
            float facHeight = actScreenHeight / refScreenHeight;

            scaleUIElements = Mathf.Clamp(Mathf.Min(facWidth, facHeight), 0.5f, 1f);

            float button150Width = 150 * scaleUIElements;
            float button120Width = 120 * scaleUIElements;

            LevelObjects_ButtonItem1.style.width = button150Width;
            LevelObjects_ButtonItem2.style.width = button150Width;
            LevelObjects_ButtonItem3.style.width = button150Width;
            LevelObjects_ButtonItem4.style.width = button150Width;
            LevelObjects_ButtonItem5.style.width = button150Width;
            LevelObjects_ButtonItem6.style.width = button150Width;
            LevelObjects_ButtonItem7.style.width = button150Width;
            LevelObjects_ButtonItem8.style.width = button150Width;
            LevelObjects_ButtonItem9.style.width = button150Width;
            LevelObjects_ButtonItem11.style.width = button150Width;
            LevelObjects_ButtonItem12.style.width = button150Width;

            FieldProperties_ButtonField.style.width = button120Width;
            FieldProperties_ButtonFieldA.style.width = button120Width;
            FieldProperties_ButtonFieldB.style.width = button120Width;
            FieldProperties_ButtonFieldC.style.width = button120Width;
            FieldProperties_ColorbarContainer.style.height = 300 * facHeight;

            Toolbar_ButtonQuestMenu.style.width = button150Width;
            Toolbar_ButtonMainMenu.style.width = button150Width;
            Toolbar_ButtonBack.style.width = button150Width;
            Toolbar_ButtonCheck.style.width = button150Width;
            Toolbar_ButtonAR.style.width = button150Width;
            Toolbar_ButtonGraph.style.width = button150Width;
            Toolbar_ButtonInfo.style.width = button150Width;
            Toolbar_ButtonFunction.style.width = button150Width;

            Toolbar_ButtonFunction = Toolbar.Q<Button>("ButtonFunction");

            Debug.Log($"[UI SCALE] " + $"Screen: {Screen.width}x{Screen.height} | " +
                      $"facW: {facWidth:F2}, facH: {facHeight:F2} | " +
                      $"scale: {scaleUIElements:F2} | " + $"Button150Width: {button150Width:F1}px"
            );

            //PrintHierarchy(QuestMenu);
        }

        private void Update() {
            // fix for input disabled bug after leaving AR scene
            // (put in ToolbarManager because it is expected that every non-AR level has a ToolbarManager)
            InputHelper.TryEnableInput();
        }

        void PrintHierarchy(VisualElement element, string indent = "") {
            Debug.Log($"{indent}{element.name} ({element.GetType().Name})");
            foreach (var child in element.Children()) {
                PrintHierarchy(child, indent + "  ");
            }
        }

        // Button-Funktionen zuweisen
        public void setToolbarButtonCheckAction(Action action) {
            Toolbar_ButtonCheck.clicked += () => action?.Invoke();
        }

        public void setToolbarButtonBackAction(Action action) {
            Toolbar_ButtonBack.clicked += () => action?.Invoke();
        }

        // Button-Sichtbarkeiten
        public void showButtonCheck() {
            Toolbar_ButtonCheck.style.display = DisplayStyle.Flex;
        }

        public void hideButtonCheck() {
            Toolbar_ButtonCheck.style.display = DisplayStyle.None;
        }

        public void showButtonBack() {
            Toolbar_ButtonBack.style.display = DisplayStyle.Flex;
        }

        public void hideButtonBack() {
            Toolbar_ButtonBack.style.display = DisplayStyle.None;
        }

        public void showButtonMainMenu() {
            Toolbar_ButtonMainMenu.style.display = DisplayStyle.Flex;
        }

        public void hideButtonMainMenu() {
            Toolbar_ButtonMainMenu.style.display = DisplayStyle.None;
        }

        public void showButtonQuestMenu() {
            Toolbar_ButtonQuestMenu.style.display = DisplayStyle.Flex;
        }

        public void hideButtonQuestMenu() {
            Toolbar_ButtonQuestMenu.style.display = DisplayStyle.None;
        }

        public void ShowButtonAR() {
            Toolbar_ButtonAR.style.display = DisplayStyle.Flex;
        }

        public void HideButtonAR() {
            Toolbar_ButtonAR.style.display = DisplayStyle.None;
        }

        public void ShowButtonFunction() {
            Toolbar_ButtonFunction.style.display = DisplayStyle.Flex;
        }

        public void HideButtonFunction() {
            Toolbar_ButtonFunction.style.display = DisplayStyle.None;
        }

        public void showAllButtons() {
            Toolbar_ButtonBack.style.display = DisplayStyle.Flex;
            Toolbar_ButtonQuestMenu.style.display = DisplayStyle.Flex;
            Toolbar_ButtonMainMenu.style.display = DisplayStyle.Flex;
            Toolbar_ButtonCheck.style.display = DisplayStyle.Flex;
            Toolbar_ButtonAR.style.display = DisplayStyle.Flex;
        }

        public void hideAllButtons() {
            Toolbar_ButtonBack.style.display = DisplayStyle.None;
            Toolbar_ButtonQuestMenu.style.display = DisplayStyle.None;
            Toolbar_ButtonMainMenu.style.display = DisplayStyle.None;
            Toolbar_ButtonCheck.style.display = DisplayStyle.None;
            Toolbar_ButtonAR.style.display = DisplayStyle.Flex;
        }

        // Menü-Sichtbarkeiten
        public void showQuestMenu() {
            onQuestMenuToggle.Invoke(true);
            Toolbar.style.display = DisplayStyle.Flex;
            QuestMenu.style.display = DisplayStyle.Flex;
        }

        public void closeQuestMenu() {
            onQuestMenuToggle.Invoke(false);
            Toolbar.style.display = DisplayStyle.Flex;
            QuestMenu.style.display = DisplayStyle.None;
        }

        public void showToolbar() {
            Toolbar.style.display = DisplayStyle.Flex;
            QuestMenu.style.display = DisplayStyle.None;
            root.style.display = DisplayStyle.Flex;
        }

        public void closeToolbar() {
            Toolbar.style.display = DisplayStyle.None;
            QuestMenu.style.display = DisplayStyle.None;
            root.style.display = DisplayStyle.None;
        }

        // LevelObjects
        public void ShowLevelObjects() {
            LevelObjects.style.display = DisplayStyle.Flex;
        }

        public void HideLevelObjects() {
            LevelObjects.style.display = DisplayStyle.None;
        }

        public void ShowPosition() {
            LevelObjects_Position.style.display = DisplayStyle.Flex;
        }

        public void HidePosition() {
            LevelObjects_Position.style.display = DisplayStyle.None;
        }

        public void ShowProperty1() {
            LevelObjects_Property1.style.display = DisplayStyle.Flex;
        }

        public void HideProperty1() {
            LevelObjects_Property1.style.display = DisplayStyle.None;
        }

        public void ShowProperty2() {
            LevelObjects_Property2.style.display = DisplayStyle.Flex;
        }

        public void HideProperty2() {
            LevelObjects_Property2.style.display = DisplayStyle.None;
        }

        public void WritePosition(float berKonst, float x, float y) {
            LevelObjects_Position_Value.text = $"Bernoulli-Konst. = {berKonst:F0}\nan Ort ({x:F2} | {y:F2})";
            //LevelObjects_PValue2.text = $"Bernoulli-Konst. = 4.3\nan Ort ({x:F2} | {y:F2})";
        }

        // FieldProperties
        public void ShowFieldProperties() {
            FieldProperties.style.display = DisplayStyle.Flex;
        }

        public void HideFieldProperties() {
            FieldProperties.style.display = DisplayStyle.None;
        }

        public void ShowCameraButtonContainer() {
            CameraButtonContainer.style.display = DisplayStyle.Flex;
        }

        public void HideCameraButtonContainer() {
            CameraButtonContainer.style.display = DisplayStyle.None;
        }

        public void showMainMenu() {
            closeToolbar();
            mainMenuManager.ShowMainMenu();
        }

        private void ShowLevelInfoBox() {
            levelInfoBox.style.display = DisplayStyle.Flex;
        }

        private void HideLevelInfoBox() {
            levelInfoBox.style.display = DisplayStyle.None;
        }

        public void ShowFunctionBox() {
            functionBox.style.display = DisplayStyle.Flex;
        }

        public void HideFunctionBox() {
            functionBox.style.display = DisplayStyle.None;
        }

        // Standard-Button-Funktionen
        public void Toolbar_ClickButtonQuestMenu() {
            if (QuestMenu.style.display == DisplayStyle.None) {
                showQuestMenu();
            } else {
                closeQuestMenu();
            }
        }

        public void Toolbar_ClickButtonMainMenu() {
            showMainMenu();
        }

        public void Toolbar_ClickButtonBack() {
            // Erst prüfen ob Graph offen ist und schließen
            var graphApi = FindAnyObjectByType<GraphApi>();
            if (graphApi != null && graphApi.IsGraphEnabled) {
                graphApi.DisableGraph();
                return; // Nur Graph schließen, nicht weitermachen
            }

            if (cameraManager != null) {
                cameraManager.OnToolbarBackClicked();
            }
            closeQuestMenu(); // close quest menu if it is open
            HideLevelInfoBox(); // hide level info box if it is open
            //HideFunctionBox(); // hide function box if it is open
        }

        private void Toolbar_ClickButtonInfo() {
            if (levelInfoBox.style.display == DisplayStyle.None) {
                ShowLevelInfoBox();
            } else {
                HideLevelInfoBox();
            }
        }

        private void Toolbar_ClickButtonCamera() {
            if (cameraManager == null) {
                return;
            }
            cameraManager.ToggleAerialMode();
        }

        public void Toolbar_ClickButtonFunction() {
            //
        }

        // LevelObjects
        public void LevelObjects_ClickButtonButtonItem1() {
            //showMainMenu();
        }

        public void LevelObjects_ClickButtonButtonItem2() {
            //closeLevelObjects();
        }

        public void LevelObjects_ClickButtonButtonItem3() {
            //showMainMenu();
        }

        public void LevelObjects_ClickButtonButtonItem4() {
            //showMainMenu();
        }

        public void LevelObjects_ClickButtonButtonItem5() {
            //showMainMenu();
        }

        public void LevelObjects_ClickButtonButtonItem6() {
            //showMainMenu();
        }

        public void LevelObjects_ClickButtonButtonItem7() {
            //showMainMenu();
        }

        public void LevelObjects_ClickButtonButtonItem8() {
            //showMainMenu();
        }

        public void LevelObjects_ClickButtonButtonItem9() {
            //showMainMenu();
        }

        public void LevelObjects_ClickButtonButtonItem10() {
            //showMainMenu();
        }

        public void LevelObjects_ClickButtonButtonItem11() {
            //showMainMenu();
        }

        public void LevelObjects_ClickButtonButtonItem12() {
            //showMainMenu();
        }

        // FieldProperties
        public void FieldProperties_ClickButtonField() {
        }

        public void FieldProperties_ClickButtonFieldA() {
        }

        public void FieldProperties_ClickButtonFieldB() {
        }

        public void FieldProperties_ClickButtonFieldC() {
        }

        //
        public void SetButtonState(Button btn, bool state) {
            if (state == true) {
                btn.SetEnabled(true);
                btn.style.opacity = 1f;
            } else {
                btn.SetEnabled(false);
                btn.style.opacity = 0.5f;
            }
        }

        /// <summary>
        /// Callback for <see cref="Quests.QuestManager.onQuestLineChanged"/>.
        /// </summary>
        public void OnQuestLineChanged(QuestLine questLine) {
            levelInfoTitle.text = questLine.Name;
            // add newlines to prevent text hiding behind the toolbar + invisible text to stop TMP from cutting them off
            levelInfoText.text = questLine.Description + "\n\n<color=#00000000>.</color>";
        }


//    public void OnFunctionChanged(string inputFunctionText)
//    {
//        functionLabel.text = inputFunctionText;// + "\n\n<color=#00000000>.</color>";
//    }


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

        public void ShowFunction() {
            functionBox.style.display = DisplayStyle.Flex;
        }

        public void HideFunction() {
            functionBox.style.display = DisplayStyle.None;
        }

        public void ShowGraphButton() {
            Toolbar_ButtonGraph.style.display = DisplayStyle.Flex;
        }

        public void HideGraphButton() {
            Toolbar_ButtonGraph.style.display = DisplayStyle.None;
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