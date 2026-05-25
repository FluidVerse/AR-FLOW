Shader "Custom/FillLevelSurfaceShader"
{
    Properties
    {
        _StandardColor ("Standard Color", Color) = (0, 0, 0, 1)
        _FillColor ("Fill Color", Color) = (1,1,1,1)
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _MinFillHeight ("Min Fill Height", Float) = 0
        _MaxFillHeight ("Max Fill Height", Float) = 1
        _FillLevel ("Fill Level", Range(0, 1)) = 0
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        struct Input
        {
            float3 model_pos;
        };

        fixed4 _StandardColor;
        fixed4 _FillColor;
        float _FillLevel;
        float _MinFillHeight;
        float _MaxFillHeight;
        half _Glossiness;
        half _Metallic;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
        // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.model_pos = v.vertex;
        }

        void surf(Input i, inout SurfaceOutputStandard o)
        {
            float model_height = _MaxFillHeight - _MinFillHeight;
            float fill_height = model_height * _FillLevel;
            // fill = 1 when y coordinate of model pos is below fill height, 0 when above
            float fill = floor(1 + (fill_height - i.model_pos.y) / model_height);
            // mix both colors depending on fill level
            fixed4 fill_color = fill * _FillColor;
            fixed4 standard_color = (1 - fill) * _StandardColor;
            fixed4 c = fill_color + standard_color;

            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}