using KenmanDesignStudio.Core.Enums;

namespace KenmanDesignStudio.Core.Entities;

/// <summary>A prestige-client testimonial shown on the public showcase.</summary>
public class Testimonial : BaseEntity
{
    public string Quote { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorTitle { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string Monogram { get; set; } = string.Empty;

    public ProjectCategory? Category { get; set; }
    public int Rating { get; set; } = 5;
    public bool IsFeatured { get; set; }
}
