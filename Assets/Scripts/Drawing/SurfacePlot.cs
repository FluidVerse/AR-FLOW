using System;
using System.Threading.Tasks;
using FlowPhysics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Drawing {
    public class SurfacePlot : MonoBehaviour {
        public enum DisplayMode {
            Cabs,
            Phi,
            Psi,
            P
        }

        private static readonly int MainTexProperty = Shader.PropertyToID("_MainTex");
        private static readonly int HeightMapProperty = Shader.PropertyToID("_HeightMap");
        private static readonly int CabsMapProperty = Shader.PropertyToID("_CabsMap");
        private static readonly int CabsHeightMapProperty = Shader.PropertyToID("_CabsHeightMap");
        private static readonly int PhiMapProperty = Shader.PropertyToID("_PhiMap");
        private static readonly int PsiMapProperty = Shader.PropertyToID("_PsiMap");
        private static readonly int HeightScaleProperty = Shader.PropertyToID("_HeightScale");
        private static readonly int PMapProperty = Shader.PropertyToID("_PMap");

        private static readonly int PhiIsoHeightsProperty = Shader.PropertyToID("_PhiIsoHeights");
        private static readonly int PsiIsoHeightsProperty = Shader.PropertyToID("_PsiIsoHeights");
        private static readonly int IsoHeightsProperty = Shader.PropertyToID("_IsoHeights");

        private static readonly int IsoHeightCountProperty = Shader.PropertyToID("_IsoHeightCount");
        private static readonly int PhiIsoHeightCountProperty = Shader.PropertyToID("_PhiIsoHeightCount");
        private static readonly int PsiIsoHeightCountProperty = Shader.PropertyToID("_PsiIsoHeightCount");

        private static readonly int IsoLineTransparencyProperty = Shader.PropertyToID("_IsoLineTransparency");

        public float width = 1;
        public float height = 1;

        public float zScale = 1;

        public int Ni = 100;
        public int Nj = 100;

        private int N;
        private int Ntriangles;

        // Colormap-Einstellungen
        public int texWidth = 801;
        public int texHeight = 801;

        private Mesh mesh;

        public PotentialFlow flowField;

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        [SerializeField] private Material sourceMat;
        private Material mat;
        private Texture2D cmap; // ColorMap
        private Texture2D heightMap;
        private Texture2D cabsMap;
        private Texture2D cabsHeightMap;
        private Texture2D phiMap;
        private Texture2D psiMap;
        private Texture2D PMap;
        private Vector3[] vertices;
        private Vector2[] uv;
        private int[] tris;

        private Texture2D phiTex;
        private Texture2D psiTex;

        public float maximumValue;
        public float minimumValue;

        // IsoLines
        [SerializeField] private int isoCount = 10;

        /// <summary>
        /// When changing this, change _PhiIsoHeights[10] in the shader too
        /// </summary>
        private const int phiIsoCount = 10;

        /// <summary>
        /// When changing this, change _PsiIsoHeights[10] in the shader too
        /// </summary>
        private const int psiIsoCount = 11;

        private readonly float CminVis = 0f;
        private readonly float CmaxVis = 2f; // m/s, “Sättigungsgrenze”
        private readonly float PminVis = 95000f;
        private readonly float PmaxVis = 200000f;

        private float[,] PhiField;
        private float[,] PsiField;
        // ReSharper disable CollectionNeverQueried.Local
        private float[,] CxField;
        private float[,] CyField;
        // ReSharper restore CollectionNeverQueried.Local
        private float[,] CabsField;
        private float[,] hField; // Dieses Feld wird für die Verschiebung in z-Richtung verwendet
        private float[,] PField; // Dieses Feld wird für die Verschiebung in z-Richtung verwendet

        public float minPsi = float.MaxValue;
        public float maxPsi = float.MinValue;
        public float minPhi = float.MaxValue;
        public float maxPhi = float.MinValue;
        public float minCx = float.MaxValue;
        public float maxCx = float.MinValue;
        public float minCy = float.MaxValue;
        public float maxCy = float.MinValue;
        public float minCabs = float.MaxValue;
        public float maxCabs = float.MinValue;
        public float minH = float.MaxValue;
        public float maxH = float.MinValue;
        public float minP = float.MaxValue;
        public float maxP = float.MinValue;

        private DisplayMode displayMode;

        private Color[] pixelBuffer; // reusable buffer

        public void Awake() {
            // Set Potential Flow Field
            flowField = new PotentialFlow();
            InitBuffers();

            N = Ni * Nj;
            Ntriangles = 2 * (Ni - 1) * (Nj - 1);
            meshRenderer = gameObject.AddComponent<MeshRenderer>();

            // copy the material to avoid modifying the original asset
            mat = new Material(sourceMat);
            meshRenderer.sharedMaterial = mat;
            meshFilter = gameObject.AddComponent<MeshFilter>();

            CreateMesh();

            PhiField = new float[texWidth, texHeight];
            PsiField = new float[texWidth, texHeight];
            CxField = new float[texWidth, texHeight];
            CyField = new float[texWidth, texHeight];
            CabsField = new float[texWidth, texHeight];
            hField = new float[texWidth, texHeight];
            PField = new float[texWidth, texHeight];

            SetIsoLineTransparency(0f, 0f, 0f);
        }

        private void InitBuffers() {
            if (pixelBuffer == null || pixelBuffer.Length != texWidth * texHeight) {
                pixelBuffer = new Color[texWidth * texHeight];
            }
        }

        private void CreateMesh() {
            mesh = new Mesh {
                indexFormat = IndexFormat.UInt32 // <-- wichtig für > 65535 Vertices
            };

            // Vertices
            vertices = new Vector3[N];
            for (int i = 0; i < Ni; i++) {
                for (int j = 0; j < Nj; j++) {
                    int n = j * Ni + i;

                    float x = i / (float)(Ni - 1) * width;
                    float y = j / (float)(Nj - 1) * height;

                    vertices[n] = new Vector3(x, y, 0f);
                }
            }
            mesh.Clear();
            mesh.vertices = vertices;

            // Triangles
            tris = new int[3 * Ntriangles];
            for (int i = 0; i < Ni - 1; i++) {
                for (int j = 0; j < Nj - 1; j++) {
                    int n = j * (Ni - 1) + i;
                    int n0 = (j + 0) * Ni + i + 0;
                    int n1 = (j + 0) * Ni + i + 1;
                    int n2 = (j + 1) * Ni + i + 0;
                    int n3 = (j + 1) * Ni + i + 1;

                    // upper left
                    tris[6 * n + 0] = n0;
                    tris[6 * n + 1] = n2;
                    tris[6 * n + 2] = n3;

                    // lower right
                    tris[6 * n + 3] = n0;
                    tris[6 * n + 4] = n3;
                    tris[6 * n + 5] = n1;
                }
            }
            mesh.triangles = tris;

            // UVs
            uv = new Vector2[N];
            for (int i = 0; i < Ni; i++) {
                for (int j = 0; j < Nj; j++) {
                    int n = j * Ni + i;
                    float u = i / (float)(Ni - 1);
                    float v = j / (float)(Nj - 1);
                    uv[n] = new Vector2(u, v);
                }
            }
            mesh.uv = uv;

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            meshFilter.mesh = mesh;
        }

        public Vector2 SnapToGrid(Vector2 pos) {
            float dx = width / (texWidth - 1);
            float dy = height / (texHeight - 1);

            float x = Mathf.Round(pos.x / dx) * dx;
            float y = Mathf.Round(pos.y / dy) * dy;

            return new Vector2(x, y);
        }

        public Vector3 SnapToGrid(Vector3 pos) {
            Vector2 res = SnapToGrid(new Vector2(pos.x, pos.y));
            return new Vector3(res.x, res.y, pos.z);
        }

        public void UpdateField() {
            CalculateFieldArrays();
            UpdateFieldTextures();
        }

        // Füllt die Arrays von Phi, Psi, Cx, Cy und Cabs
        private void CalculateFieldArrays() {
            minPhi = minPsi = minCx = minCy = minCabs = minP = float.MaxValue;
            maxPhi = maxPsi = maxCx = maxCy = maxCabs = maxP = float.MinValue;

            object lockObj = new object();

            float Uinf = flowField.Uinf();

            // Parallelize the outer loop (rows)
            Parallel.For(0, texHeight, yPix => {
                // Maintain local min/max for this row to avoid locking per pixel
                float localMinPhi = float.MaxValue;
                float localMaxPhi = float.MinValue;
                float localMinPsi = float.MaxValue;
                float localMaxPsi = float.MinValue;
                float localMinCx = float.MaxValue;
                float localMaxCx = float.MinValue;
                float localMinCy = float.MaxValue;
                float localMaxCy = float.MinValue;
                float localMinCabs = float.MaxValue;
                float localMaxCabs = float.MinValue;
                float localMinP = float.MaxValue;
                float localMaxP = float.MinValue;

                float v = (float)yPix / (texHeight - 1);
                float y = v * height;

                for (int xPix = 0; xPix < texWidth; xPix++) {
                    float u = (float)xPix / (texWidth - 1);
                    float x = u * width;
                    Vector2 pos = new Vector2(x, y);

                    // Calculate Phi
                    float Phi = flowField.Phi(pos);
                    PhiField[xPix, yPix] = Phi;
                    if (Phi < localMinPhi) localMinPhi = Phi;
                    if (Phi > localMaxPhi) localMaxPhi = Phi;

                    // Calculate Psi
                    float Psi = flowField.Psi(pos);
                    PsiField[xPix, yPix] = Psi;
                    if (Psi < localMinPsi) localMinPsi = Psi;
                    if (Psi > localMaxPsi) localMaxPsi = Psi;

                    // Calculate Velocity C
                    Vector2 C = flowField.C(pos);
                    CxField[xPix, yPix] = C.x;
                    CyField[xPix, yPix] = C.y;
                    if (C.x < localMinCx) localMinCx = C.x;
                    if (C.x > localMaxCx) localMaxCx = C.x;
                    if (C.y < localMinCy) localMinCy = C.y;
                    if (C.y > localMaxCy) localMaxCy = C.y;

                    // Calculate Cabs
                    float Cabs = Mathf.Clamp(C.magnitude, CminVis, CmaxVis);
                    CabsField[xPix, yPix] = Cabs;
                    if (Cabs < localMinCabs) localMinCabs = Cabs;
                    if (Cabs > localMaxCabs) localMaxCabs = Cabs;

                    // Calculate Pressure P
                    float v2 = C.sqrMagnitude;
                    float P = 100000f + 0.5f * 1000f * (Uinf * Uinf - v2);
                    P = Mathf.Clamp(P, PminVis, PmaxVis);
                    if (P < localMinP) localMinP = P;
                    if (P > localMaxP) localMaxP = P;
                    PField[xPix, yPix] = P;
                }

                // Sync local results to global state securely
                lock (lockObj) {
                    if (localMinPhi < minPhi) minPhi = localMinPhi;
                    if (localMaxPhi > maxPhi) maxPhi = localMaxPhi;
                    if (localMinPsi < minPsi) minPsi = localMinPsi;
                    if (localMaxPsi > maxPsi) maxPsi = localMaxPsi;
                    if (localMinCx < minCx) minCx = localMinCx;
                    if (localMaxCx > maxCx) maxCx = localMaxCx;
                    if (localMinCy < minCy) minCy = localMinCy;
                    if (localMaxCy > maxCy) maxCy = localMaxCy;
                    if (localMinCabs < minCabs) minCabs = localMinCabs;
                    if (localMaxCabs > maxCabs) maxCabs = localMaxCabs;
                    if (localMinP < minP) minP = localMinP;
                    if (localMaxP > maxP) maxP = localMaxP;
                }
            });

            Debug.Log($"Cabs range: minCabs={minCabs}, maxCabs={maxCabs}");
        }

        private void UpdateFieldTextures() {
            // 1. Ensure buffers exist
            InitBuffers();

            // 2. Update Data Textures (Always needed for Shader variables)
            UpdateColorMap(CabsField, minCabs, maxCabs, ref cabsMap);
            UpdateFloatMap(PhiField, minPhi, maxPhi, ref phiMap);
            UpdateFloatMap(PsiField, minPsi, maxPsi, ref psiMap);
            UpdateFloatMap(CabsField, minCabs, maxCabs, ref cabsHeightMap);
            UpdateFloatMap(PField, minP, maxP, ref PMap);

            // 3. Assign to Material Properties
            mat.SetTexture(CabsMapProperty, cabsMap);
            mat.SetTexture(PhiMapProperty, phiMap);
            mat.SetTexture(PsiMapProperty, psiMap);
            mat.SetTexture(CabsHeightMapProperty, cabsHeightMap);
            mat.SetTexture(PMapProperty, PMap);

            // 4. Update Isolines
            UpdateShaderIsoLines();
            UpdatePhiPsiIsoLines();

            // 5. Update Visuals (MainTex & HeightMap)
            // We pass true because we just ran UpdateColorMap(Cabs...), so hField currently contains Cabs data.
            UpdateVisuals(true);
        }

        /// <summary>
        /// Set the transparency of the iso lines for Cabs, Phi and Psi.
        ///
        /// All values should be between 0 and 1.
        /// </summary>
        public void SetIsoLineTransparency(float cabs, float phi, float psi) {
            mat.SetVector(IsoLineTransparencyProperty, new Vector4(cabs, phi, psi, 0));
        }

        /// <summary>
        /// Set the display mode of the surface plot.
        /// </summary>
        public void SetDisplayMode(DisplayMode mode) {
            displayMode = mode;
            // Optimization: ONLY update visual textures (Color + Height).
            // Do NOT call UpdateColormap(), which recalculates data textures (PhiMap, PsiMap, etc.)
            UpdateVisuals();
        }

        /// <summary>
        /// Updates only the visual aspects (MainTex, HeightMap) without recalculating physics.
        /// </summary>
        /// <param name="hFieldIsCabs">Optimization: set to true if hField was just populated with Cabs data.</param>
        private void UpdateVisuals(bool hFieldIsCabs = false) {
            InitBuffers();

            switch (displayMode) {
                case DisplayMode.Cabs:
                    // Optimization: If we came from UpdateColormap, hField is already Cabs.
                    // If we came from SetDisplayMode, we must regenerate CabsMap to ensure hField is correct.
                    if (!hFieldIsCabs) {
                        UpdateColorMap(CabsField, minCabs, maxCabs, ref cabsMap);
                    }
                    cmap = cabsMap;
                    maximumValue = maxCabs;
                    minimumValue = minCabs;
                    break;
                case DisplayMode.Phi:
                    // Generates Phi colors and overwrites hField with Phi data
                    UpdateColorMap(PhiField, minPhi, maxPhi, ref cmap);
                    maximumValue = maxPhi;
                    minimumValue = minPhi;
                    break;
                case DisplayMode.Psi:
                    // Generates Psi colors and overwrites hField with Psi data
                    UpdateColorMap(PsiField, minPsi, maxPsi, ref cmap);
                    maximumValue = maxPsi;
                    minimumValue = minPsi;
                    break;
                case DisplayMode.P:
                    // Generates Psi colors and overwrites hField with Psi data
                    UpdateColorMap(PField, minP, maxP, ref cmap);
                    maximumValue = maxP;
                    minimumValue = minP;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Apply Main Texture
            meshRenderer.sharedMaterial.mainTexture = cmap;
            mat.SetTexture(MainTexProperty, cmap);

            // Apply Height Scaling
            mat.SetFloat(HeightScaleProperty, zScale);

            // Update Height Map (Derived from current hField)
            UpdateHeightMap(ref heightMap);
            mat.SetTexture(HeightMapProperty, heightMap);
        }

        // Helper: Validates or Recreates texture only if needed
        private void EnsureTexture(ref Texture2D tex, TextureFormat format) {
            if (tex != null && tex.width == texWidth && tex.height == texHeight && tex.format == format) {
                return;
            }
            if (tex != null) {
                Destroy(tex);
            }
            tex = new Texture2D(texWidth, texHeight, format, false, true) {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
        }

        private void UpdateFloatMap(float[,] field, float minVal, float maxVal, ref Texture2D tex) {
            EnsureTexture(ref tex, TextureFormat.RFloat);

            int idx = 0;
            float range = maxVal - minVal;
            if (range < 1e-6f) range = 1f;
            float invRange = 1f / range;

            for (int y = 0; y < texHeight; y++) {
                for (int x = 0; x < texWidth; x++) {
                    float val = field[x, y];
                    // Normalize manually to avoid Mathf.InverseLerp call overhead in tight loop
                    float normVal = (val - minVal) * invRange;
                    if (normVal < 0) normVal = 0;
                    if (normVal > 1) normVal = 1;

                    pixelBuffer[idx].r = normVal;
                    // pixelBuffer[idx].g = 0; // optimized out for RFloat
                    // pixelBuffer[idx].b = 0;
                    pixelBuffer[idx].a = 1f;
                    idx++;
                }
            }
            tex.SetPixels(pixelBuffer);
            tex.Apply();
        }

        private void UpdatePhiPsiIsoLines() {
            // Update Phi Iso Lines
            int pCount = Mathf.Clamp(phiIsoCount, 0, 32);
            float[] pIsos = new float[32];
            float phiRange = maxPhi - minPhi;
            if (phiRange < 1e-6f) phiRange = 1f; // prevent div by zero
            for (int k = 0; k < pCount; k++) {
                float actualIso = minPhi + (float)(k + 1) / (pCount + 1) * phiRange;
                pIsos[k] = Mathf.InverseLerp(minPhi, maxPhi, actualIso);
            }

            // Pass array of 32 to shader to match array size
            mat.SetFloatArray(PhiIsoHeightsProperty, pIsos);
            mat.SetInt(PhiIsoHeightCountProperty, pCount);

            // Update Psi Iso Lines
            int sCount = Mathf.Clamp(psiIsoCount, 0, 32);
            float[] sIsos = new float[32];
            float psiRange = maxPsi - minPsi;
            if (psiRange < 1e-6f) psiRange = 1f; // prevent div by zero
            for (int k = 0; k < sCount; k++) {
                float actualIso = minPsi + (float)(k + 1) / (sCount + 1) * psiRange;
                sIsos[k] = Mathf.InverseLerp(minPsi, maxPsi, actualIso);
            }
            mat.SetFloatArray(PsiIsoHeightsProperty, sIsos);
            mat.SetInt(PsiIsoHeightCountProperty, sCount);
        }

        private void UpdateShaderIsoLines() {
            int count = Mathf.Clamp(isoCount, 0, 32);
            float[] isos = new float[32];

            for (int k = 0; k < count; k++) {
                float t = (float)(k + 1) / (count + 1);
                isos[k] = t;
            }

            mat.SetFloatArray(IsoHeightsProperty, isos);
            mat.SetInt(IsoHeightCountProperty, count);
        }

        // Berechnet aus hField die HeightMap (cmap muss vorher da sein für die Größe)
        // Update HeightMap from hField (Gray)
        private void UpdateHeightMap(ref Texture2D tex) {
            EnsureTexture(ref tex, TextureFormat.RGBA32); // Visual heightmap uses standard precision

            Parallel.For(0, texHeight, yPix => {
                int rowOffset = yPix * texWidth;
                for (int xPix = 0; xPix < texWidth; xPix++) {
                    float t = hField[xPix, yPix];
                    // Grayscale assignment
                    pixelBuffer[rowOffset + xPix] = new Color(t, t, t, 1f);
                }
            });

            tex.SetPixels(pixelBuffer);
            tex.Apply();
        }

        public void AddUniformFlow(Vector2 velocity) {
            UniformFlow uFlow = new UniformFlow(SnapToGrid(velocity));
            flowField.AddElement(uFlow);
        }

        public void AddSinkSource(float amplitude, Vector2 position) {
            SourceSink source = new SourceSink(amplitude, SnapToGrid(position));
            flowField.AddElement(source);
        }

        public void AddDipole(float amplitude, Vector2 position) {
            Dipole dipole = new Dipole(amplitude, SnapToGrid(position));
            flowField.AddElement(dipole);
        }

        public void AddVortex(float amplitude, Vector2 position) {
            Vortex vortex = new Vortex(amplitude, SnapToGrid(position));
            flowField.AddElement(vortex);
        }

        public void AddCylinder(float radius, Vector2 position) {
            Cylinder cyl1 = new Cylinder(radius, SnapToGrid(position));
            flowField.AddElement(cyl1);
        }

        // Direct Color Map Update (Fills hField too)
        private void UpdateColorMap(float[,] field, float min, float max, ref Texture2D tex) {
            EnsureTexture(ref tex, TextureFormat.RGBA32);

            float range = max - min;
            float invRange = Mathf.Abs(range) > 0.001f ? 1f / range : 0f;

            Parallel.For(0, texHeight, yPix => {
                // Pre-calculate Y position for mask check
                float yPos = (float)yPix / (texHeight - 1) * height;
                int rowOffset = yPix * texWidth;

                for (int xPix = 0; xPix < texWidth; xPix++) {
                    float f = field[xPix, yPix];

                    // Normalize t to 0..1
                    float t = (f - min) * invRange;
                    if (t < 0) t = 0;
                    if (t > 1) t = 1;

                    // 1. Update hField (Displacement data)
                    hField[xPix, yPix] = t;

                    // 2. Calculate Color
                    // Inline Color Map Logic: Hue = (1 - t) * 2/3 (Blue to Red)
                    float hue = (1f - t) * 0.6666f;
                    Color c = Color.HSVToRGB(hue, 1f, 1f);

                    // 3. Masking
                    float xPos = (float)xPix / (texWidth - 1) * width;
                    // Reading flowField is thread-safe as long as the list isn't modified during render
                    c.a = flowField.ValidMask(new Vector2(xPos, yPos)) < 0.9f ? 0f : 1f;

                    // Write to shared buffer (safe because indices are distinct)
                    pixelBuffer[rowOffset + xPix] = c;
                }
            });

            // Main thread upload to GPU
            tex.SetPixels(pixelBuffer);
            tex.Apply();
        }

        // ReSharper disable once UnusedMember.Local
        private void PrintMeshInfo() {
            Debug.Log("Mesh Info:");
            Debug.Log($"Vertices: {mesh.vertexCount}");
            Debug.Log($"Triangles: {mesh.triangles.Length / 3}");
            Debug.Log($"Bounds: {mesh.bounds}");

            // Optional: erste paar Vertices
            for (int i = 0; i < Mathf.Min(10, mesh.vertexCount); i++) {
                Debug.Log($"v[{i}] = {mesh.vertices[i]}");
            }

            // Optional: erste paar Triangles
            for (int i = 0; i < Mathf.Min(10, tris.Length / 3); i++) {
                int a = tris[i * 3 + 0];
                int b = tris[i * 3 + 1];
                int c = tris[i * 3 + 2];
                Debug.Log($"tri[{i}] = {a}, {b}, {c}");
            }
        }

        [ContextMenu("Log Surface Z Range")]
        private void LogSurfaceZRange() {
            if (heightMap == null) {
                Debug.LogWarning("heightMap is null – call CalculateField() first");
                return;
            }

            float zMin = float.PositiveInfinity;
            float zMax = float.NegativeInfinity;

            float hScale = mat.GetFloat(HeightScaleProperty);

            for (int y = 0; y < heightMap.height; y++) {
                for (int x = 0; x < heightMap.width; x++) {
                    Color h = heightMap.GetPixel(x, y);

                    float z = h.r * h.a * hScale; // exakt wie im Shader

                    zMin = Mathf.Min(zMin, z);
                    zMax = Mathf.Max(zMax, z);
                }
            }

            Debug.Log($"Surface Z range: zMin={zMin}, zMax={zMax}, HeightScale={hScale}");
        }
    }
}