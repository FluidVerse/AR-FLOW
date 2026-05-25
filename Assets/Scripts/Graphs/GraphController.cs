using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Graphs {
    /// <summary>
    /// Controller for a 2D graph, including the background image, axes, axis labels and points on the graph.
    ///
    /// To work properly, <see cref="CanvasScaler.uiScaleMode"/> must be set to <c>Constant Pixel Size</c>.
    ///
    /// For coordinates, there are two different spaces to keep in mind:
    /// - Screen space: The coordinate system of the screen bounded by <see cref="Screen.width"/> and
    /// <see cref="Screen.height"/>, e.g. mouse position is in screen space.
    /// 
    /// - (Canvas space: The coordinate system of the canvas bounded by <see cref="CanvasScaler.referenceResolution"/>.
    /// However, since <see cref="CanvasScaler.uiScaleMode"/> is set to <c>Constant Pixel Size</c>, canvas and screen
    /// space are equal)
    /// 
    /// - Graph space: The coordinate system of the graph itself, which is in theory infinite in all directions, but
    /// bounded in practice by <see cref="ValueRange"/> and <see cref="MaxZoomLevel"/>.
    /// </summary>
    public class GraphController : MonoBehaviour {

        /// <summary>
        /// Default <see cref="ValueRange"/>, at the same time widest supported range of values for the graph axes.
        ///
        /// Any values beyond this range are not supported because they are either buggy due to floating point
        /// imprecision, or they cannot be displayed nicely anymore.
        /// </summary>
        public static readonly Vector2 DefaultValueRange = new(1e-4f, 1e8f);

        /// <summary>
        /// Maximum value for <see cref="GraphState.zoomLevel"/>.
        ///
        /// Represents an extra check together with <see cref="ValueRange"/> because checking min values can be
        /// tricky sometimes.
        /// </summary>
        private const float MaxZoomLevel = 5e6f;

        /// <summary>
        /// Callback when the graph has been recalculated, e.g. after zooming or moving.
        /// </summary>
        public UnityEvent<RecalculationData> onGraphRecalculated = new();

        [Header("Graph components")] [SerializeField]
        private Canvas canvas;

        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image contentMask;

        /// <summary>
        /// Images for the x- and y-axis line, respectively.
        /// </summary>
        [SerializeField] private Image[] axisImages = new Image[2];

        /// <summary>
        /// Prefabs for the number labels on the x- and y-axis, respectively.
        /// </summary>
        [SerializeField] private GameObject[] labelPrefabs = new GameObject[2];

        /// <summary>
        /// Factor for the size of the graph background relative to the canvas size.
        /// </summary>
        [Space] [Header("Background settings")] [SerializeField]
        private float backgroundSizeFactor = 0.95f;

        /// <summary>
        /// Factor for the size of the content mask (for graph content like axes, labels etc.) relative to the canvas
        /// size.
        /// </summary>
        [SerializeField] private float contentMaskSizeFactor = 0.95f;

        /// <summary>
        /// Radius of the corners of the background image, relative to <c>min(width, height)</c> of the canvas rect.
        /// </summary>
        [SerializeField] private float cornerRadiusFactor = 0.05f;

        /// <summary>
        /// Width of the axes in px.
        /// </summary>
        [Space] [Header("Axis and label settings")] [SerializeField]
        private float axisWidth = 10f;

        /// <summary>
        /// Minimum distance between two lines in the graph, relative to the background rect size.
        ///
        /// The default value <c>60f/980</c> means that if the background rect is 980px high, the distance between
        /// two lines is at least 60px. 
        /// </summary>
        [SerializeField] private float minLineDistance = 80f / 980;

        /// <summary>
        /// Whether to use scientific notation for axis labels with many digits.
        ///
        /// If <c>true</c>, scientific notation is used for labels with more digits than specified in
        /// <see cref="maxDisplayedDigits"/>. This feature is currently <b>experimental</b>, since it has not been
        /// fully tested yet.
        /// </summary>
        [SerializeField] private bool useScientificNotation;

        /// <summary>
        /// Whether to use a comma as a decimal separator across the graph, e.g. in axis labels (e.g. <c>0,1</c>
        /// instead of <c>0.1</c>).
        ///
        /// Important note: if someday, this setting is made configurable at runtime, it must be ensured that
        /// other components are updated accordingly. This is not implemented yet, all components only fetch this
        /// value once during Awake().
        /// </summary>
        [SerializeField] private bool useCommaSeparator = true;

        /// <summary>
        /// Maximum number of digits to show in axis labels.
        ///
        /// For labels with more digits, scientific notation (e.g. 1.0E+5) is used.
        /// </summary>
        [SerializeField] private int maxDisplayedDigits = 5;

        /// <summary>
        /// Whether to hide some axis labels if the space for them is tight.
        ///
        /// If <c>true</c>, some labels will be hidden if <see cref="spaceBetweenLabels"/> cannot be guaranteed.
        /// </summary>
        [SerializeField] private bool hideTightAxisLabels = true;

        /// <summary>
        /// Minimum space between two labels on the x- or y-axis respectively, in px.
        /// </summary>
        [SerializeField] private Vector2 spaceBetweenLabels = new(40f, 40f);

        /// <summary>
        /// Base unit for the axes, i.e. how many graph units the space between two lines in the graph corresponds to.
        ///
        /// Depending on the zoom level, different powers of the base unit are used. With the default value of
        /// <c>10</c>, the graph will show lines at 0, 10, 20 ..., or at 0, 100, 200 ... when zooming out, or at
        /// 0, 1, 2 ... when zooming in, etc.
        ///
        /// For now, this value is hardcoded to 10 because it cannot be guaranteed that other methods like
        /// <see cref="CountDigits"/> still work correctly with other values.
        /// </summary>
        private const int axisBaseUnit = 10;

        private MaterialAdapter materialAdapter;

        private GraphState graphState = new() {
            anchor = Vector2.zero,
            zoomLevel = Vector2.one
        };

        // parents of labelPrefabs
        private Transform[] labelPrefabParents;

        /// <summary>
        /// Pools of graph labels for the x- and y-axis.
        /// </summary>
        private readonly List<List<AxisLabel>> labelPools = new(2) {
            new List<AxisLabel>(),
            new List<AxisLabel>()
        };

        /// <summary>
        /// Supported absolute range of values for the graph axes.
        /// </summary>
        public Vector2 ValueRange { get; set; } = DefaultValueRange;

        /// <summary>
        /// Current zoom level, see <see cref="GraphState.zoomLevel"/>.
        /// </summary>
        public Vector2 ZoomLevel => graphState.zoomLevel;

        /// <summary>
        /// See <see cref="useCommaSeparator"/>.
        /// </summary>
        public bool UseCommaSeparator => useCommaSeparator;

        /// <summary>
        /// Size of the background image in pixels.
        /// </summary>
        public Vector2Int BackgroundRectSize {
            get {
                Rect rect = backgroundImage.rectTransform.rect;
                return Vector2Int.RoundToInt(new Vector2(rect.width, rect.height));
            }
        }

        /// <summary>
        /// Width of the border between the background image and the screen edge in canvas space pixels.
        /// </summary>
        public Vector2Int WindowBorderWidth =>
            Vector2Int.RoundToInt((new Vector2(Screen.width, Screen.height) - BackgroundRectSize) / 2);

        /// <summary>
        /// Reference resolution of the canvas, rounded to integer values.
        ///
        /// Since <see cref="CanvasScaler.uiScaleMode"/> is set to <c>Constant Pixel Size</c>, this is equal to
        /// the screen size.
        /// </summary>
        private static Vector2Int CanvasReferenceSize => new(Screen.width, Screen.height);

        /// <summary>
        /// Extents of the graph in graph-space coordinates.
        /// </summary>
        private Vector2 GraphSpaceExtents => BackgroundRectSize / ZoomLevel;

        /// <summary>
        /// Character used as decimal separator in axis labels.
        /// </summary>
        private char DecimalSeparator => useCommaSeparator ? ',' : '.';

        private void Awake() {
            CreateMaterialAdapter();
            AdjustBackgroundSize();
            AdjustAxisSizes();
            SetupLabelPrefabs();
            SetupInitialState();
            RecalculateAll();
        }

        /// <summary>
        /// Moves the background image by a specified offset.
        /// </summary>
        /// <param name="offset">Offset in px</param>
        public void MoveBackground(Vector2 offset) {
            UpdateState(() => graphState.anchor += offset);
        }

        /// <summary>
        /// Zooms into/out of the graph by a specified factor at a specified point.
        /// </summary>
        /// <param name="zoomPoint">Screen point to zoom in/out at</param>
        /// <param name="factor">Zooming in if <c>factor > 1</c>, otherwise zooming out</param>
        public void Zoom(Vector2 zoomPoint, float factor) {
            UpdateState(() => {
                // Convert to graph-space coordinates, then adjust anchor so the graph-space point stays under the cursor
                Vector2 graphSpaceBefore = ToGraphSpace(zoomPoint);
                graphState.zoomLevel *= factor;
                graphState.anchor = graphSpaceBefore - (zoomPoint - WindowBorderWidth) / graphState.zoomLevel;
                graphState.anchor.y += GraphSpaceExtents.y;
            });
        }

        /// <summary>
        /// Resizes the graph to fit the specified axis ranges while maintaining the aspect ratio.
        ///
        /// If <paramref name="fitToEqualZoomLevel"/> is <c>true</c> and the aspect ratio of the specified ranges does
        /// not match the aspect ratio of the background image,the ranges will be adjusted so that <b>at least</b> all
        /// the specified ranges are visible.
        /// </summary>
        /// <param name="xMin">Minimum x value</param>
        /// <param name="xMax">Maximum x value</param>
        /// <param name="yMin">Minimum y value</param>
        /// <param name="yMax">Maximum y value</param>
        /// <param name="fitToEqualZoomLevel">
        /// If <c>true</c>, resizes the ranges to set the zoom levels for both axes equally
        /// </param>
        public void Resize(float xMin, float xMax, float yMin, float yMax, bool fitToEqualZoomLevel) {
            UpdateState(() => {
                Vector2 axisLength = new Vector2(xMax - xMin, yMax - yMin);
                float desiredAspectRatio = axisLength.x / axisLength.y;
                float realAspectRatio = (float)BackgroundRectSize.x / BackgroundRectSize.y;

                if (!fitToEqualZoomLevel) {
                    // set zoom levels independently
                    graphState.zoomLevel.x = BackgroundRectSize.x / axisLength.x;
                    graphState.zoomLevel.y = BackgroundRectSize.y / axisLength.y;
                    graphState.anchor = new Vector2(xMin, yMax);
                    return;
                }

                // else: set zoom levels equally, so the graph is not distorted
                if (realAspectRatio > desiredAspectRatio) {
                    // background is wider than desired, so fit y-axis and adjust x-axis
                    graphState.zoomLevel.y = BackgroundRectSize.y / axisLength.y;
                    graphState.zoomLevel.x = graphState.zoomLevel.y;
                    float newXLength = BackgroundRectSize.x / graphState.zoomLevel.x;
                    float xCenter = (xMin + xMax) / 2f;
                    graphState.anchor = new Vector2(xCenter - newXLength / 2f, yMax);
                } else {
                    // background is taller than desired, so fit x-axis and adjust y-axis
                    graphState.zoomLevel.x = BackgroundRectSize.x / axisLength.x;
                    graphState.zoomLevel.y = graphState.zoomLevel.x;
                    float newYLength = BackgroundRectSize.y / graphState.zoomLevel.y;
                    float yCenter = (yMin + yMax) / 2f;
                    graphState.anchor = new Vector2(xMin, yCenter + newYLength / 2f);
                }
            });
        }

        /// <summary>
        /// Converts a point in screen space (e.g. mouse position) to graph space (i.e. the coordinate system of the
        /// graph).
        /// </summary>
        /// <param name="screenPoint">Point on the screen</param>
        /// <returns>Corresponding point in graph space</returns>
        public Vector2 ToGraphSpace(Vector2 screenPoint) {
            Vector2 graphSpaceBefore = (screenPoint - WindowBorderWidth) / graphState.zoomLevel + graphState.anchor;
            graphSpaceBefore.y -= GraphSpaceExtents.y;
            return graphSpaceBefore;
        }

        /// <summary>
        /// Converts a point in graph space (i.e. the coordinate system of the graph) to screen space.
        /// </summary>
        /// <param name="graphPoint">Point on the graph</param>
        /// <returns>Corresponding point in screen space</returns>
        public Vector2 ToScreenSpace(Vector2 graphPoint) {
            Vector2 screenPoint = (graphPoint - graphState.anchor + new Vector2(0, GraphSpaceExtents.y)) *
                graphState.zoomLevel + WindowBorderWidth;
            return screenPoint;
        }

        /// <summary>
        /// Checks if the current <see cref="graphState"/> is valid, i.e. if the graph values are within
        /// <see cref="ValueRange"/>.
        /// </summary>
        /// <returns><c>true</c> if the graph state is valid, otherwise <c>false</c></returns>
        public bool IsGraphStateValid() {
            Vector2 unitsPerPx = Vector2.one / graphState.zoomLevel;
            unitsPerPx.y *= -1; // invert y-axis because anchor is top left, so oppositeCorner is bottom right
            Vector2 anchor = graphState.anchor;
            Vector2 oppositeCorner = anchor + BackgroundRectSize * unitsPerPx;
            bool zoomLevelTooBig = graphState.zoomLevel.x > MaxZoomLevel || graphState.zoomLevel.y > MaxZoomLevel;
            bool anyNumberTooBig = Math.Abs(anchor.x) > ValueRange.y || Math.Abs(anchor.y) > ValueRange.y ||
                                   Math.Abs(oppositeCorner.x) > ValueRange.y ||
                                   Math.Abs(oppositeCorner.y) > ValueRange.y;
            bool allNumbersTooSmall = Math.Abs(anchor.x) < ValueRange.x &&
                                      Math.Abs(anchor.y) < ValueRange.x &&
                                      Math.Abs(oppositeCorner.x) < ValueRange.x &&
                                      Math.Abs(oppositeCorner.y) < ValueRange.x;
            return !zoomLevelTooBig && !anyNumberTooBig && !allNumbersTooSmall;
        }

        /// <summary>
        /// Updates the <see cref="graphState"/> using the specified update action, but reverts to the old state if the
        /// new state is invalid (see <see cref="IsGraphStateValid"/>).
        /// </summary>
        /// <param name="updateAction">
        /// How to update the graph state. Code inside it can directly modify <see cref="graphState"/>
        /// </param>
        private void UpdateState(Action updateAction) {
            GraphState oldState = graphState;
            updateAction();
            if (IsGraphStateValid()) {
                RecalculateAll();
            } else {
                graphState = oldState;
            }
        }

        /// <summary>
        /// Creates a copy of the background image's material to avoid modifying the original material, and the
        /// corresponding instance of <see cref="MaterialAdapter"/>.
        /// </summary>
        private void CreateMaterialAdapter() {
            Material materialCopy = new Material(backgroundImage.material);
            backgroundImage.material = materialCopy;
            materialAdapter = new MaterialAdapter(materialCopy);
        }

        /// <summary>
        /// Adjusts the size of the background image based on the screen size, canvas size,
        /// <see cref="backgroundSizeFactor"/> and <see cref="cornerRadiusFactor"/>.
        /// </summary>
        private void AdjustBackgroundSize() {
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            RectTransform backgroundRect = backgroundImage.rectTransform;
            RectTransform contentMaskRect = contentMask.rectTransform;

            // set rect size of the background image and content mask 
            Vector2 canvasRectSize = new(canvasRect.rect.width, canvasRect.rect.height);
            Vector2Int backgroundRectSize = Vector2Int.RoundToInt(canvasRectSize * backgroundSizeFactor);
            Vector2Int contentMaskRectSize = Vector2Int.RoundToInt(canvasRectSize * contentMaskSizeFactor);
            backgroundRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, backgroundRectSize.x);
            backgroundRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, backgroundRectSize.y);
            contentMaskRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, contentMaskRectSize.x);
            contentMaskRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentMaskRectSize.y);

            // set shader properties to a material copy 
            Vector2Int imageSize = Vector2Int.RoundToInt((Vector2)backgroundRectSize * canvasRect.localScale);
            materialAdapter.ImageSizeX = imageSize.x;
            materialAdapter.ImageSizeY = imageSize.y;
            int cornerRadius = Mathf.RoundToInt(Mathf.Min(Screen.width, Screen.height) * cornerRadiusFactor);
            materialAdapter.CornerRadius = cornerRadius;
        }

        /// <summary>
        /// Adjusts the size of the axes based on the background image size and <see cref="axisWidth"/>.
        /// </summary>
        private void AdjustAxisSizes() {
            axisImages[0].rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, BackgroundRectSize.x);
            axisImages[0].rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, axisWidth);
            axisImages[1].rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, axisWidth);
            axisImages[1].rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, BackgroundRectSize.y);
        }

        /// <summary>
        /// Sets up the label prefabs by deactivating them and creating temporary parent objects for them.
        /// </summary>
        private void SetupLabelPrefabs() {
            labelPrefabParents = new Transform[labelPrefabs.Length];
            for (int i = 0; i < labelPrefabs.Length; i++) {
                labelPrefabs[i].SetActive(false);
                labelPrefabParents[i] = new GameObject("temp_labels") {
                    transform = {
                        parent = labelPrefabs[i].transform.parent
                    }
                }.transform;
            }
        }

        /// <summary>
        /// Sets up the initial <see cref="graphState"/>.
        /// </summary>
        private void SetupInitialState() {
            graphState = new GraphState {
                anchor = new Vector2(-BackgroundRectSize.x / 2f, BackgroundRectSize.y / 2f),
                zoomLevel = Vector2.one
            };
        }

        /// <summary>
        /// Recalculates the whole graph.
        /// </summary>
        private void RecalculateAll() {
            RecalculateBackground(out RecalculationData data);
            RecalculateAxes(data);
            onGraphRecalculated?.Invoke(data);
        }

        /// <summary>
        /// Recalculates the graph shader properties based on the current <see cref="graphState"/> and other graph
        /// settings, namely:
        ///
        /// - X and Y distance between two lines in the coordinate system
        /// - X and Y offset of the coordinate system lines
        /// 
        /// </summary>
        /// <param name="data">
        /// Additional data calculated during the process, which is needed for recalculating the axes and labels
        /// </param>
        private void RecalculateBackground(out RecalculationData data) {
            // x y line distance
            float minLineDistanceAbs = Mathf.Min(BackgroundRectSize.x, BackgroundRectSize.y) * minLineDistance;
            Vector2 requiredUnits = new Vector2(minLineDistanceAbs, minLineDistanceAbs) / graphState.zoomLevel;
            int expX = Mathf.CeilToInt(Mathf.Log10(requiredUnits.x / axisBaseUnit));
            int expY = Mathf.CeilToInt(Mathf.Log10(requiredUnits.y / axisBaseUnit));
            Vector2 baseUnitsExp = new Vector2(Mathf.Pow(axisBaseUnit, expX), Mathf.Pow(axisBaseUnit, expY));
            Vector2 axisUnits = new Vector2Int(axisBaseUnit, axisBaseUnit) * baseUnitsExp;
            Vector2 lineDistancePx = axisUnits * graphState.zoomLevel;

            materialAdapter.LineDistanceX = Mathf.RoundToInt(lineDistancePx.x);
            materialAdapter.LineDistanceY = Mathf.RoundToInt(lineDistancePx.y);

            // x y offset
            Vector2 firstOnGrid = Vector2Int.CeilToInt(graphState.anchor / axisUnits) * axisUnits;
            Vector2 offsets = (firstOnGrid - graphState.anchor) * graphState.zoomLevel;
            offsets.y *= -1;

            materialAdapter.OffsetX = Mathf.RoundToInt(offsets.x);
            materialAdapter.OffsetY = Mathf.RoundToInt(offsets.y);

            // axis positions
            Vector2 axisCoordinates = graphState.anchor * graphState.zoomLevel;
            float xAxisY = WindowBorderWidth.y + BackgroundRectSize.y - axisCoordinates.y;
            float yAxisX = WindowBorderWidth.x - axisCoordinates.x;
            Vector2 axisPositions = new(xAxisY, yAxisX);

            // output data for other recalculations
            data = new RecalculationData {
                exps = new Vector2Int(expX, expY),
                axisUnits = axisUnits,
                lineDistancePx = lineDistancePx,
                firstOnGrid = firstOnGrid,
                offsets = offsets,
                axisPositions = axisPositions
            };
        }

        /// <summary>
        /// Recalculates the axes positions and the number and positions of the axis labels based on the current
        /// <see cref="graphState"/> and other graph settings.
        /// </summary>
        /// <param name="data">
        /// Additional data calculated during <see cref="RecalculateBackground"/>, needed for the calculations
        /// </param>
        private void RecalculateAxes(RecalculationData data) {
            // axes
            SetXAxisPosition(data.axisPositions.x - 1);
            SetYAxisPosition(data.axisPositions.y + 1);

            // labels
            HideAllLabels();
            Vector2Int requiredLabels =
                Vector2Int.CeilToInt(BackgroundRectSize / data.lineDistancePx + new Vector2Int(2, 2));

            EnlargeLabelPools(requiredLabels);
            RecalculateAxisLabelsX(data, requiredLabels[0], data.axisPositions.x);
            RecalculateAxisLabelsY(data, requiredLabels[1], data.axisPositions.y);
            if (hideTightAxisLabels) {
                HideTightLabels();
            }
        }

        /// <summary>
        /// Sets the vertical position of the x-axis line.
        /// </summary>
        /// <param name="y">Y coordinate</param>
        private void SetXAxisPosition(float y) {
            Vector2 position = axisImages[0].transform.position;
            position.y = y;
            axisImages[0].transform.position = position;
        }

        /// <summary>
        /// Sets the horizontal position of the y-axis line.
        /// </summary>
        /// <param name="x">X coordinate</param>
        private void SetYAxisPosition(float x) {
            Vector2 position = axisImages[1].transform.position;
            position.x = x;
            axisImages[1].transform.position = position;
        }

        /// <summary>
        /// Hides all labels in the label pools.
        /// </summary>
        private void HideAllLabels() {
            foreach (List<AxisLabel> pool in labelPools) {
                foreach (AxisLabel label in pool) {
                    label.Show(false);
                }
            }
        }

        /// <summary>
        /// Enlarges the label pools to ensure that they contain at least the required number of labels.
        /// </summary>
        /// <param name="requiredLabels">Number of required labels for the x- and y-axis, respectively</param>
        private void EnlargeLabelPools(Vector2Int requiredLabels) {
            for (int i = 0; i < labelPrefabParents.Length; i++) {
                if (labelPools[i].Count >= requiredLabels[i]) {
                    continue;
                }

                int toCreate = requiredLabels[i] - labelPools[i].Count;
                for (int j = 0; j < toCreate; j++) {
                    GameObject obj = Instantiate(labelPrefabs[i], labelPrefabParents[i]);
                    obj.SetActive(true);
                    labelPools[i].Add(obj.GetComponent<AxisLabel>());
                }
            }
        }

        /// <summary>
        /// Recalculates the <see cref="AxisLabel"/>s on the x-axis.
        /// </summary>
        /// <param name="data">
        /// Additional data calculated during <see cref="RecalculateBackground"/>, needed for the calculations
        /// </param>
        /// <param name="labelCount">Number of labels</param>
        /// <param name="xAxisY">Y coordinate of the x-axis line</param>
        private void RecalculateAxisLabelsX(RecalculationData data, int labelCount, float xAxisY) {
            for (int i = -1; i < labelCount - 1; i++) {
                int poolIndex = i + 1;
                int decimals = Mathf.Max(0, -data.exps.x);
                double labelValue = Math.Round(data.firstOnGrid.x + i * data.axisUnits.x, decimals);

                float xPos = WindowBorderWidth.x + data.offsets.x + i * data.lineDistancePx.x;
                SetupLabel(labelPools[0][poolIndex], new Vector2(xPos, xAxisY), labelValue);
                if (Mathf.Approximately((float)labelValue, 0)) {
                    // hide label at origin (but still set it up because methods like HideTightLabels depend on it)
                    labelPools[0][poolIndex].Show(false);
                }
            }
        }

        /// <summary>
        /// Recalculates the <see cref="AxisLabel"/>s on the y-axis.
        /// </summary>
        /// <param name="data">
        /// Additional data calculated during <see cref="RecalculateBackground"/>, needed for the calculations
        /// </param>
        /// <param name="labelCount">Number of labels</param>
        /// <param name="yAxisX">X coordinate of the y-axis line</param>
        private void RecalculateAxisLabelsY(RecalculationData data, int labelCount, float yAxisX) {
            for (int i = -1; i < labelCount - 1; i++) {
                int poolIndex = i + 1;
                int decimals = Mathf.Max(0, -data.exps.y);
                double labelValue = Math.Round(data.firstOnGrid.y - i * data.axisUnits.y, decimals);

                float yPos = WindowBorderWidth.y + BackgroundRectSize.y - (data.offsets.y + i * data.lineDistancePx.y);
                SetupLabel(labelPools[1][poolIndex], new Vector2(yAxisX, yPos), labelValue);
                if (Mathf.Approximately((float)labelValue, 0)) {
                    // hide label at origin (but still set it up because methods like HideTightLabels depend on it)
                    labelPools[1][poolIndex].Show(false);
                }
            }
        }

        /// <summary>
        /// Hides labels in the label pools if the space for them is tight.
        /// </summary>
        private void HideTightLabels() {
            for (int i = 0; i < labelPools.Count; i++) {
                List<AxisLabel> labelPool = labelPools[i];
                Orientation orientation = i == 0 ? Orientation.Horizontal : Orientation.Vertical;
                if (!IsLabelSpaceTight(labelPool, orientation)) {
                    continue; // ignore if space is not tight
                }

                List<AxisLabel> visibleNonZeroLabels = labelPool.Where(label => label.IsVisible)
                    .Where(label => !Mathf.Approximately((float)label.RealValue, 0))
                    .ToList();

                // max length of the numbers in the visible labels (without minus sign)
                int maxNumberLength = visibleNonZeroLabels.Select(label => label.DisplayedText.Replace("-", ""))
                    .Max(str => str.Length);
                // min number of trailing zeros in the visible labels (but ignore label at origin)
                int minTrailingZeros = visibleNonZeroLabels.Select(label => CountTrailingZeros(label.DisplayedText))
                    .Min();
                bool hasDecimalPlaces =
                    visibleNonZeroLabels.Any(label => label.DisplayedText.Contains(DecimalSeparator));

                foreach (AxisLabel label in visibleNonZeroLabels) {
                    string displayedTextAbs = label.DisplayedText.Replace("-", "");
                    if (hasDecimalPlaces && displayedTextAbs.Length < maxNumberLength) {
                        continue; // e.g. 1,09 , 1,1 , 1,01 , 1,02..., 1,1 should not be skipped 
                    }

                    string normText = displayedTextAbs[..^minTrailingZeros];
                    if (normText.Length == 0) {
                        continue;
                    }

                    int lastVisibleDigit = int.Parse(normText.Last().ToString());
                    if (lastVisibleDigit % 2 == 1) {
                        label.Show(false); // hide labels with odd last visible non-zero digit
                    }
                }
            }
        }

        /// <summary>
        /// Counts the number of trailing zeros in a string.
        /// </summary>
        /// <param name="str">String to count trailing zeros in</param>
        /// <returns>Number of trailing zeros, e.g. <c>3</c> for <c>"12000"</c> or <c>0</c> for <c>"123"</c></returns>
        private static int CountTrailingZeros(string str) {
            int count = 0;
            for (int i = str.Length - 1; i >= 0 && str[i] == '0'; i--) {
                count++;
            }
            return count;
        }

        /// <summary>
        /// Checks if the space for labels on an axis is tight, i.e. if the labels would overlap or be too close to
        /// each other.
        ///
        /// In <paramref name="labelPool"/>, only visible labels will be considered (see
        /// <see cref="AxisLabel.IsVisible"/>).
        /// </summary>
        /// <param name="labelPool">Label pool to check</param>
        /// <param name="orientation">Orientation of the axis the labels belong to</param>
        /// <returns><c>true</c> if the space is tight, otherwise <c>false</c></returns>
        private bool IsLabelSpaceTight(List<AxisLabel> labelPool, Orientation orientation) {
            float between = orientation == Orientation.Horizontal ? spaceBetweenLabels.x : spaceBetweenLabels.y;
            float requiredSpace = labelPool.Where(label => label.IsVisible)
                .Sum(label => label.OrientedWidth + between);
            float maxSpace = orientation == Orientation.Horizontal ? CanvasReferenceSize.x : CanvasReferenceSize.y;
            return maxSpace < requiredSpace;
        }

        /// <summary>
        /// Sets up a label by activating it, setting its position and appropriately formatting its number value.
        /// </summary>
        /// <param name="label">Label to set up</param>
        /// <param name="position">New position</param>
        /// <param name="labelValue">New number value</param>
        private void SetupLabel(AxisLabel label, Vector2 position, double labelValue) {
            label.Show(true);
            label.SetPosition(position);
            bool scientific = useScientificNotation && CountDigits(labelValue) > maxDisplayedDigits;
            label.SetValue(labelValue, useCommaSeparator, scientific);
        }

        /// <summary>
        /// Counts the number of digits in a number.
        ///
        /// Works with both positive and negative numbers, and even with decimal numbers. Due to floating point
        /// imprecision, it works by counting up to the first trailing zero after the first non-zero digit after the
        /// decimal point.
        ///
        /// Real example: if <c>0.9</c> is represented <c>0.90000000000000002</c> due to floating point imprecision,
        /// the method will count up to <c>0.9</c> and return <c>2</c>.
        /// </summary>
        /// <param name="number">Number to count digits of</param>
        /// <returns>Number of digits</returns>
        private static int CountDigits(double number) {
            number = Math.Abs(number);
            const float precision = 10;

            string digitsOnly = number.ToString("G29", System.Globalization.CultureInfo.InvariantCulture);
            bool afterDecimalPoint = false;
            bool afterFirstNonZeroDigit = false; // first non-zero digit after the decimal point
            int count = 0;

            foreach (char c in digitsOnly) {
                if (c == '.') {
                    afterDecimalPoint = true;
                    continue;
                }

                if (afterDecimalPoint && c != '0') {
                    afterFirstNonZeroDigit = true;
                }
                if (afterFirstNonZeroDigit && c == '0') {
                    break;
                }
                count++;
            }
            return count < precision ? count : CountDigits(number + Math.Pow(10, -precision));
        }
    }
}