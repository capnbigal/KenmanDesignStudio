using Microsoft.EntityFrameworkCore;
using MudBlazor;
using KenmanDesignStudio.Core.Common;
using KenmanDesignStudio.Core.Entities;
using KenmanDesignStudio.Core.Enums;
using KenmanDesignStudio.Infrastructure.Data;

namespace KenmanDesignStudio.Web.Services;

public class ClientService(IDbContextFactory<AppDbContext> factory)
{
    /// <summary>All clients with their projects loaded so tier / lifetime-value compute correctly.</summary>
    public async Task<List<Client>> GetListAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Clients.AsNoTracking()
            .Include(c => c.Projects)
            .Include(c => c.Contacts)
            .OrderByDescending(c => c.Projects.Where(p => !p.IsDeleted).Sum(p => (decimal?)p.Value) ?? 0)
            .ToListAsync();
    }

    public async Task<Client?> GetByIdAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Clients.AsNoTracking()
            .Include(c => c.Contacts)
            .Include(c => c.Projects).ThenInclude(p => p.Media)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Client> CreateAsync(Client client)
    {
        await using var db = await factory.CreateDbContextAsync();
        if (string.IsNullOrWhiteSpace(client.Monogram))
            client.Monogram = Monogram(client.Name);
        client.ClientSince = client.ClientSince == default ? DateTime.UtcNow : client.ClientSince;
        client.LastContactDate ??= DateTime.UtcNow;
        db.Clients.Add(client);
        await db.SaveChangesAsync();
        return client;
    }

    public async Task UpdateAsync(Client client)
    {
        await using var db = await factory.CreateDbContextAsync();
        var existing = await db.Clients.Include(c => c.Contacts).FirstOrDefaultAsync(c => c.Id == client.Id);
        if (existing is null) return;

        existing.Name = client.Name;
        existing.Type = client.Type;
        existing.Monogram = string.IsNullOrWhiteSpace(client.Monogram) ? Monogram(client.Name) : client.Monogram;
        existing.City = client.City;
        existing.Region = client.Region;
        existing.Country = client.Country;
        existing.Website = client.Website;
        existing.About = client.About;
        existing.LastContactDate = client.LastContactDate;
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        var existing = await db.Clients.FirstOrDefaultAsync(c => c.Id == id);
        if (existing is null) return;
        existing.IsDeleted = true;
        await db.SaveChangesAsync();
    }

    public async Task LogContactAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        var existing = await db.Clients.FirstOrDefaultAsync(c => c.Id == id);
        if (existing is null) return;
        existing.LastContactDate = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    /// <summary>Derives the "insight" callouts shown on the Client 360 page.</summary>
    public async Task<List<ClientInsight>> GetInsightsAsync(Client client)
    {
        await using var db = await factory.CreateDbContextAsync();
        var allValues = await db.Clients.AsNoTracking()
            .Select(c => c.Projects.Where(p => !p.IsDeleted &&
                (p.Status == ProjectStatus.Won || p.Status == ProjectStatus.InDesign ||
                 p.Status == ProjectStatus.InConstruction || p.Status == ProjectStatus.Complete))
                .Sum(p => (decimal?)p.Value) ?? 0)
            .ToListAsync();

        var ltv = client.LifetimeValue;
        var insights = new List<ClientInsight>();

        // Percentile by revenue.
        allValues.Sort();
        var rank = allValues.Count(v => v <= ltv);
        var pct = allValues.Count == 0 ? 0 : 100.0 * rank / allValues.Count;
        if (pct >= 95)
            insights.Add(new ClientInsight("Top 5% by revenue", "Among the firm's most valuable relationships.", Icons.Material.Filled.WorkspacePremium, "secondary"));
        else if (pct >= 80)
            insights.Add(new ClientInsight("Top 20% by revenue", "A high-value, priority relationship.", Icons.Material.Filled.TrendingUp, "primary"));

        if (client.Tier == ClientTier.Signature)
            insights.Add(new ClientInsight("Signature tier", $"Lifetime commissions exceed {Money(ClientTierCalculator.SignatureThreshold)}.", Icons.Material.Filled.Diamond, "secondary"));

        if (client.IsRepeatClient)
            insights.Add(new ClientInsight("Repeat client", $"Has commissioned {client.Projects.Count(p => p.Status.IsBooked())} projects with the firm.", Icons.Material.Filled.Repeat, "primary"));

        var days = client.DaysSinceLastContact;
        if (days is >= 90)
            insights.Add(new ClientInsight("No contact in 90+ days", $"Last contact was {days} days ago — consider reaching out.", Icons.Material.Filled.NotificationsActive, "warning"));

        if (client.OpenPipelineValue > 0)
            insights.Add(new ClientInsight("Active opportunity", $"{Money(client.OpenPipelineValue)} in open pipeline across {client.Projects.Count(p => p.Status.IsOpenPipeline())} opportunity(ies).", Icons.Material.Filled.Bolt, "info"));

        var categories = client.Projects.Where(p => !p.IsDeleted).Select(p => p.Category).Distinct().Count();
        if (categories >= 3)
            insights.Add(new ClientInsight("Multi-discipline client", $"Engaged across {categories} of our six disciplines.", Icons.Material.Filled.Category, "primary"));

        return insights;
    }

    private static string Money(decimal v) => v >= 1_000_000m ? $"${v / 1_000_000m:0.0}M" : $"${v / 1_000m:0}K";

    private static string Monogram(string name)
    {
        var clean = (name ?? "").Replace("The ", "").Replace("&", " ");
        var words = clean.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0) return "VA";
        if (words.Length == 1) return words[0][..Math.Min(2, words[0].Length)].ToUpperInvariant();
        return (words[0][0].ToString() + words[1][0]).ToUpperInvariant();
    }
}
