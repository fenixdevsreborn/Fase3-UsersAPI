using Fcg.Users.Contracts.Paging;
using Fcg.Users.Contracts.Users;

namespace Fcg.Users.Application.Services;

public interface IUserService
{
    Task<UserResponse> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<PagedResponse<UserResponse>> GetUsersAsync(UserListQuery query, CancellationToken cancellationToken = default);
    Task<UserResponse?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserResponse?> UpdateUserAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserResponse?> GetMeAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserResponse?> UpdateMeAsync(Guid userId, UpdateMeRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteMeAsync(Guid userId, CancellationToken cancellationToken = default);
}
