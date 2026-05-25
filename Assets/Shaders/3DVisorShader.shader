Shader "Custom/3DVisorShader"
{
    Properties
    {
        _Color ("Visor Color", Color) = (1, 0, 0, 0.5)
        _Widening ("Widening Factor", Range(0, 10)) = 1.0
        _Curvature ("Curvature", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True"}
        LOD 100

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 localPos : TEXCOORD1;
            };

            fixed4 _Color;
            float _Widening;
            float _Curvature;

            v2f vert (appdata v)
            {
                v2f o;

                // Current local vertex position
                float3 pos = v.vertex.xyz;
                o.localPos = pos; // Pass original local pos

                // Assume standard Unity cube: Z ranges from -0.5 (back) to 0.5 (front).
                // d is 0 at eyes, 1 at far end.
                float d = pos.z + 0.5;

                // 1. Widen vertically/horizontally
                // Make it much wider at end to look like a visor
                pos.x *= (0.5 + d * _Widening);

                // 2. Vertex Curvature (bending the whole mesh back)
                // This bends lateral sides back
                // User said "wrong direction", so let's flip the sign.
                pos.z += (pos.x * pos.x) * _Curvature;
                
                // 3. Rounding the tip (Vertex manipulation)
                o.vertex = UnityObjectToClipPos(float4(pos, 1.0));
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float x = i.localPos.x; 
                float z = i.localPos.z;
                
                // Parabolic cutout at the front
                float curveAmount = (x * x) * (_Curvature * 5.0); 
                if (z > (0.5 + curveAmount)) discard;
                return _Color;
            }
            ENDCG
        }
    }
}
