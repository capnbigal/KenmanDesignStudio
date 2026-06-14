using KenmanDesignStudio.Core.Enums;

namespace KenmanDesignStudio.Core.Entities;

/// <summary>A landscape architecture commission — the central record of the firm's work.</summary>
public class Project : BaseEntity
{
    public int ClientId { get; set; }
    public Client? Client { get; set; }

    public string Name { get; set; } = string.Empty;
    public string CodeName { get; set; } = string.Empty;   // e.g. "KDS-2403"

    public ProjectCategory Category { get; set; }
    public ProjectStatus Status { get; set; }

    public string City { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string Country { get; set; } = "USA";

    /// <summary>Contract value in USD.</summary>
    public decimal Value { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public int Year { get; set; }

    public double SiteAreaAcres { get; set; }

    public string Summary { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>Collaborating architecture practice, when applicable.</summary>
    public string? ArchitectPartner { get; set; }

    /// <summary>Attribution for analytics — how the original opportunity arrived.</summary>
    public LeadSource Source { get; set; }

    public bool IsFeatured { get; set; }

    /// <summary>Completion percentage for in-construction projects (0-100).</summary>
    public int ProgressPercent { get; set; }

    public List<ProjectMedia> Media { get; set; } = new();

    public ProjectMedia? PrimaryImage =>
        Media.Where(m => !m.IsDeleted).OrderByDescending(m => m.IsPrimary).ThenBy(m => m.SortOrder).FirstOrDefault();

    public string Location => string.IsNullOrWhiteSpace(City) ? Country : $"{City}";
}
