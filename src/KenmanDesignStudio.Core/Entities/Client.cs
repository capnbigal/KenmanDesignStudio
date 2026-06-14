using KenmanDesignStudio.Core.Common;
using KenmanDesignStudio.Core.Enums;

namespace KenmanDesignStudio.Core.Entities;

/// <summary>A prestige client organisation or private individual the firm works with.</summary>
public class Client : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public ClientType Type { get; set; }

    /// <summary>Two/three-letter monogram used for the brand avatar.</summary>
    public string Monogram { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;     // e.g. "Northeast", "West Coast", "International"
    public string Country { get; set; } = "USA";

    public string? Website { get; set; }
    public string? About { get; set; }

    public DateTime ClientSince { get; set; }
    public DateTime? LastContactDate { get; set; }

    /// <summary>Marks a developer/REIT that has commissioned more than one project.</summary>
    public bool IsRepeatClient { get; set; }

    // Navigation
    public List<Contact> Contacts { get; set; } = new();
    public List<Project> Projects { get; set; } = new();

    // --- Derived (not mapped) ---

    /// <summary>Booked lifetime value across all won/in-progress/complete projects.</summary>
    public decimal LifetimeValue =>
        Projects.Where(p => !p.IsDeleted && p.Status.IsBooked()).Sum(p => p.Value);

    /// <summary>Total value of still-open opportunities (lead/proposed).</summary>
    public decimal OpenPipelineValue =>
        Projects.Where(p => !p.IsDeleted && p.Status.IsOpenPipeline()).Sum(p => p.Value);

    public ClientTier Tier => ClientTierCalculator.FromLifetimeValue(LifetimeValue);

    public int ProjectCount => Projects.Count(p => !p.IsDeleted);

    public int CompletedProjectCount =>
        Projects.Count(p => !p.IsDeleted && p.Status == ProjectStatus.Complete);

    public Contact? PrimaryContact =>
        Contacts.FirstOrDefault(c => c.IsPrimary) ?? Contacts.FirstOrDefault();

    public int? DaysSinceLastContact =>
        LastContactDate is null ? null : (int)(DateTime.UtcNow.Date - LastContactDate.Value.Date).TotalDays;
}
