Shader "Unlit/CoordinateSystemShader"
{
    Properties
    {
        _MainTex ("Main texture (not used, only for successful compilation)", 2D) = "white" { }
        _ImageSizeX ("Image size x (in actual screen px)", Integer) = 1820
        _ImageSizeY ("Image size y (in actual screen px)", Integer) = 980
        _BackgroundColor ("Background color", Color) = (1,1,1,1)
        _LineColor ("Line color", Color) = (0, 0, 0, 1)
        _LineThickness ("Line thickness", Float) = 5
        _LineDistanceX ("X axis line distance (vertical lines)", Float) = 20
        _LineDistanceY ("Y axis line distance (horizontal lines)", Float) = 20
        _OffsetX ("X offset from the left", Float) = 0
        _OffsetY ("Y offset from the top (cg-like coordinate system)", Float) = 0
        _CornerRadius ("Corner radius", Float) = 0
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            int _ImageSizeX;
            int _ImageSizeY;
            fixed4 _BackgroundColor;
            fixed4 _LineColor;
            float _LineThickness;
            float _LineDistanceX;
            float _LineDistanceY;
            float _OffsetX;
            float _OffsetY;
            float _CornerRadius;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            // converts pixel position in the screen space to pixel position in the image space
            float2 to_image_pixel(float2 pixel_pos)
            {
                float2 border_width = (_ScreenParams - float2(_ImageSizeX, _ImageSizeY)) / 2;
                return pixel_pos - border_width;
            }

            // whether pixel_pos is in one of the corners defined by _CornerRadius
            float is_corner(float2 pixel_pos)
            {
                float d_top_left = length(float2(_CornerRadius, _CornerRadius) - pixel_pos);
                float d_top_right = length(float2(_ImageSizeX - _CornerRadius, _CornerRadius) - pixel_pos);
                float d_bottom_left = length(float2(_CornerRadius, _ImageSizeY - _CornerRadius) - pixel_pos);
                float d_bottom_right = length(
                    float2(_ImageSizeX - _CornerRadius, _ImageSizeY - _CornerRadius) - pixel_pos);
                float min_distance = min(min(d_top_left, d_top_right), min(d_bottom_left, d_bottom_right));
                // if the distance to the corner is less than the corner radius, then we are in the corner

                int in_inner_rectangle_x = step(_CornerRadius, pixel_pos.x) - step(
                    _ImageSizeX - _CornerRadius, pixel_pos.x);
                int in_inner_rectangle_y = step(_CornerRadius, pixel_pos.y) - step(
                    _ImageSizeY - _CornerRadius, pixel_pos.y);
                int in_inner_rectangle = max(in_inner_rectangle_x, in_inner_rectangle_y); // logical OR

                float smooth_factor = 4.0;
                float outside_any_corner = 1 - smoothstep(_CornerRadius - smooth_factor, _CornerRadius + smooth_factor,
                                                          min_distance);

                return 1 - max(outside_any_corner, in_inner_rectangle);
            }

            // whether exactly pixel_pos is on a coordinate sytem line
            float is_pixel_on_line_exact(float pixel_pos, float line_distance, float offset)
            {
                const float eps = 0.8;
                float mod_val = abs(fmod(pixel_pos - offset + eps, line_distance));
                return step(mod_val, 0.5) + step(line_distance - mod_val, 0.5); // within 0.5 pixels of line
            }

            // whether pixel_pos is on a coordinate system line, taking line thickness into account
            float is_pixel_on_line(float pixel_pos, float line_distance, float offset)
            {
                float sum = 0.0;
                int half_thickness = int(_LineThickness * 0.5);
                for (int i = -half_thickness; i <= half_thickness; i++)
                {
                    sum += is_pixel_on_line_exact(pixel_pos + i, line_distance, offset);
                }
                return step(0.01, sum); // 1.0 if any part is on the line
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // absolute pixel position bounded to image resolution on the screen
                float2 pos = i.vertex.xy;
                #if defined(SHADER_API_GLES) || defined(SHADER_API_GLES3)
                pos.y = _ScreenParams.y - pos.y; // flip Y for OpenGL ES (android)
                #endif
                float2 pixel_pos = to_image_pixel(pos);
                // pixel position in normalized coordinates [0, 1] and cg-like y axis that goes down (and not up)
                // float2 pixel_pos_norm = i.vertex.xy / _ScreenParams.xy;

                //return fixed4(pixel_pos / float2(_ImageSizeX, _ImageSizeY), 0, 1); // for debugging purposes

                float on_line_x = is_pixel_on_line(pixel_pos.x, _LineDistanceX, _OffsetX);
                float on_line_y = is_pixel_on_line(pixel_pos.y, _LineDistanceY, _OffsetY);
                float on_any_line = max(on_line_x, on_line_y); // logical OR 

                fixed4 color = on_any_line * _LineColor + (1 - on_any_line) * _BackgroundColor;
                float is_px_in_corner = is_corner(pixel_pos);
                fixed4 color_with_corners = fixed4(0, 0, 0, 0) * is_px_in_corner + color * (1 - is_px_in_corner);
                return color_with_corners;
            }
            ENDCG
        }
    }
}