using Fcg.Users.Contracts.Paging;
using Fcg.Users.Contracts.Users;
using Fcg.Users.Domain.Entities;
using Fcg.Users.Domain.Enums;
using Fcg.Users.Domain.Paging;
using Fcg.Users.Domain.Repositories;
using Fcg.Users.Application.Exceptions;

namespace Fcg.Users.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserResponse> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var username = request.Username.Trim().ToLowerInvariant();
        if (username.Length < 4)
            throw new ConflictException("Username must be at least 4 characters.");
        if (await _userRepository.ExistsByUsernameAsync(username, null, cancellationToken))
            throw new ConflictException($"A user with username '{request.Username}' already exists.");

        var email = request.Email.Trim().ToLowerInvariant();
        if (await _userRepository.ExistsByEmailAsync(email, null, cancellationToken))
            throw new ConflictException($"A user with email '{request.Email}' already exists.");

        var role = ParseRole(request.Role);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Username = username,
            Email = email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = role,
            IsActive = request.IsActive,
            AvatarUrl = string.IsNullOrWhiteSpace(request.AvatarUrl) ? null : request.AvatarUrl.Trim(),
            Bio = string.IsNullOrWhiteSpace(request.Bio) ? null : request.Bio.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user, cancellationToken);
        return MapToResponse(user);
    }

    public async Task<PagedResponse<UserResponse>> GetUsersAsync(UserListQuery query, CancellationToken cancellationToken = default)
    {
        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        UserRole? roleFilter = null;
        if (!string.IsNullOrWhiteSpace(query.Role) && Enum.TryParse<UserRole>(query.Role, true, out var r))
            roleFilter = r;

        var paged = await _userRepository.GetPagedAsync(
            pageNumber,
            pageSize,
            name: string.IsNullOrWhiteSpace(query.Name) ? null : query.Name.Trim(),
            username: string.IsNullOrWhiteSpace(query.Username) ? null : query.Username.Trim(),
            email: string.IsNullOrWhiteSpace(query.Email) ? null : query.Email.Trim(),
            role: roleFilter,
            isActive: query.IsActive,
            includeDeleted: false,
            cancellationToken);

        return new PagedResponse<UserResponse>
        {
            Items = paged.Items.Select(MapToResponse).ToList(),
            TotalCount = paged.TotalCount,
            PageNumber = paged.PageNumber,
            PageSize = paged.PageSize,
            TotalPages = paged.TotalPages,
            HasPreviousPage = paged.HasPreviousPage,
            HasNextPage = paged.HasNextPage
        };
    }

    public async Task<UserResponse?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, includeDeleted: false, cancellationToken);
        return user == null ? null : MapToResponse(user);
    }

    public async Task<UserResponse?> UpdateUserAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdForUpdateAsync(id, cancellationToken);
        if (user == null) return null;

        if (request.Name != null) user.Name = request.Name.Trim();
        if (request.Username != null)
        {
            var username = request.Username.Trim().ToLowerInvariant();
            if (username.Length < 4)
                throw new ConflictException("Username must be at least 4 characters.");
            if (await _userRepository.ExistsByUsernameAsync(username, id, cancellationToken))
                throw new ConflictException($"A user with username '{request.Username}' already exists.");
            user.Username = username;
        }
        if (request.Email != null)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            if (await _userRepository.ExistsByEmailAsync(email, id, cancellationToken))
                throw new ConflictException($"A user with email '{request.Email}' already exists.");
            user.Email = email;
        }
        if (request.Password != null && !string.IsNullOrWhiteSpace(request.Password))
            user.PasswordHash = _passwordHasher.Hash(request.Password);
        if (request.Role != null && Enum.TryParse<UserRole>(request.Role, true, out var role))
            user.Role = role;
        if (request.IsActive.HasValue) user.IsActive = request.IsActive.Value;
        if (request.AvatarUrl != null) user.AvatarUrl = string.IsNullOrWhiteSpace(request.AvatarUrl) ? null : request.AvatarUrl.Trim();
        if (request.Bio != null) user.Bio = string.IsNullOrWhiteSpace(request.Bio) ? null : request.Bio.Trim();

        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, cancellationToken);
        return MapToResponse(user);
    }

    public async Task<bool> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdForUpdateAsync(id, cancellationToken);
        if (user == null) return false;
        user.DeletedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, cancellationToken);
        return true;
    }

    public async Task<UserResponse?> GetMeAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, includeDeleted: false, cancellationToken);
        return user == null ? null : MapToResponse(user);
    }

    public async Task<UserResponse?> UpdateMeAsync(Guid userId, UpdateMeRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdForUpdateAsync(userId, cancellationToken);
        if (user == null) return null;

        if (request.Name != null) user.Name = request.Name.Trim();
        if (request.AvatarUrl != null) user.AvatarUrl = string.IsNullOrWhiteSpace(request.AvatarUrl) ? null : request.AvatarUrl.Trim();
        if (request.Bio != null) user.Bio = string.IsNullOrWhiteSpace(request.Bio) ? null : request.Bio.Trim();

        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, cancellationToken);
        return MapToResponse(user);
    }

    public async Task<bool> DeleteMeAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdForUpdateAsync(userId, cancellationToken);
        if (user == null) return false;
        user.DeletedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, cancellationToken);
        return true;
    }

    private static UserResponse MapToResponse(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Name = user.Name,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role.ToString().ToLowerInvariant(),
            IsActive = user.IsActive,
            AvatarUrl = user.AvatarUrl,
            Bio = user.Bio,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }

    private static UserRole ParseRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role)) return UserRole.User;
        return Enum.TryParse<UserRole>(role.Trim(), true, out var r) ? r : UserRole.User;
    }
}
