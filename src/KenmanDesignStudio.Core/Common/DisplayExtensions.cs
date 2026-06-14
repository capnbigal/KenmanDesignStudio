using KenmanDesignStudio.Core.Enums;

namespace KenmanDesignStudio.Core.Common;

/// <summary>Human-friendly labels for the domain enums, used throughout the UI.</summary>
public static class DisplayExtensions
{
    public static string Display(this ProjectStatus status) => status switch
    {
        ProjectStatus.Lead => "Lead",
        ProjectStatus.Proposed => "Proposed",
        ProjectStatus.Won => "Won",
        ProjectStatus.InDesign => "In Design",
        ProjectStatus.InConstruction => "In Construction",
        ProjectStatus.Complete => "Complete",
        _ => status.ToString()
    };

    public static string Display(this ProjectCategory category) => CategoryCatalog.Name(category);

    public static string Display(this ClientTier tier) => tier switch
    {
        ClientTier.Signature => "Signature",
        ClientTier.Premier => "Premier",
        ClientTier.Standard => "Standard",
        _ => tier.ToString()
    };

    public static string Display(this LeadSource source) => source switch
    {
        LeadSource.Referral => "Client Referral",
        LeadSource.ArchitectPartner => "Architect Partner",
        LeadSource.AwardsPress => "Awards & Press",
        LeadSource.Website => "Website",
        LeadSource.Event => "Event",
        _ => source.ToString()
    };

    public static string Display(this LeadStatus status) => status switch
    {
        LeadStatus.New => "New",
        LeadStatus.Qualified => "Qualified",
        LeadStatus.Nurturing => "Nurturing",
        LeadStatus.Converted => "Converted",
        LeadStatus.Lost => "Lost",
        _ => status.ToString()
    };

    public static string Display(this RequestStatus status) => status switch
    {
        RequestStatus.New => "New",
        RequestStatus.InReview => "In Review",
        RequestStatus.Scheduled => "Scheduled",
        RequestStatus.Closed => "Closed",
        _ => status.ToString()
    };

    public static string Display(this CampaignChannel channel) => channel switch
    {
        CampaignChannel.DesignAwards => "Design Awards",
        CampaignChannel.ArchitectureDigest => "Architecture Digest",
        CampaignChannel.PrivateEvents => "Private Events",
        CampaignChannel.Search => "Search",
        CampaignChannel.Social => "Social",
        CampaignChannel.Referral => "Referral Program",
        _ => channel.ToString()
    };

    public static string Display(this ClientType type) => type switch
    {
        ClientType.Developer => "Developer",
        ClientType.REIT => "REIT",
        ClientType.AirportAuthority => "Airport Authority",
        ClientType.ResortGroup => "Resort Group",
        ClientType.CountryClub => "Country Club",
        ClientType.PrivateIndividual => "Private Client",
        ClientType.Corporate => "Corporate",
        ClientType.Municipality => "Municipality",
        _ => type.ToString()
    };

    /// <summary>True for the pipeline stages that represent booked (won) revenue.</summary>
    public static bool IsBooked(this ProjectStatus status) =>
        status is ProjectStatus.Won or ProjectStatus.InDesign
               or ProjectStatus.InConstruction or ProjectStatus.Complete;

    /// <summary>True while a project is still an open opportunity (not yet won or lost).</summary>
    public static bool IsOpenPipeline(this ProjectStatus status) =>
        status is ProjectStatus.Lead or ProjectStatus.Proposed;
}
