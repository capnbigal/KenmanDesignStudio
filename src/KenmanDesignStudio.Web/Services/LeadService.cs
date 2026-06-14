using Microsoft.EntityFrameworkCore;
using KenmanDesignStudio.Core.Entities;
using KenmanDesignStudio.Core.Enums;
using KenmanDesignStudio.Infrastructure.Data;

namespace KenmanDesignStudio.Web.Services;

public class LeadService(IDbContextFactory<AppDbContext> factory)
{
    public async Task<List<Lead>> GetListAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Leads.AsNoTracking()
            .Include(l => l.Campaign)
            .OrderByDescending(l => l.ReceivedDate)
            .ToListAsync();
    }

    public async Task<Lead> CreateAsync(Lead lead)
    {
        await using var db = await factory.CreateDbContextAsync();
        lead.ReceivedDate = lead.ReceivedDate == default ? DateTime.UtcNow : lead.ReceivedDate;
        db.Leads.Add(lead);
        await db.SaveChangesAsync();
        return lead;
    }

    public async Task UpdateAsync(Lead lead)
    {
        await using var db = await factory.CreateDbContextAsync();
        var existing = await db.Leads.FirstOrDefaultAsync(l => l.Id == lead.Id);
        if (existing is null) return;
        existing.ContactName = lead.ContactName;
        existing.CompanyName = lead.CompanyName;
        existing.Email = lead.Email;
        existing.Phone = lead.Phone;
        existing.Source = lead.Source;
        existing.Status = lead.Status;
        existing.InterestCategory = lead.InterestCategory;
        existing.Region = lead.Region;
        existing.EstimatedValue = lead.EstimatedValue;
        existing.Notes = lead.Notes;
        if (lead.Status == LeadStatus.Converted && existing.ConvertedDate is null)
            existing.ConvertedDate = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task SetStatusAsync(int id, LeadStatus status)
    {
        await using var db = await factory.CreateDbContextAsync();
        var existing = await db.Leads.FirstOrDefaultAsync(l => l.Id == id);
        if (existing is null) return;
        existing.Status = status;
        if (status == LeadStatus.Converted) existing.ConvertedDate ??= DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        var existing = await db.Leads.FirstOrDefaultAsync(l => l.Id == id);
        if (existing is null) return;
        existing.IsDeleted = true;
        await db.SaveChangesAsync();
    }
}
