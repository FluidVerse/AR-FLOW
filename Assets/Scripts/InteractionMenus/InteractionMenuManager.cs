using System.Linq;
using Audio;
using Controls;
using InteractionObjects;
using MainMenu;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using CameraType = Controls.CameraType;

namespace InteractionMenus {
    /// <summary>
    /// Manages the creation, deletion and population with elements of the interaction menu, which is displayed when
    /// the player interacts with (i.e. clicks on) an <see cref="InteractionObject"/>.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class InteractionMenuManager : MonoBehaviour {

        /// <summary>
        /// If <c>true</c>, the manager reacts on screen clicks and shows/hides the interaction menu accordingly.
        /// </summary>
        public bool IsCheckingForInteractions { get; set; } = true;

        /// <summary>
        /// Offset of the interaction menu position relative to the position of <see cref="selectedObject"/>.
        /// </summary>
        [SerializeField] private Vector2 menuPositionOffset = new(0, 0);

        private new Camera camera; // main camera
        private UIDocument uiDocument; // must be attached to the same GameObject
        private InputAction pointAction, clickAction;

        private InteractionMenu menu; // currently displayed interaction menu (only one menu at a time)

        // can be null; never change it directly, use SetSelectedObject() and ResetSelectedObject() instead
        private InteractionObject selectedObject;

        // workaround for: Calling IsPointerOverGameObject() from within event processing (such as from
        // InputAction callbacks) will not work as expected; it will query UI state from the last frame
        private bool isPointerOverGameObject;

        // flag to indicate that the menu should be recreated in the next Update() call (even if multiple properties
        // are changed at once, the menu is only recreated once)
        private bool isRecreationScheduled;

        // flag to indicate that the menu size can maybe change after the next menu recreation for one of the two
        // reasons: 1. MenuElement.IsActive is changed, 2. selectedObject is changed
        private bool isMenuSizeChanging;

        private void Awake() {
            camera = Camera.main;
            if (camera == null) {
                Debug.LogError("Main camera not found");
            }
            uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null) {
                Debug.LogError("UIDocument not found");
            }

            pointAction = InputSystem.actions.FindAction("UI/Point", true);
            clickAction = InputSystem.actions.FindAction("UI/Click", true);

