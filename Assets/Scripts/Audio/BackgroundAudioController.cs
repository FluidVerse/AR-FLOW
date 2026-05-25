using MainMenu;
using UnityEngine;

namespace Audio {
    /// <summary>
    /// Controls the playback of background audio in a level and pauses it when certain menus are opened.
    /// </summary>
    public class BackgroundAudioController : MonoBehaviour {

        /// <summary>
        /// Audio player for the background audio.
        /// </summary>
        [SerializeField] private ContinuousAudioPlayer audioPlayer;

        private void Start() {
            MainMenuManager.Instance.OnMainMenuToggle.AddListener(OnSubMenuToggle);
            if (!MainMenuManager.Instance.isActive && audioPlayer.isActiveAndEnabled) {
                audioPlayer.Play();
            }
        }

        private void OnDisable() {
            MainMenuManager.Instance.OnMainMenuToggle.RemoveListener(OnSubMenuToggle);
        }

        /// <summary>
        /// Callback for when a sub-menu is opened/closed.
        /// </summary>
        public void OnSubMenuToggle(bool isShown) {
            if (!audioPlayer.isActiveAndEnabled) {
                return;
            }
            
            if (isShown) {
                audioPlayer.Stop(true);
            } else {
                audioPlayer.Play();
            }
        }
    }
}