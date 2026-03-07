using System.Collections.Generic;
using System.Linq;
using ActionLog;
using Controls;
using Drawing;
using FlowPhysics;
using Quests;
using Toolbar;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using CameraType = Controls.CameraType;

namespace LevelScripts {
    public enum typeEditMode {
        None,
        moveObjects,
        deleteObjects,
        addUniformFlow,
        addSinkSource,
        addDipole,
        addVortex,
        addCylinder,
        probeLocation
    }

    public enum typeFunction {
        None,
        cylinder,
        vortex,
    }

    public class PotentialFlowLevel : MonoBehaviour {

        public GameObject PlotObject;
        [SerializeField] private SurfacePlotZoom plotZoom;

        public Texture2D IconButton1;
        public Texture2D IconButton2;
        public Texture2D IconButton3;
        public Texture2D IconButton4;
        public Texture2D IconButton5;
        public Texture2D IconButton6;
        public Texture2D IconButton7;
        public Texture2D IconButton12;

        public Texture2D IconButtonCabs;
        public Texture2D IconButtonPhi;
        public Texture2D IconButtonPsi;
        public Texture2D IconButtonP;

        private bool View3D = false;

        public typeEditMode mode = typeEditMode.None;
        public typeFunction funMode = typeFunction.None;

        public Texture2D IconButton2DView;
        public Texture2D IconButton3DView;

        private int plotField = 0; // 


        public Texture2D IconButtonC;
        public Texture2D IconButtonW;

        private ToolbarManager tm;
        private QuestManager questManager;
        private CameraManager cms;
        private DetailCameraMode detailCameraMode;

        private SurfacePlot plot;

        public Sprite ElementMoveMarker;
        private int ElementMoveIndex = -1;

        List<GameObject> ElementMarkers = new List<GameObject>();
        List<GameObject> ProbeMarker = new List<GameObject>();
        List<FlowElement> flowFields = new List<FlowElement>();

        private FlowElement selectedElement;
        private bool _suppressSliderCallbacks = false;

        private InputAction pointAction;
        private InputAction clickAction;

        private ActionLogManager logManager;

        bool showCabsIso = false;
        bool showPhiIso = false;
        bool showPsiIso = false;

        private Vector3 fingerPositionProbe;
        private Vector3 fingerPositionFunction;

        public float berKonst;

        private const float PosStep = 0.1f;


        private void Awake() {
            questManager = FindAnyObjectByType<QuestManager>();
            if (questManager == null) {
                Debug.LogError("QuestManager not found", this);
            }
            tm = FindAnyObjectByType<ToolbarManager>();
            if (tm == null) {
                Debug.LogError("ToolbarManager not found", this);
            }

            pointAction = InputSystem.actions.FindAction("UI/Point", true);
            clickAction = InputSystem.actions.FindAction("UI/Click", true);

            logManager = FindAnyObjectByType<ActionLogManager>();
        }

