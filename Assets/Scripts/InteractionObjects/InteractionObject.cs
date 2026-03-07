using System;
using System.Collections.Generic;
using ActionLog;
using Controls;
using InteractionMenus;
using Quests;
using UnityEngine;
using static Quests.QuestInteractionTypes;

namespace InteractionObjects {
    /// <summary>
    /// Base class for all interactable objects in the game.
    ///
    /// It contains a list of interactions that can be performed when clicking on the object in the scene.
    /// </summary>
    public class InteractionObject : MonoBehaviour {

        /// <summary>
        /// List of menu elements for this interaction object.
        ///
        /// This list is used to display the interaction menu when the player interacts with the object.
        /// </summary>
        public List<MenuElement> MenuElements { get; } = new();

        /// <summary>
        /// Display name of the interaction object.
        /// </summary>
        public string name;

        /// <summary>
        /// The quest object associated with this interaction object.
        /// </summary>
        [SerializeField] protected QuestObject questObject = QuestObject.None;

        [HideInInspector] public CameraManager cameraManager;

        protected QuestManager questManager;

        protected ActionLogManager log;

        // event that can be detected by a LevelScript
        public event Action<string> OnAnyInteraction;

        public void AddAction(string actionName) {
            OnAnyInteraction?.Invoke(actionName);
        }

        private void Awake() {
            questManager = FindAnyObjectByType<QuestManager>();
            if (questManager == null) {
                Debug.LogError("QuestManager not found", this);
            }
            log = FindAnyObjectByType<ActionLogManager>();
            if (log == null) {
                Debug.LogError("ActionLogManager not found", this);
            }
            cameraManager = FindAnyObjectByType<CameraManager>();
            if (cameraManager == null) {
                Debug.LogError("CameraManager not found", this);
            }
        }

        public void OpenMenuInteraction() {
            questManager.SendInteraction(new QuestInteraction<object>(questObject, OpenInteractionMenu));
            Debug.Log("Interaktionsmen� ge�ffnet.");
        }

        public void OpenDetailView() {
            questManager.SendInteraction(new QuestInteraction<object>(questObject, UseDetailView));
            Debug.Log("Detailansicht.");
            cameraManager.ChangeToDetailView(gameObject);
        }

        /// <summary>
        /// Adds a new <see cref="MenuElement"/> to the list of menu elements.
        /// </summary>
        /// <param name="element">Element to add</param>
        /// <typeparam name="T">Type of the menu element</typeparam>
        /// <returns>Added element</returns>
        protected T AddMenuElement<T>(T element) where T : MenuElement {
            MenuElements.Add(element);
            return element;
        }
    }
}