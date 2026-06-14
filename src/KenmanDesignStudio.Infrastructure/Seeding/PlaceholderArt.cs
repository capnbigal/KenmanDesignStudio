using System.Text;
using KenmanDesignStudio.Core.Common;
using KenmanDesignStudio.Core.Enums;

namespace KenmanDesignStudio.Infrastructure.Seeding;

/// <summary>
/// Generates refined, on-brand SVG "plates" for projects so the gallery looks gorgeous offline.
/// Each plate is an abstract architectural composition tinted to its category, captioned with the
/// project name + location. Real photographs dropped into the same folder will take precedence
/// (the seeder writes SVGs; the UI prefers a real image file when one exists at a sibling path).
/// </summary>
public static class PlaceholderArt
{
    private const int W = 1280;
    private const int H = 960;

    /// <summary>Deterministic SVG for a single plate. <paramref name="variant"/> shifts the
    /// composition so multiple images of one project look distinct.</summary>
    public static string Build(ProjectCategory category, string projectName, string location,
        string codeName, int variant)
    {
        var meta = CategoryCatalog.Get(category);
        var rng = new DeterministicRandom(HashSeed(projectName, variant));

        // Per-variant tonal shift so a project's gallery reads as a set, not duplicates.
        var from = Shade(meta.GradientFrom, -8 + variant * 6);
        var to = Shade(meta.GradientTo, variant * 5);
        var gold = meta.Accent;

        var sb = new StringBuilder();
        sb.Append($"<svg xmlns='http://www.w3.org/2000/svg' width='{W}' height='{H}' viewBox='0 0 {W} {H}' role='img'>");

        // Defs: vertical brand gradient, gold sheen, soft vignette, film grain.
        sb.Append("<defs>");
        sb.Append($"<linearGradient id='bg' x1='0' y1='0' x2='0' y2='1'>" +
                  $"<stop offset='0' stop-color='{Shade(from, 18)}'/>" +
                  $"<stop offset='0.55' stop-color='{from}'/>" +
                  $"<stop offset='1' stop-color='{Shade(to, -14)}'/></linearGradient>");
        sb.Append($"<linearGradient id='gold' x1='0' y1='0' x2='1' y2='1'>" +
                  $"<stop offset='0' stop-color='{Shade(gold, 30)}'/>" +
                  $"<stop offset='1' stop-color='{Shade(gold, -20)}'/></linearGradient>");
        sb.Append("<radialGradient id='vig' cx='0.5' cy='0.42' r='0.75'>" +
                  "<stop offset='0.55' stop-color='#000000' stop-opacity='0'/>" +
                  "<stop offset='1' stop-color='#000000' stop-opacity='0.45'/></radialGradient>");
        sb.Append("<filter id='grain'><feTurbulence type='fractalNoise' baseFrequency='0.9' numOctaves='2' stitchTiles='stitch'/>" +
                  "<feColorMatrix type='saturate' values='0'/>" +
                  "<feComponentTransfer><feFuncA type='linear' slope='0.05'/></feComponentTransfer>" +
                  "<feComposite operator='over' in2='SourceGraphic'/></filter>");
        sb.Append("</defs>");

        // Base wash.
        sb.Append($"<rect width='{W}' height='{H}' fill='url(#bg)'/>");

        // Faint topographic contour lines for depth.
        AppendContours(sb, rng, gold);

        // Category-specific abstract motif.
        AppendMotif(sb, meta.Motif, rng, gold, to);

        // Layered translucent terrain bands across the lower third.
        AppendTerrain(sb, rng, from, to, gold);

        // Vignette + grain for a photographic feel.
        sb.Append($"<rect width='{W}' height='{H}' fill='url(#vig)'/>");
        sb.Append($"<rect width='{W}' height='{H}' filter='url(#grain)' opacity='0.6'/>");

        // Thin inset gold frame.
        sb.Append($"<rect x='28' y='28' width='{W - 56}' height='{H - 56}' fill='none' " +
                  $"stroke='url(#gold)' stroke-opacity='0.6' stroke-width='2'/>");

        // Typographic plate.
        AppendType(sb, meta, projectName, location, codeName, gold);

        sb.Append("</svg>");
        return sb.ToString();
    }

    private static void AppendContours(StringBuilder sb, DeterministicRandom rng, string gold)
    {
        sb.Append($"<g stroke='{gold}' stroke-opacity='0.06' fill='none' stroke-width='1.5'>");
        for (var i = 0; i < 7; i++)
        {
            var y = 120 + i * 110 + rng.Next(-30, 30);
            var c1 = rng.Next(120, 360);
            var c2 = rng.Next(120, 360);
            sb.Append($"<path d='M -40 {y} C {W * 0.3:0} {y - c1}, {W * 0.7:0} {y + c2}, {W + 40} {y - 40}'/>");
        }
        sb.Append("</g>");
    }

