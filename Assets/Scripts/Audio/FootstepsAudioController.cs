using Controls;
using UnityEngine;

namespace Audio {
    /// <summary>
    /// Controls the playback of footsteps audio based on the player's movement speed.
    /// </summary>
    public class FootstepsAudioController : MonoBehaviour {

        /// <summary>
        /// Player object whose position is tracked to determine movement speed.
        /// </summary>
        [SerializeField] private GameObject player;

        /// <summary>
        /// Audio player for the footsteps audio.
        /// </summary>
        [SerializeField] private ContinuousAudioPlayer audioPlayer;

        /// <summary>
        /// Minimum speed threshold for playing footsteps audio.
        /// </summary>
        [SerializeField] private float minSpeed = 0.001f;

        /// <summary>
        /// Maximum speed threshold for adjusting the pitch of the footsteps audio.
        /// </summary>
        [SerializeField] private float maxSpeed = 1.8f;

        /// <summary>
        /// Minimum pitch for the footsteps audio when the player speed equals <see cref="minSpeed"/>.
        /// </summary>
        [SerializeField] private float minPitch = 0.5f;

        /// <summary>
        /// Maximum pitch for the footsteps audio when player speed >= <see cref="maxSpeed"/>.
        /// </summary>
        [SerializeField] private float maxPitch = 1.2f;

        private Vector3 oldPosition;
        private float oldSpeed;
        private bool isSoundEnabled;

        private void Start() {
            oldPosition = player.transform.position;
        }

        private void Update() {
            if (HasSpeedChanged(out float newSpeed)) {
                AdjustAudio(newSpeed);
            }
        }

        /// <summary>
        /// Callback for <see cref="SplineCameraMode.onJoystickPressed"/>.
        /// </summary>
        /// <param name="isPressed">Whether the joystick is pressed down</param>
        public void OnJoystickPressed(bool isPressed) {
            isSoundEnabled = isPressed;
            if (!isSoundEnabled && audioPlayer.IsPlaying) {
                audioPlayer.Stop();
            }
        }

        /// <summary>
        /// Returns <c>true</c> if the player speed has changed since the last check.
        /// </summary>
        /// <param name="newSpeed">New movement speed of the player</param>
        private bool HasSpeedChanged(out float newSpeed) {
            Vector3 newPosition = player.transform.position;
            newSpeed = Vector3.Distance(oldPosition, newPosition) / Time.deltaTime;
            bool hasChanged = !Mathf.Approximately(oldSpeed, newSpeed);
            oldPosition = newPosition;
            oldSpeed = newSpeed;
            return hasChanged;
        }

        /// <summary>
        /// Adjusts the audio playback based on the player's speed.
        /// </summary>
        /// <param name="speed">Current movement speed of the player</param>
        private void AdjustAudio(float speed) {
            if (speed >= minSpeed && audioPlayer.IsStopping && isSoundEnabled) {
                audioPlayer.Play();
            } else if (!isSoundEnabled || (speed < minSpeed && audioPlayer.IsPlaying)) {
                audioPlayer.Stop();
            }

            float speed01 = Mathf.Clamp01(Mathf.InverseLerp(minSpeed, maxSpeed, speed));
            audioPlayer.Pitch = Mathf.Lerp(minPitch, maxPitch, speed01);
        }
    }
}