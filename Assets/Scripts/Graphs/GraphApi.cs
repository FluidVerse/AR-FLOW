using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Graphs.Content;
using UnityEngine;
using UnityEngine.Events;

namespace Graphs {
    /// <summary>
    /// Public API that exposes all methods for creating and manipulating the graph.
    /// </summary>
    [SuppressMessage("ReSharper", "ParameterHidesMember")]
    public class GraphApi : MonoBehaviour {

        /// <summary>
        /// Callback when the graph is enabled (<c>true</c>) or disabled (<c>false</c>).
        /// </summary>
        public UnityEvent<bool> onEnableGraph;

        /// <summary>
        /// Callback when the graph is disabled (<c>true</c>) or enabled (<c>false</c>).
        ///
        /// Both callbacks exist at once to simplify hooking into either event inside the Unity editor without needing
        /// to invert the boolean value.
        /// </summary>
        public UnityEvent<bool> onDisableGraph;

        /// <summary>
        /// Whether the graph is currently enabled.
        /// </summary>
        public bool IsGraphEnabled { get; private set; }

        /// <summary>
        /// List of all points currently displayed on the graph.
        /// </summary>
        public List<PointData> DisplayedPoints => content.DisplayedPoints;

        /// <summary>
        /// Index of the currently selected point in <see cref="DisplayedPoints"/>, or <c>-1</c> if no point is
        /// selected.
        /// </summary>
        public int SelectedPointIndex => content.SelectedPointIndex;

        [SerializeField] private GraphController controller;
        [SerializeField] private GraphContent content;
        [SerializeField] private Canvas graphCanvas;

        /// <summary>
        /// Default size of the graph, used when calling <see cref="ResizeToDefault"/>.
        /// </summary>
        private readonly float[] defaultSize = { -10, 10, -10, 10 }; // [0]: xMin, [1]: xMax, [2]: yMin, [3]: yMax

        /// <summary>
        /// See <see cref="Resize"/>.
        /// </summary>
        private bool fitToEqualZoomLevel = true;

