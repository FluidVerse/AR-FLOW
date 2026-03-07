using System.Collections;
using System.Collections.Generic;
using AR;
using Controls;
using Drawing;
using Graphs;
using MainMenu;
using Quests;
using Toolbar;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace LevelScripts {
    public class PumpenlevelAR : MonoBehaviour {

        public bool ARscene = false;
        [SerializeField] private Collider pumpCollider;
        [SerializeField] private PinchToZoom pinchToZoom;
        [SerializeField] private float pinchToZoomScaleFactor = 0.002f;

        public GameObject MainObject;

        public List<GameObject> rotatingObjects;
        public float rotationSpeed = 0.2f;
        public List<GameObject> fixedObjects;

        public GameObject LevelObject1;
        public GameObject LevelObject2;
        public GameObject LevelObject3;
        public GameObject LevelObject4;
        public GameObject LevelObject5;
        public GameObject LevelObject6;

        public Texture2D IconButton1;
        public Texture2D IconButton2;
        public Texture2D IconButton3;
        public Texture2D IconButton4;
        public Texture2D IconButton5;
        public Texture2D IconButton6;
        public Texture2D IconButton7; // Volume Plot
        public Texture2D IconButton8; // Pathline
        public Texture2D IconButton9; // Force

        private int plotField = 0; // Druckfeld
        private bool showVolumePlot = false;
        private bool showPathlines = false;
        private bool showForces = false;

        public GameObject particleLaufrad;
        private VolumePlot vpLaufrad;
        public GameObject particleInlet;
        private VolumePlot vpInlet;
        public GameObject particleOutlet;
        private VolumePlot vpOutlet;

        public GameObject pathLineLaufradRelativ;
        public GameObject pathLineLaufradAbsolut;
        public GameObject pathLineSpirale;

        public GameObject arrowsFront;
        public GameObject arrowsBack;
        public GameObject arrowsImpuls;
        public GameObject arrowsWelle;

        public Texture2D IconButtonP;
        public Texture2D IconButtonC;
        public Texture2D IconButtonW;

        private ToolbarManager tm;
        private QuestManager questManager;

        private Vector3 mainObjectBasePosition;
        private Vector3 mainObjectBaseScale;

        private bool isZoomed; // is currently zoomed in/out? 

        private InputAction clickAction, pointAction;

        private void Awake() {
            questManager = FindAnyObjectByType<QuestManager>();
            if (questManager == null) {
                Debug.LogError("QuestManager not found", this);
            }
            tm = FindAnyObjectByType<ToolbarManager>();
            if (tm == null) {
                Debug.LogError("ToolbarManager not found", this);
            }

            clickAction = InputSystem.actions.FindAction("UI/Click", true);
            pointAction = InputSystem.actions.FindAction("UI/Point", true);
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() {
            pinchToZoom.IsCheckingForInput = true;
            MainMenuManager.Instance.OnMainMenuToggle.AddListener(OnMainMenuToggle);

            questManager.SetQuestLine(QuestLines.PumpenlevelAR);
            tm.hideButtonQuestMenu();
            tm.hideButtonBack();
            tm.ShowLevelObjects();
            tm.ShowFieldProperties();

            tm.LevelObjects_ButtonItem1.style.display = DisplayStyle.Flex;
            tm.LevelObjects_ButtonItem2.style.display = DisplayStyle.Flex;
            tm.LevelObjects_ButtonItem3.style.display = DisplayStyle.Flex;
            tm.LevelObjects_ButtonItem4.style.display = DisplayStyle.Flex;
            tm.LevelObjects_ButtonItem5.style.display = DisplayStyle.Flex;
            tm.LevelObjects_ButtonItem6.style.display = DisplayStyle.Flex;
            tm.LevelObjects_ButtonItem7.style.display = DisplayStyle.Flex;
            tm.LevelObjects_ButtonItem8.style.display = DisplayStyle.Flex;
            tm.LevelObjects_ButtonItem9.style.display = DisplayStyle.Flex;
            tm.LevelObjects_ButtonItem10.style.display = DisplayStyle.None;
            tm.LevelObjects_ButtonItem11.style.display = DisplayStyle.None;
            tm.LevelObjects_ButtonItem12.style.display = DisplayStyle.None;

            tm.LevelObjects_ButtonItem1.clicked += LevelObjects_ClickButton1;
            tm.LevelObjects_ButtonItem2.clicked += LevelObjects_ClickButton2;
            tm.LevelObjects_ButtonItem3.clicked += LevelObjects_ClickButton3;
            tm.LevelObjects_ButtonItem4.clicked += LevelObjects_ClickButton4;
            tm.LevelObjects_ButtonItem5.clicked += LevelObjects_ClickButton5;
            tm.LevelObjects_ButtonItem6.clicked += LevelObjects_ClickButton6;
            tm.LevelObjects_ButtonItem7.clicked += LevelObjects_ClickButton7;
            tm.LevelObjects_ButtonItem8.clicked += LevelObjects_ClickButton8;
            tm.LevelObjects_ButtonItem9.clicked += LevelObjects_ClickButton9;

            tm.LevelObjects_ButtonItem1.style.backgroundImage = new StyleBackground(IconButton1);
            tm.LevelObjects_ButtonItem2.style.backgroundImage = new StyleBackground(IconButton2);
            tm.LevelObjects_ButtonItem3.style.backgroundImage = new StyleBackground(IconButton3);
            tm.LevelObjects_ButtonItem4.style.backgroundImage = new StyleBackground(IconButton4);
            tm.LevelObjects_ButtonItem5.style.backgroundImage = new StyleBackground(IconButton5);
            tm.LevelObjects_ButtonItem6.style.backgroundImage = new StyleBackground(IconButton6);
            tm.LevelObjects_ButtonItem7.style.backgroundImage = new StyleBackground(IconButton7);
            tm.LevelObjects_ButtonItem8.style.backgroundImage = new StyleBackground(IconButton8);
            tm.LevelObjects_ButtonItem9.style.backgroundImage = new StyleBackground(IconButton9);

            vpLaufrad = particleLaufrad.GetComponent<VolumePlot>();
            vpInlet = particleInlet.GetComponent<VolumePlot>();
            vpOutlet = particleOutlet.GetComponent<VolumePlot>();

            tm.FieldProperties_ButtonField.clicked += FieldProperties_ClickButtonField;
            tm.FieldProperties_Legend.style.display = DisplayStyle.Flex;

            plotField = -1;
            FieldProperties_ClickButtonField();

            showVolumePlot = false;
            tm.LevelObjects_ButtonItem7.style.opacity = 0.5f;
            hideCurrentVolumePlot();

            showPathlines = false;
            tm.LevelObjects_ButtonItem8.style.opacity = 0.5f;
            hideCurrentPathlines();

            showForces = false;
            tm.LevelObjects_ButtonItem9.style.opacity = 0.5f;
            hideArrows();

            if (!ARscene) {
                CameraManager cms = FindAnyObjectByType<CameraManager>();
                cms.ChangeToDetailView(MainObject, true);
                cms.ChangeCameraOnBackButton = false;
            }
        }

        void SetVisible(GameObject obj, bool visible) {
            foreach (var r in obj.GetComponentsInChildren<Renderer>(true)) {
                r.enabled = visible;
            }
        }

        private void OnEnable() {
            clickAction.performed += OnClickPerformed;
            ARSceneHandler.Instance?.OnStateChanged.AddListener(OnARStateChanged);
        }

        private void OnDisable() {
            clickAction.performed -= OnClickPerformed;
            ARSceneHandler.Instance?.OnStateChanged.RemoveListener(OnARStateChanged);
            MainMenuManager.Instance.OnMainMenuToggle.RemoveListener(OnMainMenuToggle);
            tm.LevelObjects_ButtonItem1.clicked -= LevelObjects_ClickButton1;
            tm.LevelObjects_ButtonItem2.clicked -= LevelObjects_ClickButton2;
            tm.LevelObjects_ButtonItem3.clicked -= LevelObjects_ClickButton3;
            tm.LevelObjects_ButtonItem4.clicked -= LevelObjects_ClickButton4;
            tm.LevelObjects_ButtonItem5.clicked -= LevelObjects_ClickButton5;
            tm.LevelObjects_ButtonItem6.clicked -= LevelObjects_ClickButton6;
            tm.LevelObjects_ButtonItem7.clicked -= LevelObjects_ClickButton7;
            tm.LevelObjects_ButtonItem8.clicked -= LevelObjects_ClickButton8;
            tm.LevelObjects_ButtonItem9.clicked -= LevelObjects_ClickButton9;
            tm.FieldProperties_ButtonField.clicked -= FieldProperties_ClickButtonField;
        }

        // Update is called once per frame
        void Update() {
            foreach (GameObject obj in rotatingObjects) {
                obj.transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
            }
        }

        private void OnClickPerformed(InputAction.CallbackContext ctx) {
            Vector3 mousePos = pointAction.ReadValue<Vector2>();
            Vector2 panelPos = new Vector2(mousePos.x, Screen.height - mousePos.y); // flip y coordinate
            if (!isZoomed || tm.Pick(panelPos) != null) {
                return; // not zooming or clicking on some toolbar UI element, return
            }

            // reset zoom after clicking on the screen
            isZoomed = false;
            ResetZoom();
        }

        /// <summary>
        /// Callback for <see cref="ARSceneHandler.OnStateChanged"/>.
        /// </summary>
        private void OnARStateChanged(ARState newState) {
            if (newState == ARState.ModelPlaced) {
                return;
            }

            // reset toolbar UI and other state variables when AR model is reset
            ResetAllButtons();
            tm.HideLevelObjects();
            tm.HideFieldProperties();
            isZoomed = false;
        }

        public void FieldProperties_ClickButtonField() {
            plotField++;
            if (plotField > 2) plotField = 0;

            if (showVolumePlot == true) {
                showCurrentVolumePlot();
            }

            if (showPathlines == true) {
                showCurrentPathlines();
            }

            if (plotField == 0) {
                //vpLaufrad.setPField();
                //vpInlet.setPField();
                //vpOutlet.setPField();
                tm.FieldProperties_LabelDim.text = "[kPa]";
                tm.FieldProperties_LabelMaxVal.text = "70";
                tm.FieldProperties_LabelMinVal.text = "-10";
                tm.FieldProperties_ButtonField.style.backgroundImage = new StyleBackground(IconButtonP);
            } else if (plotField == 1) {
                //vpLaufrad.setCabsField();
                //vpInlet.setCabsField();
                //vpOutlet.setCabsField();
                tm.FieldProperties_LabelDim.text = "[m/s]";
                tm.FieldProperties_LabelMaxVal.text = "35";
                tm.FieldProperties_LabelMinVal.text = "0";
                tm.FieldProperties_ButtonField.style.backgroundImage = new StyleBackground(IconButtonC);
            } else if (plotField == 2) {
                //vpLaufrad.setWabsField();
                //vpInlet.gameObject.SetActive(false);
                //vpInlet.setWabsField();
                //vpOutlet.gameObject.SetActive(false);
                //vpOutlet.setWabsField();
                tm.FieldProperties_LabelDim.text = "[m/s]";
                tm.FieldProperties_LabelMaxVal.text = "35";
                tm.FieldProperties_LabelMinVal.text = "0";
                tm.FieldProperties_ButtonField.style.backgroundImage = new StyleBackground(IconButtonW);
            }
        }

        private void LevelObjects_ClickButton(Button button, GameObject obj) {
            if (obj != null) {
                if (obj.activeSelf) {
                    obj.SetActive(false);
                    button.style.opacity = 0.5f;
                } else {
                    obj.SetActive(true);
                    button.style.opacity = 1f;
                }
            }
        }

        private void LevelObjects_ClickButton1() {
            LevelObjects_ClickButton(tm.LevelObjects_ButtonItem1, LevelObject1);
        }

        private void LevelObjects_ClickButton2() {
            LevelObjects_ClickButton(tm.LevelObjects_ButtonItem2, LevelObject2);
        }

        private void LevelObjects_ClickButton3() {
            LevelObjects_ClickButton(tm.LevelObjects_ButtonItem3, LevelObject3);
        }

        private void LevelObjects_ClickButton4() {
            LevelObjects_ClickButton(tm.LevelObjects_ButtonItem4, LevelObject4);
        }

        private void LevelObjects_ClickButton5() {
            LevelObjects_ClickButton(tm.LevelObjects_ButtonItem5, LevelObject5);
        }

        private void LevelObjects_ClickButton6() {
            LevelObjects_ClickButton(tm.LevelObjects_ButtonItem6, LevelObject6);
        }

        private void LevelObjects_ClickButton7() {
            if (showVolumePlot == false) {
                showVolumePlot = true;
                tm.LevelObjects_ButtonItem7.style.opacity = 1f;
                showCurrentVolumePlot();
            } else {
                showVolumePlot = false;
                tm.LevelObjects_ButtonItem7.style.opacity = 0.5f;
                hideCurrentVolumePlot();
            }
        }

        private void LevelObjects_ClickButton8() {
            if (showPathlines == false) {
                showPathlines = true;
                tm.LevelObjects_ButtonItem8.style.opacity = 1f;
                showCurrentPathlines();
            } else {
                showPathlines = false;
                tm.LevelObjects_ButtonItem8.style.opacity = 0.5f;
                hideCurrentPathlines();
            }
        }

        private void LevelObjects_ClickButton9() {
            if (showForces == false) {
                showForces = true;
                tm.LevelObjects_ButtonItem9.style.opacity = 1f;
                showArrows();
            } else {
                showForces = false;
                tm.LevelObjects_ButtonItem9.style.opacity = 0.5f;
                hideArrows();
            }
        }

        private void ResetAllButtons() {
            Button[] buttons = {
                tm.LevelObjects_ButtonItem1, tm.LevelObjects_ButtonItem2, tm.LevelObjects_ButtonItem3,
                tm.LevelObjects_ButtonItem4, tm.LevelObjects_ButtonItem5, tm.LevelObjects_ButtonItem6,
                tm.LevelObjects_ButtonItem7, tm.LevelObjects_ButtonItem8, tm.LevelObjects_ButtonItem9
            };
            foreach (Button button in buttons) {
                button.style.opacity = 1f;
            }
        }

        private void showCurrentVolumePlot() {
            if (plotField == 0) {
                vpLaufrad.setPField();
                vpInlet.setPField();
                vpOutlet.setPField();

                SetVisible(particleInlet, true);
                SetVisible(particleOutlet, true);
                SetVisible(particleLaufrad, true);
            } else if (plotField == 1) {
                vpLaufrad.setCabsField();
                vpInlet.setCabsField();
                vpOutlet.setCabsField();

                SetVisible(particleInlet, true);
                SetVisible(particleOutlet, true);
                SetVisible(particleLaufrad, true);
            } else if (plotField == 2) {
                vpLaufrad.setWabsField();
                vpInlet.setWabsField();
                vpOutlet.setWabsField();

                SetVisible(particleInlet, false);
                SetVisible(particleOutlet, false);
                SetVisible(particleLaufrad, true);
            }
        }

        private void hideCurrentVolumePlot() {
            SetVisible(particleInlet, false);
            SetVisible(particleOutlet, false);
            SetVisible(particleLaufrad, false);
        }

        private void showCurrentPathlines() {
            if (plotField == 0) {
                SetVisible(pathLineLaufradAbsolut, true);
                SetVisible(pathLineLaufradRelativ, false);
                SetVisible(pathLineSpirale, true);
            } else if (plotField == 1) {
                SetVisible(pathLineLaufradAbsolut, true);
                SetVisible(pathLineLaufradRelativ, false);
                SetVisible(pathLineSpirale, true);
            } else if (plotField == 2) {
                SetVisible(pathLineLaufradAbsolut, false);
                SetVisible(pathLineLaufradRelativ, true);
                SetVisible(pathLineSpirale, false);
            }
        }

        private void hideCurrentPathlines() {
            SetVisible(pathLineLaufradAbsolut, false);
            SetVisible(pathLineLaufradRelativ, false);
            SetVisible(pathLineSpirale, false);
        }

        private void showArrows() {
            SetVisible(arrowsFront, true);
            SetVisible(arrowsBack, true);
            SetVisible(arrowsImpuls, true);
            SetVisible(arrowsWelle, true);
        }

        private void hideArrows() {
            SetVisible(arrowsFront, false);
            SetVisible(arrowsBack, false);
            SetVisible(arrowsImpuls, false);
            SetVisible(arrowsWelle, false);
        }


        /// <summary>
        /// Callback for <see cref="PinchToZoom.onZoomIn"/>.
        /// </summary>
        public void OnZoomIn(Vector2 touchCenter, float distance) {
            Vector3 newScale = MainObject.transform.localScale + Vector3.one * pinchToZoomScaleFactor * distance;
            ScaleAround(MainObject.transform, pumpCollider.bounds.center, newScale);
        }

        /// <summary>
        /// Callback for <see cref="PinchToZoom.onZoomOut"/>.
        /// </summary>
        public void OnZoomOut(Vector2 touchCenter, float distance) {
            Vector3 newScale = MainObject.transform.localScale - Vector3.one * pinchToZoomScaleFactor * distance;
            if (newScale.x < mainObjectBaseScale.x) {
                newScale = mainObjectBaseScale;
            }
            ScaleAround(MainObject.transform, pumpCollider.bounds.center, newScale);
        }

        /// <summary>
        /// Callback for <see cref="PinchToZoom.onZoomStart"/>.
        /// </summary>
        public void OnZoomStart() {
            mainObjectBasePosition = MainObject.transform.localPosition;
            mainObjectBaseScale = MainObject.transform.localScale;
        }

        /// <summary>
        /// Callback for <see cref="PinchToZoom.onZoomEnd"/>.
        /// </summary>
        public void OnZoomEnd() {
            // don't snap back immediately after letting go 
            // ResetZoom();
            StartCoroutine(SetIsZoomedCoroutine());
        }

        /// <summary>
        /// Sets <see cref="isZoomed"/> to <c>true</c> in the next frame.
        /// </summary>
        private IEnumerator SetIsZoomedCoroutine() {
            yield return null;
            isZoomed = true;
        }

        /// <summary>
        /// Resets the zoom level of the main object to its initial position and scale.
        /// </summary>
        private void ResetZoom() {
            MainObject.transform.localPosition = mainObjectBasePosition;
            MainObject.transform.localScale = mainObjectBaseScale;
        }

        /// <summary>
        /// Callback for <see cref="MainMenuManager.OnMainMenuToggle"/>.
        /// </summary>
        private void OnMainMenuToggle(bool isOpen) {
            pinchToZoom.IsCheckingForInput = !isOpen;
        }

        private static void ScaleAround(Transform obj, Vector3 pivot, Vector3 newScale) {
            Vector3 oldScale = obj.localScale;
            Vector3 scaleRatio = new(newScale.x / oldScale.x, newScale.y / oldScale.y, newScale.z / oldScale.z);
            Vector3 dir = obj.position - pivot;
            dir = Vector3.Scale(dir, scaleRatio);
            obj.position = pivot + dir;
            obj.localScale = newScale;
        }
    }
}