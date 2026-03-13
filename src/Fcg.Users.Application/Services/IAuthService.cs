using Fcg.Users.Contracts.Auth;

namespace Fcg.Users.Application.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
