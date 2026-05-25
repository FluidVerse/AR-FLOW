using UnityEngine;

namespace Shaders {
    /// <summary>
    /// Controls the fill level of a fillable tank on a per-object basis.
    ///
    /// This script needs to be used ton control the fill level of a tank. If the shader property is modified directly,
    /// it would be applied to all objects using the same material.
    /// </summary>
    public class TankFillLevel : MonoBehaviour {

        private static readonly int FillLevelProperty = Shader.PropertyToID("_FillLevel");

        /// <summary>
        /// Mesh renderer that displays the tank.
        /// </summary>
        [SerializeField] private MeshRenderer meshRenderer;

        /// <summary>
        /// Fill level of the tank. Must be between 0 and 1 and can be modified directly here.
        /// </summary>
        [Range(0, 1)] public float fillLevel;

        private MaterialPropertyBlock propertyBlock;

        private void Awake() {
            propertyBlock = new MaterialPropertyBlock();
        }

        private void Update() {
            propertyBlock.SetFloat(FillLevelProperty, fillLevel);
            meshRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}