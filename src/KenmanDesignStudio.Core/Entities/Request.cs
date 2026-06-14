using KenmanDesignStudio.Core.Enums;

namespace KenmanDesignStudio.Core.Entities;

/// <summary>An incoming "Request a Consultation" enquiry from the public showcase.</summary>
public class Request : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Company { get; set; }

    public ProjectCategory InterestCategory { get; set; }
    public string Region { get; set; } = string.Empty;

    /// <summary>Free-text budget band the prospect selected, e.g. "$2M – $5M".</summary>
    public string BudgetBand { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public RequestStatus Status { get; set; }
    public DateTime SubmittedAt { get; set; }

    /// <summary>Lead spawned from this request, if any.</summary>
    public int? LeadId { get; set; }
}
