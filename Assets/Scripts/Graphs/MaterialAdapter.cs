using UnityEngine;

// ReSharper disable UnusedMember.Global
namespace Graphs {
    /// <summary>
    /// Adapter for a material using the <c>Unlit/CoordinateSystemShader</c>, providing easy access to its properties.
    /// </summary>
    public class MaterialAdapter {

        private static readonly int ImageSizeXProperty = Shader.PropertyToID("_ImageSizeX");
        private static readonly int ImageSizeYProperty = Shader.PropertyToID("_ImageSizeY");
        private static readonly int LineDistanceXProperty = Shader.PropertyToID("_LineDistanceX");
        private static readonly int LineDistanceYProperty = Shader.PropertyToID("_LineDistanceY");
        private static readonly int OffsetXProperty = Shader.PropertyToID("_OffsetX");
        private static readonly int OffsetYProperty = Shader.PropertyToID("_OffsetY");
        private static readonly int CornerRadiusProperty = Shader.PropertyToID("_CornerRadius");

        private readonly Material mat;

        public MaterialAdapter(Material mat) {
            this.mat = mat;
        }

        public int ImageSizeX {
            get => mat.GetInteger(ImageSizeXProperty);
            set => mat.SetInteger(ImageSizeXProperty, value);
        }

        public int ImageSizeY {
            get => mat.GetInteger(ImageSizeYProperty);
            set => mat.SetInteger(ImageSizeYProperty, value);
        }

        public float LineDistanceX {
            get => mat.GetFloat(LineDistanceXProperty);
            set => mat.SetFloat(LineDistanceXProperty, value);
        }

        public float LineDistanceY {
            get => mat.GetFloat(LineDistanceYProperty);
            set => mat.SetFloat(LineDistanceYProperty, value);
        }

        public float OffsetX {
            get => mat.GetFloat(OffsetXProperty);
            set => mat.SetFloat(OffsetXProperty, value);
        }

        public float OffsetY {
            get => mat.GetFloat(OffsetYProperty);
            set => mat.SetFloat(OffsetYProperty, value);
        }

        public float CornerRadius {
            get => mat.GetFloat(CornerRadiusProperty);
            set => mat.SetFloat(CornerRadiusProperty, value);
        }
    }
}