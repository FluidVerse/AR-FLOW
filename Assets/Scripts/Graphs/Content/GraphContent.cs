using System.Collections.Generic;
using System.Linq;
using Audio;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Graphs.Content {
    /// <summary>
    /// Manages the content displayed on the graph, including points and point info UI.
    /// </summary>
    public class GraphContent : MonoBehaviour {

        /// <summary>
        /// Callback for a valid click on the graph background that has been successfully differentiated from a drag
        /// and which was not already consumed for one of these two actions:
        /// - selecting a point
        /// - unselecting the currently selected point
        ///
        /// Argument is the screen position where the user clicked.
        /// </summary>
        public UnityEvent<Vector2> OnClickOutsideContent;

        /// <summary>
        /// List of all points currently displayed on the graph.
        /// </summary>
        public List<PointData> DisplayedPoints => points.Select(p => p.Data).ToList();

        /// <summary>
        /// Index of the currently selected point in <see cref="points"/>, or <c>-1</c> if no point is selected.
        /// </summary>
        public int SelectedPointIndex => selectedPoint != null ? points.IndexOf(selectedPoint) : -1;

        [SerializeField] private GraphController controller;
        [SerializeField] private GameObject pointPrefab;
        [SerializeField] private PointInfo pointInfo;

        /// <summary>
        /// Labels for the units of the x- and y-axis, respectively.
        /// </summary>
        [SerializeField] private TMP_Text[] axisUnitLabels = new TMP_Text[2];

        /// <summary>
        /// Offset of the point info box relative to the point's screen position.
        /// </summary>
        [SerializeField] private Vector2 pointInfoOffset;

        /// <summary>
        /// Maximum distance (in px) between the user's click position and the point's screen position to select it.
        /// </summary>
        [SerializeField] private float pointSelectionDistance = 20f;

        /// <summary>
        /// Size of the point marker in pixels. If 0, uses prefab default size.
        /// </summary>
        [SerializeField] private float pointSize = 0f;

        /// <summary>
        /// Offset of the axis unit labels relative to the axes, in px.
        ///
        /// <c>x</c> is the y-offset of the x-axis unit label, <c>y</c> is the x-offset of the y-axis unit label.
        /// </summary>
        [SerializeField] private Vector2 axisUnitLabelOffsets = new(35f, 85f);

        private readonly List<PointObject> points = new();
        private PointObject selectedPoint; // can be null; if not null, must be in points list

        private void Awake() {
            pointPrefab.SetActive(false);
            pointInfo.UseCommaSeparator = controller.UseCommaSeparator;
        }

        /// <summary>
        /// Adds a new point to the graph with the specified data.
        /// </summary>
        /// <param name="pointData">See <see cref="PointData"/></param>
        public void AddPoint(PointData pointData) {
            GameObject obj = Instantiate(pointPrefab, pointPrefab.transform.parent);
            PointObject point = obj.GetComponent<PointObject>();
            point.InitializeWithData(pointData);
            if (pointSize > 0f) {
                point.SetSize(pointSize);
            }
            points.Add(point);
            obj.SetActive(true);
            RecalculatePoints();
        }

        /// <summary>
        /// Removes all points from the graph.
        /// </summary>
        public void ClearPoints() {
            foreach (PointObject point in points) {
                Destroy(point.gameObject);
            }
            points.Clear();
            selectedPoint = null;
        }

        /// <summary>
        /// Selects the point at the given index in <see cref="DisplayedPoints"/>.
        /// </summary>
        /// <param name="index">Point index</param>
        public void SelectPointAtIndex(int index) {
            SelectPoint(points[index]);
            UpdatePointInfoPosition();
        }

        /// <summary>
        /// Shows the axis unit labels with the specified texts.
        /// </summary>
        /// <param name="xUnitText">Text for the x-axis</param>
        /// <param name="yUnitText">Text for the y-axis</param>
        public void SetAxisUnitLabels(string xUnitText, string yUnitText) {
            axisUnitLabels[0].text = $"[{xUnitText}]";
            axisUnitLabels[1].text = $"[{yUnitText}]";
        }

        /// <summary>
        /// Hides the axis unit labels.
        /// </summary>
        public void HideAxisUnitLabels() {
            axisUnitLabels[0].text = "";
            axisUnitLabels[1].text = "";
        }

        /// <summary>
        /// Callback for <see cref="GraphController.onGraphRecalculated"/>.
        /// </summary>
        /// <param name="data">See <see cref="RecalculationData"/></param>
        public void OnGraphRecalculated(RecalculationData data) {
            RecalculatePoints();
            UpdatePointInfoPosition();
            RecalculateAxisUnitLabels(data);
        }

        /// <summary>
        /// Callback for <see cref="GraphInput.OnValidGraphClick"/>.
        /// </summary>
        /// <param name="touchPosition">Screen position where the user clicked</param>
        public void OnClickOnGraph(Vector2 touchPosition) {
            UnselectAll();
            bool wasSelected = SelectNearestPoint(touchPosition);
            if (wasSelected) {
                ButtonClickAudioInjector.Instance?.PlayOnce();
            }
            // Klick IMMER weiterreichen, damit MoodyDiagramController auch prüfen kann
            OnClickOutsideContent?.Invoke(touchPosition);
            UpdatePointInfoPosition();
        }

        /// <summary>
        /// Selects the point nearest to the given touch position, if within <see cref="pointSelectionDistance"/>.
        ///
        /// If no point is within that distance, no point is selected.
        /// </summary>
        /// <param name="touchPosition"> Screen position where the user clicked</param>
        /// <returns>Whether a point was successfully selected</returns>
        private bool SelectNearestPoint(Vector2 touchPosition) {
            float pointSelectionDistanceSqr = Mathf.Pow(pointSelectionDistance, 2);
            PointObject nearestPoint = null;
            float nearestDistanceSqr = float.MaxValue;
            bool wasSelected = false;

            foreach (PointObject point in points) {
                Vector2 screenPoint = controller.ToScreenSpace(point.Data.position);
                float distanceSqr = (screenPoint - touchPosition).sqrMagnitude;
                if (distanceSqr < nearestDistanceSqr && distanceSqr <= pointSelectionDistanceSqr) {
                    nearestDistanceSqr = distanceSqr;
                    nearestPoint = point;
                }
            }

            if (nearestPoint != null) {
                SelectPoint(nearestPoint);
                wasSelected = true;
            }
            return wasSelected;
        }

        /// <summary>
        /// Selects the given point and shows its info in the point info box.
        /// </summary>
        /// <param name="point">Point to select</param>
        private void SelectPoint(PointObject point) {
            if (selectedPoint != null) {
                selectedPoint.Select(false); // unselect previously selected point
            }
            selectedPoint = point;
            selectedPoint.Select(true);
            pointInfo.SetContent(selectedPoint.Data);
        }

        /// <summary>
        /// Unselects any currently selected point and hides the point info box.
        /// </summary>
        /// <returns><c>true</c> if a point was unselected, <c>false</c> if no point was selected anyway</returns>
        private bool UnselectAll() {
            bool wasUnselected = false;
            if (selectedPoint != null) {
                selectedPoint.Select(false);
                selectedPoint = null;
                wasUnselected = true;
            }

            pointInfo.SetContent(null);
            return wasUnselected;
        }

        /// <summary>
        /// Recalculates the screen positions of all points based on their data positions.
        /// </summary>
        private void RecalculatePoints() {
            foreach (PointObject point in points) {
                Vector2 screenPoint = controller.ToScreenSpace(point.Data.position);
                point.transform.position = screenPoint;
            }
        }

        /// <summary>
        /// Updates the position of the point info box to follow <see cref="selectedPoint"/>.
        /// </summary>
        private void UpdatePointInfoPosition() {
            if (selectedPoint == null) {
                return;
            }
            Vector2 screenPoint = controller.ToScreenSpace(selectedPoint.Data.position);
            pointInfo.transform.position = screenPoint + pointInfoOffset;
        }


        /// <summary>
        /// Recalculates the positions of <see cref="axisUnitLabels"/>.
        /// </summary>
        private void RecalculateAxisUnitLabels(RecalculationData data) {
            Vector2 windowBorderWidth = controller.WindowBorderWidth;

            // x-axis unit label
            Vector2 xUnitLabelSize = axisUnitLabels[0].rectTransform.rect.size;
            Vector2 xLabelPosition = new Vector2(Screen.width - windowBorderWidth.x - xUnitLabelSize.x / 2f,
                data.axisPositions.x + axisUnitLabelOffsets.x);
            axisUnitLabels[0].rectTransform.position = xLabelPosition;

            // y-axis unit label
            Vector2 yUnitLabelSize = axisUnitLabels[1].rectTransform.rect.size;
            Vector2 yLabelPosition = new Vector2(data.axisPositions.y + axisUnitLabelOffsets.y,
                Screen.height - windowBorderWidth.y - yUnitLabelSize.y / 2f);
            axisUnitLabels[1].rectTransform.position = yLabelPosition;
        }
    }
}