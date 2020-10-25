Shader "Hidden/Color Focus"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Mask("Mask", 2D) = "black" {}
        _Saturation("Effect Color Saturation", Range(0, 1)) = 0
        _Darkness("Effect Darkness", Range(0, 1)) = 0.5
        _BalanceR("B/W Balance (R)", Range(0, 2)) = 1
        _BalanceG("B/W Balance (G)", Range(0, 2)) = 1
        _BalanceB("B/W Balance (B)", Range(0, 2)) = 1
    }

    SubShader
    {
        // No culling or depth
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM

            #pragma target 3.0

            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest

            #include "UnityCG.cginc"

            uniform sampler2D _MainTex;
            uniform sampler2D _Mask;
            uniform float4 _MainTex_TexelSize;
            uniform float4 _Mask_TexelSize;
            uniform fixed _Saturation;
            uniform fixed _Darkness;
            uniform half _BalanceR, _BalanceG, _BalanceB;

            struct appdata
            {
                float4 vertex : POSITION;
                half2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                half2 uv : TEXCOORD0;
                half2 uv1 : TEXCOORD1;
            };

            // Vertex program
            v2f vert(appdata v)
            {
                v2f o;

                //Direct3D compatibility
                #if UNITY_UV_STARTS_AT_TOP
                if(_MainTex_TexelSize.y < 0)
                        v.texcoord.y = 1 - v.texcoord.y;
                #endif

                o.vertex = UnityObjectToClipPos(v.vertex);

                // Calculate UVs
                o.uv = MultiplyUV(UNITY_MATRIX_TEXTURE0, v.texcoord);
                o.uv1 = o.uv * _Mask_TexelSize.xy * _Mask_TexelSize.zw;

                return o;
            }

            // Fragment program
            fixed4 frag(v2f i) : SV_Target
            {
                fixed mask = tex2D(_Mask, i.uv1);   // Greyscale mask
                fixed4 c = tex2D(_MainTex, i.uv);   // Color image

                // Color conversion: ITU-R Recommendation BT.601
                fixed3 bw = (c.r * 0.3 * _BalanceR) + (c.g * 0.59 * _BalanceG) + (c.b * 0.11 * _BalanceB);
                fixed4 image = c;

                image.rgb = saturate(lerp(bw, c.rgb, _Saturation)); // Desaturate
                image.rgb *= _Darkness;                             // Darken
                image.rgb = lerp(image.rgb, c.rgb, mask);           // Mask

                return image;
            }

            ENDCG
        }
    }
}
