using System.IO;
using UnityEngine;

namespace BCI {
    public class StateImageTransformer : MonoBehaviour
    {
        public string basePath = "Assets/Resources/FlowData"; // Basisordner mit Originalbildern

        void Start()
        {
            GenerateTransformedImages();
        }

        void GenerateTransformedImages()
        {
            DirectoryInfo dir = new DirectoryInfo(basePath);
            DirectoryInfo[] stateFolders = dir.GetDirectories();

            foreach (var folder in stateFolders)
            {
                string originalState = folder.Name; // z.B. "1234"
                FileInfo[] imageFiles = folder.GetFiles("*.png"); // Lade alle Bilder

                foreach (var imageFile in imageFiles)
                {
                    Texture2D original = LoadTexture(imageFile.FullName);
                    if (original == null) continue;

                    // Korrekte Transformationen gemäß deiner Vorgaben
                    string rotated90State = RotateState(originalState, 90);
                    string rotated180State = RotateState(originalState, 180);
                    string rotated270State = RotateState(originalState, 270);
                    string mirroredHState = MirrorState(originalState, true);
                    string mirroredVState = MirrorState(originalState, false);

                    // Speichere transformierte Bilder mit den neuen Zustandsnamen
                    SaveTexture(RotateImage(original, 90), basePath, rotated90State, imageFile.Name);
                    SaveTexture(RotateImage(original, 180), basePath, rotated180State, imageFile.Name);
                    SaveTexture(RotateImage(original, 270), basePath, rotated270State, imageFile.Name);
                    SaveTexture(FlipImage(original, true), basePath, mirroredHState, imageFile.Name);
                    SaveTexture(FlipImage(original, false), basePath, mirroredVState, imageFile.Name);
                }
            }

            Debug.Log("✅ Alle Bildtransformationen mit korrekten Namen abgeschlossen!");
        }

        string RotateState(string state, int angle)
        {
            char[] rotated = state.ToCharArray();

            switch (angle)
            {
                case 90:  rotated = new char[] { state[3], state[0], state[1], state[2] }; break; // 4123
                case 180: rotated = new char[] { state[2], state[3], state[0], state[1] }; break; // 3412
                case 270: rotated = new char[] { state[1], state[2], state[3], state[0] }; break; // 2341
            }

            return new string(rotated);
        }

        string MirrorState(string state, bool horizontal)
        {
            char[] mirrored = state.ToCharArray();
            if (horizontal) (mirrored[1], mirrored[3]) = (mirrored[3], mirrored[1]); // 1432 (horizontal)
            else (mirrored[0], mirrored[2]) = (mirrored[2], mirrored[0]); // 3214 (vertikal)

            return new string(mirrored);
        }

        Texture2D LoadTexture(string filePath)
        {
            byte[] fileData = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(2, 2);
            if (texture.LoadImage(fileData))
                return texture;
            return null;
        }

        Texture2D FlipImage(Texture2D original, bool horizontal)
        {
            Texture2D flipped = new Texture2D(original.width, original.height);
            for (int y = 0; y < original.height; y++)
            {
                for (int x = 0; x < original.width; x++)
                {
                    int newX = horizontal ? original.width - x - 1 : x;
                    int newY = horizontal ? y : original.height - y - 1;
                    flipped.SetPixel(newX, newY, original.GetPixel(x, y));
                }
            }
            flipped.Apply();
            return flipped;
        }

        Texture2D RotateImage(Texture2D original, int angle)
        {
            int width = original.width;
            int height = original.height;
            Texture2D rotated = new Texture2D(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixel = original.GetPixel(x, y);
                    Vector2 rotatedPos = RotatePoint(new Vector2(x, y), angle, width, height);
                    rotated.SetPixel((int)rotatedPos.x, (int)rotatedPos.y, pixel);
                }
            }
            rotated.Apply();
            return rotated;
        }

        Vector2 RotatePoint(Vector2 point, int angle, int width, int height)
        {
            switch (angle)
            {
                case 90: return new Vector2(point.y, width - point.x - 1);
                case 180: return new Vector2(width - point.x - 1, height - point.y - 1);
                case 270: return new Vector2(height - point.y - 1, point.x);
                default: return point;
            }
        }

        void SaveTexture(Texture2D texture, string basePath, string state, string originalFileName)
        {
            string folderPath = Path.Combine(basePath, state);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string newFilePath = Path.Combine(folderPath, originalFileName);
            if (!File.Exists(newFilePath)) // Verhindert doppelte Dateien
            {
                byte[] bytes = texture.EncodeToPNG();
                File.WriteAllBytes(newFilePath, bytes);
                Debug.Log($"📸 Gespeichert: {newFilePath}");
            }
        }
    }
}
