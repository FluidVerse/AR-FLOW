using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Utils {
    /// <summary>
    /// Helper class to manage the new Unity input system.
    /// </summary>
    public static class InputHelper {

        private static bool IsInputEnabled =>
            InputSystem.devices.Select(device => device.enabled).Any() && InputSystem.actions.enabled;

        /// <summary>
        /// Enables input for all devices and actions if they are not already enabled.
        ///
        /// This method is a fix for the bug that the input system disables itself after leaving the AR scene and entering
        /// another level. It needs to be called in every non-AR level.
        /// </summary>
        public static void TryEnableInput() {
            if (!IsInputEnabled) {
                EnableInput();
            }
        }
    
        // TODO test and fix broken AR scene when 1. entering AR scene, 2. leaving AR scene, 3. entering AR scene again?

        private static void EnableInput() {
            foreach (InputDevice device in InputSystem.devices) {
                InputSystem.EnableDevice(device);
            }
            InputSystem.actions.Enable();
            Debug.Log("Input enabled for all devices and actions!");
        }
    }
}