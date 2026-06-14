using KenmanDesignStudio.Core.Enums;

namespace KenmanDesignStudio.Core.Entities;

/// <summary>An in-app activity-feed notification.</summary>
public class Notification : BaseEntity
{
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    /// <summary>Optional deep-link into the internal app.</summary>
    public string? Link { get; set; }

    /// <summary>When the underlying event occurred (drives the feed ordering).</summary>
    public DateTime OccurredAt { get; set; }
}
