using System.Collections.Generic;
using UnityEngine;

namespace FlowPhysics {
    public class PotentialFlow {

        public List<FlowElement> elements = new();

        public int Count() {
            return elements.Count;
        }

        public void AddElement(FlowElement element) {
            elements.Add(element);
            Debug.Log("PotentialFlow: Add Element (total = " + elements.Count + ")");
        }

        public void RemoveElement(int index) {
            elements.RemoveAt(index);
            Debug.Log("PotentialFlow: Remove Element (remaining = " + elements.Count + ")");
        }

        public void RemoveLastElement() {
            if (elements.Count > 0) {
                elements.RemoveAt(elements.Count - 1);
                Debug.Log("PotentialFlow: Removed last element (remaining = " + elements.Count + ")");
            } else {
                Debug.LogWarning("PotentialFlow: Tried to remove element, but list is empty!");
            }
        }

        public Vector2 C(Vector2 x) {
            Vector2 v = Vector2.zero;
            foreach (var e in elements)
                v += e.Velocity(x);
            return v;
        }

        public float Uinf() {
            Vector2 U = Vector2.zero;
            foreach (var e in elements) {
                if (e is UniformFlow flow) {
                    U += flow.c;
                }
            }
            return U.magnitude;
        }

        public float Cabs(Vector2 x) {
            return C(x).magnitude;
        }

        public float Phi(Vector2 x) {
            float phi = 0f;
            foreach (var e in elements)
                phi += e.Phi(x);
            return phi;
        }

        public float Psi(Vector2 x) {
            float psi = 0f;
            foreach (var e in elements)
                psi += e.Psi(x);
            return psi;
        }

        // Bernoulli: p - p_inf = 0.5*rho*(U_inf^2 - |v|^2), wenn du magst
        public float Pressure(Vector2 x) {
            float rho = 1000f;
            float vmag = Cabs(x);
            float Ui = Uinf();
            float pInf = 100000f;
            return pInf + 0.5f * rho * (Ui * Ui - vmag * vmag);
        }

        public float ValidMask(Vector2 x) {
            float val = 1f;
            foreach (var e in elements) {
                if (e.ValidMask(x) < 0.9f) {
                    val = 0f;
                }
            }

            return val;
        }
    }
}