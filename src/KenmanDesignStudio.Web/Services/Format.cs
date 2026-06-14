using KenmanDesignStudio.Core.Enums;

namespace KenmanDesignStudio.Web.Services;

/// <summary>Shared presentation formatting helpers.</summary>
public static class Format
{
    public static string Money(decimal v)
    {
        if (v >= 1_000_000_000m) return $"${v / 1_000_000_000m:0.0}B";
        if (v >= 1_000_000m) return $"${v / 1_000_000m:0.0}M";
        if (v >= 1_000m) return $"${v / 1_000m:0}K";
        return $"${v:0}";
    }

    public static string MoneyFull(decimal v) => v.ToString("C0", System.Globalization.CultureInfo.GetCultureInfo("en-US"));

    public static string Percent(double v) => $"{v * 100:0}%";
    public static string Percent1(double v) => $"{v * 100:0.0}%";

    /// <summary>Brand colour token for a project status (used for chips / pipeline columns).</summary>
    public static string StatusColor(ProjectStatus s) => s switch
    {
        ProjectStatus.Lead => "#8A94A6",
        ProjectStatus.Proposed => "#C08A2E",
        ProjectStatus.Won => "#3C6E8F",
        ProjectStatus.InDesign => "#6B5BA8",
        ProjectStatus.InConstruction => "#C8702E",
        ProjectStatus.Complete => "#3E8F63",
        _ => "#8A94A6",
    };

    public static string TierColor(ClientTier t) => t switch
    {
        ClientTier.Signature => "#C8A95B",
        ClientTier.Premier => "#3C6E8F",
        _ => "#8A94A6",
    };

    public static string TierIcon(ClientTier t) => t switch
    {
        ClientTier.Signature => MudBlazor.Icons.Material.Filled.Diamond,
        ClientTier.Premier => MudBlazor.Icons.Material.Filled.WorkspacePremium,
        _ => MudBlazor.Icons.Material.Filled.Star,
    };

    public static string SourceColor(LeadSource s) => s switch
    {
        LeadSource.Referral => "#3E8F63",
        LeadSource.ArchitectPartner => "#3C6E8F",
        LeadSource.AwardsPress => "#C8A95B",
        LeadSource.Website => "#6B5BA8",
        LeadSource.Event => "#C8702E",
        _ => "#8A94A6",
    };

    public static string LeadStatusColor(LeadStatus s) => s switch
    {
        LeadStatus.Converted => "#3E8F63",
        LeadStatus.Qualified => "#3C6E8F",
        LeadStatus.Nurturing => "#C08A2E",
        LeadStatus.New => "#8A94A6",
        LeadStatus.Lost => "#B23A3A",
        _ => "#8A94A6",
    };

    public static string RequestStatusColor(RequestStatus s) => s switch
    {
        RequestStatus.New => "#C8A95B",
        RequestStatus.InReview => "#3C6E8F",
        RequestStatus.Scheduled => "#3E8F63",
        RequestStatus.Closed => "#8A94A6",
        _ => "#8A94A6",
    };
}
