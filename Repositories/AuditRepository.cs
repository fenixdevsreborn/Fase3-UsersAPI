using ms_users.Infrastructure;
using ms_users.Models;

namespace ms_users.Repositories;

public interface IAuditRepository
{
    Task Log(AuditLog log);
    Task<IEnumerable<AuditLog>> GetByUserId(string userId);
    Task<IEnumerable<AuditLog>> GetByTableName(string tableName);
}

public class AuditRepository : IAuditRepository
{
    private readonly ApplicationDbContext _context;

    public AuditRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Log(AuditLog log)
    {
        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByUserId(string userId)
    {
        return _context.AuditLogs
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.Timestamp)
            .ToList();
    }

    public async Task<IEnumerable<AuditLog>> GetByTableName(string tableName)
    {
        return _context.AuditLogs
            .Where(a => a.TableName == tableName)
            .OrderByDescending(a => a.Timestamp)
            .ToList();
    }
}