using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Graphs {
    /// <summary>
    /// UI controller for the buttons above the graph which interact with it, e.g. the close or the reset zoom button.
    /// </summary>
    public class GraphUIControls : MonoBehaviour {

        public UnityEvent onClickCloseButton;
        public UnityEvent onClickResetZoomButton;

        [SerializeField] private GraphController controller;
        [SerializeField] private Image closeButton;
        [SerializeField] private Button resetZoomButton;

        private void Start() {
            RegisterCallbacks();
            AdjustButtonPositions();
            InjectClickAudio();
        }

        private void RegisterCallbacks() {
            resetZoomButton.onClick.AddListener(() => onClickResetZoomButton?.Invoke());
        }

        private void InjectClickAudio() {
            onClickCloseButton.AddListener(() => ButtonClickAudioInjector.Instance?.PlayOnce());
            onClickResetZoomButton.AddListener(() => ButtonClickAudioInjector.Instance?.PlayOnce());
        }

        /// <summary>
        /// Adjusts the positions of the buttons to fit perfectly in the corner.
        /// </summary>
        private void AdjustButtonPositions() {
            Vector2 backgroundRectSize = controller.BackgroundRectSize;
            // top right corner
            RectTransform closeTransform = closeButton.rectTransform;
            closeTransform.anchoredPosition = new Vector2(backgroundRectSize.x / 2 - closeTransform.rect.width / 2 + 4,
                backgroundRectSize.y / 2 - closeTransform.rect.height / 2);
            // top left corner
            RectTransform resetTransform = (RectTransform)resetZoomButton.transform;
            resetTransform.anchoredPosition = new Vector2(-backgroundRectSize.x / 2 + resetTransform.rect.width / 2 - 4,
                backgroundRectSize.y / 2 - resetTransform.rect.height / 2);
        }

        /// <summary>
        /// Callback for <see cref="GraphInput.onNewRaycastResult"/>.
        /// </summary>
        /// <param name="results">List of raycast results</param>
        public void OnNewRaycastResult(List<RaycastResult> results) {
            foreach (RaycastResult result in results) {
                if (result.gameObject == closeButton.gameObject) {
                    onClickCloseButton?.Invoke();
                }
            }
        }
    }
}