using System.Collections.Generic;
using UnityEngine;

namespace H8.UI
{
    /// <summary>
    /// Every colour token from the H8 UI kit. Explicit numbering: the
    /// values are serialized in scenes and prefabs, so inserting a token must
    /// never renumber the ones after it.
    /// </summary>
    public enum StylizedPalette
    {
        Custom = 0,

        GoGreen  = 10,
        SkyBlue  = 11,
        Grape    = 12,
        Coral    = 13,
        CoralHot = 14,
        // One rung darker than SkyBlue on the same hue — for a nav/utility
        // button that has to read as a separate pressable object next to
        // SkyBlue chrome (header bars, icon tiles) in the same popup, without
        // borrowing GoGreen's "confirm" or Coral's "dismiss/forbid" meaning.
        SkyBlueDeep = 15,

        CoinGold = 20,
        Gem      = 21,

        SkyBG      = 30,
        Grass      = 31,
        Panel      = 32,
        PanelWhite = 33,
        Ink        = 34,
        Muted      = 35,
        InkOutline = 36,
        Locked     = 37,

        Surface900 = 40,
        Surface800 = 41,
        Surface700 = 42,
        Surface600 = 43,
        Surface500 = 44,

        RarityCommon    = 50,
        RarityRare      = 51,
        RarityEpic      = 52,
        RarityLegendary = 53,
    }

    /// <summary>How a palette token is turned into the four corner colours.</summary>
    public enum StylizedStyle
    {
        Flat     = 0,
        Juicy    = 1,
        Glossy   = 2,
        Deep     = 3,
        Pressed  = 4,
        Diagonal = 5,
        Halo     = 6,
    }

    /// <summary>Bevel + drop shadow recipes lifted from the kit's box-shadows.</summary>
    public enum StylizedDepth
    {
        Custom      = 0,
        None        = 1,
        Card        = 2,
        Popup       = 3,
        ButtonSmall = 4,
        Button      = 5,
        ButtonBig   = 6,
        IconRound   = 7,
        Pressed     = 8,
    }

    /// <summary>Animated overlay. Costs nothing on the CPU — driven by _Time.</summary>
    public enum StylizedEffect
    {
        None    = 0,
        Shine   = 1,
        Glitter = 2,
        Pulse   = 3,
        Rainbow = 4,
        Rays    = 5,
    }

    /// <summary>Repeating in-hue motif drawn under the highlight.</summary>
    public enum StylizedPattern
    {
        None     = 0,
        Stripes  = 1,
        Dots     = 2,
        Checker  = 3,
        Chevron  = 4,
        Grid     = 5,
        Sunburst = 6,
        Diamonds = 7,
    }

    public readonly struct StylizedSwatch
    {
        public readonly Color Light;
        public readonly Color Base;
        public readonly Color Dark;

        public StylizedSwatch(Color light, Color mid, Color dark)
        {
            Light = light;
            Base  = mid;
            Dark  = dark;
        }
    }

    public static class StylizedPalettes
    {
        // light / base / dark. "dark" is the extruded lip colour from the kit's
        // `0 Npx 0 <dark>` box-shadow, not an arbitrary multiply of base.
        static readonly Dictionary<StylizedPalette, StylizedSwatch> Table = new()
        {
            { StylizedPalette.GoGreen,  Swatch("#63D879", "#33B14C", "#2A8F3E") },
            { StylizedPalette.SkyBlue,  Swatch("#4CC9F7", "#1EA3E4", "#1786C4") },
            { StylizedPalette.Grape,    Swatch("#B292F5", "#8A63E6", "#6E49C6") },
            { StylizedPalette.Coral,    Swatch("#FF7B7B", "#F0504F", "#CE3B3B") },
            // Punchier variant: wider light/dark spread + more saturated base,
            // for cases where the standard Coral reads too soft against white (e.g. error/warning popups).
            { StylizedPalette.CoralHot, Swatch("#FF6152", "#E42A20", "#970F0F") },
            // Continues the SkyBlue ramp one rung down: Light/Base here equal
            // SkyBlue's Base/Dark, so a Flat or Glossy fill lands visibly
            // deeper than anything SkyBlue can produce, while staying the
            // same hue. Use for a button that must sit next to SkyBlue chrome
            // (e.g. Leave, in a Settings popup) and still read as "navigation",
            // just elevated above the bar rather than painted flat into it.
            { StylizedPalette.SkyBlueDeep, Swatch("#1EA3E4", "#1786C4", "#124F72") },

            { StylizedPalette.CoinGold, Swatch("#FFD54D", "#F6B321", "#D9971A") },
            { StylizedPalette.Gem,      Swatch("#FF7FD4", "#F042B6", "#C42A91") },

            { StylizedPalette.SkyBG,      Swatch("#CFF3FF", "#8FE0FF", "#3FC8F4") },
            { StylizedPalette.Grass,      Swatch("#8FE06A", "#7ED957", "#5CB33E") },
            { StylizedPalette.Panel,      Swatch("#F5F7FD", "#E9ECF9", "#D7DCEC") },
            { StylizedPalette.PanelWhite, Swatch("#FFFFFF", "#EEF1FA", "#D7DCEC") },
            { StylizedPalette.Ink,        Swatch("#4C5D85", "#3C4A6E", "#313D5E") },
            { StylizedPalette.Muted,      Swatch("#6E7BA5", "#5A6892", "#404E76") },
            { StylizedPalette.InkOutline, Swatch("#323C58", "#232B44", "#151A2B") },
            { StylizedPalette.Locked,     Swatch("#F0F2FA", "#E4E8F4", "#D7DCEC") },

            // Surface ramp: each step's light/dark are its ramp neighbours, so a
            // Juicy gradient on Surface700 lands exactly between 600 and 800.
            { StylizedPalette.Surface900, Swatch("#242C42", "#181F2F", "#0F1420") },
            { StylizedPalette.Surface800, Swatch("#323C58", "#242C42", "#181F2F") },
            { StylizedPalette.Surface700, Swatch("#3C4A6D", "#323C58", "#242C42") },
            { StylizedPalette.Surface600, Swatch("#4C5D85", "#3C4A6D", "#323C58") },
            { StylizedPalette.Surface500, Swatch("#5A6892", "#4C5D85", "#3C4A6D") },

            { StylizedPalette.RarityCommon,    Swatch("#AEB9D4", "#93A0BE", "#7C88AC") },
            { StylizedPalette.RarityRare,      Swatch("#5CD3F8", "#1EA3E4", "#1786C4") },
            { StylizedPalette.RarityEpic,      Swatch("#B292F5", "#8A63E6", "#6E49C6") },
            { StylizedPalette.RarityLegendary, Swatch("#FFD54D", "#F6B321", "#D9971A") },
        };

