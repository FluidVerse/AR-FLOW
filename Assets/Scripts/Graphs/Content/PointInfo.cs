using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Graphs.Content {
    /// <summary>
    /// Manages the point info box UI, displaying information about a selected point.
    /// </summary>
    public class PointInfo : MonoBehaviour {

        /// <summary>
        /// Text element for displaying the name of the point.
        /// </summary>
        [SerializeField] private TMP_Text nameText;

        /// <summary>
        /// Text element for displaying the coordinates of the point.
        /// </summary>
        [SerializeField] private TMP_Text coordinateText;

        /// <summary>
        /// Triangle sprite used for the point info box pointer.
        /// </summary>
        [SerializeField] private SpriteRenderer triangleSpriteRenderer;

        /// <summary>
        /// Image component for displaying the triangle pointer.
        /// </summary>
        [SerializeField] private Image triangleImage;

        /// <summary>
        /// Number of decimal places to display for the coordinates in <see cref="coordinateText"/>, for x and y
        /// respectively.
        /// </summary>
        [SerializeField] private Vector2Int decimalPlaces = new(2, 2);

        /// <summary>
        /// Whether to use a comma as a decimal separator in <see cref="coordinateText"/> (e.g. <c>0,1</c> instead of
        /// <c>0.1</c>).
        /// </summary>
        public bool UseCommaSeparator { get; set; }

        private void Awake() {
            Show(false);
            triangleImage.sprite = triangleSpriteRenderer.sprite;
            Destroy(triangleSpriteRenderer.gameObject);
            DisableRaycastTargets();
        }

        /// <summary>
        /// Disables raycast targets on all UI elements to prevent blocking clicks on the graph.
        /// </summary>
        private void DisableRaycastTargets() {
            foreach (Graphic graphic in GetComponentsInChildren<Graphic>(true)) {
                graphic.raycastTarget = false;
            }
        }

        /// <summary>
        /// Sets the content of the point info UI based on the provided point data.
        ///
        /// If <paramref name="pointData"/> is null, the UI will be hidden.
        /// </summary>
        /// <param name="pointData">Point data to display</param>
        public void SetContent(PointData? pointData) {
            if (pointData == null) {
                Show(false);
                return;
            }

            Show(true);
            nameText.text = pointData.Value.name;
            string coordinateString = $"({pointData.Value.displayedPosition.x.ToString($"F{decimalPlaces.x}")}, " +
                                      $"{pointData.Value.displayedPosition.y.ToString($"F{decimalPlaces.y}")})";
            coordinateString =
                UseCommaSeparator ? coordinateString.Replace('.', ',') : coordinateString.Replace(',', '.');
            coordinateText.text = coordinateString;
        }

        /// <summary>
        /// Shows/hides the point info UI.
        /// </summary>
        /// <param name="showOrHide">Whether to show or hide it</param>
        private void Show(bool showOrHide) {
            foreach (Transform child in transform) {
                child.gameObject.SetActive(showOrHide);
            }
        }
    }
}