using Microsoft.EntityFrameworkCore;
using KenmanDesignStudio.Core.Entities;
using KenmanDesignStudio.Infrastructure.Data;

namespace KenmanDesignStudio.Web.Services;

public class NotificationService(IDbContextFactory<AppDbContext> factory)
{
    public async Task<List<Notification>> GetListAsync(int? take = null)
    {
        await using var db = await factory.CreateDbContextAsync();
        var query = db.Notifications.AsNoTracking().OrderByDescending(n => n.OccurredAt).AsQueryable();
        if (take is not null) query = query.Take(take.Value);
        return await query.ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Notifications.CountAsync(n => !n.IsRead);
    }

    public async Task MarkReadAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        var n = await db.Notifications.FirstOrDefaultAsync(x => x.Id == id);
        if (n is null) return;
        n.IsRead = true;
        await db.SaveChangesAsync();
    }

    public async Task MarkAllReadAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        await db.Notifications.Where(n => !n.IsRead).ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
    }
}
