Shader "Custom/3DVisorShaderAnimated"
{
    Properties
    {
        _Color ("Visor Color", Color) = (1, 0, 0, 0.5)
        _Widening ("Widening Factor", Range(0, 10)) = 1.0
        _Curvature ("Curvature", Range(0, 1)) = 0.5
        _AnimSpeed ("Animation Speed", Float) = 3.0
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
            float _AnimSpeed;

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
                // Instead of fragment discard (which can look jagged), let's pull the center of the tip out, or corners in
                o.vertex = UnityObjectToClipPos(float4(pos, 1.0));
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Discard based on X to creating a rounded tip in UV/Local space
                float x = i.localPos.x; 
                float z = i.localPos.z;
                
                // Parabolic cutout at the front
                float curveAmount = (x * x) * (_Curvature * 5.0); 
                
                if (z > (0.5 - curveAmount)) discard; 

                // Add dynamic flow effect
                
                // Let's make it look like a digital pulse moving outwards (from eyes to tip).
                
                // 1. Primary "Scanline" effect
                // Use a sawtooth wave or repeating gradient that moves along Z.
                float speed = _Time.y * _AnimSpeed;
                float flowZ = (i.localPos.z + 0.5); // 0 to 1
                
                // Create multiple thin pulse lines
                // frac(flowZ * density - time) gives a saw wave 0..1
                float pulse = frac(-flowZ * 5.0 + speed); 
                
                // Sharpen the pulse to make it a thin line
                // We want the value to be high only when pulse is near 0 or 1.
                // pow(pulse, 20) makes it a curve peaking at 1.
                float lineShape = pow(pulse, 20.0);
                
                // 2. Secondary "Tech" pattern (High frequency noise/lines along X)
                // We can use a simply sin wave on X to break up the line
                float techPattern = sin(i.localPos.x * 50.0 + _Time.y * 10.0);
                // Make it binary-ish
                techPattern = step(0.0, techPattern); // 0 or 1 stripes
                
                // Combine: The scanline is broken by the tech pattern
                float energy = lineShape * (0.5 + 0.5 * techPattern);
                
                // 3. Add a base glow that fades out at the tip
                float baseGlow = 0.2 * (1.0 - flowZ); // Brighter near eyes
                
                fixed4 col = _Color;
                
                // Add energy to alpha and color
                // Energy is white/bright addition
                col.rgb += energy * col.a * 2.0; // Boost brightness where energy is
                col.a = max(col.a, energy); // Ensure energy is opaque-ish
                
                // Add base glow
                col.rgb += baseGlow * col.rgb;
                
                return col;
            }
            ENDCG
        }
    }
}
