using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace DeepLink {
    /// <summary>
    /// Handles the action when the app is opened via a deep link.
    ///
    /// As defined in the android manifest (Plugins/Android/AndroidManifest.xml), the deep link must have the following
    /// structure: "app://tu-dortmund-ar-flow".
    ///
    /// Additional parameters can be appended with a question mark and separated with an ampersand. For example, the
    /// following deep link would pass the parameter "ar=test1": "app://tu-dortmund-ar-flow?ar=test1".
    /// </summary>
    public class DeepLinkManager : MonoBehaviour {

        /// <summary>
        /// Callback when the app is opened via a deep link.
        ///
        /// The parameter is a dictionary of all parsed url arguments. For example, the deep link
        /// "app://tu-dortmund-ar-flow?ar=test1" leads to a dictionary with one entry: dict["ar"] = "test1".
        /// </summary>
        [SerializeField] private UnityEvent<Dictionary<string, string>> onDeepLinkReceived;

        private static DeepLinkManager Instance { get; set; }
        private const string baseUrl = "app://tu-dortmund-ar-flow";

        private void Awake() {
            if (Instance == null) {
                Instance = this;
                // DontDestroyOnLoad handled by GlobalSingleton class in parent object
            } else {
                Destroy(gameObject);
            }
        }

        private void Start() {
            Application.deepLinkActivated += OnDeepLinkActivated;
            if (!string.IsNullOrEmpty(Application.absoluteURL)) {
                // absoluteURL is not empty after a cold start, so the app was launched via a deep link
                OnDeepLinkActivated(Application.absoluteURL);
            }
        }

        private void OnDeepLinkActivated(string url) {
            Debug.Log("Deep link received: " + url);
            // Parse the link and extract the parameters
            string suffix = url.Replace(baseUrl, "");
            if (!suffix.StartsWith('?')) {
                return; // no parameters, nothing to parse
            }

            // Build a dictionary out of the url parameters
            string parameterString = suffix.Remove(0, 1); // remove ? after baseUrl
            Dictionary<string, string> args = parameterString
                .Split('&')
                .Select(pair => pair.Split('='))
                .Where(keyValue => keyValue.Length == 2)
                .ToDictionary(keyValue => keyValue[0], keyValue => keyValue[1]);

            Debug.Log("Deep link parsed with the following args: " + string.Join(", ", args));
            StartCoroutine(InvokeDeepLinkCoroutine(args));
        }

        private IEnumerator InvokeDeepLinkCoroutine(Dictionary<string, string> args) {
            yield return null; // wait one frame to ensure all systems are initialized
            onDeepLinkReceived.Invoke(args);
        }
    }
}