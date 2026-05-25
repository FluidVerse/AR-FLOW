using System;
using System.Collections.Generic;
using MainMenu;
using TMPro;
using UnityEngine;

namespace AR {
    /// <summary>
    /// Controller for the AR alignment UI.
    ///
    /// It differentiates between the different <see cref="ARState"/>s and shows/hides the relevant UI elements. Also
    /// provides a simple hint box that has to be closed manually by the user.
    /// </summary>
    public class ARAlignmentUIController : MonoBehaviour {

        private const string positionUnstableMessage =
            "It appears that the model's position is unstable. Try to keep the anchor surface in the frame at all times. If necessary, hold the camera briefly in front of the anchor surface to stabilize the position.";

        [SerializeField] private ARAnchorAlignedSpawner spawner;

        /// <summary>
        /// The different sub-UIs for the various <see cref="ARState"/>s.
        /// </summary>
        [SerializeField] private List<StateUIMapping> uiElementMappings;

        [SerializeField] private Canvas canvas;
        [SerializeField] private RectTransform hintBox;
        [SerializeField] private RectTransform hintBoxBackground;
        [SerializeField] private TMP_Text hintText;

        private ARSceneHandler sceneHandler;

        private void Awake() {
            foreach (var mapping in uiElementMappings) {
                mapping.uiElement.SetActive(false);
            }
            PositionHintBox();
            OnCloseHintBoxButtonClick();
        }

        private void Start() {
            sceneHandler = ARSceneHandler.Instance;
            if (sceneHandler == null) {
                Debug.LogError("ARSceneHandler not found", this);
            }

            sceneHandler.OnStateChanged.AddListener(OnARStateChanged);
            sceneHandler.OnEventPublished.AddListener(OnEventPublished);
            MainMenuManager.Instance.OnMainMenuToggle.AddListener(OnMainMenuToggle);

            if (sceneHandler.State != ARState.Inactive) {
                // if state has already changed before we could register, update UI now
                OnARStateChanged(sceneHandler.State);
            }
        }

        private void OnDisable() {
            sceneHandler.OnStateChanged.RemoveListener(OnARStateChanged);
            sceneHandler.OnEventPublished.RemoveListener(OnEventPublished);
            MainMenuManager.Instance.OnMainMenuToggle.RemoveListener(OnMainMenuToggle);
        }

        public void OnConfirmAlignmentButtonClick() {
            spawner.TrySpawnObject();
        }

        public void OnCloseHintBoxButtonClick() {
            hintBox.gameObject.SetActive(false);
        }

        /// <summary>
        /// Callback for <see cref="ARSceneHandler.OnStateChanged"/>.
        /// </summary>
        private void OnARStateChanged(ARState newState) {
            foreach (var mapping in uiElementMappings) {
                mapping.uiElement.SetActive(mapping.state == newState);
            }
        }

        /// <summary>
        /// Callback for <see cref="MainMenuManager.OnMainMenuToggle"/>.
        /// </summary>
        private void OnMainMenuToggle(bool isActive) {
            canvas.enabled = !isActive;
        }

        private void OnEventPublished(object sender, AREvent arEvent) {
            if (ReferenceEquals(sender, this)) {
                return; // ignore events published by ourselves
            }

            if (arEvent == AREvent.PositionUnstable) {
                ShowHintBox(positionUnstableMessage);
            }
        }

        /// <summary>
        /// Positions the hint box in the top right corner of the screen.
        /// </summary>
        private void PositionHintBox() {
            const float margin = 16f;
            Rect canvasRect = canvas.GetComponent<RectTransform>().rect;
            hintBox.anchoredPosition = new Vector2(canvasRect.width / 2f - hintBoxBackground.rect.width / 2f - margin,
                canvasRect.height / 2f - hintBoxBackground.rect.height / 2f - margin);
        }

        /// <summary>
        /// Shows the hint box with the given message.
        /// </summary>
        /// <param name="message">Message to show</param>
        private void ShowHintBox(string message) {
            hintText.text = message;
            hintBox.gameObject.SetActive(true);
        }

        [Serializable]
        private class StateUIMapping {
            public ARState state;
            public GameObject uiElement;
        }
    }
}