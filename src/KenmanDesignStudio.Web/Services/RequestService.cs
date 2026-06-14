using Microsoft.EntityFrameworkCore;
using KenmanDesignStudio.Core.Entities;
using KenmanDesignStudio.Core.Enums;
using KenmanDesignStudio.Infrastructure.Data;

namespace KenmanDesignStudio.Web.Services;

public class RequestService(IDbContextFactory<AppDbContext> factory)
{
    public async Task<List<Request>> GetListAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Requests.AsNoTracking()
            .OrderByDescending(r => r.SubmittedAt)
            .ToListAsync();
    }

    public async Task SetStatusAsync(int id, RequestStatus status)
    {
        await using var db = await factory.CreateDbContextAsync();
        var existing = await db.Requests.FirstOrDefaultAsync(r => r.Id == id);
        if (existing is null) return;
        existing.Status = status;
        await db.SaveChangesAsync();
    }
}
