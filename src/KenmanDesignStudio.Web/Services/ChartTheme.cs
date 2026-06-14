using ApexCharts;

namespace KenmanDesignStudio.Web.Services;

/// <summary>Brand-consistent ApexCharts option builders.</summary>
public static class ChartTheme
{
    public static readonly List<string> Palette = new()
    {
        "#2F5D50", "#C8A95B", "#3C6E8F", "#6B5BA8", "#C8702E", "#3E8F63", "#8A94A6"
    };

    public static ApexChartOptions<T> Base<T>(bool dark) where T : class
    {
        return new ApexChartOptions<T>
        {
            Chart = new Chart
            {
                Toolbar = new Toolbar { Show = false },
                Background = "transparent",
                FontFamily = "Jost, Helvetica Neue, sans-serif",
                Animations = new Animations { Enabled = true, Speed = 500 },
            },
            Theme = new Theme { Mode = dark ? Mode.Dark : Mode.Light },
            Colors = Palette,
            Grid = new Grid
            {
                BorderColor = dark ? "#243036" : "#E6E1D6",
                StrokeDashArray = 4,
            },
            DataLabels = new DataLabels { Enabled = false },
            Tooltip = new Tooltip { Theme = dark ? Mode.Dark : Mode.Light },
        };
    }
}
