using UnityEngine;
using UnityEngine.EventSystems;

namespace Audio {
    /// <summary>
    /// Little helper class that plays an AudioSource when the user clicks on a UI element.
    ///
    /// Can be used when a UI element does not provide an easy way to attach <c>onClick</c>-style callbacks.
    /// </summary>
    public class OnPointerDownAudio : MonoBehaviour, IPointerDownHandler {

        [SerializeField] private AudioSource audioSource;

        public void OnPointerDown(PointerEventData eventData) {
            audioSource.Play();
        }
    }
}