    private static void AppendTerrain(StringBuilder sb, DeterministicRandom rng, string from, string to, string gold)
    {
        // Three overlapping ridgelines, back-to-front, lightening toward the gold horizon.
        var baseY = H * 0.62;
        string[] fills = { Shade(to, -22), Shade(to, -6), Shade(from, -28) };
        var op = new[] { 0.55, 0.7, 0.92 };
        for (var layer = 0; layer < 3; layer++)
        {
            var y = baseY + layer * 70;
            var sb2 = new StringBuilder();
            sb2.Append($"<path d='M 0 {y:0}");
            var x = 0;
            var yy = y;
            while (x <= W)
            {
                x += rng.Next(140, 230);
                yy = y + rng.Next(-46, 46) - layer * 6;
                sb2.Append($" L {x} {yy:0}");
            }
            sb2.Append($" L {W} {H} L 0 {H} Z'");
            sb.Append($"{sb2} fill='{fills[layer]}' opacity='{op[layer].ToString(System.Globalization.CultureInfo.InvariantCulture)}'/>");
        }
        // Gold horizon line behind the ridges.
        sb.Append($"<line x1='0' y1='{baseY - 8:0}' x2='{W}' y2='{baseY - 22:0}' stroke='{gold}' stroke-opacity='0.5' stroke-width='2'/>");
    }

    private static void AppendMotif(StringBuilder sb, string motif, DeterministicRandom rng, string gold, string deep)
    {
        switch (motif)
        {
            case "skyline":
                sb.Append("<g opacity='0.5'>");
                var bx = 720;
                while (bx < W - 80)
                {
                    var bw = rng.Next(48, 96);
                    var bh = rng.Next(160, 460);
                    sb.Append($"<rect x='{bx}' y='{H * 0.58 - bh:0}' width='{bw}' height='{bh}' fill='{Shade(deep, -30)}' opacity='0.55'/>");
                    bx += bw + rng.Next(14, 30);
                }
                sb.Append("</g>");
                break;
            case "runway":
                sb.Append($"<g stroke='{gold}' stroke-opacity='0.25' stroke-width='2'>");
                sb.Append($"<path d='M {W * 0.5:0} {H * 0.2:0} L {W * 0.2:0} {H * 0.62:0}' fill='none'/>");
                sb.Append($"<path d='M {W * 0.5:0} {H * 0.2:0} L {W * 0.8:0} {H * 0.62:0}' fill='none'/>");
                for (var i = 1; i < 7; i++)
                {
                    var t = i / 7.0;
                    var y = H * 0.2 + t * (H * 0.42);
                    var half = 6 + t * 70;
                    sb.Append($"<line x1='{W * 0.5 - half:0}' y1='{y:0}' x2='{W * 0.5 + half:0}' y2='{y:0}' stroke-opacity='0.4'/>");
                }
                sb.Append("</g>");
                break;
            case "palms":
                sb.Append("<g opacity='0.45'>");
                for (var i = 0; i < 3; i++)
                {
                    var px = 180 + i * 150 + rng.Next(-30, 30);
                    var py = H * 0.6;
                    sb.Append($"<path d='M {px} {py:0} q -8 -160 -2 -240' stroke='{Shade(gold, -30)}' stroke-width='4' fill='none'/>");
                    for (var f = 0; f < 7; f++)
                    {
                        var ang = -160 + f * 28;
                        var ex = px - 2 + Math.Cos(ang * Math.PI / 180) * 120;
                        var ey = py - 238 + Math.Sin(ang * Math.PI / 180) * 120;
                        sb.Append($"<path d='M {px - 2} {py - 238:0} Q {(px + ex) / 2:0} {(py - 238 + ey) / 2 - 18:0} {ex:0} {ey:0}' stroke='{gold}' stroke-opacity='0.5' stroke-width='2.5' fill='none'/>");
                    }
                }
                sb.Append("</g>");
                break;
            case "parterre":
                sb.Append($"<g stroke='{gold}' stroke-opacity='0.22' fill='none' stroke-width='2'>");
                var cx = W * 0.5; var cyy = H * 0.4;
                for (var r = 60; r <= 260; r += 50)
                    sb.Append($"<circle cx='{cx:0}' cy='{cyy:0}' r='{r}'/>");
                sb.Append($"<line x1='{cx:0}' y1='{cyy - 280:0}' x2='{cx:0}' y2='{cyy + 280:0}'/>");
                sb.Append($"<line x1='{cx - 280:0}' y1='{cyy:0}' x2='{cx + 280:0}' y2='{cyy:0}'/>");
                sb.Append("</g>");
                break;
            case "fairway":
                sb.Append($"<g fill='none' stroke='{gold}' stroke-opacity='0.2' stroke-width='2'>");
                for (var i = 0; i < 4; i++)
                {
                    var y = H * 0.3 + i * 60;
                    sb.Append($"<path d='M -20 {y:0} C {W * 0.35:0} {y - 70}, {W * 0.65:0} {y + 70}, {W + 20} {y - 10}'/>");
                }
                sb.Append($"<line x1='{W * 0.74:0}' y1='{H * 0.3:0}' x2='{W * 0.74:0}' y2='{H * 0.48:0}' stroke-opacity='0.5'/>");
                sb.Append($"<path d='M {W * 0.74:0} {H * 0.3:0} l 46 12 l -46 12 z' fill='{gold}' fill-opacity='0.45' stroke='none'/>");
                sb.Append("</g>");
                break;
            default: // plaza
                sb.Append($"<g stroke='{gold}' stroke-opacity='0.16' fill='none' stroke-width='1.5'>");
                for (var gx = 760; gx < W - 60; gx += 70)
                    sb.Append($"<line x1='{gx}' y1='{H * 0.18:0}' x2='{gx}' y2='{H * 0.58:0}'/>");
                for (var gy = (int)(H * 0.18); gy < H * 0.58; gy += 70)
                    sb.Append($"<line x1='760' y1='{gy}' x2='{W - 60}' y2='{gy}'/>");
                sb.Append("</g>");
                break;
        }
    }

