using MudBlazor;

namespace KenmanDesignStudio.Web.Services;

/// <summary>The Kenman Design Studio brand theme — deep botanical greens, charcoal/stone neutrals,
/// and a brass-gold accent. Pairs an elegant serif display face with a refined geometric sans.</summary>
public static class BrandTheme
{
    private static readonly string[] Sans = { "Jost", "Helvetica Neue", "Arial", "sans-serif" };
    private static readonly string[] Serif = { "Cormorant Garamond", "Georgia", "serif" };

    // Brand constants reused by inline styles / charts.
    public const string Emerald = "#2F5D50";
    public const string EmeraldDeep = "#16302B";
    public const string Brass = "#C8A95B";
    public const string BrassDeep = "#A8842F";
    public const string Stone = "#F6F4EF";
    public const string Charcoal = "#10171A";

    public static readonly MudTheme Theme = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = Emerald,
            PrimaryContrastText = "#F6F4EF",
            Secondary = BrassDeep,
            SecondaryContrastText = "#FFFFFF",
            Tertiary = "#3E7A3E",
            Background = Stone,
            BackgroundGray = "#EDEAE2",
            Surface = "#FFFFFF",
            AppbarBackground = EmeraldDeep,
            AppbarText = "#F2EFE7",
            DrawerBackground = "#FBFAF6",
            DrawerText = "#2A332F",
            DrawerIcon = "#5A6B62",
            TextPrimary = "#1E2A26",
            TextSecondary = "#5E6B64",
            ActionDefault = "#5E6B64",
            Success = "#3E8F63",
            Info = "#3C6E8F",
            Warning = "#C08A2E",
            Error = "#B23A3A",
            Dark = EmeraldDeep,
            Divider = "#E2DED4",
            LinesDefault = "#E2DED4",
            TableLines = "#ECE8DF",
            GrayLight = "#EDEAE2",
            GrayLighter = "#F4F1EA",
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#5FB89A",
            PrimaryContrastText = "#08110E",
            Secondary = Brass,
            SecondaryContrastText = "#1A1407",
            Tertiary = "#79C28F",
            Background = Charcoal,
            BackgroundGray = "#0B1114",
            Surface = "#161E22",
            AppbarBackground = "#0C1417",
            AppbarText = "#ECE9E1",
            DrawerBackground = "#0E1619",
            DrawerText = "#C6CFC9",
            DrawerIcon = "#8FA096",
            TextPrimary = "#ECE9E1",
            TextSecondary = "#9DA8A1",
            ActionDefault = "#9DA8A1",
            Success = "#5FB89A",
            Info = "#6FA8C8",
            Warning = "#D8B66A",
            Error = "#E07A7A",
            Dark = "#06100D",
            Divider = "#243036",
            LinesDefault = "#243036",
            TableLines = "#1E282D",
        },
        Typography = new Typography
        {
            Default = new DefaultTypography
            {
                FontFamily = Sans,
                FontSize = "0.9rem",
                FontWeight = "300",
                LineHeight = "1.55",
                LetterSpacing = "normal",
            },
            H1 = new H1Typography { FontFamily = Serif, FontWeight = "600", FontSize = "3.4rem", LineHeight = "1.05", LetterSpacing = "-.5px" },
            H2 = new H2Typography { FontFamily = Serif, FontWeight = "600", FontSize = "2.6rem", LineHeight = "1.1", LetterSpacing = "-.5px" },
            H3 = new H3Typography { FontFamily = Serif, FontWeight = "600", FontSize = "2.0rem", LineHeight = "1.15" },
            H4 = new H4Typography { FontFamily = Serif, FontWeight = "600", FontSize = "1.6rem", LineHeight = "1.2" },
            H5 = new H5Typography { FontFamily = Sans, FontWeight = "500", FontSize = "1.25rem" },
            H6 = new H6Typography { FontFamily = Sans, FontWeight = "500", FontSize = "1.05rem", LetterSpacing = ".3px" },
            Subtitle1 = new Subtitle1Typography { FontFamily = Sans, FontWeight = "400", FontSize = "1rem" },
            Subtitle2 = new Subtitle2Typography { FontFamily = Sans, FontWeight = "500", FontSize = ".85rem", LetterSpacing = ".5px" },
            Body1 = new Body1Typography { FontFamily = Sans, FontWeight = "300", FontSize = ".95rem", LineHeight = "1.6" },
            Body2 = new Body2Typography { FontFamily = Sans, FontWeight = "300", FontSize = ".85rem", LineHeight = "1.55" },
            Button = new ButtonTypography { FontFamily = Sans, FontWeight = "500", FontSize = ".82rem", LetterSpacing = "1.2px", TextTransform = "uppercase" },
            Caption = new CaptionTypography { FontFamily = Sans, FontWeight = "300", FontSize = ".75rem", LetterSpacing = ".3px" },
            Overline = new OverlineTypography { FontFamily = Sans, FontWeight = "500", FontSize = ".7rem", LetterSpacing = "2.5px", TextTransform = "uppercase" },
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "4px",
            DrawerWidthLeft = "260px",
            AppbarHeight = "68px",
        },
    };
}
