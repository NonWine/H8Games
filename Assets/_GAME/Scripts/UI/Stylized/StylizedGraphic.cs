using UnityEngine;
using UnityEngine.UI;

namespace H8.UI
{
    public enum GradientMode { Linear = 0, Corner = 1, Radial = 2 }

    /// <summary>RoundedBox is the classic card shape. Diamond renders the same rect
    /// as a rhombus inscribed edge-to-edge (top/right/bottom/left midpoints), for
    /// gem-style rarity frames — cornerRadius and outlineWidth still apply to it.</summary>
    public enum StylizedShape { RoundedBox = 0, Diamond = 1 }

    /// <summary>
    /// Sprite-free UI shape. Draws a rounded box entirely from a signed distance
    /// field, plus outline, hard bevel and soft ambient shadow.
    ///
    /// Everything that varies per element travels in vertex channels, so a whole
    /// screen of these batches into one draw call as long as they share the
    /// material and no other Graphic breaks the run.
    /// </summary>
    [AddComponentMenu("H8/Stylized Graphic")]
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class StylizedGraphic : MaskableGraphic, ILayoutElement
    {
        [Header("Preset")]
        [Tooltip("Pick a token from the UI kit. Anything but Custom overwrites the " +
                 "four Fill colours and the gradient mode below.")]
        [SerializeField] StylizedPalette palette = StylizedPalette.Custom;
        [SerializeField] StylizedStyle style = StylizedStyle.Juicy;
        [Tooltip("Bevel + shadow recipe. Custom keeps the Depth values below.")]
        [SerializeField] StylizedDepth depth = StylizedDepth.Custom;

        [Header("Shape")]
        [SerializeField] StylizedShape shape = StylizedShape.RoundedBox;
        [SerializeField] float cornerRadius = 28f;
        [SerializeField] float outlineWidth = 5f;

        [Header("Fill")]
        [Tooltip("Style suggests a mode (Diagonal->Corner, Halo->Radial), but " +
                 "picking one here overrides it and sticks.")]
        [SerializeField] GradientMode gradientMode = GradientMode.Linear;

        // Last mode a preset wrote. If gradientMode has drifted from it, the
        // designer changed it by hand and the preset must stop overwriting.
        [SerializeField, HideInInspector] GradientMode presetMode = GradientMode.Linear;
        [SerializeField, HideInInspector] bool hasPresetMode;

        [SerializeField] Color topLeft     = new Color(0.30f, 0.36f, 0.55f);
        [SerializeField] Color topRight    = new Color(0.30f, 0.36f, 0.55f);
        [SerializeField] Color bottomRight = new Color(0.20f, 0.24f, 0.39f);
        [SerializeField] Color bottomLeft  = new Color(0.20f, 0.24f, 0.39f);

        [Header("Depth")]
        [Tooltip("Hard extruded lip below the shape, like box-shadow: 0 Npx 0.")]
        [SerializeField, Range(0f, 24f)] float bevelSize = 5f;
        [SerializeField, Range(0f, 48f)] float shadowOffsetY = 10f;
        [SerializeField, Range(0.5f, 48f)] float shadowSoftness = 12f;
        [SerializeField, Range(0f, 1f)] float shadowAlpha = 0.30f;

        [Header("Animated effect")]
        [SerializeField] StylizedEffect effect = StylizedEffect.None;
        [SerializeField, Range(0f, 1f)] float effectStrength = 0.5f;
        [SerializeField, Range(0f, 6f)] float effectSpeed = 1f;
        [Tooltip("Offsets this element's animation so a grid of cards does not " +
                 "twinkle in lockstep.")]
        [SerializeField] bool desyncFromSiblings = true;

        [Header("Pattern")]
        [SerializeField] StylizedPattern pattern = StylizedPattern.None;
        [Tooltip("Repeats across the shorter side. Ray count when Pattern = Sunburst.")]
        [SerializeField, Range(1f, 40f)] float patternScale = 8f;
        [SerializeField, Range(0f, 1f)] float patternStrength = 0.35f;
        [SerializeField, Range(-2f, 2f)] float patternSpeed = 0.1f;

        // ------------------------------------------------------------------
        // Public API
        // ------------------------------------------------------------------

        public void SetFill(Color top, Color bottom)
        {
            palette = StylizedPalette.Custom;
            topLeft = topRight = top;
            bottomLeft = bottomRight = bottom;
            SetVerticesDirty();
        }

        public void SetFill(Color tl, Color tr, Color br, Color bl)
        {
            palette = StylizedPalette.Custom;
            topLeft = tl; topRight = tr; bottomRight = br; bottomLeft = bl;
            SetVerticesDirty();
        }

        public void SetPalette(StylizedPalette newPalette, StylizedStyle newStyle)
        {
            palette = newPalette;
            style = newStyle;
            ApplyPreset();
            SetVerticesDirty();
        }

        public void SetEffect(StylizedEffect newEffect, float strength = 0.5f, float speed = 1f)
        {
            effect = newEffect;
            effectStrength = Mathf.Clamp01(strength);
            effectSpeed = Mathf.Clamp(speed, 0f, 6f);
            EnableShaderChannels();
            SetVerticesDirty();
        }

        public void SetPattern(StylizedPattern newPattern, float strength = 0.35f, float scale = 8f)
        {
            pattern = newPattern;
            patternStrength = Mathf.Clamp01(strength);
            patternScale = Mathf.Clamp(scale, 1f, 40f);
            EnableShaderChannels();
            SetVerticesDirty();
        }

        public StylizedPalette Palette
        {
            get => palette;
            set { palette = value; ApplyPreset(); SetVerticesDirty(); }
        }

        public StylizedStyle Style
        {
            get => style;
            set { style = value; ApplyPreset(); SetVerticesDirty(); }
        }

        public StylizedDepth Depth
        {
            get => depth;
            set { depth = value; ApplyPreset(); SetVerticesDirty(); }
        }

        public StylizedEffect Effect
        {
            get => effect;
            set { effect = value; EnableShaderChannels(); SetVerticesDirty(); }
        }

        public StylizedPattern Pattern
        {
            get => pattern;
            set { pattern = value; EnableShaderChannels(); SetVerticesDirty(); }
        }

        public StylizedShape Shape
        {
            get => shape;
            set { shape = value; SetVerticesDirty(); }
        }

        public float CornerRadius
        {
            get => cornerRadius;
            set { cornerRadius = value; SetVerticesDirty(); }
        }

        public float OutlineWidth
        {
            get => outlineWidth;
            set { outlineWidth = value; SetVerticesDirty(); }
        }

        public GradientMode Mode
        {
            get => gradientMode;
            set { gradientMode = value; SetVerticesDirty(); }
        }

        /// <summary>Base colour of the current token — use it to tint labels and icons in-family.</summary>
        public Color PaletteBase => palette == StylizedPalette.Custom
            ? bottomLeft
            : StylizedPalettes.Base(palette);

        // ------------------------------------------------------------------
        // Presets
        // ------------------------------------------------------------------

        /// <summary>
        /// Presets write into the serialized fields rather than being resolved at
        /// draw time, so the Inspector always shows the colours that will ship and
        /// a designer can switch to Custom and keep tweaking from there.
        /// </summary>
        void ApplyPreset()
        {
            if (palette != StylizedPalette.Custom)
            {
                StylizedPalettes.Resolve(palette, style,
                    out topLeft, out topRight, out bottomRight, out bottomLeft,
                    out GradientMode resolvedMode);

                // Diagonal and Halo ARE their gradient mode — Diagonal ignores the
                // TR/BL corners under Linear and Halo needs Radial — so those two
                // win. The rest resolve to Linear, where the mode is genuinely
                // free and a hand-picked one is kept.
                bool styleDictatesMode = resolvedMode != GradientMode.Linear;

                if (styleDictatesMode || !hasPresetMode || gradientMode == presetMode)
                    gradientMode = resolvedMode;

                presetMode = resolvedMode;
                hasPresetMode = true;
            }

            if (StylizedPalettes.TryGetDepth(depth, out Vector4 d))
            {
                bevelSize      = d.x;
                shadowOffsetY  = d.y;
                shadowSoftness = Mathf.Max(d.z, 0.5f);
                shadowAlpha    = d.w;
            }
        }

        [ContextMenu("Bake Preset To Custom")]
        void BakePresetToCustom()
        {
            ApplyPreset();
            palette = StylizedPalette.Custom;
            depth = StylizedDepth.Custom;
            hasPresetMode = false;
            SetVerticesDirty();
        }

        /// <summary>Drops a hand-picked gradient mode and hands control back to Style.</summary>
        [ContextMenu("Reset Gradient Mode To Style")]
        void ResetGradientModeToStyle()
        {
            hasPresetMode = false;
            ApplyPreset();
            SetVerticesDirty();
        }

        // ------------------------------------------------------------------
        // Mesh
        // ------------------------------------------------------------------

        protected override void OnEnable()
        {
            ApplyPreset();
            base.OnEnable();
            EnableShaderChannels();
        }

        void EnableShaderChannels()
        {
            Canvas c = canvas;
            if (c == null) return;

            // Without this UGUI silently strips uv1..uv3 and every element
            // renders with zeroed size, i.e. nothing at all.
            c.additionalShaderChannels |=
                AdditionalCanvasShaderChannels.TexCoord1 |
                AdditionalCanvasShaderChannels.TexCoord2 |
                AdditionalCanvasShaderChannels.TexCoord3;

            // Normal + Tangent grow EVERY vertex on this canvas, Text and Image
            // included, so they are only requested once something actually
            // animates. With the streams absent the shader reads normal.x as 0,
            // which decodes to effect None / pattern None anyway.
            if (effect != StylizedEffect.None || pattern != StylizedPattern.None)
            {
                c.additionalShaderChannels |=
                    AdditionalCanvasShaderChannels.Normal |
                    AdditionalCanvasShaderChannels.Tangent;
            }
        }

        protected override void OnCanvasHierarchyChanged()
        {
            base.OnCanvasHierarchyChanged();
            EnableShaderChannels();
        }

        /// <summary>
        /// The shadow and bevel spill outside the RectTransform, so the quad has
        /// to be grown or they get clipped by the mesh itself. The shader is told
        /// the padding amount and maps UVs back onto the true rect.
        /// </summary>
        float Padding()
        {
            float shadowReach = Mathf.Abs(shadowOffsetY) + shadowSoftness;
            return Mathf.Max(shadowReach, bevelSize) + 2f;
        }

        /// <summary>
        /// Stable per-element phase in 0..1 so neighbouring cards sparkle out of
        /// step. Instance ids are not stable across sessions, which is fine — the
        /// only requirement is that siblings differ.
        /// </summary>
        float Phase()
        {
            if (!desyncFromSiblings) return 0f;
            uint h = (uint)GetInstanceID() * 2654435761u;
            return (h >> 8 & 0xFFFF) / 65535f;
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            Rect r = rectTransform.rect;
            float w = r.width, h = r.height;
            if (w <= 0f || h <= 0f) return;

            float pad = Padding();

            // Corner radii above half the shorter side make the SDF corners
            // overlap and the shape degenerates, so clamp hard.
            float radius = Mathf.Min(cornerRadius, Mathf.Min(w, h) * 0.5f);
            float outline = Mathf.Min(outlineWidth, Mathf.Min(w, h) * 0.5f);

            var shapeData = new Vector4(w, h, radius, outline);
            var cols = new Vector4(
                PackRGB(topLeft), PackRGB(topRight),
                PackRGB(bottomRight), PackRGB(bottomLeft));
            var depthV = new Vector4(bevelSize, shadowOffsetY, shadowSoftness, shadowAlpha);

            // Both enum ids ride in one float because NORMAL only has three
            // components and uv0..uv3 were already full. See the shader header.
            float packedIds = (int)effect + 16 * (int)pattern;
            var fx = new Vector3(packedIds, effectStrength, effectSpeed);
            var pat = new Vector4(patternScale, patternStrength, patternSpeed, Phase());

            float xMin = r.xMin - pad, xMax = r.xMax + pad;
            float yMin = r.yMin - pad, yMax = r.yMax + pad;

            // gradientMode only ever holds 0..2, so the shape id rides in the same
            // float one decade up — same packing trick uv0.w's sibling channels use
            // for effect+pattern. The shader unpacks both with floor/mod.
            float mode = (float)gradientMode + 10f * (float)shape;

            AddVert(vh, xMin, yMin, 0f, 0f, pad, mode, shapeData, cols, depthV, fx, pat);
            AddVert(vh, xMin, yMax, 0f, 1f, pad, mode, shapeData, cols, depthV, fx, pat);
            AddVert(vh, xMax, yMax, 1f, 1f, pad, mode, shapeData, cols, depthV, fx, pat);
            AddVert(vh, xMax, yMin, 1f, 0f, pad, mode, shapeData, cols, depthV, fx, pat);

            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(2, 3, 0);
        }

        void AddVert(VertexHelper vh, float x, float y, float u, float v,
                     float pad, float mode, Vector4 shape, Vector4 cols, Vector4 depthV,
                     Vector3 fx, Vector4 pat)
        {
            var vert = UIVertex.simpleVert;
            vert.position = new Vector3(x, y, 0f);
            vert.color = color;
            vert.uv0 = new Vector4(u, v, pad, mode);
            vert.uv1 = shape;
            vert.uv2 = cols;
            vert.uv3 = depthV;
            vert.normal = fx;
            vert.tangent = pat;
            vh.AddVert(vert);
        }

        /// <summary>
        /// RGB888 -> one float. 255*65536 + 255*256 + 255 = 2^24 - 1, which is
        /// exactly the float32 mantissa width, so the round trip is lossless.
        /// Alpha is deliberately excluded — it would push the value past 2^24
        /// and start quantising the red channel.
        /// </summary>
        static float PackRGB(Color c)
        {
            int r = Mathf.RoundToInt(Mathf.Clamp01(c.r) * 255f);
            int g = Mathf.RoundToInt(Mathf.Clamp01(c.g) * 255f);
            int b = Mathf.RoundToInt(Mathf.Clamp01(c.b) * 255f);
            return r * 65536f + g * 256f + b;
        }

        // No texture: the shape is generated, so the default white texture is fine.
        public override Texture mainTexture => s_WhiteTexture;

        // ------------------------------------------------------------------
        // ILayoutElement — lets the shadow reserve space in layout groups
        // ------------------------------------------------------------------

        public void CalculateLayoutInputHorizontal() { }
        public void CalculateLayoutInputVertical() { }
        public float minWidth => -1f;
        public float preferredWidth => -1f;
        public float flexibleWidth => -1f;
        public float minHeight => -1f;
        public float preferredHeight => -1f;
        public float flexibleHeight => -1f;
        public int layoutPriority => 0;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            cornerRadius = Mathf.Max(0f, cornerRadius);
            outlineWidth = Mathf.Max(0f, outlineWidth);
            ApplyPreset();
            EnableShaderChannels();
            SetVerticesDirty();
        }
#endif
    }
}
