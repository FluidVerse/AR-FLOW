using UnityEditor;
using UnityEngine;
using System.IO;

public class CropSpritesTo787_Copy : MonoBehaviour
{
    [MenuItem("Tools/Crop FlowData Sprites to 787x787 (Save Copy)")]
    static void CropAndCopySprites()
    {
        string sourceFolder = "Assets/Resources/FlowData";
        string targetFolder = "Assets/Resources/FlowData_Cropped";

        string[] files = Directory.GetFiles(sourceFolder, "*.png", SearchOption.AllDirectories);

        foreach (string sourcePath in files)
        {
            string assetPath = sourcePath.Replace(Application.dataPath, "Assets");
            Texture2D original = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);

            if (original == null)
            {
                Debug.LogWarning($"⚠️ Kein Texture2D gefunden für {assetPath}");
                continue;
            }

            TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(assetPath);

            if (!importer.isReadable || importer.textureCompression != TextureImporterCompression.Uncompressed)
            {
                importer.isReadable = true;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }

            // Zuschneiden auf 787×787 zentriert
            int cropSize = 787;
            int startX = Mathf.Max((original.width - cropSize) / 2, 0);
            int startY = Mathf.Max((original.height - cropSize) / 2, 0);

            Texture2D cropped = new Texture2D(cropSize, cropSize);
            cropped.SetPixels(original.GetPixels(startX, startY, cropSize, cropSize));
            cropped.Apply();

            // Zielpfad berechnen (entsprechend Ordnerstruktur)
            string relativeSubPath = assetPath.Substring(sourceFolder.Length);
            string newPath = Path.Combine(targetFolder, relativeSubPath).Replace('\\', '/');
            string directory = Path.GetDirectoryName(newPath);

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllBytes(newPath, cropped.EncodeToPNG());

            Debug.Log($"✅ Neue Version gespeichert: {newPath}");

            // Reimportiere neue Datei
            AssetDatabase.ImportAsset(newPath, ImportAssetOptions.ForceUpdate);
        }

        Debug.Log("🎯 Zuschneiden abgeschlossen. Alle Bilder liegen jetzt in FlowData_Cropped.");
    }
}
