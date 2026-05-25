using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Editor.Scripts {
    [ScriptedImporter(1, "fga")]
    public class VectorFieldImporter : ScriptedImporter
    {
        [SerializeField] private bool normalizeForces;
        [SerializeField] private bool importAsForceField;

        public override void OnImportAsset(AssetImportContext ctx)
        {
            string contents = File.ReadAllText(ctx.assetPath);
            string[] entries = contents.Split(',');

            Vector3Int size = new Vector3Int(int.Parse(entries[0]), int.Parse(entries[1]), int.Parse(entries[2]));
            Texture3D vectorField = new Texture3D(size.x, size.y, size.z, TextureFormat.RGBAFloat, false);
            Vector3 dimensionsMin = new Vector3(float.Parse(entries[3]), float.Parse(entries[4]), float.Parse(entries[5]));
            Vector3 dimensionsMax = new Vector3(float.Parse(entries[6]), float.Parse(entries[7]), float.Parse(entries[8]));
            List<Vector3> data = new List<Vector3>();
            for (int i = 9; i + 2 < entries.Length; i += 3)
            {
                data.Add(new Vector3(
                    float.Parse(entries[i + 0], CultureInfo.InvariantCulture),
                    float.Parse(entries[i + 1], CultureInfo.InvariantCulture),
                    float.Parse(entries[i + 2], CultureInfo.InvariantCulture))
                );
            }

            if (normalizeForces)
            {
                float max = data.Max(AbsMaxFloatV3);
                data = data.Select(x => x / max).ToList();
            }

            vectorField.SetPixels(data.Select(x => new Color(x.x, x.y, x.z)).ToArray());
            vectorField.Apply(false);
            vectorField.name = "VectorFieldTexture3D";
            ctx.AddObjectToAsset("mainTex", vectorField);

            if (importAsForceField)
            {
                GameObject forceFieldObj = new GameObject("Particle Force Field", typeof(ParticleSystemForceField))
                    { transform = { localScale = dimensionsMax - dimensionsMin } };
                ParticleSystemForceField forceField = forceFieldObj.GetComponent<ParticleSystemForceField>();
                forceField.shape = ParticleSystemForceFieldShape.Box;
                forceField.gravity = 0;
                forceField.vectorField = vectorField;
                forceField.vectorFieldAttraction = 1;
                ctx.AddObjectToAsset("forceField", forceFieldObj);
                ctx.SetMainObject(forceFieldObj);
            }
        }

        private float AbsMaxFloatV3(Vector3 value)
        {
            return Mathf.Max(Mathf.Abs(value.x), Mathf.Max(Mathf.Abs(value.y), Mathf.Abs(value.z)));
        }
    }
}