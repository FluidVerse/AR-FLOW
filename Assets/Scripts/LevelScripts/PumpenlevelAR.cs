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
using static Toolbar.ToolbarButton;

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
            tm.SetButtonVisibility(QuestMenu, false);
            tm.SetButtonVisibility(Back, false);
            tm.SetAreaVisibility(ToolbarArea.LevelObjects, true);
            tm.SetAreaVisibility(ToolbarArea.FieldProperties, true);
            tm.SetButtonVisibility(ToolbarButtonGroups.LevelObjects, false);
            tm.SetupButton(LevelObjects1, IconButton1, LevelObjects_ClickButton1);
            tm.SetupButton(LevelObjects2, IconButton2, LevelObjects_ClickButton2);
            tm.SetupButton(LevelObjects3, IconButton3, LevelObjects_ClickButton3);
            tm.SetupButton(LevelObjects4, IconButton4, LevelObjects_ClickButton4);
            tm.SetupButton(LevelObjects5, IconButton5, LevelObjects_ClickButton5);
            tm.SetupButton(LevelObjects6, IconButton6, LevelObjects_ClickButton6);
            tm.SetupButton(LevelObjects7, IconButton7, LevelObjects_ClickButton7);
            tm.SetupButton(LevelObjects8, IconButton8, LevelObjects_ClickButton8);
            tm.SetupButton(LevelObjects9, IconButton9, LevelObjects_ClickButton9); 

            vpLaufrad = particleLaufrad.GetComponent<VolumePlot>();
            vpInlet = particleInlet.GetComponent<VolumePlot>();
            vpOutlet = particleOutlet.GetComponent<VolumePlot>();

            tm.AddButtonAction(ButtonField, FieldProperties_ClickButtonField);
            tm.SetFieldPropertiesLegendVisibility(true);

            plotField = -1;
            FieldProperties_ClickButtonField();

            showVolumePlot = false;
            tm.SetButtonEnabled(LevelObjects7, false, true);
            hideCurrentVolumePlot();

            showPathlines = false;
            tm.SetButtonEnabled(LevelObjects8, false, true);
            hideCurrentPathlines();

            showForces = false;
            tm.SetButtonEnabled(LevelObjects9, false, true);
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
            tm.RemoveButtonAction(LevelObjects1, LevelObjects_ClickButton1);
            tm.RemoveButtonAction(LevelObjects2, LevelObjects_ClickButton2);
            tm.RemoveButtonAction(LevelObjects3, LevelObjects_ClickButton3);
            tm.RemoveButtonAction(LevelObjects4, LevelObjects_ClickButton4);
            tm.RemoveButtonAction(LevelObjects5, LevelObjects_ClickButton5);
            tm.RemoveButtonAction(LevelObjects6, LevelObjects_ClickButton6);
            tm.RemoveButtonAction(LevelObjects7, LevelObjects_ClickButton7);
            tm.RemoveButtonAction(LevelObjects8, LevelObjects_ClickButton8);
            tm.RemoveButtonAction(LevelObjects9, LevelObjects_ClickButton9);
            tm.RemoveButtonAction(ButtonField, FieldProperties_ClickButtonField);
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
            tm.SetAreaVisibility(ToolbarArea.LevelObjects, false);
            tm.SetAreaVisibility(ToolbarArea.FieldProperties, false);
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
                tm.SetFieldPropertiesLabelDimText("[kPa]");
                tm.SetFieldPropertiesMinMax("-10", "70");
                tm.SetButtonIcon(ButtonField, IconButtonP);
            } else if (plotField == 1) {
                //vpLaufrad.setCabsField();
                //vpInlet.setCabsField();
                //vpOutlet.setCabsField();
                tm.SetFieldPropertiesLabelDimText("[m/s]");
                tm.SetFieldPropertiesMinMax("0", "35");
                tm.SetButtonIcon(ButtonField, IconButtonC);
            } else if (plotField == 2) {
                //vpLaufrad.setWabsField();
                //vpInlet.gameObject.SetActive(false);
                //vpInlet.setWabsField();
                //vpOutlet.gameObject.SetActive(false);
                //vpOutlet.setWabsField();
                tm.SetFieldPropertiesLabelDimText("[m/s]");
                tm.SetFieldPropertiesMinMax("0", "35");
                tm.SetButtonIcon(ButtonField, IconButtonW);
            }
        }

        private void LevelObjects_ClickButton(int index, GameObject obj) {
            if (obj != null) {
                if (obj.activeSelf) {
                    obj.SetActive(false);
                    tm.SetButtonEnabled(ToolbarButtonGroups.LevelObjects[index], false, true);
                } else {
                    obj.SetActive(true);
                    tm.SetButtonEnabled(ToolbarButtonGroups.LevelObjects[index], true, true);
                }
            }
        }

        private void LevelObjects_ClickButton1() {
            LevelObjects_ClickButton(0, LevelObject1);
        }

        private void LevelObjects_ClickButton2() {
            LevelObjects_ClickButton(1, LevelObject2);
        }

        private void LevelObjects_ClickButton3() {
            LevelObjects_ClickButton(2, LevelObject3);
        }

        private void LevelObjects_ClickButton4() {
            LevelObjects_ClickButton(3, LevelObject4);
        }

        private void LevelObjects_ClickButton5() {
            LevelObjects_ClickButton(4, LevelObject5);
        }

        private void LevelObjects_ClickButton6() {
            LevelObjects_ClickButton(5, LevelObject6);
        }

        private void LevelObjects_ClickButton7() {
            if (showVolumePlot == false) {
                showVolumePlot = true;
                tm.SetButtonEnabled(LevelObjects7, true, true);
                showCurrentVolumePlot();
            } else {
                showVolumePlot = false;
                tm.SetButtonEnabled(LevelObjects7, false, true);
                hideCurrentVolumePlot();
            }
        }

        private void LevelObjects_ClickButton8() {
            if (showPathlines == false) {
                showPathlines = true;
                tm.SetButtonEnabled(LevelObjects8, true, true);      
                showCurrentPathlines();
            } else {
                showPathlines = false;
                tm.SetButtonEnabled(LevelObjects8, false, true);
                hideCurrentPathlines();
            }
        }

        private void LevelObjects_ClickButton9() {
            if (showForces == false) {
                showForces = true;
                tm.SetButtonEnabled(LevelObjects9, true, true);;
                showArrows();
            } else {
                showForces = false;
                tm.SetButtonEnabled(LevelObjects9, false, true);
                hideArrows();
            }
        }

        private void ResetAllButtons() {
            tm.SetButtonVisibility(ToolbarButtonGroups.LevelObjects, true);
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