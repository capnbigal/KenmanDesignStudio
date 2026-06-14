namespace KenmanDesignStudio.Core.Entities;

/// <summary>A named point of contact at a client organisation.</summary>
public class Contact : BaseEntity
{
    public int ClientId { get; set; }
    public Client? Client { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();
}