    private static void AppendType(StringBuilder sb, CategoryMeta meta, string projectName, string location,
        string codeName, string gold)
    {
        var font = "'Georgia','Times New Roman',serif";
        var sans = "'Helvetica Neue',Arial,sans-serif";

        // Category eyebrow.
        sb.Append($"<text x='72' y='118' font-family='{sans}' font-size='22' letter-spacing='6' " +
                  $"fill='{gold}' fill-opacity='0.85'>{Escape(meta.ShortName.ToUpperInvariant())}</text>");
        sb.Append($"<line x1='72' y1='134' x2='168' y2='134' stroke='{gold}' stroke-width='2'/>");

        // Project name (wrapped to two lines if long).
        var lines = WrapText(projectName.ToUpperInvariant(), 18);
        var ty = H - 230;
        foreach (var line in lines.Take(2))
        {
            sb.Append($"<text x='72' y='{ty}' font-family='{font}' font-size='62' font-weight='600' " +
                      $"fill='#F4F1EA' letter-spacing='1'>{Escape(line)}</text>");
            ty += 70;
        }

        sb.Append($"<text x='72' y='{H - 140}' font-family='{sans}' font-size='26' letter-spacing='2' " +
                  $"fill='#D9D5CC'>{Escape(location)}</text>");

        // Footer: wordmark + code.
        sb.Append($"<text x='72' y='{H - 60}' font-family='{font}' font-size='24' letter-spacing='8' " +
                  $"fill='{gold}'>KENMAN DESIGN STUDIO</text>");
        sb.Append($"<text x='{W - 72}' y='{H - 60}' text-anchor='end' font-family='{sans}' font-size='20' " +
                  $"letter-spacing='3' fill='#B9C0B4' fill-opacity='0.8'>{Escape(codeName)}</text>");
    }

    // ---- helpers ----

    private static IEnumerable<string> WrapText(string text, int maxChars)
    {
        var words = text.Split(' ');
        var line = new StringBuilder();
        foreach (var w in words)
        {
            if (line.Length > 0 && line.Length + w.Length + 1 > maxChars)
            {
                yield return line.ToString();
                line.Clear();
            }
            if (line.Length > 0) line.Append(' ');
            line.Append(w);
        }
        if (line.Length > 0) yield return line.ToString();
    }

    private static int HashSeed(string s, int variant)
    {
        unchecked
        {
            var h = 17;
            foreach (var c in s) h = h * 31 + c;
            return h * 31 + variant;
        }
    }

    private static string Escape(string s) =>
        s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

    /// <summary>Lighten (positive) or darken (negative) a #rrggbb hex colour by a percentage.</summary>
    private static string Shade(string hex, int percent)
    {
        hex = hex.TrimStart('#');
        var r = Convert.ToInt32(hex.Substring(0, 2), 16);
        var g = Convert.ToInt32(hex.Substring(2, 2), 16);
        var b = Convert.ToInt32(hex.Substring(4, 2), 16);
        r = Clamp(r + (255 * percent / 100));
        g = Clamp(g + (255 * percent / 100));
        b = Clamp(b + (255 * percent / 100));
        return $"#{r:X2}{g:X2}{b:X2}";
    }

    private static int Clamp(int v) => v < 0 ? 0 : v > 255 ? 255 : v;

    /// <summary>Tiny deterministic LCG so generated art is identical across re-seeds.</summary>
    private sealed class DeterministicRandom
    {
        private uint _state;
        public DeterministicRandom(int seed) => _state = (uint)(seed == 0 ? 1 : seed);
        public int Next(int minInclusive, int maxExclusive)
        {
            _state = _state * 1664525 + 1013904223;
            var range = (uint)(maxExclusive - minInclusive);
            return minInclusive + (int)(_state % range);
        }
    }
}
