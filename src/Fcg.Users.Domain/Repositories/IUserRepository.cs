using Fcg.Users.Domain.Entities;
using Fcg.Users.Domain.Enums;
using Fcg.Users.Domain.Paging;

namespace Fcg.Users.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, bool includeDeleted = false, CancellationToken cancellationToken = default);
    Task<User?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, bool includeDeleted = false, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, Guid? excludeUserId = null, CancellationToken cancellationToken = default);
    Task<PagedResult<User>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? name = null,
        string? email = null,
        UserRole? role = null,
        bool? isActive = null,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default);
    Task<User> AddAsync(User user, CancellationToken cancellationToken = default);
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task<int> CountAdminsAsync(CancellationToken cancellationToken = default);
}
