using Microsoft.EntityFrameworkCore;
using KenmanDesignStudio.Core.Entities;
using KenmanDesignStudio.Core.Enums;
using KenmanDesignStudio.Infrastructure.Data;

namespace KenmanDesignStudio.Web.Services;

/// <summary>Read/display data for the public showcase, plus the consultation intake.</summary>
public class ShowcaseService(IDbContextFactory<AppDbContext> factory)
{
    /// <summary>Gallery projects — completed or in-construction work shown to the public.</summary>
    public async Task<List<Project>> GetGalleryAsync(ProjectCategory? category = null)
    {
        await using var db = await factory.CreateDbContextAsync();
        var query = db.Projects.AsNoTracking()
            .Include(p => p.Client)
            .Include(p => p.Media)
            .Where(p => p.Status == ProjectStatus.Complete || p.Status == ProjectStatus.InConstruction);

        if (category is not null) query = query.Where(p => p.Category == category);

        return await query.OrderByDescending(p => p.Status == ProjectStatus.Complete)
            .ThenByDescending(p => p.Value)
            .ToListAsync();
    }

    /// <summary>A curated set of hero/featured projects spread across the disciplines.</summary>
    public async Task<List<Project>> GetFeaturedAsync(int count = 6)
    {
        var gallery = await GetGalleryAsync();
        return gallery
            .GroupBy(p => p.Category)
            .Select(g => g.OrderByDescending(p => p.Value).First())
            .OrderByDescending(p => p.Value)
            .Take(count)
            .ToList();
    }

    public async Task<Project?> GetProjectAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Projects.AsNoTracking()
            .Include(p => p.Client)
            .Include(p => p.Media.OrderBy(m => m.SortOrder))
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Testimonial>> GetTestimonialsAsync(bool featuredOnly = false)
    {
        await using var db = await factory.CreateDbContextAsync();
        var query = db.Testimonials.AsNoTracking().AsQueryable();
        if (featuredOnly) query = query.Where(t => t.IsFeatured);
        return await query.ToListAsync();
    }

    public async Task<(int Projects, int Clients, decimal Value, int Awards)> GetStatsAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        var projects = await db.Projects.AsNoTracking().Where(p => p.Status == ProjectStatus.Complete).ToListAsync();
        var clients = await db.Clients.AsNoTracking().CountAsync();
        return (projects.Count, clients, projects.Sum(p => p.Value), 24);
    }

    /// <summary>Public consultation intake — creates a Request (inbox) and an attributed Lead.</summary>
    public async Task SubmitConsultationAsync(Request request)
    {
        await using var db = await factory.CreateDbContextAsync();
        request.Status = RequestStatus.New;
        request.SubmittedAt = DateTime.UtcNow;
        db.Requests.Add(request);
        await db.SaveChangesAsync();

        var lead = new Lead
        {
            ContactName = request.Name,
            CompanyName = request.Company ?? "Private enquiry",
            Email = request.Email,
            Phone = request.Phone,
            Source = LeadSource.Website,
            Status = LeadStatus.New,
            InterestCategory = request.InterestCategory,
            Region = request.Region,
            EstimatedValue = BudgetMidpoint(request.BudgetBand),
            ReceivedDate = DateTime.UtcNow,
            Notes = $"From website consultation request #{request.Id}. {request.Message}",
        };
        db.Leads.Add(lead);
        await db.SaveChangesAsync();

        request.LeadId = lead.Id;
        db.Requests.Attach(request);
        db.Entry(request).Property(r => r.LeadId).IsModified = true;
        await db.SaveChangesAsync();
    }

    private static decimal BudgetMidpoint(string band) => band switch
    {
        "$500K – $1M" => 750_000m,
        "$1M – $2.5M" => 1_750_000m,
        "$2.5M – $5M" => 3_750_000m,
        "$5M – $10M" => 7_500_000m,
        "$10M+" => 12_000_000m,
        _ => 1_500_000m,
    };
}
