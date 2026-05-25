using System;
using System.Collections;
using UnityEngine;

namespace Audio {
    /// <summary>
    /// Utility class for audio operations on <see cref="AudioSource"/> components etc., such as fading volume.
    /// </summary>
    public static class AudioUtils {

        /// <summary>
        /// Coroutine that fades the volume of an <see cref="AudioSource"/> from a start volume to an end volume over a
        /// specified duration.
        ///
        /// If the current audio source volume is somewhere in between the start and end volume, it will still be faded
        /// as if it started at the start volume (see implementation below).
        /// </summary>
        /// <param name="audioSource">Audio source</param>
        /// <param name="startVolume">Start volume</param>
        /// <param name="endVolume">End volume</param>
        /// <param name="duration">Fade duration</param>
        /// <param name="onComplete">Callback when the coroutine is completed. Can be <c>null</c></param>
        public static IEnumerator FadeVolume(AudioSource audioSource, float startVolume, float endVolume,
            float duration, Action onComplete = null) {
            float direction = Mathf.Sign(endVolume - startVolume);
            float elapsedTime = 0;

            while (elapsedTime < duration) {
                elapsedTime += Time.deltaTime;
                float currentVolume = audioSource.volume;
                float newVolume = Mathf.Lerp(startVolume, endVolume, elapsedTime / duration);

                if ((direction > 0 && newVolume > currentVolume) || (direction < 0 && newVolume < currentVolume)) {
                    audioSource.volume = newVolume;
                }
                yield return null;
            }

            audioSource.volume = endVolume;
            onComplete?.Invoke();
        }
    }
}