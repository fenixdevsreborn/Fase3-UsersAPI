using Fcg.Users.Contracts.Auth;
using Fcg.Users.Domain.Repositories;
using Fcg.Users.Application.Exceptions;

namespace Fcg.Users.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email.Trim(), includeDeleted: false, cancellationToken);
        if (user == null || !user.IsActive || user.IsDeleted)
            throw new UnauthorizedException();

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException();

        var accessToken = _tokenService.GenerateAccessToken(user);
        var expiresIn = _tokenService.GetExpirationSeconds();

        return new LoginResponse
        {
            AccessToken = accessToken,
            TokenType = "Bearer",
            ExpiresInSeconds = expiresIn,
            User = new UserInfo
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role.ToString().ToLowerInvariant()
            }
        };
    }
}
