using UnityEngine;

namespace BCI {
    public class GateUILayoutUI : MonoBehaviour
    {
        [Header("Referenzen")]
        public RectTransform crossPanel;     // dein bestehendes Cross-Rect
        public RectTransform gateTopUI;
        public RectTransform gateRightUI;
        public RectTransform gateBottomUI;
        public RectTransform gateLeftUI;

        [Header("Abstand-Einstellungen")]
        [Tooltip("Faktor auf die halbe Breite des Cross")]
        public float radiusFactor = 1.0f;    // 1.0 = genau halbe Breite
        [Tooltip("Zusätzlicher Abstand in Pixeln")]
        public float extraDistance = 0f;

        private Vector2 _lastSize = new Vector2(float.NaN, float.NaN);

        private void LateUpdate()
        {
            if (crossPanel == null) return;

            Rect r = crossPanel.rect;
            Vector2 size = r.size;

            // nur neu rechnen wenn sich die Größe geändert hat
            if (size == _lastSize) return;
            _lastSize = size;

            float half = Mathf.Min(r.width, r.height) * 0.5f;
            float radius = half * radiusFactor + extraDistance;

            ApplyGatePosition(gateTopUI,    new Vector2(0f,  radius));
            ApplyGatePosition(gateBottomUI, new Vector2(0f, -radius));
            ApplyGatePosition(gateRightUI,  new Vector2( radius, 0f));
            ApplyGatePosition(gateLeftUI,   new Vector2(-radius, 0f));
        }

        private void ApplyGatePosition(RectTransform gate, Vector2 localPos)
        {
            if (gate == null) return;

            // wir arbeiten im lokalen Raum des crossPanel
            gate.SetParent(crossPanel, worldPositionStays: false);
            gate.anchorMin = gate.anchorMax = new Vector2(0.5f, 0.5f);
            gate.pivot = new Vector2(0.5f, 0.5f);
            gate.anchoredPosition = localPos;
        }
    }
}
