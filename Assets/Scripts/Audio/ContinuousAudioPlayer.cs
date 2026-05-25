using UnityEngine;
using static Audio.AudioUtils;

namespace Audio {
    /// <summary>
    /// Helper class to play an audio clip continuously with fade-in and fade-out effects.
    ///
    /// It expects an <see cref="AudioSource"/> component to be attached to the same GameObject.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class ContinuousAudioPlayer : MonoBehaviour {

        /// <summary>
        /// Duration of the fade-in and fade-out effects in seconds.
        /// </summary>
        [SerializeField] private float fadeDuration = 1f;

        /// <summary>
        /// Maximum volume of the audio source to which the sound should fade in.
        /// </summary>
        [SerializeField] private float maxVolume = 1f;

        /// <summary>
        /// Initial volume of the audio source when this script is created.
        /// </summary>
        [SerializeField] private float initialVolume;

        /// <summary>
        /// If <c>true</c>, the audio source volume is reset to <see cref="initialVolume"/> whenever <see cref="Play"/>
        /// is called.
        /// </summary>
        [SerializeField] private bool resetVolumeOnPlay = true;

        private AudioSource audioSource;
        private Coroutine fadeCoroutine;

        private State state = State.Stopped;
        private float lastPlaybackTime;

        /// <summary>
        /// Pitch of the audio source.
        /// </summary>
        public float Pitch {
            get => audioSource.pitch;
            set => audioSource.pitch = value;
        }

        /// <summary>
        /// Whether the audio clip is currently playing or fading in.
        /// </summary>
        public bool IsPlaying => state is State.Playing or State.FadingIn;

        /// <summary>
        /// Whether the audio clip is currently stopped or fading out.
        /// </summary>
        public bool IsStopping => state is State.Stopped or State.FadingOut;

        private void Awake() {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) {
                Debug.LogError("AudioSource not found");
            }

            audioSource.playOnAwake = false;
            audioSource.loop = true; // ensure that the audio source loops
            audioSource.volume = initialVolume;
        }

        private void OnEnable() {
            // if whole game object is disabled and then enabled again (e.g. because of detail view of another object),
            // restart audio if it was playing before
            if (IsPlaying && !audioSource.isPlaying) {
                Play();
            }
        }

        /// <summary>
        /// Starts playing the audio clip with a fade-in effect.
        /// </summary>
        public void Play() {
            state = State.FadingIn;
            if (fadeCoroutine != null) {
                StopCoroutine(fadeCoroutine);
            }
            if (resetVolumeOnPlay) {
                audioSource.volume = initialVolume;
            }
            fadeCoroutine = StartCoroutine(FadeVolume(audioSource, 0, maxVolume, fadeDuration, () => {
                if (state == State.FadingIn) {
                    state = State.Playing;
                }
            }));
            audioSource.Play();
            audioSource.time = lastPlaybackTime;
        }

        /// <summary>
        /// Toggles the audio clip between playing and stopped.
        /// </summary>
        public void Toggle() {
            if (IsPlaying) {
                Stop();
            } else {
                Play();
            }
        }

        /// <summary>
        /// Resets the audio clip to the beginning and starts playing it again.
        ///
        /// No fade effects are applied here. Does nothing if the audio is currently fading out or stopped.
        /// </summary>
        public void Reset() {
            if (IsStopping) {
                return;
            }
            audioSource.Stop();
            audioSource.Play();
        }

        /// <summary>
        /// Stops the audio clip with a fade-out effect.
        /// </summary>
        /// <param name="rememberPlaybackTime">
        /// Remembers the playback time of the audio clip and resumes playback from that time at the next
        /// <see cref="Play"/> call.
        /// </param>
        public void Stop(bool rememberPlaybackTime = false) {
            state = State.FadingOut;
            lastPlaybackTime = rememberPlaybackTime ? audioSource.time : 0f;
            if (fadeCoroutine != null) {
                StopCoroutine(fadeCoroutine);
            }
            fadeCoroutine = StartCoroutine(FadeVolume(audioSource, maxVolume, 0, fadeDuration, () => {
                if (state == State.FadingOut) {
                    state = State.Stopped;
                    audioSource.Stop();
                }
            }));
        }

        private enum State {
            Stopped,
            FadingIn,
            Playing,
            FadingOut
        }
    }
}