        public static StylizedSwatch Get(StylizedPalette palette) =>
            Table.TryGetValue(palette, out StylizedSwatch s) ? s : Table[StylizedPalette.Panel];

        public static Color Light(StylizedPalette palette) => Get(palette).Light;
        public static Color Base(StylizedPalette palette)  => Get(palette).Base;
        public static Color Dark(StylizedPalette palette)  => Get(palette).Dark;

        /// <summary>Ink stroke to put under white label text — see the kit's AA note.</summary>
        public static Color InkOutlineColor => Base(StylizedPalette.InkOutline);

        public static StylizedPalette RarityOf(int rarityIndex) => rarityIndex switch
        {
            <= 0 => StylizedPalette.RarityCommon,
            1    => StylizedPalette.RarityRare,
            2    => StylizedPalette.RarityEpic,
            _    => StylizedPalette.RarityLegendary,
        };

        public static void Resolve(StylizedPalette palette, StylizedStyle style,
                                   out Color topLeft, out Color topRight,
                                   out Color bottomRight, out Color bottomLeft,
                                   out GradientMode mode)
        {
            StylizedSwatch s = Get(palette);
            mode = GradientMode.Linear;

            Color top, bottom;
            switch (style)
            {
                case StylizedStyle.Flat:
                    top = s.Base; bottom = s.Base;
                    break;
                case StylizedStyle.Glossy:
                    top = Lift(s.Light, 0.30f); bottom = s.Base;
                    break;
                case StylizedStyle.Deep:
                    top = s.Base; bottom = s.Dark;
                    break;
                case StylizedStyle.Pressed:
                    top = s.Dark; bottom = s.Base;
                    break;

                case StylizedStyle.Diagonal:
                    mode = GradientMode.Corner;
                    topLeft = s.Light;
                    bottomRight = s.Base;
                    topRight = bottomLeft = Color.Lerp(s.Light, s.Base, 0.5f);
                    return;

                case StylizedStyle.Halo:
                    mode = GradientMode.Radial;
                    topLeft = topRight = s.Light;
                    bottomLeft = bottomRight = s.Dark;
                    return;

                default:
                    top = s.Light; bottom = s.Base;
                    break;
            }

            topLeft = topRight = top;
            bottomLeft = bottomRight = bottom;
        }

        /// <summary>(bevel, shadowOffsetY, shadowSoftness, shadowAlpha)</summary>
        public static bool TryGetDepth(StylizedDepth depth, out Vector4 values)
        {
            switch (depth)
            {
                case StylizedDepth.None:        values = new Vector4(0f,  0f,  0.5f, 0f);    return true;
                case StylizedDepth.Card:        values = new Vector4(0f,  14f, 30f,  0.12f); return true;
                case StylizedDepth.Popup:       values = new Vector4(0f,  26f, 48f,  0.30f); return true;
                case StylizedDepth.ButtonSmall: values = new Vector4(6f,  10f, 16f,  0.35f); return true;
                case StylizedDepth.Button:      values = new Vector4(8f,  14f, 20f,  0.40f); return true;
                case StylizedDepth.ButtonBig:   values = new Vector4(10f, 18f, 26f,  0.45f); return true;
                case StylizedDepth.IconRound:   values = new Vector4(6f,  10f, 14f,  0.35f); return true;
                case StylizedDepth.Pressed:     values = new Vector4(2f,  4f,  8f,   0.40f); return true;
                default:                        values = default;                            return false;
            }
        }

        static Color Lift(Color c, float t) => Color.Lerp(c, Color.white, t);

        static StylizedSwatch Swatch(string light, string mid, string dark) =>
            new(Hex(light), Hex(mid), Hex(dark));

        static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out Color c);
            return c;
        }
    }
}
