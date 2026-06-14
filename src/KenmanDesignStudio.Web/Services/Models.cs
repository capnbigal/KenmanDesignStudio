using KenmanDesignStudio.Core.Enums;

namespace KenmanDesignStudio.Web.Services;

public record MonthlyValue(string Label, DateTime Month, decimal Value);
public record CategoryValue(ProjectCategory Category, string Label, decimal Value, int Count);
public record StatusCount(ProjectStatus Status, string Label, int Count, decimal Value);
public record SourceCount(LeadSource Source, string Label, int Count, decimal Value);
public record RegionValue(string Region, decimal Value, int Count);
public record SourceWinRate(LeadSource Source, string Label, int Total, int Won, double Rate);

public class DashboardData
{
    public int ActiveClients { get; set; }
    public int TotalClients { get; set; }
    public decimal PipelineValue { get; set; }
    public double WinRate { get; set; }
    public decimal YtdRevenue { get; set; }
    public int OpenRequests { get; set; }
    public decimal TotalBookedValue { get; set; }
    public int ActiveProjects { get; set; }
    public int CompletedProjects { get; set; }
    public int UnreadNotifications { get; set; }

    public List<MonthlyValue> RevenueTrend { get; set; } = new();
    public List<CategoryValue> RevenueByCategory { get; set; } = new();
    public List<SourceCount> LeadsBySource { get; set; } = new();
    public List<StatusCount> ProjectsByStatus { get; set; } = new();
    public List<CategoryValue> ProjectsByCategory { get; set; } = new();
}

/// <summary>A derived "insight" callout shown on the Client 360 page.</summary>
public record ClientInsight(string Title, string Detail, string Icon, string Color);