        void Start() {
            questManager.SetQuestLine(QuestLines.Potential);
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
            tm.LevelObjects_ButtonItem11.style.display = DisplayStyle.Flex;
            tm.LevelObjects_ButtonItem12.style.display = DisplayStyle.Flex;
            tm.ShowButtonFunction();
            tm.FieldProperties_ButtonField.style.display = DisplayStyle.Flex;
            tm.FieldProperties_ButtonFieldA.style.display = DisplayStyle.Flex;
            tm.FieldProperties_ButtonFieldB.style.display = DisplayStyle.Flex;
            tm.FieldProperties_ButtonFieldC.style.display = DisplayStyle.Flex;
            tm.FieldProperties_ButtonFieldA.style.opacity = 0.5f;
            tm.FieldProperties_ButtonFieldB.style.opacity = 0.5f;
            tm.FieldProperties_ButtonFieldC.style.opacity = 0.5f;

            tm.LevelObjects_ButtonItem1.clicked += LevelObjects_ClickButton1;
            tm.LevelObjects_ButtonItem2.clicked += LevelObjects_ClickButton2;
            tm.LevelObjects_ButtonItem3.clicked += LevelObjects_ClickButton3;
            tm.LevelObjects_ButtonItem4.clicked += LevelObjects_ClickButton4;
            tm.LevelObjects_ButtonItem5.clicked += LevelObjects_ClickButton5;
            tm.LevelObjects_ButtonItem6.clicked += LevelObjects_ClickButton6;
            tm.LevelObjects_ButtonItem7.clicked += LevelObjects_ClickButton7;
            tm.LevelObjects_ButtonItem12.clicked += LevelObjects_ClickButton12;

            tm.LevelObjects_ButtonItem11.clicked += LevelObjects_ClickButton11;

            tm.Toolbar_ButtonFunction.clicked += LevelObjects_ClickButtonFunction;

            tm.LevelObjects_ButtonItem1.style.backgroundImage = new StyleBackground(IconButton1);
            tm.LevelObjects_ButtonItem2.style.backgroundImage = new StyleBackground(IconButton2);
            tm.LevelObjects_ButtonItem3.style.backgroundImage = new StyleBackground(IconButton3);
            tm.LevelObjects_ButtonItem4.style.backgroundImage = new StyleBackground(IconButton4);
            tm.LevelObjects_ButtonItem5.style.backgroundImage = new StyleBackground(IconButton5);
            tm.LevelObjects_ButtonItem6.style.backgroundImage = new StyleBackground(IconButton6);
            tm.LevelObjects_ButtonItem7.style.backgroundImage = new StyleBackground(IconButton7);
            tm.LevelObjects_ButtonItem12.style.backgroundImage = new StyleBackground(IconButton12);

            tm.LevelObjects_ButtonItem11.style.display = DisplayStyle.Flex;
            tm.LevelObjects_ButtonItem11.style.backgroundImage = new StyleBackground(IconButton2DView);

            plot = PlotObject.GetComponent<SurfacePlot>();

            tm.FieldProperties_ButtonField.clicked += FieldProperties_ClickButtonField;
            tm.FieldProperties_ButtonFieldA.clicked += FieldProperties_ClickButtonFieldCabs;
            tm.FieldProperties_ButtonFieldB.clicked += FieldProperties_ClickButtonFieldPhi;
            tm.FieldProperties_ButtonFieldC.clicked += FieldProperties_ClickButtonFieldPsi;

            plotField = -1;
            FieldProperties_ClickButtonField();

            tm.FieldProperties_ButtonFieldA.style.display = DisplayStyle.Flex;
            tm.FieldProperties_ButtonFieldA.style.backgroundImage = new StyleBackground(IconButtonCabs);
            tm.FieldProperties_ButtonFieldB.style.display = DisplayStyle.Flex;
            tm.FieldProperties_ButtonFieldB.style.backgroundImage = new StyleBackground(IconButtonPhi);
            tm.FieldProperties_ButtonFieldC.style.display = DisplayStyle.Flex;
            tm.FieldProperties_ButtonFieldC.style.backgroundImage = new StyleBackground(IconButtonPsi);

            tm.LevelObjects_PSlider1.RegisterValueChangedCallback(OnSlider1Changed);
            tm.LevelObjects_PSlider2.RegisterValueChangedCallback(OnSlider2Changed);

            cms = FindAnyObjectByType<CameraManager>();
            cms.ChangeCameraOnBackButton = false;
            detailCameraMode = FindAnyObjectByType<DetailCameraMode>();
            detailCameraMode.DetailObject = PlotObject;
            detailCameraMode.MidPoint = new Vector3(1f, 1f, 0f);

            AddUniformFlow(new Vector2(1, 0));
            //AddCylinder(0.2f, new Vector2(1, 1));
            AddSinkSource(0.2f, new Vector2(1, 1));
            SetMarkersActive(false);
            plot.UpdateField();
            updateColorbar();

            ChangeTo2DView();
            positionToFunctions();
        }


        private void AddUniformFlow(Vector2 Velocity) {
            plot.AddUniformFlow(Velocity);
            NewMarker(new Vector2(0, 0));
            calculateBernoulliNumber();
            //berKonst = 1000/2 * Velocity.magnitude * Velocity.magnitude + 100000; // (rho/2*|(u,v)_infinity|^2+p_infinity)
        }

        private void AddSinkSource(float amplitude, Vector2 position) {
            plot.AddSinkSource(amplitude, position);
            NewMarker(position);
        }

        private void AddDipole(float amplitude, Vector2 position) {
            plot.AddDipole(amplitude, position);
            NewMarker(position);
        }

        private void AddVortex(float amplitude, Vector2 position) {
            plot.AddVortex(amplitude, position);
            NewMarker(position);
        }

        private void AddCylinder(float radius, Vector2 position) {
            plot.AddCylinder(radius, position);
            NewMarker(position);
        }

        private void NewMarker(Vector2 position) {
            var go = new GameObject("MoveMarker");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ElementMoveMarker;
            sr.sortingOrder = 100;

            int N = plot.flowField.Count();
            sr.transform.position = position;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.01f;

            go.SetActive(false);
            ElementMarkers.Add(go);

            // SurfacePlot sp = PlotObject.GetComponent<SurfacePlot>();
            //sr.transform.position = sp.flowField.elements[1].GetPosition();
        }

        private void NewProbe(Vector2 position) {
            //ALEX: check if there's an existing probe marker and if yes, use this, if false create one
            //if (ProbeMarker)
            //{
            //    
            //}
            var go = new GameObject("MoveMarker");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ElementMoveMarker;
            sr.sortingOrder = 100;

            int N = plot.flowField.Count();
            sr.transform.position = position;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.01f;

            go.SetActive(false);
            ProbeMarker.Add(go);
        }


        private void OnDisable() {
            tm.LevelObjects_ButtonItem1.clicked -= LevelObjects_ClickButton1;
            tm.LevelObjects_ButtonItem2.clicked -= LevelObjects_ClickButton2;
            tm.LevelObjects_ButtonItem3.clicked -= LevelObjects_ClickButton3;
            tm.LevelObjects_ButtonItem4.clicked -= LevelObjects_ClickButton4;
            tm.LevelObjects_ButtonItem5.clicked -= LevelObjects_ClickButton5;
            tm.LevelObjects_ButtonItem6.clicked -= LevelObjects_ClickButton6;
            tm.FieldProperties_ButtonField.clicked -= FieldProperties_ClickButtonField;
            tm.FieldProperties_ButtonFieldA.clicked -= FieldProperties_ClickButtonFieldCabs;
            tm.FieldProperties_ButtonFieldB.clicked -= FieldProperties_ClickButtonFieldPhi;
            tm.FieldProperties_ButtonFieldC.clicked -= FieldProperties_ClickButtonFieldPsi;
        }

