Shader "Custom/URP/CartoonSmokeVolume"
{
    Properties
    {
        _DarkColor ("Dark Color", Color) = (0.38, 0.38, 0.42, 0.65)
        _LightColor ("Light Color", Color) = (0.72, 0.72, 0.76, 0.65)
        _Density ("Density", Range(0, 2)) = 0
        _Coverage ("Coverage", Range(0, 1)) = 0.58
        _NoiseScale ("Noise Scale", Float) = 3.0
        _DetailScale ("Detail Scale", Float) = 6.0
        _WindDir ("Wind Dir", Vector) = (0.2, 0.0, 0.1, 0)
        _WindSpeed ("Wind Speed", Float) = 0.18
        _Steps ("Steps", Range(12, 96)) = 40
        _Bands ("Bands", Range(2, 6)) = 3
        _EdgeFade ("Edge Fade", Range(0.02, 0.5)) = 0.12
        _Opacity ("Opacity", Range(0, 1)) = 0.85
        _HeightBias ("Height Bias", Range(0, 2)) = 1.0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
        }

        Pass
        {
            Name "CartoonSmoke"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Front

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionOS : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _DarkColor;
                half4 _LightColor;
                float _Density;
                float _Coverage;
                float _NoiseScale;
                float _DetailScale;
                float4 _WindDir;
                float _WindSpeed;
                float _Steps;
                float _Bands;
                float _EdgeFade;
                float _Opacity;
                float _HeightBias;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionOS = IN.positionOS.xyz;
                return OUT;
            }

            float hash31(float3 p)
            {
                p = frac(p * 0.1031);
                p += dot(p, p.yzx + 33.33);
                return frac((p.x + p.y) * p.z);
            }

            float noise3D(float3 p)
            {
                float3 i = floor(p);
                float3 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);

                float n000 = hash31(i + float3(0,0,0));
                float n100 = hash31(i + float3(1,0,0));
                float n010 = hash31(i + float3(0,1,0));
                float n110 = hash31(i + float3(1,1,0));
                float n001 = hash31(i + float3(0,0,1));
                float n101 = hash31(i + float3(1,0,1));
                float n011 = hash31(i + float3(0,1,1));
                float n111 = hash31(i + float3(1,1,1));

                float nx00 = lerp(n000, n100, f.x);
                float nx10 = lerp(n010, n110, f.x);
                float nx01 = lerp(n001, n101, f.x);
                float nx11 = lerp(n011, n111, f.x);

                float nxy0 = lerp(nx00, nx10, f.y);
                float nxy1 = lerp(nx01, nx11, f.y);

                return lerp(nxy0, nxy1, f.z);
            }

            float fbm(float3 p)
            {
                float v = 0.0;
                float a = 0.5;

                [unroll]
                for (int i = 0; i < 4; i++)
                {
                    v += noise3D(p) * a;
                    p *= 2.0;
                    a *= 0.5;
                }
                return v;
            }

            float2 RayBoxDst(float3 ro, float3 rd, float3 bmin, float3 bmax)
            {
                float3 invRd = 1.0 / rd;
                float3 t0 = (bmin - ro) * invRd;
                float3 t1 = (bmax - ro) * invRd;
                float3 tmin = min(t0, t1);
                float3 tmax = max(t0, t1);

                float nearDst = max(max(tmin.x, tmin.y), tmin.z);
                float farDst  = min(min(tmax.x, tmax.y), tmax.z);

                return float2(nearDst, farDst);
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 camOS = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1)).xyz;
                float3 rayDirOS = normalize(IN.positionOS - camOS);

                float3 boxMin = float3(-0.5, -0.5, -0.5);
                float3 boxMax = float3( 0.5,  0.5,  0.5);

                float2 hit = RayBoxDst(camOS, rayDirOS, boxMin, boxMax);

                if (hit.y <= 0.0 || hit.x > hit.y)
                    discard;

                float t0 = max(hit.x, 0.0);
                float t1 = hit.y;
                float totalDist = max(t1 - t0, 0.0001);

                int steps = (int)_Steps;
                float stepSize = totalDist / steps;

                float3 wind = normalize(_WindDir.xyz + float3(0.0001, 0.0001, 0.0001)) * (_WindSpeed * _Time.y);

                float3 accumColor = 0.0;
                float accumAlpha = 0.0;

                [loop]
                for (int i = 0; i < 96; i++)
                {
                    if (i >= steps) break;

                    float t = t0 + stepSize * (i + 0.5);
                    float3 p = camOS + rayDirOS * t;

                    float ex = smoothstep(0.0, _EdgeFade, p.x + 0.5) * smoothstep(0.0, _EdgeFade, 0.5 - p.x);
                    float ey = smoothstep(0.0, _EdgeFade, p.y + 0.5) * smoothstep(0.0, _EdgeFade, 0.5 - p.y);
                    float ez = smoothstep(0.0, _EdgeFade, p.z + 0.5) * smoothstep(0.0, _EdgeFade, 0.5 - p.z);
                    float edgeMask = ex * ey * ez;

                    float heightMask = saturate((p.y + 0.5) * _HeightBias);

                    float baseNoise   = fbm(p * _NoiseScale + wind);
                    float detailNoise = noise3D(p * _DetailScale - wind * 1.7) * 0.25;

                    float joinedSmoke = saturate(baseNoise + detailNoise);
                    float puff = smoothstep(_Coverage, 0.98, joinedSmoke);

                    float bands = max(_Bands - 1.0, 1.0);
                    float stepped = floor(puff * _Bands) / bands;

                    float density = stepped * edgeMask * lerp(0.75, 1.0, heightMask);
                    float sampleAlpha = density * _Density * stepSize * 2.1 * _Opacity;

                    float3 sampleColor = lerp(_DarkColor.rgb, _LightColor.rgb, stepped);

                    accumColor += (1.0 - accumAlpha) * sampleColor * sampleAlpha;
                    accumAlpha += (1.0 - accumAlpha) * sampleAlpha;

                    if (accumAlpha >= 0.98)
                        break;
                }

                return half4(accumColor, saturate(accumAlpha));
            }
            ENDHLSL
        }
    }
}