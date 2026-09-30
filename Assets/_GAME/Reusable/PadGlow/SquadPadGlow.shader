Shader "H8/Squad Pad Glow"
{
    Properties
    {
        [Header(Color and Glow)]
        [HDR] _GlowColor ("Glow Color", Color) = (2.0, 1.05, 0.18, 1)
        [HDR] _CoreColor ("Core Tint", Color) = (2.6, 2.3, 1.3, 1)
        _CoreSharpness ("Core Sharpness", Range(0.5, 8)) = 2
        _HaloStrength ("Halo Strength", Range(0, 2)) = 0.9
        _InnerGlow ("Inner Glow", Range(0, 1)) = 0.3

        [Header(Shape)]
        _CornerRadius ("Corner Radius", Range(0, 1)) = 0.17
        _Thickness ("Outline Thickness", Range(0, 0.5)) = 0.07
        _Spread ("Glow Spread", Range(0.001, 1)) = 0.35
        _FalloffPower ("Glow Falloff Power", Range(0.5, 6)) = 2.5
        _EdgeOffset ("Edge Offset", Range(-0.5, 0.5)) = 0

        [Header(Pulse)]
        _PulseSpeed ("Pulse Speed", Float) = 2.5
        _PulseMin ("Pulse Min Alpha", Range(0, 1)) = 0.65
        _PulseMax ("Pulse Max Alpha", Range(0, 1)) = 1

        [Header(Flow)]
        _FlowSpeed ("Flow Speed", Float) = 0.2
        _FlowStrength ("Flow Strength", Range(0, 3)) = 0.8
        _FlowLength ("Flow Length", Range(0.01, 0.5)) = 0.12
        [IntRange] _FlowCount ("Flow Count", Range(1, 4)) = 2

        [Header(Fill)]
        [Enum(Perimeter, 0, Horizontal, 1)] _FillMode ("Fill Mode", Float) = 0
        _Fill ("Fill", Range(0, 1)) = 1
        _HeadLength ("Fill Head Length", Range(0.001, 0.5)) = 0.08
        _HeadBoost ("Fill Head Boost", Range(0, 5)) = 2

        [Header(Blending and Depth)]
        [Enum(AlphaBlend, 0, Additive, 1)] _BlendMode ("Blend Mode", Float) = 0
        [Enum(Off, 0, On, 1)] _ZWrite ("ZWrite", Float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Float) = 4
        _DepthOffset ("Depth Offset", Range(-50, 50)) = -1

        [HideInInspector] _Size ("Quad Size", Vector) = (1, 1, 0, 0)
        [HideInInspector] _ShapeSize ("Shape Size", Vector) = (1, 1, 0, 0)
        [HideInInspector] _Intensity ("Intensity", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent+1"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "SQUAD_PAD_GLOW"
            Cull Off
            ZWrite [_ZWrite]
            ZTest [_ZTest]
            Offset 0, [_DepthOffset]
            Blend One OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            static const float Epsilon = 0.0001;
            static const float FillFeather = 0.01;
            static const float HeadFadeRate = 10.0;
            static const float EdgeFadeWidth = 0.04;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _GlowColor;
                float4 _CoreColor;
                float4 _Size;
                float4 _ShapeSize;
                float _CoreSharpness;
                float _HaloStrength;
                float _InnerGlow;
                float _CornerRadius;
                float _Thickness;
                float _Spread;
                float _FalloffPower;
                float _EdgeOffset;
                float _PulseSpeed;
                float _PulseMin;
                float _PulseMax;
                float _FlowSpeed;
                float _FlowStrength;
                float _FlowLength;
                float _FlowCount;
                float _FillMode;
                float _Fill;
                float _HeadLength;
                float _HeadBoost;
                float _BlendMode;
                float _ZWrite;
                float _ZTest;
                float _DepthOffset;
                float _Intensity;
            CBUFFER_END

            float RoundedBoxDistance(float2 samplePoint, float2 halfSize, float radius)
            {
                float2 q = abs(samplePoint) - halfSize + radius;
                return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - radius;
            }

            float PerimeterCoordinate(float2 samplePoint, float2 halfSize, float radius)
            {
                float2 q = abs(samplePoint);
                float2 cornerCenter = halfSize - radius;
                float2 fromCorner = q - cornerCenter;
                float arcLength = HALF_PI * radius;
                float quarterLength = cornerCenter.x + arcLength + cornerCenter.y;

                float quarterCoordinate;

                if (fromCorner.x > 0.0 && fromCorner.y > 0.0)
                {
                    quarterCoordinate = cornerCenter.x + atan2(fromCorner.x, fromCorner.y) * radius;
                }
                else if (fromCorner.y >= fromCorner.x)
                {
                    quarterCoordinate = q.x;
                }
                else
                {
                    quarterCoordinate = cornerCenter.x + arcLength + (cornerCenter.y - q.y);
                }

                float fullCoordinate;

                if (samplePoint.x >= 0.0)
                {
                    fullCoordinate = samplePoint.y >= 0.0 ? quarterCoordinate : 2.0 * quarterLength - quarterCoordinate;
                }
                else
                {
                    fullCoordinate = samplePoint.y < 0.0 ? 2.0 * quarterLength + quarterCoordinate : 4.0 * quarterLength - quarterCoordinate;
                }

                return fullCoordinate / max(4.0 * quarterLength, Epsilon);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 position = (input.uv - 0.5) * _Size.xy;
                float2 halfSize = max(_ShapeSize.xy * 0.5 + _EdgeOffset, Epsilon);
                float radius = _CornerRadius * min(halfSize.x, halfSize.y);

                float signedDistance = RoundedBoxDistance(position, halfSize, radius);
                float lineDistance = abs(signedDistance);
                float halfThickness = _Thickness * 0.5;
                float antialias = fwidth(signedDistance);

                float lineMask = 1.0 - smoothstep(halfThickness - antialias, halfThickness + antialias, lineDistance);
                float coreBlend = pow(1.0 - saturate(lineDistance / max(halfThickness, Epsilon)), _CoreSharpness);
                float3 lineColor = lerp(_GlowColor.rgb, _CoreColor.rgb, coreBlend);

                float haloDistance = max(lineDistance - halfThickness, 0.0);
                float halo = pow(saturate(1.0 - haloDistance / _Spread), _FalloffPower);
                halo *= (signedDistance >= 0.0 ? 1.0 : _InnerGlow) * _HaloStrength;

                float3 color = lineColor * lineMask + _GlowColor.rgb * halo * (1.0 - lineMask);
                float alpha = lineMask + halo * (1.0 - lineMask);

                float perimeter = PerimeterCoordinate(position, halfSize, radius);

                float fillCoordinate = _FillMode < 0.5 ? abs(perimeter - 0.5) * 2.0 : input.uv.x;
                float fillAhead = _Fill * (1.0 + FillFeather) - fillCoordinate;
                float reveal = saturate(fillAhead / FillFeather);
                float head = (1.0 - smoothstep(0.0, _HeadLength, fillAhead)) * reveal * saturate((1.0 - _Fill) * HeadFadeRate);

                float flowPhase = frac(perimeter * _FlowCount - _Time.y * _FlowSpeed);
                float flowDistance = min(flowPhase, 1.0 - flowPhase);
                float flow = pow(saturate(1.0 - flowDistance / _FlowLength), 2.0);

                float pulse = lerp(_PulseMin, _PulseMax, sin(_Time.y * _PulseSpeed) * 0.5 + 0.5);

                float2 borderDistance = min(input.uv, 1.0 - input.uv);
                float edgeFade = smoothstep(0.0, EdgeFadeWidth, min(borderDistance.x, borderDistance.y));

                float visibility = reveal * pulse * edgeFade * _Intensity;
                float brightness = (1.0 + head * _HeadBoost) * (1.0 + flow * _FlowStrength);

                color *= visibility * brightness;
                alpha = saturate(alpha * visibility);

                return half4(color, alpha * (1.0 - _BlendMode));
            }
            ENDHLSL
        }
    }
}
