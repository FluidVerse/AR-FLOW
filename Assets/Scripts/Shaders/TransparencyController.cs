using System;
using UnityEngine;

namespace Shaders {
    /// <summary>
    /// Helper script that controls the transparency of an object.
    ///
    /// It can be used to toggle the transparency mode (opaque or transparent) of one or many assigned mesh renderers.
    /// In addition, the exact transparency value can be set.
    ///
    /// If multiple mesh renderers are assigned to one script instance, the transparency value is applied to all of
    /// them, and it is assumed that they all share the same transparency value all the time.
    /// </summary>
    public class TransparencyController : MonoBehaviour {

        private static readonly int ColorProperty = Shader.PropertyToID("_Color");
        private static readonly int SrcBlendProperty = Shader.PropertyToID("_SrcBlend");
        private static readonly int DstBlendProperty = Shader.PropertyToID("_DstBlend");
        private static readonly int ZWriteProperty = Shader.PropertyToID("_ZWrite");

        /// <summary>
        /// Current transparency mode of the assigned objects.
        /// </summary>
        public TransparencyMode CurrentMode { get; private set; } = TransparencyMode.Opaque;

        /// <summary>
        /// Alpha value of the material color (between 0 and 1).
        /// 0 = fully transparent, 1 = fully opaque.
        /// </summary>
        public float Alpha {
            get => FirstAlpha;
            set {
                ForAllMaterials(material => {
                    Color color = material.GetColor(ColorProperty);
                    color.a = Mathf.Clamp(value, 0, 1);
                    material.SetColor(ColorProperty, color);
                });
            }
        }

        /// <summary>
        /// Alpha value of the first mesh renderer's first material.
        /// </summary>
        private float FirstAlpha => meshRenderers[0].material.GetColor(ColorProperty).a;

        /// <summary>
        /// Mesh renderers whose material transparency should be controlled by this script.
        /// </summary>
        [SerializeField] private MeshRenderer[] meshRenderers;

        private void Awake() {
            if (meshRenderers.Length == 0) {
                Debug.LogError("No mesh renderers assigned to the transparency controller.");
            }

            Alpha = FirstAlpha; // ensure that *all* materials have the same alpha value
        }

        /// <summary>
        /// Sets the rendering mode of the assigned objects to <see cref="mode"/>.
        /// </summary>
        public void SetMode(TransparencyMode mode) {
            switch (mode) {
                case TransparencyMode.Opaque:
                    SetToOpaque();
                    break;
                case TransparencyMode.Transparent:
                    SetToTransparent();
                    break;
            }
        }

        /// <summary>
        /// Toggles the rendering mode of the assigned objects between opaque and transparent.
        /// </summary>
        public void ToggleMode() {
            switch (CurrentMode) {
                case TransparencyMode.Opaque:
                    SetToTransparent();
                    break;
                case TransparencyMode.Transparent:
                    SetToOpaque();
                    break;
            }
        }

        /// <summary>
        /// Sets the rendering mode of the assigned objects to opaque (not transparent).
        /// </summary>
        private void SetToOpaque() {
            CurrentMode = TransparencyMode.Opaque;
            ForAllMaterials(material => {
                material.SetOverrideTag("RenderType", "Opaque");
                material.SetInt(SrcBlendProperty, (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt(DstBlendProperty, (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt(ZWriteProperty, 1);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = -1;
            });
        }

        /// <summary>
        /// Sets the rendering mode of the assigned objects to transparent.
        /// </summary>
        private void SetToTransparent() {
            CurrentMode = TransparencyMode.Transparent;
            ForAllMaterials(material => {
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt(SrcBlendProperty, (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt(DstBlendProperty, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt(ZWriteProperty, 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            });
        }

        /// <summary>
        /// Helper function to iterate over all materials of all assigned mesh renderers.
        /// </summary>
        private void ForAllMaterials(Action<Material> action) {
            foreach (var meshRenderer in meshRenderers) {
                foreach (var material in meshRenderer.materials) {
                    action(material);
                }
            }
        }
    }
}