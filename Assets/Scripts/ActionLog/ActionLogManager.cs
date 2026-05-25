using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace ActionLog {
    /// <summary>
    /// Manages the action log UI, which can display messages for a limited time.
    /// </summary>
    public class ActionLogManager : MonoBehaviour {

        /// <summary>
        /// Top margin of the action log UI in pixels.
        /// </summary>
        [SerializeField] private float marginTop = 0f;

        /// <summary>
        /// Time in seconds that a message remains visible in the log.
        /// </summary>
        private const float visibilityTime = 8f;

        private VisualElement ui;
        private ScrollView scrollView;

        private readonly List<DisplayedMessage> displayedMessages = new();

        private void Awake() {
            ui = GetComponent<UIDocument>().rootVisualElement;
            scrollView = ui.Q<ScrollView>("ActionLog_ScrollView");
        }

        private void Start() {
            ui.style.marginTop = marginTop;
        }

        private void Update() {
            RemoveOldMessages();
            ToggleVisibility();
        }

        /// <summary>
        /// Writes a new message to the action log.
        /// </summary>
        /// <param name="message">Message to add</param>
        public void Write(LogMessage message) {
            ActionLogItem item = new();
            item.Create(message);
            displayedMessages.Add(new DisplayedMessage {
                item = item,
                creationTime = Time.time
            });
            scrollView.contentContainer.Add(item);
            scrollView.schedule.Execute(() => ScrollToBottom(item));
        }

        /// <summary>
        /// Scrolls the scroll view to the bottom to show the newly added item and animates it in.
        /// </summary>
        /// <param name="newItem">Newly added item</param>
        private void ScrollToBottom(ActionLogItem newItem) {
            newItem.MarkDirtyRepaint();
            scrollView.contentContainer.MarkDirtyRepaint();
            scrollView.schedule.Execute(() => {
                scrollView.ScrollTo(newItem);
                newItem.AnimateIn();
            });
        }

        /// <summary>
        /// Removes old messages that have exceeded their visibility time.
        /// </summary>
        private void RemoveOldMessages() {
            float currentTime = Time.time;
            DisplayedMessage[] toRemove =
                displayedMessages.Where(msg => currentTime - msg.creationTime > visibilityTime).ToArray();

            foreach (DisplayedMessage msg in toRemove) {
                StartCoroutine(RemoveMessage(msg));
            }
        }

        private IEnumerator RemoveMessage(DisplayedMessage msg) {
            displayedMessages.Remove(msg);
            msg.item.AnimateOut();
            yield return new WaitForSeconds(ActionLogItem.AnimationDuration * 1.5f);
            scrollView.contentContainer.Remove(msg.item);
        }

        /// <summary>
        /// Toggles the visibility of the action log UI based on whether there are messages to display.
        /// </summary>
        private void ToggleVisibility() {
            int childCount = scrollView.contentContainer.hierarchy.childCount;
            ui.style.visibility = childCount > 0 ? Visibility.Visible : Visibility.Hidden;
        }

        private class DisplayedMessage {
            public ActionLogItem item;
            public float creationTime;
        }
    }
}