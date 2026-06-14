namespace KenmanDesignStudio.Core.Entities;

/// <summary>Marker for entities that participate in soft-delete query filtering.</summary>
public interface ISoftDelete
{
    bool IsDeleted { get; set; }
}

/// <summary>Base type carrying the surrogate key, audit timestamps and soft-delete flag
/// shared by every core entity.</summary>
public abstract class BaseEntity : ISoftDelete
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
}
