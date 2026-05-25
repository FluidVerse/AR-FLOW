Shader "Custom/HeightmapDisplacement"
{
    Properties
    {
        _MainTex ("Color + AlphaMask", 2D) = "white" {}
        _HeightMap ("Heightmap (Visual Displacement)", 2D) = "gray" {}
        _CabsHeightMap ("Cabs Data (High Precision)", 2D) = "black" {}

        // Data maps (RFloat required)
        _PhiMap ("Phi Map", 2D) = "black" {}
        _PsiMap ("Psi Map", 2D) = "black" {}

        _HeightScale ("Height Scale", Float) = 1
        _AlphaCutoff ("Alpha Cutoff", Float) = 0.5

        // Pixel thickness controls
        _IsoWidth ("Cabs Iso Pixel Width", Range(0.5, 10.0)) = 2
        _PhiIsoWidth ("Phi Iso Pixel Width", Range(0.5, 10.0)) = 2
        _PsiIsoWidth ("Psi Iso Pixel Width", Range(0.5, 10.0)) = 2

        _MainIsoColor ("Main Iso Color", Color) = (0,0,0,1)
        _PhiIsoColor ("Phi Iso Color", Color) = (1,0,0,1)
        _PsiIsoColor ("Psi Iso Color", Color) = (0,0,1,1)

        // [0] = cabs, [1] = phi, [2] = psi
        _IsoLineTransparency ("Iso Line Transparency", Vector) = (1,1,1,0)
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _HeightMap;
            sampler2D _CabsHeightMap;
            sampler2D _PhiMap;
            sampler2D _PsiMap;
            float4 _MainTex_ST;
            float _HeightScale;
            float _AlphaCutoff;

            float4 _MainIsoColor;
            float4 _PhiIsoColor;
            float4 _PsiIsoColor;

            float _IsoWidth;
            float _PhiIsoWidth;
            float _PsiIsoWidth;

            // Arrays for iso heights. Values [0..1]
            float _IsoHeights[32];
            int _IsoHeightCount;

            float _PhiIsoHeights[32];
            int _PhiIsoHeightCount;

            float _PsiIsoHeights[32];
            int _PsiIsoHeightCount;

            float3 _IsoLineTransparency;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                float2 uv = TRANSFORM_TEX(v.uv, _MainTex);
                float4 hCol = tex2Dlod(_HeightMap, float4(uv, 0, 0));
                float h = hCol.r;

                v.vertex.z += h * _HeightScale;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = uv;
                return o;
            }

            // Superior Distance Field Iso-Line function
            // 1. Finds the NEAREST iso line in value space.
            // 2. Converts that value-distance to screen-pixels using local gradient.
            // 3. Returns alpha for that single nearest line.
            float GetIsoLineAlpha(float val, float pxWidth, float heights[32], int count)
            {
                if (count <= 0) return 0.0;

                // 1. Calculate rate of change (Gradient Magnitude)
                // fwidth = abs(ddx) + abs(ddy), a fast approximation of gradient length.
                float grad = fwidth(val);

                // Prevent division by zero in perfectly flat areas.
                // Because we use High-Precision textures now, gradients are valid even if tiny.
                grad = max(grad, 1e-6);

                // 2. Find NEAREST Iso-Value
                float minDist = 1e9;

                // Unrolled loop for small fixed number is faster, but variable loop is needed here
                for (int k = 0; k < count; k++)
                {
                    float d = abs(val - heights[k]);
                    if (d < minDist) minDist = d;
                }

                // 3. Calculate distance in pixels
                float distPixels = minDist / grad;

                // 4. Smoothstep for AA (Thickness control)
                // We want: 1.0 if dist < width/2
                // We want: 0.0 if dist > width/2 + 1 (antialias edge)
                // distPixels is the distance from the CENTER of the line.

                float halfWidth = pxWidth * 0.5;

                // Using 1.0 pixel feather for Anti-Aliasing
                return 1.0 - smoothstep(halfWidth - 0.5, halfWidth + 0.5, distPixels);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                if (col.a < _AlphaCutoff) discard;

                // Sample High Precision Data
                float cabs = tex2D(_CabsHeightMap, i.uv).r;
                float phi = tex2D(_PhiMap, i.uv).r;
                float psi = tex2D(_PsiMap, i.uv).r;

                // Calculate Alphas independently using Nearest-Neighbor Distance (solves black blobs)

                // CABS
                if (_IsoLineTransparency.x > 0.01)
                {
                    float a = GetIsoLineAlpha(cabs, _IsoWidth, _IsoHeights, _IsoHeightCount);
                    // Use Blend: Source * alpha + Dest * (1-alpha)
                    if (a > 0.0) col.rgb = lerp(col.rgb, _MainIsoColor.rgb, a * _IsoLineTransparency.x);
                }

                // PHI
                if (_IsoLineTransparency.y > 0.01)
                {
                    float a = GetIsoLineAlpha(phi, _PhiIsoWidth, _PhiIsoHeights, _PhiIsoHeightCount);
                    if (a > 0.0) col.rgb = lerp(col.rgb, _PhiIsoColor.rgb, a * _IsoLineTransparency.y);
                }

                // PSI
                if (_IsoLineTransparency.z > 0.01)
                {
                    float a = GetIsoLineAlpha(psi, _PsiIsoWidth, _PsiIsoHeights, _PsiIsoHeightCount);
                    if (a > 0.0) col.rgb = lerp(col.rgb, _PsiIsoColor.rgb, a * _IsoLineTransparency.z);
                }

                return col;
            }
            ENDCG
        }
    }
}