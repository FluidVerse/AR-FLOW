using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BCI {
    public class GateControlUI : MonoBehaviour, IPointerClickHandler
    {
        [Header("Referenzen")]
        public GateState gateState;     // GateState am selben Objekt
        public Image gateImage;         // UI-Image dieses Tors

        [Header("Sprites für Zustände")]
        public Sprite closedSprite;     // längliches Rechteck (geschlossen)
        public Sprite openSprite;       // Pfeil (Basisrichtung, z.B. rein)
        public Sprite inputSprite;      // Pfeil (kann derselbe Sprite sein)

        [Header("Richtung des Tores")]
        public Direction gateDirection;

        // 0 = geschlossen, 1 = offen, 2 = Input (wie vorher)
        private int state = 0;

        // Farben wie im alten Script
        private Color orange = new Color(1f, 0.5f, 0f);

        // Grundrotation für die Pfeilrichtung
        private float baseRotation = 0f;

        public enum Direction
        {
            Right,  // Pfeil zeigt nach rechts
            Left,   // Pfeil zeigt nach links
            Up,     // Pfeil zeigt nach oben
            Down    // Pfeil zeigt nach unten
        }

        private void Awake()
        {
            if (gateState == null)
                gateState = GetComponent<GateState>();
            if (gateImage == null)
                gateImage = GetComponent<Image>();

            // Startzustand aus GateState übernehmen
            if (gateState != null)
                state = gateState.GetState();

            SetBaseRotation();
            UpdateAppearance();
        }

        // UI-Klick
        public void OnPointerClick(PointerEventData eventData)
        {
            // Zustand umschalten: 0 -> 1 -> 2 -> 0
            state = (state + 1) % 3;

            // GateState aktualisieren
            if (gateState != null)
                gateState.SetState(state);

            UpdateAppearance();
        }

        private void UpdateAppearance()
        {
            if (gateImage == null) return;

            // Sprite je nach Zustand
            switch (state)
            {
                case 0:
                    gateImage.sprite = closedSprite;
                    break;
                case 1:
                    gateImage.sprite = openSprite;
                    break;
                case 2:
                    gateImage.sprite = inputSprite;
                    break;
            }

            // Rotation: wie im alten Script
            float rotZ = (state == 2) ? baseRotation + 180f : baseRotation;
            gateImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, rotZ);

            // Farbe: state 2 = grün, state 1 = orange, sonst weiß
            if (state == 2)
                gateImage.color = Color.green;
            else if (state == 1)
                gateImage.color = orange;
            else
                gateImage.color = Color.white;
        }

        private void SetBaseRotation()
        {
            switch (gateDirection)
            {
                case Direction.Right:
                    baseRotation = 0f;
                    break;
                case Direction.Left:
                    baseRotation = 180f;
                    break;
                case Direction.Up:
                    baseRotation = 90f;
                    break;
                case Direction.Down:
                    baseRotation = -90f;
                    break;
            }

            gateImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, baseRotation);
        }
    }
}
