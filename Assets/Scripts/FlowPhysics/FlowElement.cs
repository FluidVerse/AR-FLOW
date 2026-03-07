using UnityEngine;

namespace FlowPhysics {
    public interface FlowElement {
        
        Vector2 GetPosition();

        void SetPosition(Vector2 position);

        // Velocity field
        Vector2 Velocity(Vector2 x);

        // Potential Phi
        float Phi(Vector2 x);

        // Strem Function Psi
        float Psi(Vector2 x);

        float ValidMask(Vector2 x);

    }

    public class UniformFlow : FlowElement {
        
        public Vector2 c;

        float tol = 1e-9f;

        public UniformFlow(Vector2 c) {
            this.c = c;
        }

        public Vector2 GetPosition() {
            return new Vector2(0f, 0f);
        }

        public void SetPosition(Vector2 position) {
        }

        public Vector2 Velocity(Vector2 x) {
            return new Vector2(c.x, c.y);
        }

        public float Phi(Vector2 x) {
            return c.x * x.x + c.y * x.y;
        }

        public float Psi(Vector2 x) {
            return c.x * x.y - c.y * x.x;
        }

        public float ValidMask(Vector2 x) {
            return 1f;
        }
    }

    public class SourceSink : FlowElement {
        public float strength; // Q > 0 Quelle, Q < 0 Senke
        public Vector2 position;

        float tol = 1e-9f;

        public SourceSink(float strength, Vector2 position) {
            this.strength = strength;
            this.position = position;
        }

        public Vector2 GetPosition() {
            return position;
        }

        public void SetPosition(Vector2 position) {
            this.position = position;
        }

        public Vector2 Velocity(Vector2 x) {
            Vector2 r = x - position;
            float x2py2 = r.x * r.x + r.y * r.y;
            if (x2py2 < tol) x2py2 = tol;

            float cx = strength / (2 * Mathf.PI) * r.x / x2py2;
            float cy = strength / (2 * Mathf.PI) * r.y / x2py2;

            return new Vector2(cx, cy);
        }

        public float Phi(Vector2 x) {
            Vector2 r = x - position;
            float rr = r.magnitude;
            if (rr < tol) rr = tol;
            return (strength / (2f * Mathf.PI)) * Mathf.Log(rr);
        }

        public float Psi(Vector2 x) {
            Vector2 r = x - position;
            return (strength / (2f * Mathf.PI)) * Mathf.Atan2(r.y, r.x);
        }

        public float ValidMask(Vector2 x) {
            return 1f;
        }
    }

    public class Dipole : FlowElement {
        public float strength; // M
        public Vector2 position;

        float tol = 1e-9f;

        public Dipole(float strength, Vector2 position) {
            this.strength = strength;
            this.position = position;
        }

        public Vector2 GetPosition() {
            return position;
        }

        public void SetPosition(Vector2 position) {
            this.position = position;
        }

        public Vector2 Velocity(Vector2 x) {
            Vector2 r = x - position;
            float x2py2 = r.x * r.x + r.y * r.y;
            if (x2py2 < tol) x2py2 = tol;

            float cx = strength * (r.y * r.y - r.x * r.x) / (x2py2 * x2py2);
            float cy = -strength * 2 * r.x * r.y / (x2py2 * x2py2);

            return new Vector2(cx, cy);
        }

        public float Phi(Vector2 x) {
            Vector2 r = x - position;
            float x2py2 = r.x * r.x + r.y * r.y;
            if (x2py2 < 1e-3f) x2py2 = 1e-3f;
            return strength * r.x / x2py2;
        }

        public float Psi(Vector2 x) {
            Vector2 r = x - position;
            float x2py2 = r.x * r.x + r.y * r.y;
            if (x2py2 < 1e-3f) x2py2 = 1e-3f;
            return -strength * r.y / x2py2;
        }

        public float ValidMask(Vector2 x) {
            return 1f;
        }
    }

    public class Cylinder : FlowElement {
        public float radius;
        public Vector2 center;

        public Cylinder(float radius, Vector2 center) {
            this.radius = radius;
            this.center = center;
        }

        public Vector2 GetPosition() {
            return center;
        }

        public void SetPosition(Vector2 position) {
            this.center = position;
        }

        public Vector2 Velocity(Vector2 x) {
            // relative Koordinaten
            float X = x.x - center.x;
            float Y = x.y - center.y;

            float r2 = X * X + Y * Y;
            float r = Mathf.Sqrt(r2);

            // Innerhalb des Kreises → keine Strömung
            if (r < radius)
                return Vector2.zero;

            // Dipol-Stärke K = R² (U∞ wird später vom UniformFlow geliefert!)
            float K = radius * radius;

            // Dipol-Geschwindigkeitsfeld
            float factor = K / (r2 * r2); // = R² / r⁴

            float Vx = factor * (2f * X * X - r2);
            float Vy = factor * (2f * X * Y);

            return new Vector2(Vx, Vy);
        }

        public float Phi(Vector2 x) {
            // relative Koordinaten zum Zentrum
            float X = x.x - center.x;
            float Y = x.y - center.y;

            float r2 = X * X + Y * Y;
            float r = Mathf.Sqrt(r2);

            if (r < radius)
                return 0f;

            float theta = Mathf.Atan2(Y, X);

            // Dipolstärke K = R² (U∞ wird durch UniformFlow geliefert)
            float K = radius * radius;

            return K * Mathf.Cos(theta) / r;
        }

        public float Psi(Vector2 x) {
            // relative Koordinaten zum Zentrum
            float X = x.x - center.x;
            float Y = x.y - center.y;

            float r2 = X * X + Y * Y;
            float r = Mathf.Sqrt(r2);

            if (r < radius)
                return 0f;

            float theta = Mathf.Atan2(Y, X);

            // Dipolstärke K = R²
            float K = radius * radius;

            return K * Mathf.Sin(theta) / r;
        }

        public float ValidMask(Vector2 x) {
            float X = x.x - center.x;
            float Y = x.y - center.y;

            float r2 = X * X + Y * Y;
            float r = Mathf.Sqrt(r2);

            if (r < radius) {
                return 0f;
            } else {
                return 1f;
            }
        }
    }

    public class Vortex : FlowElement {
// WARNING: DUMMY CLASS COPIED FROM Cylinder!
        public float strength;
        public Vector2 position;

        float tol = 1e-9f;

        public Vortex(float strength, Vector2 position) {
            this.strength = strength;
            this.position = position;
        }

        public Vector2 GetPosition() {
            return position;
        }

        public void SetPosition(Vector2 position) {
            this.position = position;
        }

        public Vector2 Velocity(Vector2 x) {
            Vector2 r = x - position;
            float x2py2 = r.x * r.x + r.y * r.y;
            if (x2py2 < tol) x2py2 = tol;

            float cx = strength / (2 * Mathf.PI) * r.y / x2py2;
            float cy = -strength / (2 * Mathf.PI) * r.x / x2py2;

            return new Vector2(cx, cy);
        }

        public float Phi(Vector2 x) {
            Vector2 r = x - position;
            return -(strength / (2f * Mathf.PI)) * Mathf.Atan2(r.y, r.x);
        }

        public float Psi(Vector2 x) {
            Vector2 r = x - position;
            float rr = r.magnitude;
            if (rr < tol) rr = tol;
            return (strength / (2f * Mathf.PI)) * Mathf.Log(rr);
        }


        public float ValidMask(Vector2 x) {
            return 1f;
        }
    }
}