using KenmanDesignStudio.Core.Enums;

namespace KenmanDesignStudio.Core.Entities;

/// <summary>A marketing campaign with spend, attributed leads and ROI.</summary>
public class Campaign : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public CampaignChannel Channel { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }

    public decimal Spend { get; set; }

    /// <summary>Denormalised attributed numbers — kept consistent with seeded leads.</summary>
    public int LeadsGenerated { get; set; }
    public int Conversions { get; set; }
    public decimal RevenueAttributed { get; set; }

    public List<Lead> Leads { get; set; } = new();

    // --- Derived ---
    public decimal Roi => Spend <= 0 ? 0 : (RevenueAttributed - Spend) / Spend;
    public decimal CostPerLead => LeadsGenerated <= 0 ? 0 : Spend / LeadsGenerated;
    public decimal ConversionRate => LeadsGenerated <= 0 ? 0 : (decimal)Conversions / LeadsGenerated;
}
