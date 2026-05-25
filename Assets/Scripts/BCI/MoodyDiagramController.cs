using Audio;
using Graphs;
using Graphs.Content;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BCI {
    /// <summary>
    /// Controller for the Moody diagram graph content.
    /// </summary>
    public class MoodyDiagramController : MonoBehaviour {

        /// <summary>
        /// Callback when the user clicks on the diagram.
        ///
        /// If the passed argument is <c>true</c>, it means that the user has correctly clicked on
        /// <see cref="desiredPoint"/>.
        /// </summary>
        public UnityEvent<bool> onClickOnDiagram;

        [SerializeField] private GraphApi graphApi;
        [SerializeField] private GraphController graphController;
        [SerializeField] private Image diagram;

        /// <summary>
        /// Sprite to show after all points have been found (explanation diagram).
        /// </summary>
        [SerializeField] private Sprite explanationDiagramSprite;

        /// <summary>
        /// Ruler between the selected point and y-axis to emphasize the friction factor value.
        /// </summary>
        [SerializeField] private Image ruler;

        /// <summary>
        /// Background image for <see cref="rulerText"/>.
        /// </summary>
        [SerializeField] private Image rulerTextBackground;

        /// <summary>
        /// Text element displaying the friction factor value on the ruler.
        /// </summary>
        [SerializeField] private TMP_Text rulerText;

        /// <summary>
        /// Calibrated graph position (0-1) of the desired point. Device-independent.
        /// </summary>
        private Vector2? desiredGraphPosition;

        /// <summary>
        /// Point in (Re, f) that we are looking for, for display purposes.
        /// </summary>
        private Vector2? desiredDiagramPosition;

        /// <summary>
        /// Name of the desired point to display in the UI after it is guessed.
        /// </summary>
        private string desiredPointName = "";

        /// <summary>
        /// Max click distance in graph space (0-1) between the desired point and the clicked point to consider it a
        /// valid click.
        /// </summary>
        [SerializeField] private float maxClickDistance = 0.01f;

        private void Start() {
            Vector2Int backgroundRectSize = graphController.BackgroundRectSize;
            diagram.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, backgroundRectSize.x);
            diagram.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, backgroundRectSize.y);
        }

        private void Update() {
            UpdateRuler();
        }

        /// <summary>
        /// Sets the desired point that the user should click on.
        /// </summary>
        /// <param name="graphPosition">
        /// Calibrated position in graph space (0-1); if <c>null</c>, no point is desired and clicks are ignored
        /// </param>
        /// <param name="diagramPosition">Position in (Re, f) for display purposes</param>
        /// <param name="pointName">Name of the desired point to display in the UI after it is guessed</param>
        public void SetDesiredPoint(Vector2? graphPosition, Vector2? diagramPosition, string pointName) {
            desiredGraphPosition = graphPosition;
            desiredDiagramPosition = diagramPosition;
            desiredPointName = pointName;
        }

        /// <summary>
        /// Switches to the explanation diagram after all points have been found.
        /// </summary>
        public void ShowExplanationDiagram() {
            if (explanationDiagramSprite != null) {
                diagram.sprite = explanationDiagramSprite;
            }
        }

        /// <summary>
        /// Callback for <see cref="GraphContent.OnClickOutsideContent"/>.
        /// </summary>
        /// <param name="touchPosition">Screen position where the user clicked</param>
        public void OnClickOnGraph(Vector2 touchPosition) {
            ButtonClickAudioInjector.Instance?.PlayOnce();
            Vector2 graphPosition = graphController.ToGraphSpace(touchPosition);
            Vector2 diagramPosition = GraphToDiagramSpace(graphPosition);
            Debug.Log(
                $"Clicked on touch position {touchPosition}, graph position ({graphPosition.x:F4}, {graphPosition.y:F4}), diagram position {diagramPosition}");
            if (desiredGraphPosition == null) {
                Debug.Log("No desired point set, ignoring click");
                return; // ignore clicks if we don't have a desired point
            }

            float distance = Vector2.Distance(graphPosition, desiredGraphPosition.Value);
            Debug.Log(
                $"Desired point {desiredDiagramPosition}: graph position {desiredGraphPosition.Value}, distance {distance}");

            if (distance <= maxClickDistance) {
                // clicked correctly - add point BEFORE invoking callback (callback changes desiredPointName)
                AddPoint(desiredPointName, desiredGraphPosition.Value, desiredDiagramPosition ?? diagramPosition);
                onClickOnDiagram?.Invoke(true);
            } else {
                // clicked incorrectly
                onClickOnDiagram?.Invoke(false);
            }
        }

        /// <summary>
        /// Adds a point to the Moody diagram at the specified graph position.
        /// </summary>
        /// <param name="pointName">Name of the point</param>
        /// <param name="graphPosition">Calibrated position in graph space (0-1)</param>
        /// <param name="diagramPosition">Position in (Re, f) for display</param>
        private void AddPoint(string pointName, Vector2 graphPosition, Vector2 diagramPosition) {
            graphApi.AddPoint(new PointData(pointName, graphPosition, Color.red, diagramPosition));
            graphApi.SelectPointAtIndex(graphApi.DisplayedPoints.Count - 1); // select the newly added point
        }

        /// <summary>
        /// Updates the ruler and text based on the currently selected point.
        ///
        /// What's fixed: ruler y position, text y position (because it's a child of the point info box), enabled/
        /// disabled state (same reason)
        /// What needs to be updated: ruler x position, ruler width, text x position, text content
        /// </summary>
        private void UpdateRuler() {
            int selectedIndex = graphApi.SelectedPointIndex;
            if (selectedIndex == -1) {
                return;
            }

            PointData selectedPoint = graphApi.DisplayedPoints[selectedIndex];
            Vector2 selectedPointScreenPos = graphController.ToScreenSpace(selectedPoint.position);

            const float selectedPointWidth = 55f; // in screen space (px)
            float rulerWidth = graphController.ToScreenSpace(new Vector2(selectedPoint.position.x, 0)).x;
            float rulerXPos = selectedPointScreenPos.x - rulerWidth / 2f - selectedPointWidth / 2f;

            ruler.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rulerWidth);
            ruler.transform.position = new Vector2(rulerXPos, ruler.transform.position.y);

            const float textStartXPos = 0.0325f; // in graph space
            float textXPos = graphController.ToScreenSpace(new Vector2(textStartXPos, 0)).x;
            rulerTextBackground.transform.position = new Vector2(textXPos, rulerText.transform.position.y);
            rulerText.text = $"{selectedPoint.displayedPosition.y:F3}";
        }

        /// <summary>
        /// Converts a position in graph space (x, y in [0; 1]) to the corresponding (Re, f) in the Moody diagram.
        /// </summary>
        /// <param name="graphPosition">
        /// Position in graph space, where (0,0) is bottom-left and (1,1) is top-right of the diagram area.
        /// </param>
        /// <returns>Corresponding (Re, f) values</returns>
        private static Vector2 GraphToDiagramSpace(Vector2 graphPosition) {
            // --- Image properties (Moody_Final_English.png: 2048x1365) ---
            const float imageWidth = 2048f;
            const float imageHeight = 1365f;

            // --- Diagram region (in pixels, gemessen aus dem Bild) ---
            const float xMinPx = 266f;   // linker Rand (Y-Achse)
            const float xMaxPx = 1742f;  // rechter Rand (2048 - 306)
            const float yMinPx = 1125f;  // bottom (1365 - 240)
            const float yMaxPx = 114f;   // top

            // --- Diagram axis ranges (logarithmic, aus dem Bild abgelesen) ---
            const float xMinVal = 1000f;  // 10^3
            const float xMaxVal = 1e8f;   // 10^8
            const float yMinVal = 0.008f; // unterer Rand
            const float yMaxVal = 0.15f;  // oberer Rand

            // --- Step 1: Convert from normalized (0–1) graph space to pixel coordinates ---
            float pxX = graphPosition.x * imageWidth;
            float pxY = graphPosition.y * imageHeight;

            // --- Step 2: Clamp to the diagram area (optional) ---
            pxX = Mathf.Clamp(pxX, xMinPx, xMaxPx);
            pxY = Mathf.Clamp(pxY, yMaxPx, yMinPx); // note: Y axis is inverted (image origin is top-left)

            // --- Step 3: Map pixel position to normalized coordinates within the diagram area ---
            float normX = (pxX - xMinPx) / (xMaxPx - xMinPx);
            float normY = (pxY - yMaxPx) / (yMinPx - yMaxPx);

            // --- Step 4: Map normalized diagram coords (0–1) to log scale values ---
            float logX = Mathf.Lerp(Mathf.Log10(xMinVal), Mathf.Log10(xMaxVal), normX);
            float logY = Mathf.Lerp(Mathf.Log10(yMinVal), Mathf.Log10(yMaxVal), normY);

            // --- Step 5: Convert back from log10 space to real values ---
            float valueX = Mathf.Pow(10f, logX);
            float valueY = Mathf.Pow(10f, logY);

            return new Vector2(valueX, valueY);
        }

        /// <summary>
        /// Converts (Re, f) in the Moody diagram to a position in graph space (x, y in [0; 1]).
        /// </summary>
        /// <param name="diagramPosition">(Re, f) in the Moody diagram</param>
        /// <returns>Corresponding (Re, f) values</returns>
        private static Vector2 DiagramToGraphSpace(Vector2 diagramPosition) {
            // --- Image properties (Moody_Final_English.png: 2048x1365) ---
            const float imageWidth = 2048f;
            const float imageHeight = 1365f;

            // --- Diagram region (in pixels, gemessen aus dem Bild) ---
            const float xMinPx = 266f;   // linker Rand (Y-Achse)
            const float xMaxPx = 1742f;  // rechter Rand (2048 - 306)
            const float yMinPx = 1125f;  // bottom (1365 - 240)
            const float yMaxPx = 114f;   // top

            // --- Diagram axis ranges (logarithmic, aus dem Bild abgelesen) ---
            const float xMinVal = 1000f;  // 10^3
            const float xMaxVal = 1e8f;   // 10^8
            const float yMinVal = 0.008f; // unterer Rand
            const float yMaxVal = 0.15f;  // oberer Rand

            // Step 1: Diagram values → log space
            float logXVal = Mathf.Log10(diagramPosition.x);
            float logYVal = Mathf.Log10(diagramPosition.y);

            // Step 2: Log space → normalized diagram coords (0–1)
            float normX = Mathf.InverseLerp(Mathf.Log10(xMinVal), Mathf.Log10(xMaxVal), logXVal);
            float normY = Mathf.InverseLerp(Mathf.Log10(yMinVal), Mathf.Log10(yMaxVal), logYVal);

            // Step 3: Normalized diagram → pixel coords
            float pxX = Mathf.Lerp(xMinPx, xMaxPx, normX);
            float pxY = Mathf.Lerp(yMaxPx, yMinPx, normY); // Y mapping matches forward function

            // Step 4: Pixel → graph normalized coords (0–1)
            float graphX = pxX / imageWidth;
            float graphY = pxY / imageHeight;

            return new Vector2(graphX, graphY);
        }
    }
}