        private void Start() {
            IsGraphEnabled = true;
            DisableGraph();
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
        /// <param name="setAsDefault">
        /// If <c>true</c>, sets the specified size as the new default size for <see cref="ResizeToDefault"/>
        /// </param>
        public void Resize(float xMin, float xMax, float yMin, float yMax, bool fitToEqualZoomLevel,
            bool setAsDefault = true) {
            if (xMin >= xMax) {
                Debug.LogError("xMin must be less than xMax.");
                return;
            }
            if (yMin >= yMax) {
                Debug.LogError("yMin must be less than yMax.");
                return;
            }

            controller.Resize(xMin, xMax, yMin, yMax, fitToEqualZoomLevel);
            Debug.Log(
                $"Resized graph to fit x: [{xMin},{xMax}] and y: [{yMin},{yMax}] with fitToEqualZoomLevel={fitToEqualZoomLevel}");
            if (setAsDefault) {
                SetDefaultSize(xMin, xMax, yMin, yMax, fitToEqualZoomLevel);
            }
        }

        /// <summary>
        /// Sets the default size of the graph to be used when calling <see cref="ResizeToDefault"/>.
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
        public void SetDefaultSize(float xMin, float xMax, float yMin, float yMax, bool fitToEqualZoomLevel) {
            defaultSize[0] = xMin;
            defaultSize[1] = xMax;
            defaultSize[2] = yMin;
            defaultSize[3] = yMax;
            this.fitToEqualZoomLevel = fitToEqualZoomLevel;
            Debug.Log(
                $"Set default size to x: [{xMin},{xMax}] and y: [{yMin},{yMax}] with fitToEqualZoomLevel={fitToEqualZoomLevel}");
        }

        /// <summary>
        /// Resizes the graph to the default size set by <see cref="SetDefaultSize"/>.
        /// </summary>
        public void ResizeToDefault() {
            Resize(defaultSize[0], defaultSize[1], defaultSize[2], defaultSize[3], fitToEqualZoomLevel, false);
        }

        /// <summary>
        /// Sets the value range of the graph.
        ///
        /// When setting a custom value range, make sure that the graph does not display values outside of this range
        /// at that time. 
        ///
        /// If <c>null</c> is passed, <see cref="GraphController.DefaultValueRange"/> will be used.
        /// </summary>
        /// <param name="newRangeOrNull">
        /// New value range as <see cref="Vector2"/>, where x is the minimal absolute value for both axes and y is the
        /// maximal absolute value for both axes; or <c>null</c> to reset to default.
        /// </param>
        public void SetValueRange(Vector2? newRangeOrNull) {
            if (newRangeOrNull == null) {
                controller.ValueRange = GraphController.DefaultValueRange;
                Debug.Log("Set value range to default.");
                return;
            }

            Vector2 newRange = newRangeOrNull.Value;
            if (newRange.x >= newRange.y) {
                Debug.LogError("Invalid value range: min must be less than max.");
                return;
            }
            if (newRange.x < GraphController.DefaultValueRange.x || newRange.y > GraphController.DefaultValueRange.y) {
                Debug.LogError("Invalid value range: cannot be larger than the default range.");
                return;
            }

            Vector2 oldRange = controller.ValueRange;
            controller.ValueRange = newRange;

            if (!controller.IsGraphStateValid()) {
                controller.ValueRange = oldRange;
                Debug.LogError("Cannot set value range: current graph state would become invalid.");
                return;
            }

            Debug.Log($"Set value range to [{newRange.x}, {newRange.y}].");
        }

        /// <summary>
        /// Adds a new point to the graph with the specified data.
        /// </summary>
        /// <param name="pointData">See <see cref="PointData"/></param>
        public void AddPoint(PointData pointData) {
            content.AddPoint(pointData);
            Debug.Log($"Added point '{pointData.name}' at {pointData.position}");
        }

        /// <summary>
        /// Adds multiple points to the graph with the specified data.
        /// </summary>
        /// <param name="points">List of points, see <see cref="PointData"/></param>
        public void AddPoints(List<PointData> points) {
            foreach (PointData point in points) {
                content.AddPoint(point);
            }
        }

        /// <summary>
        /// Removes all points from the graph.
        /// </summary>
        public void ClearPoints() {
            content.ClearPoints();
            Debug.Log("Cleared all points.");
        }

        /// <summary>
        /// Selects the point at the given index in <see cref="DisplayedPoints"/>.
        /// </summary>
        /// <param name="index">Point index</param>
        public void SelectPointAtIndex(int index) {
            if (index < 0 || index >= DisplayedPoints.Count) {
                Debug.LogError($"Index {index} is out of bounds for displayed points (count: {DisplayedPoints.Count})",
                    this);
                return;
            }
            content.SelectPointAtIndex(index);
        }

        /// <summary>
        /// Shows the axis unit labels with the specified texts.
        /// </summary>
        /// <param name="xUnitText">Text for the x-axis</param>
        /// <param name="yUnitText">Text for the y-axis</param>
        public void SetAxisUnitLabels(string xUnitText, string yUnitText) {
            content.SetAxisUnitLabels(xUnitText, yUnitText);
            Debug.Log($"Set axis unit texts to '{xUnitText}' (x) and '{yUnitText}' (y).");
        }

        /// <summary>
        /// Hides the axis unit labels.
        /// </summary>
        public void HideAxisUnitLabels() {
            content.HideAxisUnitLabels();
            Debug.Log("Hid axis unit labels.");
        }

        /// <summary>
        /// Enables the graph, making it visible and interactive.
        /// </summary>
        public void EnableGraph() {
            IsGraphEnabled = true;
            graphCanvas.enabled = true;
            onEnableGraph?.Invoke(true);
            onDisableGraph?.Invoke(false);
            Debug.Log("Graph enabled.");
        }

        /// <summary>
        /// Disables the graph, making it invisible.
        /// </summary>
        public void DisableGraph() {
            IsGraphEnabled = false;
            graphCanvas.enabled = false;
            onEnableGraph?.Invoke(false);
            onDisableGraph?.Invoke(true);
            Debug.Log("Graph disabled.");
        }
    }
}