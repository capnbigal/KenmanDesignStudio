using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace KenmanDesignStudio.Infrastructure.Data;

/// <summary>
/// Used only by the EF Core tooling (`dotnet ef migrations` / `database update`).
/// The running app configures the context through DI in Program.cs instead.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable("VERDANT_CONNECTION")
            ?? "Server=.;Database=KenmanDesignStudio;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=True";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connection)
            .Options;

        return new AppDbContext(options);
    }
}
