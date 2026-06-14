using Microsoft.EntityFrameworkCore;
using KenmanDesignStudio.Core.Entities;
using KenmanDesignStudio.Core.Enums;
using KenmanDesignStudio.Infrastructure.Data;

namespace KenmanDesignStudio.Web.Services;

public class ProjectService(IDbContextFactory<AppDbContext> factory)
{
    public async Task<List<Project>> GetListAsync(ProjectCategory? category = null, ProjectStatus? status = null)
    {
        await using var db = await factory.CreateDbContextAsync();
        var query = db.Projects.AsNoTracking()
            .Include(p => p.Client)
            .Include(p => p.Media)
            .AsQueryable();

        if (category is not null) query = query.Where(p => p.Category == category);
        if (status is not null) query = query.Where(p => p.Status == status);

        return await query.OrderByDescending(p => p.StartDate).ToListAsync();
    }

    public async Task<Project?> GetByIdAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Projects.AsNoTracking()
            .Include(p => p.Client)
            .Include(p => p.Media.OrderBy(m => m.SortOrder))
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Client>> GetClientOptionsAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Clients.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<Project> CreateAsync(Project project)
    {
        await using var db = await factory.CreateDbContextAsync();
        project.Year = (project.CompletionDate ?? project.StartDate).Year;
        if (string.IsNullOrWhiteSpace(project.CodeName))
            project.CodeName = $"KDS-{project.Year % 100:D2}{Random.Shared.Next(100, 999)}";
        db.Projects.Add(project);
        await db.SaveChangesAsync();
        return project;
    }

    public async Task UpdateAsync(Project project)
    {
        await using var db = await factory.CreateDbContextAsync();
        var existing = await db.Projects.FirstOrDefaultAsync(p => p.Id == project.Id);
        if (existing is null) return;

        existing.Name = project.Name;
        existing.Category = project.Category;
        existing.Status = project.Status;
        existing.City = project.City;
        existing.Region = project.Region;
        existing.Country = project.Country;
        existing.Value = project.Value;
        existing.StartDate = project.StartDate;
        existing.CompletionDate = project.CompletionDate;
        existing.Year = (project.CompletionDate ?? project.StartDate).Year;
        existing.ProgressPercent = project.Status == ProjectStatus.Complete ? 100 : project.ProgressPercent;
        existing.ArchitectPartner = project.ArchitectPartner;
        existing.Summary = project.Summary;
        existing.Description = project.Description;
        existing.IsFeatured = project.IsFeatured;
        existing.ClientId = project.ClientId;
        await db.SaveChangesAsync();
    }

    public async Task AdvanceStatusAsync(int id, ProjectStatus status)
    {
        await using var db = await factory.CreateDbContextAsync();
        var existing = await db.Projects.FirstOrDefaultAsync(p => p.Id == id);
        if (existing is null) return;
        existing.Status = status;
        if (status == ProjectStatus.Complete)
        {
            existing.ProgressPercent = 100;
            existing.CompletionDate ??= DateTime.UtcNow;
        }
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        var existing = await db.Projects.FirstOrDefaultAsync(p => p.Id == id);
        if (existing is null) return;
        existing.IsDeleted = true;
        await db.SaveChangesAsync();
    }
}