        private int MarkerClick(Vector2 clickPosition) {
            float tol = 0.1f;

            for (int i = 0; i < ElementMarkers.Count; i++) {
                if (Vector2.Distance(clickPosition, (Vector2)ElementMarkers[i].transform.position) <= tol) {
                    return i;
                }
            }
            return -1;
        }

        private int ProbeClick(Vector2 clickPosition) {
            float tol = 0.1f;

            for (int i = 0; i < ProbeMarker.Count; i++) {
                if (Vector2.Distance(clickPosition, (Vector2)ProbeMarker[i].transform.position) <= tol) {
                    return i;
                }
            }
            return -1;
        }

        private void updateColorbar() {
            //tm.FieldProperties_LabelDim.text = "[m/s]";
            tm.FieldProperties_LabelMaxVal.text = plot.maximumValue.ToString("F1");
            tm.FieldProperties_LabelMinVal.text = plot.minimumValue.ToString("F1");
        }

        // Update is called once per frame
        void Update() {
            if (!View3D) {
                Vector2 screenPos = pointAction.ReadValue<Vector2>();
                Vector2 uiPos = new Vector2(screenPos.x, Screen.height - screenPos.y);
                if (tm.Pick(uiPos) == null) {
                    if (clickAction.WasPressedThisFrame()) {
                        if (mode != typeEditMode.addUniformFlow && mode != typeEditMode.addSinkSource &&
                            mode != typeEditMode.addDipole && mode != typeEditMode.addVortex) {
                            tm.HidePosition();
                            tm.HideProperty1();
                            tm.HideProperty2();
                        }
                    }

                    if (mode == typeEditMode.moveObjects) {
                        Vector3 fingerPosition;
                        var cam = Camera.main;
                        //Vector2 screenPos = pointAction.ReadValue<Vector2>();
                        fingerPosition = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 5f));
                        //tm.ShowFunctionCyl(fingerPosition.x, fingerPosition.y);


                        if (clickAction.WasPressedThisFrame()) {
                            //fingerPosition = plot.SnapToGrid(cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 5f)));
                            fingerPosition = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 5f));
                            Vector2 snapped = Snap((Vector2)fingerPosition, PosStep);
                            fingerPosition = new Vector3(snapped.x, snapped.y, fingerPosition.z);

                            ElementMoveIndex = MarkerClick((Vector2)fingerPosition);
                            if (ElementMoveIndex != -1) {
                                tm.ShowPosition();
                                tm.WritePosition(berKonst, fingerPosition.x, fingerPosition.y);

                                if (funMode == typeFunction.cylinder) {
                                    tm.ShowFunctionCyl(fingerPosition.x, fingerPosition.y);
                                } else if (funMode == typeFunction.vortex) {
                                    tm.ShowFunctionVort(fingerPosition.x, fingerPosition.y);
                                }
                                //positionToFunctions();
                                //tm.ShowFunctionCyl(fingerPosition.x, fingerPosition.y);

                                selectedElement = plot.flowField.elements[ElementMoveIndex];
                                if (selectedElement is UniformFlow flow) {
                                    _suppressSliderCallbacks = true;
                                    tm.ShowProperty1();
                                    tm.ShowProperty2();
                                    tm.LevelObjects_PSlider1.label = "cx";
                                    tm.LevelObjects_PSlider1.lowValue = -1;
                                    tm.LevelObjects_PSlider1.highValue = 1;
                                    tm.LevelObjects_PSlider1.pageSize = 0f;
                                    tm.LevelObjects_PSlider1.SetValueWithoutNotify(flow.c.x);
                                    tm.LevelObjects_PValue1.text = flow.c.x.ToString("F1");
                                    tm.LevelObjects_PSlider2.label = "cy";
                                    tm.LevelObjects_PSlider2.lowValue = -1;
                                    tm.LevelObjects_PSlider2.highValue = 1;
                                    tm.LevelObjects_PSlider2.pageSize = 0f;
                                    tm.LevelObjects_PSlider2.SetValueWithoutNotify(flow.c.y);
                                    tm.LevelObjects_PValue2.text = flow.c.y.ToString("F1");
                                    _suppressSliderCallbacks = false;
                                } else if (selectedElement is SourceSink source) {
                                    _suppressSliderCallbacks = true;
                                    tm.ShowProperty1();
                                    tm.HideProperty2();

                                    tm.LevelObjects_PSlider1.label = "Source intesity";
                                    tm.LevelObjects_PSlider1.SetValueWithoutNotify(source.strength);
                                    tm.LevelObjects_PSlider1.lowValue = -2f;
                                    tm.LevelObjects_PSlider1.highValue = 2f;
                                    tm.LevelObjects_PValue1.text = source.strength.ToString("F1");
                                    _suppressSliderCallbacks = false;
                                } else if (selectedElement is Dipole dipole) {
                                    _suppressSliderCallbacks = true;
                                    tm.ShowProperty1();
                                    tm.HideProperty2();

                                    tm.LevelObjects_PSlider1.label = "Dipole intesity";
                                    tm.LevelObjects_PSlider1.SetValueWithoutNotify(dipole.strength);
                                    tm.LevelObjects_PSlider1.lowValue = -0.5f;
                                    tm.LevelObjects_PSlider1.highValue = 0.5f;
                                    tm.LevelObjects_PValue1.text = dipole.strength.ToString("F1");
                                    _suppressSliderCallbacks = false;
                                } else if (selectedElement is Vortex vortex) {
                                    _suppressSliderCallbacks = true;
                                    tm.ShowProperty1();
                                    tm.HideProperty2();

                                    tm.LevelObjects_PSlider1.label = "Vortex intesity";
                                    tm.LevelObjects_PSlider1.SetValueWithoutNotify(vortex.strength);
                                    tm.LevelObjects_PSlider1.lowValue = -0.5f;
                                    tm.LevelObjects_PSlider1.highValue = 0.5f;
                                    tm.LevelObjects_PValue1.text = vortex.strength.ToString("F1");
                                    _suppressSliderCallbacks = false;
                                } else if (selectedElement is Cylinder cylinder) {
                                    _suppressSliderCallbacks = true;

                                    _suppressSliderCallbacks = false;
                                }
                            }
                        } else if (clickAction.IsPressed()) {
                            if (ElementMoveIndex != -1) {
                                //fingerPosition = plot.SnapToGrid(cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 5f)));
                                fingerPosition = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 5f));
                                Vector2 snapped = Snap((Vector2)fingerPosition, PosStep);
                                fingerPosition = new Vector3(snapped.x, snapped.y, fingerPosition.z);

