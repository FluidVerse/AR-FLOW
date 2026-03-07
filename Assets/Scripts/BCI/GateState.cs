using UnityEngine;

namespace BCI {
    public class GateState : MonoBehaviour
    {
        [Header("Gate State (0 = zu, 1 = halb, 2 = offen)")]
        [Range(0, 2)]
        [SerializeField] private int state = 0;

        [Tooltip("Optionaler Name für Debug-Ausgaben (z.B. 'oben', 'rechts' usw.)")]
        public string gateName = "";

        // Wird vom DisplayManager und GateControlUI benutzt
        public int GetState()
        {
            return Mathf.Clamp(state, 0, 2);
        }

        public void SetState(int newState)
        {
            int clamped = Mathf.Clamp(newState, 0, 2);
            if (clamped == state) return;

            state = clamped;

            if (!string.IsNullOrEmpty(gateName))
            {
                Debug.Log($"{gateName} neuer Zustand: {state}");
            }
        }
    }
}