            menu = new InteractionMenu();
            uiDocument.rootVisualElement.Add(menu);
        }

        private void Start() {
            RegisterMainMenu();
        }

        private void OnEnable() {
            RegisterMainMenu();
            clickAction.performed += ClickAction;
        }

        private void OnDisable() {
            MainMenuManager.Instance.OnMainMenuToggle.RemoveListener(OnMainMenuToggle);
            clickAction.performed -= ClickAction;
        }

        private void RegisterMainMenu() {
            MainMenuManager.Instance?.OnMainMenuToggle.RemoveListener(OnMainMenuToggle);
            MainMenuManager.Instance?.OnMainMenuToggle.AddListener(OnMainMenuToggle);
        }

        private void Update() {
            isPointerOverGameObject = EventSystem.current.IsPointerOverGameObject();
            if (isRecreationScheduled) {
                RecreateMenu();
                isRecreationScheduled = false;
            }
            if (menu.IsDisplayed) {
                UpdateMenuPosition();
            }
        }

        private void ClickAction(InputAction.CallbackContext ctx) {
            if (ctx.ReadValueAsButton()) {
                OnClickScreen();
            }
        }

        /// <summary>
        /// Callback for the click input action, i.e. when the player clicks on the screen.
        /// </summary>
        private void OnClickScreen() {
            if (!IsCheckingForInteractions || menu.MouseOver(pointAction.ReadValue<Vector2>())) {
                return; // not checking for interactions or pointer over currently opened menu
            }
            if (isPointerOverGameObject) {
                HideInteractionMenu();
                return; // pointer over other UI element 
            }

            Ray ray = camera.ScreenPointToRay(pointAction.ReadValue<Vector2>());
            if (!(Physics.Raycast(ray, out RaycastHit h) && h.collider.TryGetComponent(out InteractionObject obj))) {
                HideInteractionMenu();
                return; // no raycast hit or hit object has no InteractionObject component
            }
            if (selectedObject == obj && menu.IsDisplayed) {
                return; // clicked on the same object whose menu is already displayed, do nothing
            }

            ButtonClickAudioInjector.Instance?.PlayOnce();
            SetSelectedObject(obj);
            CreateMenu();
        }

        /// <summary>
        /// Sets the currently selected <see cref="InteractionObject"/> and registers the property callback for all
        /// menu elements.
        /// </summary>
        /// <param name="obj">New selected object</param>
        private void SetSelectedObject(InteractionObject obj) {
            if (selectedObject == obj) {
                return; // already selected, do nothing
            }

            selectedObject = obj;
            isMenuSizeChanging = true;
            foreach (IProperty property in selectedObject.MenuElements.SelectMany(e => e.Properties)) {
                property.OnValueChanged += OnPropertyValueChanged; // register callback
            }
            Debug.Log("Selected interactable object: " + selectedObject.name);
        }

        /// <summary>
        /// Creates the interaction menu based on <see cref="selectedObject"/>.
        /// </summary>
        private void CreateMenu() {
            menu.Create(selectedObject.name, selectedObject.MenuElements, isMenuSizeChanging);
            selectedObject.OpenMenuInteraction();
            ButtonClickAudioInjector.Instance?.InjectAudio(uiDocument);
            isMenuSizeChanging = false; // reset flag after creating the menu
        }

        /// <summary>
        /// Updates the position of the currently displayed interaction menu based on the world position of
        /// <see cref="selectedObject"/>.
        /// </summary>
        private void UpdateMenuPosition() {
            Vector2 objectPosition2D = camera.WorldToViewportPoint(selectedObject.transform.position);
            objectPosition2D.x *= Screen.width;
            objectPosition2D.y *= Screen.height;
            objectPosition2D += menuPositionOffset;
            menu.SetPosition(objectPosition2D.x, objectPosition2D.y, true);
        }

        /// <summary>
        /// Deletes the currently displayed interaction menu and resets the selected object.
        /// </summary>
        /// <param name="resetSelectedObject">If <c>true</c>, the selected object is reset to <c>null</c></param>
        private void HideInteractionMenu(bool resetSelectedObject = true) {
            menu.Delete();
            if (resetSelectedObject) {
                ResetSelectedObject();
            }
        }

        /// <summary>
        /// Resets the currently selected <see cref="InteractionObject"/> and unregisters the property callback for all
        /// menu elements.
        /// </summary>
        private void ResetSelectedObject() {
            if (!selectedObject) {
                return; // nothing to reset
            }

            foreach (IProperty property in selectedObject.MenuElements.SelectMany(e => e.Properties)) {
                property.OnValueChanged -= OnPropertyValueChanged; // unregister callback
            }
            selectedObject = null;
        }

        /// <summary>
        /// Callback for property value changes of <see cref="selectedObject"/>, see
        /// <see cref="IProperty.OnValueChanged"/>.
        /// </summary>
        private void OnPropertyValueChanged(IProperty property) {
            isRecreationScheduled = true;
            // set to true if the IsActive property of any menu element is changed
            isMenuSizeChanging = selectedObject.MenuElements.Any(e => e.IsActive == property);
        }

        /// <summary>
        /// Recreates the interaction menu with the same <see cref="selectedObject"/>.
        /// </summary>
        private void RecreateMenu() {
            if (!menu.IsDisplayed) {
                return; // menu is not displayed, no need to update it
            }

            // recreate menu to update the displayed values
            HideInteractionMenu(false);
            CreateMenu();
        }

        /// <summary>
        /// Hides the interaction menu and disables interactions when the main menu is shown.
        ///
        /// Also used for <see cref="DetailCameraMode.onDetailViewToggle"/> with the same purpose.
        /// </summary>
        public void OnMainMenuToggle(bool isShown) {
            if (isShown) {
                IsCheckingForInteractions = false;
                HideInteractionMenu();
            } else {
                IsCheckingForInteractions = true; // enable it again after entering main menu and clicking continue
            }
        }

        /// <summary>
        /// Hides the interaction menu and disables interactions when switching to aerial view, since the player cannot
        /// interact with objects in this camera mode.
        /// </summary>
        public void OnCameraModeChanged(CameraType cameraType) {
            if (cameraType == CameraType.AerialView) {
                IsCheckingForInteractions = false;
                HideInteractionMenu();
            } else {
                IsCheckingForInteractions = true;
            }
        }
    }
}