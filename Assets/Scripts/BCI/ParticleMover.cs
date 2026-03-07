using UnityEngine;

namespace BCI {
    public class ParticleMover : MonoBehaviour
    {
        private Rigidbody2D rb;
        private ParticleSystemForceField forceField;

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0; // Damit das Partikel nur durch das Force Field bewegt wird
            }

            // Nächstgelegenes Force Field finden
            forceField = FindNearestForceField();
        }

        void FixedUpdate()
        {
            if (forceField != null)
            {
                Vector3 force = GetForceAtPosition(transform.position);
                rb.AddForce(force);
            }
        }

        Vector3 GetForceAtPosition(Vector3 position)
        {
            if (forceField == null) return Vector3.zero;

            // Berechne den Vektor von der Partikelposition zum Zentrum des Force Fields
            Vector3 direction = (forceField.transform.position - position).normalized;

            // Stärke des Kraftfelds anpassen
            float forceStrength = forceField.vectorFieldAttraction.constant;

            return direction * forceStrength;
        }

        private ParticleSystemForceField FindNearestForceField()
        {
            ParticleSystemForceField[] allFields = FindObjectsOfType<ParticleSystemForceField>();
            ParticleSystemForceField nearest = null;
            float minDistance = float.MaxValue;

            foreach (var field in allFields)
            {
                float distance = Vector3.Distance(transform.position, field.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = field;
                }
            }

            if (nearest != null)
            {
                Debug.Log($"Nächstgelegenes Force Field gefunden: {nearest.name} an {nearest.transform.position}");
            }
            return nearest;
        }
    }
}
