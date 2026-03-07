using UnityEngine;

namespace BCI {
    public class GateControl : MonoBehaviour
    {
        public GameObject gate; // Verknüpftes Tor
        private SpriteRenderer spriteRenderer;
        private int state = 0; // 0 = geschlossen, 1 = offen, 2 = Input

        // Verschiedene Sprites für die Zustände
        public Sprite closedSprite;
        public Sprite openSprite;
        public Sprite inputSprite;

        // Richtung des Pfeils (im Inspector einstellbar)
        public Direction gateDirection;

        // Farben für Zustände
        private Color orange = new Color(1f, 0.5f, 0f); // Orange definieren

        // Grundrotation für die Pfeilrichtung
        private float baseRotation = 0f;

        public enum Direction
        {
            Right,  // Pfeil zeigt nach rechts
            Left,   // Pfeil zeigt nach links
            Up,     // Pfeil zeigt nach oben
            Down    // Pfeil zeigt nach unten
        }

        void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            SetBaseRotation(); // Pfeil zu Beginn korrekt ausrichten
            UpdateAppearance();
        }

        void OnMouseDown()
        {
            // Zustand umschalten
            state = (state + 1) % 3; // 0 -> 1 -> 2 -> 0
            UpdateAppearance();

            // Verknüpftes Tor aktualisieren
            if (gate != null)
            {
                gate.GetComponent<GateState>().SetState(state);
            }
        }

        void UpdateAppearance()
        {
            switch (state)
            {
                case 0:
                    spriteRenderer.sprite = closedSprite;
                    break;
                case 1:
                    spriteRenderer.sprite = openSprite;
                    break;
                case 2:
                    spriteRenderer.sprite = inputSprite;
                    break;
            }

            // Drehe den Pfeil um 180°, wenn im Output-State (state == 2)
            transform.rotation = Quaternion.Euler(0, 0, (state == 2) ? baseRotation + 180f : baseRotation);

            // Setze Farbe basierend auf Zustand
            spriteRenderer.color = (state == 2) ? Color.green : (state == 1) ? orange : Color.white;
        }

        void SetBaseRotation()
        {
            switch (gateDirection)
            {
                case Direction.Right:
                    baseRotation = 0f; // Pfeil zeigt nach rechts
                    break;
                case Direction.Left:
                    baseRotation = 180f; // Pfeil zeigt nach links
                    break;
                case Direction.Up:
                    baseRotation = 90f; // Pfeil zeigt nach oben
                    break;
                case Direction.Down:
                    baseRotation = -90f; // Pfeil zeigt nach unten
                    break;
            }
            transform.rotation = Quaternion.Euler(0, 0, baseRotation);
        }
    }
}
