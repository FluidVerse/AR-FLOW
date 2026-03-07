using UnityEngine;

namespace BCI {
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class CrossCornerManualFitter : MonoBehaviour
    {
        [Header("References")]
        public RectTransform cross;      // dein Cross-Container (quadratisch, parent der Ecken)
        public RectTransform cornerUL;   // oben links
        public RectTransform cornerUR;   // oben rechts
        public RectTransform cornerLL;   // unten links
        public RectTransform cornerLR;   // unten rechts

        [Header("Größe der Ecken (relativ zur Cross-Kantenlänge)")]
        [Range(0f, 1f)] public float widthRatio  = 0.30f;  // z.B. 0.30 = 30% der Cross-Breite
        [Range(0f, 1f)] public float heightRatio = 0.30f;  // z.B. 0.30 = 30% der Cross-Höhe

        [Header("Abstand vom Rand (Pixel, identisch für alle vier Ecken)")]
        public Vector2 margin = new Vector2(0f, 0f);       // +x: weiter von links/rechts weg, +y: weiter von oben/unten weg

        [Header("Fein-Offsets pro Ecke (Pixel)")]
        public Vector2 offsetUL = Vector2.zero;
        public Vector2 offsetUR = Vector2.zero;
        public Vector2 offsetLL = Vector2.zero;
        public Vector2 offsetLR = Vector2.zero;

        void OnEnable() { Apply(); }
#if UNITY_EDITOR
        void Update() { if (!Application.isPlaying) Apply(); }
#endif
        void OnRectTransformDimensionsChange() { Apply(); }

        public void Apply()
        {
            if (!cross) cross = transform as RectTransform;
            if (!cross) return;

            float S = Mathf.Min(cross.rect.width, cross.rect.height);
            float w = Mathf.Max(0f, widthRatio  * S);
            float h = Mathf.Max(0f, heightRatio * S);

            // Hilfsfunktion: richtet eine Ecke ein (Anchor/Pivot an Ecke, Größe und Position)
            void FitCorner(RectTransform rt, Vector2 anchor, Vector2 baseOffset, Vector2 extra)
            {
                if (!rt) return;

                rt.anchorMin = rt.anchorMax = anchor;   // an die Ecke kleben
                rt.pivot     = anchor;                  // Pivot ebenfalls an Ecke
                rt.localScale = Vector3.one;
                rt.localRotation = Quaternion.identity;

                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,   h);

                // baseOffset: Margin richtungsrichtig anwenden (oben -> -Y, rechts -> -X)
                // extra: zusätzlicher Feinoffset
                rt.anchoredPosition = baseOffset + extra;
            }

            // Basis-Margins richtungsrichtig aufbereiten:
            // UL: +X (von links), -Y (von oben)
            Vector2 mUL = new Vector2(+margin.x, -margin.y);
            // UR: -X (von rechts), -Y (von oben)
            Vector2 mUR = new Vector2(-margin.x, -margin.y);
            // LL: +X (von links), +Y (von unten)
            Vector2 mLL = new Vector2(+margin.x, +margin.y);
            // LR: -X (von rechts), +Y (von unten)
            Vector2 mLR = new Vector2(-margin.x, +margin.y);

            // Anker der vier Ecken: UL(0,1), UR(1,1), LL(0,0), LR(1,0)
            FitCorner(cornerUL, new Vector2(0f, 1f), mUL, offsetUL);
            FitCorner(cornerUR, new Vector2(1f, 1f), mUR, offsetUR);
            FitCorner(cornerLL, new Vector2(0f, 0f), mLL, offsetLL);
            FitCorner(cornerLR, new Vector2(1f, 0f), mLR, offsetLR);
        }
    }
}
