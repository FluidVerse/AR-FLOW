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
using static Toolbar.ToolbarButton;
using Camera = UnityEngine.Camera;
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
            tm.SetButtonVisibility(QuestMenu, false);
            tm.SetButtonVisibility(Back, false);
            tm.SetAreaVisibility(ToolbarArea.LevelObjects, true);
            tm.SetAreaVisibility(ToolbarArea.FieldProperties, true);
            tm.SetupButton(LevelObjects1, IconButton1, LevelObjects_ClickButton1);
            tm.SetupButton(LevelObjects2, IconButton2, LevelObjects_ClickButton2);
            tm.SetupButton(LevelObjects3, IconButton3, LevelObjects_ClickButton3);
            tm.SetupButton(LevelObjects4, IconButton4, LevelObjects_ClickButton4);
            tm.SetupButton(LevelObjects5, IconButton5, LevelObjects_ClickButton5);
            tm.SetupButton(LevelObjects6, IconButton6, LevelObjects_ClickButton6);
            tm.SetupButton(LevelObjects7, IconButton7, LevelObjects_ClickButton7);
            tm.SetupButton(LevelObjects11, IconButton2DView, LevelObjects_ClickButton11);
            tm.SetupButton(LevelObjects12, IconButton12, LevelObjects_ClickButton12);
            tm.SetButtonVisibility(Function, true);
            tm.SetButtonVisibility(ButtonField, true);
            tm.SetButtonVisibility(ButtonFieldA, true);
            tm.SetButtonVisibility(ButtonFieldB, true);
            tm.SetButtonVisibility(ButtonFieldC, true);
            tm.SetButtonEnabled(ButtonField, true, true);
            tm.SetButtonEnabled(ButtonFieldA, false, true);
            tm.SetButtonEnabled(ButtonFieldB, false, true);
            tm.SetButtonEnabled(ButtonFieldC, false, true);
            tm.AddButtonAction(ButtonField, FieldProperties_ClickButtonField);
            tm.SetupButton(ButtonFieldA, IconButtonCabs, FieldProperties_ClickButtonFieldCabs);
            tm.SetupButton(ButtonFieldB, IconButtonPhi, FieldProperties_ClickButtonFieldPhi);
            tm.SetupButton(ButtonFieldC, IconButtonPsi, FieldProperties_ClickButtonFieldPsi);

            tm.AddButtonAction(Function, LevelObjects_ClickButtonFunction);
            plot = PlotObject.GetComponent<SurfacePlot>();
            
            plotField = -1;
            FieldProperties_ClickButtonField();

            tm.LevelObjects_PSliders[0].RegisterValueChangedCallback(OnSlider1Changed);
            tm.LevelObjects_PSliders[1].RegisterValueChangedCallback(OnSlider2Changed);

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
            tm.RemoveButtonAction(ButtonField, FieldProperties_ClickButtonField);
            tm.RemoveButtonAction(ButtonFieldA, FieldProperties_ClickButtonFieldCabs);
            tm.RemoveButtonAction(ButtonFieldB, FieldProperties_ClickButtonFieldPhi);
            tm.RemoveButtonAction(ButtonFieldC, FieldProperties_ClickButtonFieldPsi);
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
            tm.SetFieldPropertiesMinMax(plot.minimumValue.ToString("F1"), plot.maximumValue.ToString("F1"));
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
                            tm.SetAreaVisibility(ToolbarArea.Position, false);
                            tm.SetAreaVisibility(ToolbarArea.Property1, false);
                            tm.SetAreaVisibility(ToolbarArea.Property2, false);
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
                                tm.SetAreaVisibility(ToolbarArea.Position, true);
                                tm.SetPositionText(berKonst, fingerPosition.x, fingerPosition.y);

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
                                    tm.SetAreaVisibility(ToolbarArea.Property1, true);
                                    tm.SetAreaVisibility(ToolbarArea.Property2, true);
                                    tm.LevelObjects_PSliders[0].label = "cx";
                                    tm.LevelObjects_PSliders[0].lowValue = -1;
                                    tm.LevelObjects_PSliders[0].highValue = 1;
                                    tm.LevelObjects_PSliders[0].pageSize = 0f;
                                    tm.LevelObjects_PSliders[0].SetValueWithoutNotify(flow.c.x);
                                    tm.LevelObjects_PValues[0].text = flow.c.x.ToString("F1");
                                    tm.LevelObjects_PSliders[1].label = "cy";
                                    tm.LevelObjects_PSliders[1].lowValue = -1;
                                    tm.LevelObjects_PSliders[1].highValue = 1;
                                    tm.LevelObjects_PSliders[1].pageSize = 0f;
                                    tm.LevelObjects_PSliders[1].SetValueWithoutNotify(flow.c.y);
                                    tm.LevelObjects_PValues[1].text = flow.c.y.ToString("F1");
                                    _suppressSliderCallbacks = false;
                                } else if (selectedElement is SourceSink source) {
                                    _suppressSliderCallbacks = true;
                                    tm.SetAreaVisibility(ToolbarArea.Property1, true);
                                    tm.SetAreaVisibility(ToolbarArea.Property2, false);

                                    tm.LevelObjects_PSliders[0].label = "Source intesity";
                                    tm.LevelObjects_PSliders[0].SetValueWithoutNotify(source.strength);
                                    tm.LevelObjects_PSliders[0].lowValue = -2f;
                                    tm.LevelObjects_PSliders[0].highValue = 2f;
                                    tm.LevelObjects_PValues[0].text = source.strength.ToString("F1");
                                    _suppressSliderCallbacks = false;
                                } else if (selectedElement is Dipole dipole) {
                                    _suppressSliderCallbacks = true;
                                    tm.SetAreaVisibility(ToolbarArea.Property1, true);
                                    tm.SetAreaVisibility(ToolbarArea.Property2, false);

                                    tm.LevelObjects_PSliders[0].label = "Dipole intesity";
                                    tm.LevelObjects_PSliders[0].SetValueWithoutNotify(dipole.strength);
                                    tm.LevelObjects_PSliders[0].lowValue = -0.5f;
                                    tm.LevelObjects_PSliders[0].highValue = 0.5f;
                                    tm.LevelObjects_PValues[0].text = dipole.strength.ToString("F1");
                                    _suppressSliderCallbacks = false;
                                } else if (selectedElement is Vortex vortex) {
                                    _suppressSliderCallbacks = true;
                                    tm.SetAreaVisibility(ToolbarArea.Property1, true);
                                    tm.SetAreaVisibility(ToolbarArea.Property2, false);

                                    tm.LevelObjects_PSliders[0].label = "Vortex intesity";
                                    tm.LevelObjects_PSliders[0].SetValueWithoutNotify(vortex.strength);
                                    tm.LevelObjects_PSliders[0].lowValue = -0.5f;
                                    tm.LevelObjects_PSliders[0].highValue = 0.5f;
                                    tm.LevelObjects_PValues[0].text = vortex.strength.ToString("F1");
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
                                tm.SetPositionText(berKonst, fingerPosition.x, fingerPosition.y);
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
                            tm.SetAreaVisibility(ToolbarArea.Position, false);
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

                        tm.LevelObjects_PValues[0].text = tm.LevelObjects_PSliders[0].value.ToString("F1");
                        tm.LevelObjects_PValues[1].text = tm.LevelObjects_PSliders[1].value.ToString("F1");

                        if (clickAction.WasPressedThisFrame()) {
                            //fingerPosition = plot.SnapToGrid(cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 5f)));
                            fingerPosition = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 5f));
                            Vector2 snapped = Snap((Vector2)fingerPosition, PosStep);
                            fingerPosition = new Vector3(snapped.x, snapped.y, fingerPosition.z);

                            float cx = tm.LevelObjects_PSliders[0].value;
                            float cy = tm.LevelObjects_PSliders[1].value;
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

                            float strength = tm.LevelObjects_PSliders[0].value;
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

                            float strength = tm.LevelObjects_PSliders[0].value;
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

                            float strength = tm.LevelObjects_PSliders[0].value;
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
                            tm.SetAreaVisibility(ToolbarArea.Position, true);
                            tm.SetPositionText(berKonst, fingerPositionProbe.x, fingerPositionProbe.y);
                        } else if (clickAction.IsPressed()) {
                            fingerPositionProbe =
                                plot.SnapToGrid(cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 5f)));
                            tm.SetAreaVisibility(ToolbarArea.Position, true);
                            tm.SetPositionText(berKonst, fingerPositionProbe.x, fingerPositionProbe.y);
                        }

                        //
                    }
                } else {
                    /*tm.LevelObjects_PValues[0].text = tm.LevelObjects_PSliders[0].value.ToString("F1");
                    tm.LevelObjects_PValues[1].text = tm.LevelObjects_PSliders[1].value.ToString("F1");
                }
                else if (mode == typeEditMode.addSinkSource)
                {
                    tm.LevelObjects_PValues[0].text = tm.LevelObjects_PSliders[0].value.ToString("F1");
                }
                else if (mode == typeEditMode.addDipole)
                {
                    tm.LevelObjects_PValues[0].text = tm.LevelObjects_PSliders[0].value.ToString("F1");
                }
                else if (mode == typeEditMode.addVortex)
                {
                    tm.LevelObjects_PValues[0].text = tm.LevelObjects_PSliders[0].value.ToString("F1");
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
                tm.SetFieldPropertiesLabelDimText("[m/s]");
                tm.SetFieldPropertiesMinMax(plot.minCabs.ToString("F1"), plot.maxCabs.ToString("F1"));
                tm.SetButtonIcon(ButtonField, IconButtonCabs);
                plot.SetDisplayMode(SurfacePlot.DisplayMode.Cabs);
            } else if (plotField == 1) {
                tm.SetFieldPropertiesLabelDimText("[m²/s]");
                tm.SetFieldPropertiesMinMax(plot.minPhi.ToString("F1"), plot.maxPhi.ToString("F1"));
                tm.SetButtonIcon(ButtonField, IconButtonPhi);
                plot.SetDisplayMode(SurfacePlot.DisplayMode.Phi);
            } else if (plotField == 2) {
                tm.SetFieldPropertiesLabelDimText("[m²/s]");
                tm.SetFieldPropertiesMinMax(plot.minPsi.ToString("F1"), plot.maxPsi.ToString("F1"));
                tm.SetButtonIcon(ButtonField, IconButtonPsi);
                plot.SetDisplayMode(SurfacePlot.DisplayMode.Psi);
            } else if (plotField == 3) {
                tm.SetFieldPropertiesLabelDimText("[Pa]");
                tm.SetFieldPropertiesMinMax(plot.minP.ToString("F1"), plot.maxP.ToString("F1"));
                tm.SetButtonIcon(ButtonField, IconButtonP);
                plot.SetDisplayMode(SurfacePlot.DisplayMode.P);
            }
        }

        public void FieldProperties_ClickButtonFieldCabs() {
            if (showCabsIso) {
                showCabsIso = false;
                tm.SetButtonEnabled(ButtonFieldA, false, true);
            } else {
                showCabsIso = true;
                tm.SetButtonEnabled(ButtonFieldA, true, true);
            }
            plot.SetIsoLineTransparency(showCabsIso ? 1 : 0, showPhiIso ? 1 : 0, showPsiIso ? 1 : 0);
        }

        public void FieldProperties_ClickButtonFieldPhi() {
            if (showPhiIso) {
                showPhiIso = false;
                tm.SetButtonEnabled(ButtonFieldB, false, true);
            } else {
                showPhiIso = true;
                tm.SetButtonEnabled(ButtonFieldB, true, true);
            }
            plot.SetIsoLineTransparency(showCabsIso ? 1 : 0, showPhiIso ? 1 : 0, showPsiIso ? 1 : 0);
        }

        public void FieldProperties_ClickButtonFieldPsi() {
            if (showPsiIso) {
                showPsiIso = false;
                tm.SetButtonEnabled(ButtonFieldC, false, true);
            } else {
                showPsiIso = true;
                tm.SetButtonEnabled(ButtonFieldC, true, true);
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
            tm.LevelObjects_PSliders[0].SetValueWithoutNotify(value);
            tm.LevelObjects_PValues[0].text = value.ToString("F1");

            if (mode == typeEditMode.addSinkSource) {
                value = Mathf.Round(evt.newValue * 5f) / 5f;
                tm.LevelObjects_PSliders[0].SetValueWithoutNotify(value);
            }

            // Nur wenn gerade ein SourceSink selektiert ist:
            if (mode == typeEditMode.moveObjects) {
                if (selectedElement is UniformFlow flow) {
                    flow.c = new Vector2(value, flow.c.y);
                    plot.UpdateField();
                    updateColorbar();
                } else if (selectedElement is SourceSink source) {
                    value = Mathf.Round(evt.newValue * 5f) / 5f;
                    tm.LevelObjects_PSliders[0].SetValueWithoutNotify(value);
                    tm.LevelObjects_PValues[0].text = value.ToString("F1");
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
            tm.LevelObjects_PSliders[1].SetValueWithoutNotify(value);
            tm.LevelObjects_PValues[1].text = value.ToString("F1");

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
            tm.SetButtonIcon(LevelObjects11, IconButton2DView);
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
            
            tm.SetButtonEnabled(LevelObjects1, true);
            tm.SetButtonEnabled(LevelObjects2, true);
            tm.SetButtonEnabled(LevelObjects3, true);
            tm.SetButtonEnabled(LevelObjects4, true);
            tm.SetButtonEnabled(LevelObjects5, true);
            tm.SetButtonEnabled(LevelObjects6, true);
            tm.SetButtonEnabled(LevelObjects7, true);
            tm.SetButtonEnabled(LevelObjects12, true);
            Debug.Log("ChangeTo2DView");
        }

        public void ChangeTo3DView() {
            plotZoom.ZoomEnabled = true;
            tm.SetButtonIcon(LevelObjects11, IconButton3DView);
            //cms.ChangeToDetailView(PlotObject);
            cms.camType = CameraType.DetailView;
            detailCameraMode.IsActive = true;
            Camera.main.orthographic = false;
            View3D = true;
            tm.SetButtonEnabled(LevelObjects1, false);
            tm.SetButtonEnabled(LevelObjects2, false);
            tm.SetButtonEnabled(LevelObjects3, false);
            tm.SetButtonEnabled(LevelObjects4, false);
            tm.SetButtonEnabled(LevelObjects5, false);
            tm.SetButtonEnabled(LevelObjects6, false);
            tm.SetButtonEnabled(LevelObjects7, false);
            tm.SetButtonEnabled(LevelObjects12, false);
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
            tm.SetAreaVisibility(ToolbarArea.Position, false);
            tm.SetAreaVisibility(ToolbarArea.Property1, true);
            tm.SetAreaVisibility(ToolbarArea.Property2, true);
            tm.LevelObjects_PSliders[0].label = "cx";
            tm.LevelObjects_PSliders[0].value = 0;
            tm.LevelObjects_PSliders[0].lowValue = -1;
            tm.LevelObjects_PSliders[0].highValue = 1;
            tm.LevelObjects_PValues[0].text = tm.LevelObjects_PSliders[0].value.ToString("F1");
            tm.LevelObjects_PSliders[1].label = "cy";
            tm.LevelObjects_PSliders[1].value = 0;
            tm.LevelObjects_PSliders[1].lowValue = -1;
            tm.LevelObjects_PSliders[1].highValue = 1;
            tm.LevelObjects_PValues[1].text = tm.LevelObjects_PSliders[1].value.ToString("F1");
        }

        private void LevelObjects_ClickButton2() {
            SetMarkersActive(false);
            mode = typeEditMode.addSinkSource;
            logManager.Write(LogMessages.ChoseToolHeatSink);
            tm.SetAreaVisibility(ToolbarArea.Position, false);
            tm.SetAreaVisibility(ToolbarArea.Property1, true);
            tm.SetAreaVisibility(ToolbarArea.Property2, false);

            tm.LevelObjects_PSliders[0].label = "Source intesity";
            tm.LevelObjects_PSliders[0].value = 1.0f;
            tm.LevelObjects_PSliders[0].lowValue = -2f;
            tm.LevelObjects_PSliders[0].highValue = 2f;
            tm.LevelObjects_PValues[0].text = tm.LevelObjects_PSliders[0].value.ToString("F1");
        }

        private void LevelObjects_ClickButton3() {
            SetMarkersActive(false);
            mode = typeEditMode.addDipole;
            logManager.Write(LogMessages.ChoseToolDipole);
            tm.SetAreaVisibility(ToolbarArea.Position, false);
            tm.SetAreaVisibility(ToolbarArea.Property1, true);
            tm.SetAreaVisibility(ToolbarArea.Property2, false);

            tm.LevelObjects_PSliders[0].label = "Dipole intesity";
            tm.LevelObjects_PSliders[0].value = 0.1f;
            tm.LevelObjects_PSliders[0].lowValue = -0.5f;
            tm.LevelObjects_PSliders[0].highValue = 0.5f;
            tm.LevelObjects_PValues[0].text = tm.LevelObjects_PSliders[0].value.ToString("F1");
        }

        private void LevelObjects_ClickButton4() {
            SetMarkersActive(false);
            logManager.Write(LogMessages.ChoseToolVortex);
            mode = typeEditMode.addVortex;
            tm.SetAreaVisibility(ToolbarArea.Position, false);
            tm.SetAreaVisibility(ToolbarArea.Property1, true);
            tm.SetAreaVisibility(ToolbarArea.Property2, false);

            tm.LevelObjects_PSliders[0].label = "Vortex intesity";
            tm.LevelObjects_PSliders[0].value = 0.1f;
            tm.LevelObjects_PSliders[0].lowValue = -0.5f;
            tm.LevelObjects_PSliders[0].highValue = 0.5f;
            tm.LevelObjects_PValues[0].text = tm.LevelObjects_PSliders[0].value.ToString("F1");
        }

        private void LevelObjects_ClickButton5() {
            SetMarkersActive(false);
            mode = typeEditMode.addCylinder;
            logManager.Write(LogMessages.ChoseToolCylinder);
            tm.SetAreaVisibility(ToolbarArea.Position, false);
            tm.SetAreaVisibility(ToolbarArea.Property1, false);
            tm.SetAreaVisibility(ToolbarArea.Property2, false);
        }

        private void LevelObjects_ClickButton6() {
            tm.SetAreaVisibility(ToolbarArea.Position, false);
            tm.SetAreaVisibility(ToolbarArea.Property1, false);
            tm.SetAreaVisibility(ToolbarArea.Property2, false);
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
            tm.SetAreaVisibility(ToolbarArea.Position, false);
            tm.SetAreaVisibility(ToolbarArea.Property1, false);
            tm.SetAreaVisibility(ToolbarArea.Property2, false);
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
            tm.SetAreaVisibility(ToolbarArea.Position, false);
            tm.SetAreaVisibility(ToolbarArea.Property1, false);
            tm.SetAreaVisibility(ToolbarArea.Property2, false);
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
                if (!tm.IsLevelObjectsPositionVisible) {
                    tm.SetAreaVisibility(ToolbarArea.Position, true);
                    tm.SetPositionText(berKonst, cylXpos, cylYpos);
                }
            } else if (condVort) {
                Vortex vortInst = plot.flowField.elements.OfType<Vortex>().FirstOrDefault();
                funMode = typeFunction.vortex;
                float vortXpos = vortInst.GetPosition().x;
                float vortYpos = vortInst.GetPosition().y;
                tm.ShowFunctionVort(vortXpos, vortYpos);
                if (!tm.IsLevelObjectsPositionVisible) {
                    tm.SetAreaVisibility(ToolbarArea.Position, true);
                    tm.SetPositionText(berKonst, vortXpos, vortYpos);
                }
            } else {
                funMode = typeFunction.None;
                tm.SetAreaVisibility(ToolbarArea.FunctionBox, false);
            }
        }

        private void LevelObjects_ClickButtonFunction() {
            if (!tm.IsFunctionBoxVisible) {
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
                    tm.SetAreaVisibility(ToolbarArea.FunctionBox, true);
                }
            } else {
                tm.SetAreaVisibility(ToolbarArea.FunctionBox, false);
            }
        }
    }
}


