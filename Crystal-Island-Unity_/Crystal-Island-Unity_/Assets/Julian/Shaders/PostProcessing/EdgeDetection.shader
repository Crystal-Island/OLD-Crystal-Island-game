Shader "Hidden/Edge Detection"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Threshold("Threshold", float) = 0.02
        _EdgeColor("Edge color", Color) = (0,0,0,1)
        _Mask("Mask", 2D) = "white" {}
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

            #include "UnityCG.cginc"

            uniform sampler2D _MainTex;
            uniform sampler2D _Mask;
            uniform sampler2D _CameraDepthTexture;
            uniform sampler2D _CameraDepthNormalsTexture;

            uniform float4 _MainTex_TexelSize;
            uniform float4 _Mask_TexelSize;
            uniform half _Threshold;
            uniform fixed4 _EdgeColor;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 screenCoord : TEXCOORD1;
            };

            // Get camera depth normal value.
            fixed4 GetNormalDepth(in float2 uv)
            {
                fixed3 normal;
                fixed depth;
                DecodeDepthNormal(tex2D(_CameraDepthNormalsTexture, uv), depth, normal);
                return fixed4(normal, depth);
            }

            // Get camera depth distance value
            fixed GetDepthValue(in float2 uv)
            {
                return Linear01Depth(tex2D(_CameraDepthTexture, uv).r);
            }

            // Vertex program
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.screenCoord = v.uv * _Mask_TexelSize.xy * _ScreenParams.xy;

                return o;
            }

            // Fragment program
            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 image = tex2D(_MainTex, i.uv);

                fixed4 initalNormalDepth = GetNormalDepth(i.uv);
                fixed initialDepthValue = GetDepthValue(i.uv);

                // Sample surrounding pixels
                fixed2 offsets[8] = {
                    fixed2(-1, -1),
                    fixed2(-1, 0),
                    fixed2(-1, 1),
                    fixed2(0, -1),
                    fixed2(0, 1),
                    fixed2(1, -1),
                    fixed2(1, 0),
                    fixed2(1, 1)
                };

                half4 sampledNormals = half4(0,0,0,0);
                half sampledDepth = 0;

                for(int j = 0; j < 8; j++)
                {
                    sampledNormals += GetNormalDepth(i.uv + offsets[j] * _MainTex_TexelSize.xy);
                    sampledDepth += GetDepthValue(i.uv + offsets[j] * _MainTex_TexelSize.xy);
                }

                sampledNormals = sampledNormals / 8;
                sampledDepth = sampledDepth / 8;

                // Extract edges
                fixed edges = saturate((initialDepthValue - sampledDepth) + length(initalNormalDepth - sampledNormals));
                edges = lerp(0, edges, tex2D(_Mask, i.screenCoord * 20));

                // Mix into image
                return lerp(image, _EdgeColor, step(_Threshold, edges) * _EdgeColor.a);
            }

            ENDCG
        }
    }
}
