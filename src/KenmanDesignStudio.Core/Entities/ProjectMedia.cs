using KenmanDesignStudio.Core.Enums;

namespace KenmanDesignStudio.Core.Entities;

/// <summary>An image (photograph, rendering or site plan) belonging to a project.
/// <see cref="Path"/> is a local wwwroot-relative path so real photos can be dropped in later.</summary>
public class ProjectMedia : BaseEntity
{
    public int ProjectId { get; set; }
    public Project? Project { get; set; }

    /// <summary>wwwroot-relative path, e.g. "/images/projects/resorts/aurelia-1.svg".</summary>
    public string Path { get; set; } = string.Empty;

    public string Caption { get; set; } = string.Empty;
    public MediaKind Kind { get; set; }
    public int SortOrder { get; set; }
    public bool IsPrimary { get; set; }
}
