Shader "Unlit/FillLevelShader"
{
    Properties
    {
        _StandardColor ("Standard Color", Color) = (0, 0, 0, 1)
        _FillColor ("Fill Color", Color) = (1,1,1,1)
        _FillLevel ("Fill Level", Range(0, 1)) = 0
        _MinFillHeight ("Min Fill Height", Float) = 0
        _MaxFillHeight ("Max Fill Height", Float) = 1
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }

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
                float3 model_pos : TEXCOORD0; // Position in model coordinates
                float4 vertex : SV_POSITION;
            };

            fixed4 _StandardColor;
            fixed4 _FillColor;
            float _FillLevel;
            float _MinFillHeight;
            float _MaxFillHeight;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.model_pos = v.vertex.xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float model_height = _MaxFillHeight - _MinFillHeight;
                float fill_height = model_height * _FillLevel;
                // fill = 1 when y coordinate of model pos is below fill height, 0 when above
                float fill = floor(1 + (fill_height - i.model_pos.y) / model_height);
                // mix both colors depending on fill level
                fixed4 fill_color = fill * _FillColor;
                fixed4 standard_color = (1 - fill) * _StandardColor;
                return fill_color + standard_color;
            }
            ENDCG
        }
    }
}