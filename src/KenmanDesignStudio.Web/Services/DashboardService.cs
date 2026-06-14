using Microsoft.EntityFrameworkCore;
using KenmanDesignStudio.Core.Common;
using KenmanDesignStudio.Core.Enums;
using KenmanDesignStudio.Infrastructure.Data;

namespace KenmanDesignStudio.Web.Services;

public class DashboardService(IDbContextFactory<AppDbContext> factory)
{
    public async Task<DashboardData> GetAsync()
    {
        await using var db = await factory.CreateDbContextAsync();

        var projects = await db.Projects.AsNoTracking().ToListAsync();
        var leads = await db.Leads.AsNoTracking().ToListAsync();
        var clients = await db.Clients.AsNoTracking().Select(c => c.Id).ToListAsync();
        var openRequests = await db.Requests.CountAsync(r => r.Status == RequestStatus.New || r.Status == RequestStatus.InReview);
        var unread = await db.Notifications.CountAsync(n => !n.IsRead);

        var now = DateTime.UtcNow;
        var booked = projects.Where(p => p.Status.IsBooked()).ToList();

        DateTime Recognition(Core.Entities.Project p) => p.CompletionDate ?? p.StartDate;

        var data = new DashboardData
        {
            TotalClients = clients.Count,
            ActiveClients = projects
                .Where(p => p.Status is ProjectStatus.Proposed or ProjectStatus.Won or ProjectStatus.InDesign or ProjectStatus.InConstruction)
                .Select(p => p.ClientId).Distinct().Count(),
            PipelineValue = projects.Where(p => p.Status.IsOpenPipeline()).Sum(p => p.Value),
            YtdRevenue = booked.Where(p => Recognition(p).Year == now.Year).Sum(p => p.Value),
            TotalBookedValue = booked.Sum(p => p.Value),
            ActiveProjects = projects.Count(p => p.Status is ProjectStatus.InDesign or ProjectStatus.InConstruction or ProjectStatus.Won),
            CompletedProjects = projects.Count(p => p.Status == ProjectStatus.Complete),
            OpenRequests = openRequests,
            UnreadNotifications = unread,
        };

        var decided = leads.Count(l => l.Status is LeadStatus.Converted or LeadStatus.Lost);
        var wonLeads = leads.Count(l => l.Status == LeadStatus.Converted);
        data.WinRate = decided == 0 ? 0 : (double)wonLeads / decided;

        // Revenue trend — last 18 months of booked recognition.
        var start = new DateTime(now.Year, now.Month, 1).AddMonths(-17);
        for (var i = 0; i < 18; i++)
        {
            var m = start.AddMonths(i);
            var val = booked.Where(p => { var r = Recognition(p); return r.Year == m.Year && r.Month == m.Month; }).Sum(p => p.Value);
            data.RevenueTrend.Add(new MonthlyValue(m.ToString("MMM yy"), m, val));
        }

        data.RevenueByCategory = booked
            .GroupBy(p => p.Category)
            .Select(g => new CategoryValue(g.Key, CategoryCatalog.Name(g.Key), g.Sum(p => p.Value), g.Count()))
            .OrderByDescending(c => c.Value).ToList();

        data.ProjectsByCategory = projects
            .GroupBy(p => p.Category)
            .Select(g => new CategoryValue(g.Key, CategoryCatalog.Get(g.Key).ShortName, g.Sum(p => p.Value), g.Count()))
            .OrderByDescending(c => c.Count).ToList();

        data.ProjectsByStatus = Enum.GetValues<ProjectStatus>()
            .Select(s => new StatusCount(s, s.Display(),
                projects.Count(p => p.Status == s), projects.Where(p => p.Status == s).Sum(p => p.Value)))
            .ToList();

        data.LeadsBySource = Enum.GetValues<LeadSource>()
            .Select(s => new SourceCount(s, s.Display(),
                leads.Count(l => l.Source == s), leads.Where(l => l.Source == s).Sum(l => l.EstimatedValue)))
            .OrderByDescending(s => s.Count).ToList();

        return data;
    }
}
