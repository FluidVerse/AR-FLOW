using UnityEngine;
using UnityEngine.UI;

namespace Graphs.Content {
    /// <summary>
    /// Represents a displayed point on the graph, including its visual representation and selection state.
    /// </summary>
    public class PointObject : MonoBehaviour {

        [SerializeField] private Image pointImage;
        [SerializeField] private Image pointSelectedImage;

        /// <summary>
        /// The data associated with this point.
        /// </summary>
        public PointData Data { get; private set; }

        /// <summary>
        /// Whether this point is currently selected.
        /// </summary>
        public bool IsSelected { get; private set; }

        /// <summary>
        /// Initializes the point with the given data, setting its color and visibility.
        /// </summary>
        /// <param name="pointData">Point data</param>
        public void InitializeWithData(PointData pointData) {
            Data = pointData;
            pointImage.color = pointData.color;
            pointSelectedImage.color = pointData.color;
            pointImage.gameObject.SetActive(true);
            pointSelectedImage.gameObject.SetActive(false);
        }

        /// <summary>
        /// Sets the selection state of the point, updating its visual representation accordingly.
        /// </summary>
        /// <param name="selected">Whether the point should be selected</param>
        public void Select(bool selected) {
            IsSelected = selected;
            pointImage.gameObject.SetActive(!selected);
            pointSelectedImage.gameObject.SetActive(selected);
        }

        /// <summary>
        /// Sets the size of the point marker.
        /// </summary>
        /// <param name="size">Size in pixels</param>
        public void SetSize(float size) {
            RectTransform rect = pointImage.rectTransform;
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);

            RectTransform selectedRect = pointSelectedImage.rectTransform;
            selectedRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
            selectedRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
        }
    }
}