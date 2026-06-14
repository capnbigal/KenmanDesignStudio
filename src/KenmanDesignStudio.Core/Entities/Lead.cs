using KenmanDesignStudio.Core.Enums;

namespace KenmanDesignStudio.Core.Entities;

/// <summary>A sales lead — a prospective commission, with source attribution.</summary>
public class Lead : BaseEntity
{
    public string ContactName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

    public LeadSource Source { get; set; }
    public LeadStatus Status { get; set; }

    /// <summary>Category of work the prospect is interested in.</summary>
    public ProjectCategory InterestCategory { get; set; }

    public string Region { get; set; } = string.Empty;
    public decimal EstimatedValue { get; set; }

    public DateTime ReceivedDate { get; set; }
    public string? Notes { get; set; }

    // Attribution
    public int? CampaignId { get; set; }
    public Campaign? Campaign { get; set; }

    /// <summary>Set when the lead converts into a client/project.</summary>
    public int? ConvertedClientId { get; set; }
    public DateTime? ConvertedDate { get; set; }

    public bool IsConverted => Status == LeadStatus.Converted;
}
