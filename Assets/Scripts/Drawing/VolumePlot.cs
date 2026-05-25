using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Drawing {
    public class VolumePlot : MonoBehaviour {

        public TextAsset geometryFile;
        public TextAsset pressureFile;
        public TextAsset cabsFile;
        public TextAsset wabsFile;

        public float pointSize = 0.1f;

        private ParticleSystem ps;
        private ParticleSystem.Particle[] particles;

        List<Vector3> points = new();
        private List<float> pressure = new();
        private List<float> cabs = new();
        private List<float> wabs = new();

        private int Npoints = 0;

        private void Awake() {
            // read Geometry
            string[] points_lines = geometryFile.text.Split('\n');
            foreach (string line in points_lines) {
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] parts = line.Split(','); // oder ',' je nach Format

                float px = float.Parse(parts[0], CultureInfo.InvariantCulture);
                float py = float.Parse(parts[1], CultureInfo.InvariantCulture);
                float pz = float.Parse(parts[2], CultureInfo.InvariantCulture);

                points.Add(new Vector3(px, py, pz));
            }
            Npoints = points.Count;

            // read p Field
            string[] pressure_lines = pressureFile.text.Split('\n');
            foreach (string line in pressure_lines) {
                if (string.IsNullOrWhiteSpace(line)) continue;

                float pval = float.Parse(line, CultureInfo.InvariantCulture);

                pressure.Add(pval);
            }

            // read cabs Field
            string[] cabs_lines = cabsFile.text.Split('\n');
            foreach (string line in cabs_lines) {
                if (string.IsNullOrWhiteSpace(line)) continue;

                float val = float.Parse(line, CultureInfo.InvariantCulture);

                cabs.Add(val);
            }

            // read wabs Field
            string[] wabs_lines = wabsFile.text.Split('\n');
            foreach (string line in wabs_lines) {
                if (string.IsNullOrWhiteSpace(line)) continue;

                float val = float.Parse(line, CultureInfo.InvariantCulture);

                wabs.Add(val);
            }


            ps = GetComponent<ParticleSystem>();


            //ps.SetParticles(particles, particles.Length);
            createParticles();
            //setCabsField();
            //setWabsField();
            setPField();
        }

        void createParticles() {
            particles = new ParticleSystem.Particle[Npoints];

            for (int i = 0; i < Npoints; i++) {
                Debug.Log(points[i]);
                var p = new ParticleSystem.Particle();


                p.position = points[i];
                p.startColor = MapToRGB(pressure[i], -100000, 700000);
                p.startSize = pointSize;
                p.remainingLifetime = float.MaxValue; // praktisch "unendlich"
                p.startLifetime = float.MaxValue;
                particles[i] = p;
            }

            ps.SetParticles(particles, particles.Length);
        }

        public void setPField() {
            int count = ps.GetParticles(particles);
            for (int i = 0; i < count; i++) {
                particles[i].startColor = MapToRGB(pressure[i], -100000, 700000);
            }
            ps.SetParticles(particles, count);
        }

        public void setCabsField() {
            int count = ps.GetParticles(particles);
            for (int i = 0; i < count; i++) {
                particles[i].startColor = MapToRGB(cabs[i], 0, 35f);
            }
            ps.SetParticles(particles, count);
        }

        public void setWabsField() {
            int count = ps.GetParticles(particles);
            for (int i = 0; i < count; i++) {
                particles[i].startColor = MapToRGB(wabs[i], 0, 35f);
            }
            ps.SetParticles(particles, count);
        }

        Color MapToRGB(float value, float minVal = 0f, float maxVal = 1f, float alpha = 0.1f) {
            float t = (value - minVal) / (maxVal - minVal);
            if (t < 0f) t = 0f;
            if (t > 1f) t = 1f;

            if (t < 0.25f) {
                return new Color(0f, 4 * t, 1f, alpha);
            } else if (t >= 0.25f && t < 0.5f) {
                return new Color(0f, 1, -1f / 0.25f * (t - 0.25f) + 1f, alpha);
            } else if (t >= 0.5f && t < 0.75f) {
                return new Color(1f / 0.25f * (t - 0.5f), 1f, 0f, alpha);
            } else {
                return new Color(1f, -1f / 0.25f * (t - 0.75f) + 1f, 0f, alpha);
            }
        }
    }
}