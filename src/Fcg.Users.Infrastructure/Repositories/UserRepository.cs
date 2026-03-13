using Fcg.Users.Domain.Entities;
using Fcg.Users.Domain.Enums;
using Fcg.Users.Domain.Paging;
using Fcg.Users.Domain.Repositories;
using Fcg.Users.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fcg.Users.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UsersDbContext _db;

    public UserRepository(UsersDbContext db)
    {
        _db = db;
    }

    public async Task<User?> GetByIdAsync(Guid id, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        var query = _db.Users.AsNoTracking();
        if (includeDeleted)
            query = query.IgnoreQueryFilters();
        return await query.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Users.FirstOrDefaultAsync(u => u.Id == id && u.DeletedAt == null, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        var query = _db.Users.AsNoTracking();
        if (includeDeleted)
            query = query.IgnoreQueryFilters();
        return await query.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, Guid? excludeUserId = null, CancellationToken cancellationToken = default)
    {
        var query = _db.Users.IgnoreQueryFilters().Where(u => u.Email == email);
        if (excludeUserId.HasValue)
            query = query.Where(u => u.Id != excludeUserId.Value);
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<PagedResult<User>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? name = null,
        string? email = null,
        UserRole? role = null,
        bool? isActive = null,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Users.AsNoTracking();
        if (includeDeleted)
            query = query.IgnoreQueryFilters();

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(u => u.Name.ToLower().Contains(name.ToLower()));
        if (!string.IsNullOrWhiteSpace(email))
            query = query.Where(u => u.Email.ToLower().Contains(email.ToLower()));
        if (role.HasValue)
            query = query.Where(u => u.Role == role.Value);
        if (isActive.HasValue)
            query = query.Where(u => u.IsActive == isActive.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(u => u.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<User>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _db.Users.Update(user);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> CountAdminsAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Users.IgnoreQueryFilters()
            .CountAsync(u => u.Role == UserRole.Admin && u.DeletedAt == null, cancellationToken);
    }
}
