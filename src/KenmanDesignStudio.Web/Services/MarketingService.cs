using Microsoft.EntityFrameworkCore;
using KenmanDesignStudio.Core.Entities;
using KenmanDesignStudio.Infrastructure.Data;

namespace KenmanDesignStudio.Web.Services;

public class MarketingService(IDbContextFactory<AppDbContext> factory)
{
    public async Task<List<Campaign>> GetCampaignsAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Campaigns.AsNoTracking()
            .OrderByDescending(c => c.StartDate)
            .ToListAsync();
    }

    public async Task<(decimal Spend, decimal Revenue, int Leads, int Conversions)> GetTotalsAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        var campaigns = await db.Campaigns.AsNoTracking().ToListAsync();
        return (campaigns.Sum(c => c.Spend), campaigns.Sum(c => c.RevenueAttributed),
                campaigns.Sum(c => c.LeadsGenerated), campaigns.Sum(c => c.Conversions));
    }
}
