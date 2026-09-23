Shader "H8/Stylized UI"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Style constants shared by the whole UI)]
        _GradientRotation  ("Linear Gradient Rotation", Range(0,360)) = 90
        _OutlineDarken     ("Outline Darken", Range(0,1)) = 0.38
        _HighlightStrength ("Top Highlight", Range(0,1)) = 0.20
        _HighlightHeight   ("Highlight Height", Range(0.01,1)) = 0.38
        _InnerShadow       ("Bottom Inner Shadow", Range(0,1)) = 0.22
        _ShadowColor       ("Ambient Shadow Color", Color) = (0.09,0.11,0.18,1)

        [Header(UI stencil boilerplate)]
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"            = "Transparent"
            "IgnoreProjector"  = "True"
            "RenderType"       = "Transparent"
            "PreviewType"      = "Plane"
            "CanUseSpriteAtlas"= "True"
        }

        Stencil
        {
            Ref       [_Stencil]
            Comp      [_StencilComp]
            Pass      [_StencilOp]
            ReadMask  [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        ColorMask [_ColorMask]
        // Premultiplied alpha: required, because we composite 3 layers
        // (shadow / bevel / body) inside the fragment before writing out.
        Blend One OneMinusSrcAlpha

        Pass
        {
            Name "STYLIZED_UI"
            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma target   3.5
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT

            // ---------------------------------------------------------------
            // Per-element data arrives through vertex channels, NOT material
            // properties, so every element on the canvas shares one material
            // and stays in a single draw call.
            //
            //   uv0     = (u, v, padding_px, gradientMode)
            //   uv1     = (width, height, cornerRadius, outlineWidth)     px
            //   uv2     = (packTL, packTR, packBR, packBL)   RGB888 -> float
            //   uv3     = (bevelSize, shadowOffsetY, shadowSoftness, shadowAlpha)
            //   normal  = (effectType + 16*patternType, effectStrength, effectSpeed)
            //   tangent = (patternScale, patternStrength, patternSpeed, phase)
            //
            // The two enum ids share normal.x because uv0..uv3 were already full
            // and NORMAL only carries three floats; both are small integers, so
            // one multiply-add packs them losslessly.
            //
            // Packed colours survive rasterizer interpolation only because all
            // four verts of a quad carry identical values, so interpolation is
            // the identity. Never pack per-vertex-varying data this way.
            // ---------------------------------------------------------------

            struct appdata_t
            {
                float4 vertex  : POSITION;
                float4 color   : COLOR;
                float4 uv0     : TEXCOORD0;
                float4 uv1     : TEXCOORD1;
                float4 uv2     : TEXCOORD2;
                float4 uv3     : TEXCOORD3;
                float3 normal  : NORMAL;
                float4 tangent : TANGENT;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // uv1..uv3, fx and pat are PER-QUAD CONSTANTS: all four verts carry
            // identical values. Interpolating them is not just wasted work, it is
            // wrong on a perspective (World Space canvas) quad -- the rasterizer's
            // perspective-correct divide is done in fp32, and uv2 holds colours
            // packed up to 2^24, exactly the mantissa width. A 1e-7 relative error
            // there lands as ~1.7 absolute, which UnpackRGB turns into per-pixel
            // channel flips (visible as coloured hatching on the far end of a
            // tilted quad). 'nointerpolation' takes the provoking vertex verbatim,
            // so the value is exact at any angle -- and cheaper.
            // uv0.xy must stay interpolated; uv0.zw ride along but are small
            // integers that floor() snaps back anyway.
            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color  : COLOR;
                float4 uv0    : TEXCOORD0;
                nointerpolation float4 uv1 : TEXCOORD1;
                nointerpolation float4 uv2 : TEXCOORD2;
                nointerpolation float4 uv3 : TEXCOORD3;
                float4 wpos   : TEXCOORD4;
                nointerpolation float3 fx  : TEXCOORD5;
                nointerpolation float4 pat : TEXCOORD6;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float4 _ClipRect;
            fixed4 _Color;
            fixed4 _ShadowColor;
            float  _GradientRotation, _OutlineDarken;
            float  _HighlightStrength, _HighlightHeight, _InnerShadow;

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.wpos   = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color  = v.color * _Color;
                o.uv0 = v.uv0;  o.uv1 = v.uv1;
                o.uv2 = v.uv2;  o.uv3 = v.uv3;
                o.fx  = v.normal;
                o.pat = v.tangent;
                return o;
            }

            // RGB888 packed into a float32. Mantissa is 24 bits, and
            // 255*65536 + 255*256 + 255 = 16777215 = 2^24 - 1, so the encoding
            // is exact — no rounding drift.
            float3 UnpackRGB(float v)
            {
                float r = floor(v * (1.0 / 65536.0));
                float g = floor((v - r * 65536.0) * (1.0 / 256.0));
                float b = v - r * 65536.0 - g * 256.0;
                return float3(r, g, b) * (1.0 / 255.0);
            }

            // Exact SDF of a rounded box (Inigo Quilez).
            // Returns signed distance in pixels: < 0 inside, 0 on the edge.
            float sdRoundedBox(float2 p, float2 halfSize, float r)
            {
                float2 q = abs(p) - halfSize + r;
                return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - r;
            }

            float ndot(float2 a, float2 b) { return a.x * b.x - a.y * b.y; }

            // Exact SDF of a rhombus (Inigo Quilez). b = half-diagonals, i.e. the
            // diamond's tips sit at (+-b.x, 0) and (0, +-b.y) — the rect's own
            // edge midpoints when b == halfSize, so it reads as the same rect
            // rotated 45 degrees rather than a smaller shape floating inside it.
            float sdRhombus(float2 p, float2 b)
            {
                p = abs(p);
                float h = clamp(ndot(b - 2.0 * p, b) / dot(b, b), -1.0, 1.0);
                float d = length(p - 0.5 * b * float2(1.0 - h, 1.0 + h));
                return d * sign(p.x * b.y + p.y * b.x - b.x * b.y);
            }

            // Corner-rounded rhombus via the same shrink-then-regrow trick
            // sdRoundedBox uses. Exact for the square case (b.x == b.y, the usual
            // gem-frame look); a close, artifact-free approximation otherwise.
            float sdRoundedRhombus(float2 p, float2 b, float r)
            {
                return sdRhombus(p, max(b - r, 0.001)) - r;
            }

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            // Rodrigues rotation of the RGB vector around the grey axis. Same
            // result as an HSV round trip for hue, at a fraction of the cost.
            float3 HueShift(float3 c, float h)
            {
                const float3 k = float3(0.57735027, 0.57735027, 0.57735027);
                float a = h * 6.28318531;
                float ca = cos(a);
                return c * ca + cross(k, c) * sin(a) + k * dot(k, c) * (1.0 - ca);
            }

            // Antialiased 50% duty band. Takes the UNWRAPPED coordinate: fwidth of
            // an already-fracced value spikes at every wrap and paints a seam.
            float Band(float x)
            {
                float w = max(fwidth(x) * 1.5, 0.015);
                float d = abs(frac(x) - 0.5) * 2.0;
                return smoothstep(0.5 + w, 0.5 - w, d);
            }

            float PatternMask(float id, float2 n, float2 size, float scale, float speed)
            {
                float t = _Time.y * speed;
                float minSide = max(min(size.x, size.y), 1.0);
                float2 pn = (n - 0.5) * (size / minSide) * scale;

                if (id < 1.5)                       // 1 = Stripes
                {
                    return Band((pn.x + pn.y) * 0.5 + t);
                }
                if (id < 2.5)                       // 2 = Dots
                {
                    float2 g = frac(pn + t) - 0.5;
                    return smoothstep(0.34, 0.24, length(g));
                }
                if (id < 3.5)                       // 3 = Checker
                {
                    float2 c = floor(pn + float2(t, 0.0));
                    return frac((c.x + c.y) * 0.5) * 2.0;
                }
                if (id < 4.5)                       // 4 = Chevron
                {
                    float zig = abs(frac(pn.x) - 0.5) * 2.0;
                    return Band(pn.y * 0.5 + zig * 0.5 + t);
                }
                if (id < 5.5)                       // 5 = Grid
                {
                    float2 g = abs(frac(pn + t) - 0.5) * 2.0;
                    return max(smoothstep(0.84, 0.97, g.x), smoothstep(0.84, 0.97, g.y));
                }
                if (id < 6.5)                       // 6 = Sunburst
                {
                    float2 d = n - 0.5;
                    float ang = atan2(d.y, d.x) * (1.0 / 6.28318531);
                    return Band(ang * max(round(scale), 3.0) + t);
                }
                // 7 = Diamonds
                float2 gd = abs(frac(pn + t) - 0.5);
                return smoothstep(0.36, 0.26, gd.x + gd.y);
            }

            fixed4 frag (v2f IN) : SV_Target
            {
                float  pad      = IN.uv0.z;
                // gradMode rides in the low decade, shape id in the next one up —
                // see StylizedGraphic.OnPopulateMesh for the packing.
                float  modeSnap = floor(IN.uv0.w + 0.5);
                float  shapeId  = floor(modeSnap * 0.1);
                float  gradMode = modeSnap - shapeId * 10.0;
                float2 size     = IN.uv1.xy;
                float  radius   = IN.uv1.z;
                float  outlineW = IN.uv1.w;

                float  bevel     = IN.uv3.x;
                float  shOffsetY = IN.uv3.y;
                float  softness  = max(IN.uv3.z, 0.001);
                float  shAlpha   = IN.uv3.w;

                // Snap to an exact integer first: interpolation can leave the
                // packed value a hair below, and 111.9999 would decode as
                // pattern 6 / effect 15 instead of pattern 7 / effect 0.
                float  packedIds = floor(IN.fx.x + 0.5);
                float  patternId = floor(packedIds * 0.0625);
                float  effectId  = packedIds - patternId * 16.0;
                float  fxStr     = IN.fx.y;
                float  fxSpeed   = IN.fx.z;
                float  patScale  = IN.pat.x;
                float  patStr    = IN.pat.y;
                float  patSpeed  = IN.pat.z;
                float  phase     = IN.pat.w;

                // uv0.xy spans the PADDED quad; map back to pixel space
                // centred on the actual RectTransform.
                float2 padded   = size + 2.0 * pad;
                float2 p        = (IN.uv0.xy - 0.5) * padded;
                float2 halfSize = size * 0.5;

                float dMain, dBevel, dShadow;
                if (shapeId > 0.5)   // 1 = Diamond
                {
                    dMain   = sdRoundedRhombus(p, halfSize, radius);
                    dBevel  = sdRoundedRhombus(p + float2(0.0, bevel), halfSize, radius);
                    dShadow = sdRoundedRhombus(p + float2(0.0, shOffsetY), halfSize, radius);
                }
                else                 // 0 = RoundedBox
                {
                    dMain   = sdRoundedBox(p, halfSize, radius);
                    dBevel  = sdRoundedBox(p + float2(0.0, bevel), halfSize, radius);
                    dShadow = sdRoundedBox(p + float2(0.0, shOffsetY), halfSize, radius);
                }

                // fwidth(d) is the per-pixel screen-space rate of change of the
                // distance field, i.e. exactly one pixel in field units. Using it
                // as the smoothstep width gives a 1px edge at ANY canvas scale.
                float aa = fwidth(dMain) * 0.6;

                // ---- gradient -------------------------------------------------
                float3 cTL = UnpackRGB(IN.uv2.x);
                float3 cTR = UnpackRGB(IN.uv2.y);
                float3 cBR = UnpackRGB(IN.uv2.z);
                float3 cBL = UnpackRGB(IN.uv2.w);

                float2 n = p / size + 0.5;   // 0..1 across the shape itself
                float3 grad;

                if (gradMode < 0.5)               // 0 = Linear
                {
                    float  a   = radians(_GradientRotation);
                    float2 dir = float2(cos(a), sin(a));
                    float  t   = saturate(dot(n - 0.5, dir) + 0.5);
                    grad = lerp(cBL, cTL, t);
                }
                else if (gradMode < 1.5)          // 1 = Corner (bilinear)
                {
                    float2 s = saturate(n);
                    grad = lerp(lerp(cBL, cBR, s.x), lerp(cTL, cTR, s.x), s.y);
                }
                else                              // 2 = Radial
                {
                    float2 aspect = float2(max(size.x / size.y, 1.0),
                                           max(size.y / size.x, 1.0));
                    float t = saturate(length((n - 0.5) * aspect) * 2.0);
                    grad = lerp(cTL, cBL, t);
                }

                // ---- masks ----------------------------------------------------
                float shapeM = 1.0 - smoothstep(-aa, aa, dMain);
                float fillM  = 1.0 - smoothstep(-aa, aa, dMain + outlineW);
                float bevelM = 1.0 - smoothstep(-aa, aa, dBevel);
                float shadowM= (1.0 - smoothstep(-softness, softness, dShadow)) * shAlpha;

                // ---- body shading ---------------------------------------------
                float3 fill = grad;

                // ---- pattern (tinted in-hue so it reads as one material) -------
                if (patternId > 0.5 && patStr > 0.001)
                {
                    float m = saturate(PatternMask(patternId, saturate(n), size, patScale, patSpeed));
                    fill = lerp(fill, saturate(fill * 1.28 + 0.07), m * patStr);
                }

                // specular-style highlight hugging the top inner surface
                float topT = 1.0 - smoothstep(0.0, _HighlightHeight, 1.0 - saturate(n.y));
                fill += _HighlightStrength * topT * fillM;

                // ambient-occlusion cue along the bottom
                float botT = 1.0 - smoothstep(0.0, 0.30, saturate(n.y));
                fill *= 1.0 - _InnerShadow * botT;

                // ---- animated effects ------------------------------------------
                float ft = _Time.y * fxSpeed + phase * 6.28318531;

                if (effectId > 0.5 && fxStr > 0.001)
                {
                    if (effectId < 1.5)                 // 1 = Shine
                    {
                        // Sweeps, then rests: the pause is what makes it read as
                        // a glint rather than a strobe.
                        float sweep = frac(ft * 0.30) * 1.9 - 0.45;
                        float x     = n.x * 0.72 + (1.0 - n.y) * 0.28;
                        float streak = smoothstep(0.17, 0.0, abs(x - sweep));
                        fill += streak * fxStr * 0.9 * fillM;
                    }
                    else if (effectId < 2.5)            // 2 = Glitter
                    {
                        float2 gp   = n * float2(max(size.x / max(size.y, 1.0), 0.35), 1.0) * 6.0;
                        float2 cell = floor(gp);
                        float2 f    = frac(gp) - 0.5;
                        float  r    = Hash21(cell);
                        float  r2   = Hash21(cell + 17.0);

                        float tw = saturate(sin(ft * 2.2 + r * 6.2831));
                        tw = pow(tw, 14.0);

                        float2 c = f - (float2(r, r2) - 0.5) * 0.55;
                        float core  = saturate(1.0 - (abs(c.x) + abs(c.y)) * 9.0);
                        float crossX = saturate(1.0 - abs(c.x) * 34.0) * saturate(1.0 - abs(c.y) * 5.5);
                        float crossY = saturate(1.0 - abs(c.y) * 34.0) * saturate(1.0 - abs(c.x) * 5.5);
                        float spark = core * core + crossX + crossY;

                        fill += spark * tw * fxStr * 1.15 * fillM;
                    }
                    else if (effectId < 3.5)            // 3 = Pulse
                    {
                        float b = sin(ft * 2.4) * 0.5 + 0.5;
                        fill = lerp(fill, saturate(fill * 1.45 + 0.05), b * fxStr);
                    }
                    else if (effectId < 4.5)            // 4 = Rainbow
                    {
                        fill = lerp(fill, HueShift(fill, frac(ft * 0.18)), fxStr);
                    }
                    else                                // 5 = Rays
                    {
                        float2 d   = n - 0.5;
                        float  ang = atan2(d.y, d.x);
                        float  ray = sin(ang * 12.0 + ft * 1.6) * 0.5 + 0.5;
                        float  fade = saturate(1.0 - length(d) * 1.8);
                        fill += pow(ray, 3.0) * fade * fxStr * 0.55 * fillM;
                    }
                }

                // outline derived from the bottom colour so it is always in-family
                float3 outlineRGB = cBL * (1.0 - _OutlineDarken);
                float3 bodyRGB    = lerp(outlineRGB, fill, fillM);

                // ---- composite back to front, premultiplied --------------------
                float3 outRGB = _ShadowColor.rgb * shadowM;
                float  outA   = shadowM;

                outRGB = outlineRGB * bevelM + outRGB * (1.0 - bevelM);
                outA   = bevelM              + outA   * (1.0 - bevelM);

                outRGB = bodyRGB    * shapeM + outRGB * (1.0 - shapeM);
                outA   = shapeM              + outA   * (1.0 - shapeM);

                // CanvasGroup / Graphic.color alpha
                outRGB *= IN.color.a;
                outA   *= IN.color.a;

                #ifdef UNITY_UI_CLIP_RECT
                float clipMask = UnityGet2DClipping(IN.wpos.xy, _ClipRect);
                outRGB *= clipMask;
                outA   *= clipMask;
                #endif

                clip(outA - 0.001);
                return fixed4(outRGB, outA);
            }
            ENDCG
        }
    }
    Fallback "UI/Default"
}