                                ElementMarkers[ElementMoveIndex].transform.position =
                                    new Vector3(fingerPosition.x, fingerPosition.y, 0f);
                                tm.WritePosition(berKonst, fingerPosition.x, fingerPosition.y);
                                positionToFunctions();
                                if (funMode == typeFunction.cylinder) {
                                    tm.ShowFunctionCyl(fingerPosition.x, fingerPosition.y);
                                } else if (funMode == typeFunction.vortex) {
                                    tm.ShowFunctionVort(fingerPosition.x, fingerPosition.y);
                                }
                            }
                        } else if (clickAction.WasReleasedThisFrame()) {
                            if (ElementMoveIndex != -1) {
                                //fingerPosition = plot.SnapToGrid(cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 5f)));
                                fingerPosition = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 5f));
                                Vector2 snapped = Snap((Vector2)fingerPosition, PosStep);
                                fingerPosition = new Vector3(snapped.x, snapped.y, fingerPosition.z);

                                plot.flowField.elements[ElementMoveIndex].SetPosition((Vector2)fingerPosition);
                                plot.UpdateField();
                                updateColorbar();
                            }
                            tm.HidePosition();
                            ElementMoveIndex = -1;
                        }
                    } else if (mode == typeEditMode.deleteObjects) {
                        Vector3 fingerPosition;
                        var cam = Camera.main;
                        //Vector2 screenPos = pointAction.ReadValue<Vector2>();

                        if (clickAction.WasPressedThisFrame()) {
                            fingerPosition =
                                plot.SnapToGrid(cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 5f)));
                            ElementMoveIndex = MarkerClick((Vector2)fingerPosition);

                            if (ElementMoveIndex != -1) {
                                RemoveElement(ElementMoveIndex);
                                plot.UpdateField();
                                updateColorbar();
                                calculateBernoulliNumber();
                            }
                        }
                    } else if (mode == typeEditMode.addUniformFlow) {
                        Vector3 fingerPosition;
                        var cam = Camera.main;
                        //Vector2 screenPos = pointAction.ReadValue<Vector2>();

                        tm.LevelObjects_PValue1.text = tm.LevelObjects_PSlider1.value.ToString("F1");
                        tm.LevelObjects_PValue2.text = tm.LevelObjects_PSlider2.value.ToString("F1");

                        if (clickAction.WasPressedThisFrame()) {
                            //fingerPosition = plot.SnapToGrid(cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 5f)));
                            fingerPosition = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 5f));
                            Vector2 snapped = Snap((Vector2)fingerPosition, PosStep);
                            fingerPosition = new Vector3(snapped.x, snapped.y, fingerPosition.z);

                            float cx = tm.LevelObjects_PSlider1.value;
                            float cy = tm.LevelObjects_PSlider2.value;
                            AddUniformFlow(new Vector2(cx, cy));
                            plot.UpdateField();
                            updateColorbar();
                            calculateBernoulliNumber();
                        }
                    } else if (mode == typeEditMode.addSinkSource) {
                        Vector3 fingerPosition;
                        var cam = Camera.main;
                        //Vector2 screenPos = pointAction.ReadValue<Vector2>();

                        if (clickAction.WasPressedThisFrame()) {
                            //fingerPosition = plot.SnapToGrid(cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 5f)));
                            fingerPosition = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 5f));
                            Vector2 snapped = Snap((Vector2)fingerPosition, PosStep);
                            fingerPosition = new Vector3(snapped.x, snapped.y, fingerPosition.z);

                            float strength = tm.LevelObjects_PSlider1.value;
                            AddSinkSource(strength, (Vector2)fingerPosition);
                            plot.UpdateField();
                            updateColorbar();
                        }
                    } else if (mode == typeEditMode.addDipole) {
                        Vector3 fingerPosition;
                        var cam = Camera.main;
                        //Vector2 screenPos = pointAction.ReadValue<Vector2>();

                        if (clickAction.WasPressedThisFrame()) {
                            //fingerPosition = plot.SnapToGrid(cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 5f)));
                            fingerPosition = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 5f));
                            Vector2 snapped = Snap((Vector2)fingerPosition, PosStep);
                            fingerPosition = new Vector3(snapped.x, snapped.y, fingerPosition.z);

                            float strength = tm.LevelObjects_PSlider1.value;
                            AddDipole(strength, (Vector2)fingerPosition);
                            plot.UpdateField();
                            updateColorbar();
                        }
                    } else if (mode == typeEditMode.addVortex) {
                        Vector3 fingerPosition;
                        var cam = Camera.main;
                        //Vector2 screenPos = pointAction.ReadValue<Vector2>();

                        positionToFunctions();
                        if (clickAction.WasPressedThisFrame()) {
                            //fingerPosition = plot.SnapToGrid(cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 5f)));
                            fingerPosition = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 5f));
                            Vector2 snapped = Snap((Vector2)fingerPosition, PosStep);
                            fingerPosition = new Vector3(snapped.x, snapped.y, fingerPosition.z);

                            float strength = tm.LevelObjects_PSlider1.value;
                            AddVortex(strength, (Vector2)fingerPosition);
                            plot.UpdateField();
                            updateColorbar();
                        }
                    } else if (mode == typeEditMode.addCylinder) {
                        Vector3 fingerPosition;
                        var cam = Camera.main;
                        //Vector2 screenPos = pointAction.ReadValue<Vector2>();
                        positionToFunctions();
                        if (clickAction.WasPressedThisFrame()) {
                            //fingerPosition = plot.SnapToGrid(cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 5f)));
                            fingerPosition = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 5f));
                            Vector2 snapped = Snap((Vector2)fingerPosition, PosStep);
                            fingerPosition = new Vector3(snapped.x, snapped.y, fingerPosition.z);
                            AddCylinder(0.1f, (Vector2)fingerPosition);
                            plot.UpdateField();
                            updateColorbar();
                        }
                    } else if (mode == typeEditMode.probeLocation) {
                        //
                        var cam = Camera.main;
                        fingerPositionProbe = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 5f));
                        //tm.ShowFunctionCyl(fingerPositionProbe.x, fingerPositionProbe.y);

                        if (clickAction.WasPressedThisFrame()) {
                            fingerPositionProbe =
                                plot.SnapToGrid(cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 5f)));
                            tm.ShowPosition();
                            tm.WritePosition(berKonst, fingerPositionProbe.x, fingerPositionProbe.y);
                        } else if (clickAction.IsPressed()) {
                            fingerPositionProbe =
                                plot.SnapToGrid(cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 5f)));
                            tm.ShowPosition();
                            tm.WritePosition(berKonst, fingerPositionProbe.x, fingerPositionProbe.y);
                        }

                        //
                    }
                } else {
                    /*tm.LevelObjects_PValue1.text = tm.LevelObjects_PSlider1.value.ToString("F1");
                    tm.LevelObjects_PValue2.text = tm.LevelObjects_PSlider2.value.ToString("F1");
                }
                else if (mode == typeEditMode.addSinkSource)
                {
                    tm.LevelObjects_PValue1.text = tm.LevelObjects_PSlider1.value.ToString("F1");
                }
                else if (mode == typeEditMode.addDipole)
                {
                    tm.LevelObjects_PValue1.text = tm.LevelObjects_PSlider1.value.ToString("F1");
                }
                else if (mode == typeEditMode.addVortex)
                {
                    tm.LevelObjects_PValue1.text = tm.LevelObjects_PSlider1.value.ToString("F1");
                }
                else if (mode == typeEditMode.moveObjects)
                {
                }*/
                }
            }
        }


        public void FieldProperties_ClickButtonField() {
            plotField++;
            if (plotField > 3) plotField = 0;

            if (plotField == 0) {
                tm.FieldProperties_LabelDim.text = "[m/s]";
                tm.FieldProperties_LabelMaxVal.text = plot.maxCabs.ToString("F1");
                tm.FieldProperties_LabelMinVal.text = plot.minCabs.ToString("F1");
                tm.FieldProperties_ButtonField.style.backgroundImage = new StyleBackground(IconButtonCabs);
                plot.SetDisplayMode(SurfacePlot.DisplayMode.Cabs);
            } else if (plotField == 1) {
                tm.FieldProperties_LabelDim.text = "[m²/s]";
                tm.FieldProperties_LabelMaxVal.text = plot.maxPhi.ToString("F1");
                tm.FieldProperties_LabelMinVal.text = plot.minPhi.ToString("F1");
                tm.FieldProperties_ButtonField.style.backgroundImage = new StyleBackground(IconButtonPhi);
                plot.SetDisplayMode(SurfacePlot.DisplayMode.Phi);
            } else if (plotField == 2) {
                tm.FieldProperties_LabelDim.text = "[m²/s]";
                tm.FieldProperties_LabelMaxVal.text = plot.maxPsi.ToString("F1");
                tm.FieldProperties_LabelMinVal.text = plot.minPsi.ToString("F1");
                tm.FieldProperties_ButtonField.style.backgroundImage = new StyleBackground(IconButtonPsi);
                plot.SetDisplayMode(SurfacePlot.DisplayMode.Psi);
            } else if (plotField == 3) {
                tm.FieldProperties_LabelDim.text = "[Pa]";
                tm.FieldProperties_LabelMaxVal.text = plot.maxP.ToString("F1");
                tm.FieldProperties_LabelMinVal.text = plot.minP.ToString("F1");
                tm.FieldProperties_ButtonField.style.backgroundImage = new StyleBackground(IconButtonP);
                plot.SetDisplayMode(SurfacePlot.DisplayMode.P);
            }
        }

        public void FieldProperties_ClickButtonFieldCabs() {
            if (showCabsIso) {
                showCabsIso = false;
                tm.FieldProperties_ButtonFieldA.style.opacity = 0.5f;
            } else {
                showCabsIso = true;
                tm.FieldProperties_ButtonFieldA.style.opacity = 1.0f;
            }
            plot.SetIsoLineTransparency(showCabsIso ? 1 : 0, showPhiIso ? 1 : 0, showPsiIso ? 1 : 0);
        }

        public void FieldProperties_ClickButtonFieldPhi() {
            if (showPhiIso) {
                showPhiIso = false;
                tm.FieldProperties_ButtonFieldB.style.opacity = 0.5f;
            } else {
                showPhiIso = true;
                tm.FieldProperties_ButtonFieldB.style.opacity = 1.0f;
            }
            plot.SetIsoLineTransparency(showCabsIso ? 1 : 0, showPhiIso ? 1 : 0, showPsiIso ? 1 : 0);
        }

        public void FieldProperties_ClickButtonFieldPsi() {
            if (showPsiIso) {
                showPsiIso = false;
                tm.FieldProperties_ButtonFieldC.style.opacity = 0.5f;
            } else {
                showPsiIso = true;
                tm.FieldProperties_ButtonFieldC.style.opacity = 1.0f;
            }
            plot.SetIsoLineTransparency(showCabsIso ? 1 : 0, showPhiIso ? 1 : 0, showPsiIso ? 1 : 0);
        }

        private static Vector2 Snap(Vector2 p, float step) {
            float x = Mathf.Round(p.x / step) * step;
            float y = Mathf.Round(p.y / step) * step;
            return new Vector2(x, y);
        }

        private void OnSlider1Changed(ChangeEvent<float> evt) {
            if (_suppressSliderCallbacks) return;


            float value = Mathf.Round(evt.newValue * 10f) / 10f;
            tm.LevelObjects_PSlider1.SetValueWithoutNotify(value);
            tm.LevelObjects_PValue1.text = value.ToString("F1");

            if (mode == typeEditMode.addSinkSource) {
                value = Mathf.Round(evt.newValue * 5f) / 5f;
                tm.LevelObjects_PSlider1.SetValueWithoutNotify(value);
            }

            // Nur wenn gerade ein SourceSink selektiert ist:
            if (mode == typeEditMode.moveObjects) {
                if (selectedElement is UniformFlow flow) {
                    flow.c = new Vector2(value, flow.c.y);
                    plot.UpdateField();
                    updateColorbar();
                } else if (selectedElement is SourceSink source) {
                    value = Mathf.Round(evt.newValue * 5f) / 5f;
                    tm.LevelObjects_PSlider1.SetValueWithoutNotify(value);
                    tm.LevelObjects_PValue1.text = value.ToString("F1");
                    source.strength = value;
                    plot.UpdateField();
                    updateColorbar();
                } else if (selectedElement is Dipole dipole) {
                    dipole.strength = value;
                    plot.UpdateField();
                    updateColorbar();
                } else if (selectedElement is Vortex vortex) {
                    vortex.strength = value;
                    plot.UpdateField();
                    updateColorbar();
                } else if (selectedElement is Cylinder cylinder) {
                    // ToDo
                }
            }
        }

        private void OnSlider2Changed(ChangeEvent<float> evt) {
            if (_suppressSliderCallbacks) return;

            float value = Mathf.Round(evt.newValue * 10f) / 10f;
            tm.LevelObjects_PSlider2.SetValueWithoutNotify(value);
            tm.LevelObjects_PValue2.text = value.ToString("F1");

            // Nur wenn gerade ein SourceSink selektiert ist:
            if (mode == typeEditMode.moveObjects) {
                if (selectedElement is UniformFlow flow) {
                    flow.c = new Vector2(flow.c.x, value);
                    plot.UpdateField();
                    updateColorbar();
                }
            }
        }

        public void ChangeTo2DView() {
            plotZoom.ZoomEnabled = false;
            tm.LevelObjects_ButtonItem11.style.backgroundImage = new StyleBackground(IconButton2DView);
            //cms.ChangeBackToOldView();
            cms.camType = CameraType.Fixed;
            detailCameraMode.IsActive = false;
            View3D = false;

            Camera.main.transform.position = new Vector3(1f, 1.18f, -5f);
            Camera.main.transform.rotation = Quaternion.LookRotation(new Vector3(0f, 0f, 1f), Vector3.up);
            Camera.main.orthographic = true;
            Camera.main.orthographicSize = 1.2f;

            PlotObject.transform.rotation = Quaternion.identity;
            PlotObject.transform.position = new Vector3(0f, 0f, 0f);
            tm.SetButtonState(tm.LevelObjects_ButtonItem1, true);
            tm.SetButtonState(tm.LevelObjects_ButtonItem2, true);
            tm.SetButtonState(tm.LevelObjects_ButtonItem3, true);
            tm.SetButtonState(tm.LevelObjects_ButtonItem4, true);
            tm.SetButtonState(tm.LevelObjects_ButtonItem5, true);
            tm.SetButtonState(tm.LevelObjects_ButtonItem6, true);
            tm.SetButtonState(tm.LevelObjects_ButtonItem7, true);
            tm.SetButtonState(tm.LevelObjects_ButtonItem12, true);
            Debug.Log("ChangeTo2DView");
        }

        public void ChangeTo3DView() {
            plotZoom.ZoomEnabled = true;
            tm.LevelObjects_ButtonItem11.style.backgroundImage = new StyleBackground(IconButton3DView);
            //cms.ChangeToDetailView(PlotObject);
            cms.camType = CameraType.DetailView;
            detailCameraMode.IsActive = true;
            Camera.main.orthographic = false;
            View3D = true;
            tm.SetButtonState(tm.LevelObjects_ButtonItem1, false);
            tm.SetButtonState(tm.LevelObjects_ButtonItem2, false);
            tm.SetButtonState(tm.LevelObjects_ButtonItem3, false);
            tm.SetButtonState(tm.LevelObjects_ButtonItem4, false);
            tm.SetButtonState(tm.LevelObjects_ButtonItem5, false);
            tm.SetButtonState(tm.LevelObjects_ButtonItem6, false);
            tm.SetButtonState(tm.LevelObjects_ButtonItem7, false);
            tm.SetButtonState(tm.LevelObjects_ButtonItem12, false);
            Debug.Log("ChangeTo3DView");
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

        private void SetMarkersActive(bool state) {
            for (int i = 0; i < ElementMarkers.Count; i++) {
                ElementMarkers[i].SetActive(state);
            }
        }

        private void SetProbeActive(bool state) {
        }

        private void RemoveElement(int index) {
            if (index >= 0) {
                GameObject obj = ElementMarkers[index];
                Destroy(obj);
                ElementMarkers.RemoveAt(index);
                plot.flowField.RemoveElement(index);
            }
        }

        private void LevelObjects_ClickButton1() {
            SetMarkersActive(false);
            mode = typeEditMode.addUniformFlow;
            logManager.Write(LogMessages.ChoseToolUniformFlow);
            tm.HidePosition();
            tm.ShowProperty1();
            tm.ShowProperty2();
            tm.LevelObjects_PSlider1.label = "cx";
            tm.LevelObjects_PSlider1.value = 0;
            tm.LevelObjects_PSlider1.lowValue = -1;
            tm.LevelObjects_PSlider1.highValue = 1;
            tm.LevelObjects_PValue1.text = tm.LevelObjects_PSlider1.value.ToString("F1");
            tm.LevelObjects_PSlider2.label = "cy";
            tm.LevelObjects_PSlider2.value = 0;
            tm.LevelObjects_PSlider2.lowValue = -1;
            tm.LevelObjects_PSlider2.highValue = 1;
            tm.LevelObjects_PValue2.text = tm.LevelObjects_PSlider2.value.ToString("F1");
        }

        private void LevelObjects_ClickButton2() {
            SetMarkersActive(false);
            mode = typeEditMode.addSinkSource;
            logManager.Write(LogMessages.ChoseToolHeatSink);
            tm.HidePosition();
            tm.ShowProperty1();
            tm.HideProperty2();

            tm.LevelObjects_PSlider1.label = "Source intesity";
            tm.LevelObjects_PSlider1.value = 1.0f;
            tm.LevelObjects_PSlider1.lowValue = -2f;
            tm.LevelObjects_PSlider1.highValue = 2f;
            tm.LevelObjects_PValue1.text = tm.LevelObjects_PSlider1.value.ToString("F1");
        }

        private void LevelObjects_ClickButton3() {
            SetMarkersActive(false);
            mode = typeEditMode.addDipole;
            logManager.Write(LogMessages.ChoseToolDipole);
            tm.HidePosition();
            tm.ShowProperty1();
            tm.HideProperty2();

            tm.LevelObjects_PSlider1.label = "Dipole intesity";
            tm.LevelObjects_PSlider1.value = 0.1f;
            tm.LevelObjects_PSlider1.lowValue = -0.5f;
            tm.LevelObjects_PSlider1.highValue = 0.5f;
            tm.LevelObjects_PValue1.text = tm.LevelObjects_PSlider1.value.ToString("F1");
        }

        private void LevelObjects_ClickButton4() {
            SetMarkersActive(false);
            logManager.Write(LogMessages.ChoseToolVortex);
            mode = typeEditMode.addVortex;
            tm.HidePosition();
            tm.ShowProperty1();
            tm.HideProperty2();

            tm.LevelObjects_PSlider1.label = "Vortex intesity";
            tm.LevelObjects_PSlider1.value = 0.1f;
            tm.LevelObjects_PSlider1.lowValue = -0.5f;
            tm.LevelObjects_PSlider1.highValue = 0.5f;
            tm.LevelObjects_PValue1.text = tm.LevelObjects_PSlider1.value.ToString("F1");
        }

        private void LevelObjects_ClickButton5() {
            SetMarkersActive(false);
            mode = typeEditMode.addCylinder;
            logManager.Write(LogMessages.ChoseToolCylinder);
            tm.HidePosition();
            tm.HideProperty1();
            tm.HideProperty2();
        }

        private void LevelObjects_ClickButton6() {
            tm.HidePosition();
            tm.HideProperty1();
            tm.HideProperty2();
            if (mode != typeEditMode.moveObjects) {
                SetMarkersActive(true);
                mode = typeEditMode.moveObjects;
                logManager.Write(LogMessages.ChoseToolMoveElement);
            } else {
                SetMarkersActive(false);
                mode = typeEditMode.None;
            }
        }

        private void LevelObjects_ClickButton7() {
            tm.HidePosition();
            tm.HideProperty1();
            tm.HideProperty2();
            if (mode != typeEditMode.deleteObjects) {
                SetMarkersActive(true);
                mode = typeEditMode.deleteObjects;
                logManager.Write(LogMessages.ChoseToolRemoveElement);
            } else {
                SetMarkersActive(false);
                mode = typeEditMode.None;
            }
        }


        private void LevelObjects_ClickButton11() {
            tm.HidePosition();
            tm.HideProperty1();
            tm.HideProperty2();
            SetMarkersActive(false);
            mode = typeEditMode.None;
            if (View3D) {
                ChangeTo2DView();
                logManager.Write(LogMessages.ChangeTo2DView);
            } else {
                ChangeTo3DView();
                logManager.Write(LogMessages.ChangeTo3DView);
            }
        }

        private void LevelObjects_ClickButton12() {
            //ALEX: TBD - make own "SetMarkersActive" for probe
            SetMarkersActive(false);
            //mode = typeEditMode.probeLocation;
            //NewProbe();

            if (mode != typeEditMode.probeLocation) {
                //enter probe mode
                //SetMarkersActive(true);
                SetProbeActive(true);
                mode = typeEditMode.probeLocation;
                logManager.Write(LogMessages.ChoseProbe);
            } else {
                //exit probe mode
                //SetMarkersActive(false);
                SetProbeActive(false);
                mode = typeEditMode.None;
            }
        }

        private void calculateBernoulliNumber() {
            Vector2 totalVelocity = new Vector2(0, 0);
            IEnumerable<UniformFlow> flowFields = plot.flowField.elements.OfType<UniformFlow>();
            foreach (UniformFlow field in flowFields) {
                totalVelocity += field.c;
            }
            berKonst = 1000 / 2 * totalVelocity.magnitude * totalVelocity.magnitude +
                       100000; // (rho/2*|(u,v)_infinity|^2+p_infinity)
        }

        private void positionToFunctions() {
            bool condCyl =
                plot.flowField.Count() == 2 &&
                plot.flowField.elements.Any(e => e.GetType().Name.Contains("Uniform")) &&
                plot.flowField.elements.Any(e => e.GetType().Name.Contains("Cylinder"));

            bool condVort =
                plot.flowField.Count() == 2 &&
                plot.flowField.elements.Any(e => e.GetType().Name.Contains("Uniform")) &&
                plot.flowField.elements.Any(e => e.GetType().Name.Contains("Vortex"));

            if (condCyl) {
                Cylinder cylInst = plot.flowField.elements.OfType<Cylinder>().FirstOrDefault();
                funMode = typeFunction.cylinder;
                float cylXpos = cylInst.GetPosition().x;
                float cylYpos = cylInst.GetPosition().y;
                tm.ShowFunctionCyl(cylXpos, cylYpos);
                if (tm.LevelObjects_Position.style.display == DisplayStyle.None) {
                    tm.ShowPosition();
                    tm.WritePosition(berKonst, cylXpos, cylYpos);
                }
            } else if (condVort) {
                Vortex vortInst = plot.flowField.elements.OfType<Vortex>().FirstOrDefault();
                funMode = typeFunction.vortex;
                float vortXpos = vortInst.GetPosition().x;
                float vortYpos = vortInst.GetPosition().y;
                tm.ShowFunctionVort(vortXpos, vortYpos);
                if (tm.LevelObjects_Position.style.display == DisplayStyle.None) {
                    tm.ShowPosition();
                    tm.WritePosition(berKonst, vortXpos, vortYpos);
                }
            } else {
                funMode = typeFunction.None;
                tm.HideFunctionBox();
            }
        }

        private void LevelObjects_ClickButtonFunction() {
            if (tm.functionBox.style.display == DisplayStyle.None) {
                bool condCyl =
                    plot.flowField.Count() == 2 &&
                    plot.flowField.elements.Any(e => e.GetType().Name.Contains("Uniform")) &&
                    plot.flowField.elements.Any(e => e.GetType().Name.Contains("Cylinder"));

                bool condVort =
                    plot.flowField.Count() == 2 &&
                    plot.flowField.elements.Any(e => e.GetType().Name.Contains("Uniform")) &&
                    plot.flowField.elements.Any(e => e.GetType().Name.Contains("Vortex"));

                if (condCyl || condVort) {
                    positionToFunctions();
                    tm.ShowFunction();
                }
            } else {
                tm.HideFunctionBox();
            }
        }
    }
}