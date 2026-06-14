using Microsoft.EntityFrameworkCore;
using KenmanDesignStudio.Core.Common;
using KenmanDesignStudio.Core.Enums;
using KenmanDesignStudio.Infrastructure.Data;

namespace KenmanDesignStudio.Web.Services;

public class AnalyticsService(IDbContextFactory<AppDbContext> factory)
{
    public async Task<List<RegionValue>> GetValueByRegionAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        var booked = await db.Projects.AsNoTracking().Where(p =>
            p.Status == ProjectStatus.Won || p.Status == ProjectStatus.InDesign ||
            p.Status == ProjectStatus.InConstruction || p.Status == ProjectStatus.Complete).ToListAsync();

        return booked.GroupBy(p => p.Region)
            .Select(g => new RegionValue(g.Key, g.Sum(p => p.Value), g.Count()))
            .OrderByDescending(r => r.Value).ToList();
    }

    public async Task<List<SourceWinRate>> GetWinRateBySourceAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        var leads = await db.Leads.AsNoTracking().ToListAsync();

        return Enum.GetValues<LeadSource>().Select(s =>
        {
            var group = leads.Where(l => l.Source == s).ToList();
            var decided = group.Count(l => l.Status is LeadStatus.Converted or LeadStatus.Lost);
            var won = group.Count(l => l.Status == LeadStatus.Converted);
            return new SourceWinRate(s, s.Display(), group.Count, won, decided == 0 ? 0 : (double)won / decided);
        }).OrderByDescending(s => s.Rate).ToList();
    }

    public async Task<List<CategoryValue>> GetRevenueByCategoryAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        var booked = await db.Projects.AsNoTracking().Where(p =>
            p.Status == ProjectStatus.Won || p.Status == ProjectStatus.InDesign ||
            p.Status == ProjectStatus.InConstruction || p.Status == ProjectStatus.Complete).ToListAsync();

        return booked.GroupBy(p => p.Category)
            .Select(g => new CategoryValue(g.Key, CategoryCatalog.Get(g.Key).ShortName, g.Sum(p => p.Value), g.Count()))
            .OrderByDescending(c => c.Value).ToList();
    }

    public async Task<List<MonthlyValue>> GetMonthlyLeadsAsync(int months = 12)
    {
        await using var db = await factory.CreateDbContextAsync();
        var leads = await db.Leads.AsNoTracking().ToListAsync();
        var now = DateTime.UtcNow;
        var start = new DateTime(now.Year, now.Month, 1).AddMonths(-(months - 1));
        var result = new List<MonthlyValue>();
        for (var i = 0; i < months; i++)
        {
            var m = start.AddMonths(i);
            var count = leads.Count(l => l.ReceivedDate.Year == m.Year && l.ReceivedDate.Month == m.Month);
            result.Add(new MonthlyValue(m.ToString("MMM yy"), m, count));
        }
        return result;
    }

    public async Task<List<CategoryValue>> GetPipelineByCategoryAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        var open = await db.Projects.AsNoTracking().Where(p =>
            p.Status == ProjectStatus.Lead || p.Status == ProjectStatus.Proposed).ToListAsync();
        return open.GroupBy(p => p.Category)
            .Select(g => new CategoryValue(g.Key, CategoryCatalog.Get(g.Key).ShortName, g.Sum(p => p.Value), g.Count()))
            .OrderByDescending(c => c.Value).ToList();
    }
}
