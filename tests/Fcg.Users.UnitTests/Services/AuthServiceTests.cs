using Fcg.Users.Application.Exceptions;
using Fcg.Users.Application.Services;
using Fcg.Users.Contracts.Auth;
using Fcg.Users.Domain.Entities;
using Fcg.Users.Domain.Enums;
using Fcg.Users.Domain.Repositories;
using Moq;
using Xunit;

namespace Fcg.Users.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<ITokenService> _tokenService = new();

    [Fact]
    public async Task LoginAsync_WhenUserNotFound_ThrowsUnauthorized()
    {
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var sut = new AuthService(_userRepo.Object, _hasher.Object, _tokenService.Object);
        var request = new LoginRequest { Email = "nobody@test.com", Password = "any" };

        await Assert.ThrowsAsync<UnauthorizedException>(() => sut.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordInvalid_ThrowsUnauthorized()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "u@t.com", PasswordHash = "hash", IsActive = true, Role = UserRole.User };
        _userRepo.Setup(r => r.GetByEmailAsync("u@t.com", false, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _hasher.Setup(h => h.Verify("wrong", "hash")).Returns(false);

        var sut = new AuthService(_userRepo.Object, _hasher.Object, _tokenService.Object);
        var request = new LoginRequest { Email = "u@t.com", Password = "wrong" };

        await Assert.ThrowsAsync<UnauthorizedException>(() => sut.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_WhenValid_ReturnsLoginResponse()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@test.com",
            Name = "Admin",
            PasswordHash = "hash",
            IsActive = true,
            Role = UserRole.Admin
        };
        _userRepo.Setup(r => r.GetByEmailAsync("admin@test.com", false, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _hasher.Setup(h => h.Verify("secret", "hash")).Returns(true);
        _tokenService.Setup(t => t.GenerateAccessToken(user)).Returns("jwt-token");
        _tokenService.Setup(t => t.GetExpirationSeconds()).Returns(3600);

        var sut = new AuthService(_userRepo.Object, _hasher.Object, _tokenService.Object);
        var request = new LoginRequest { Email = "admin@test.com", Password = "secret" };

        var result = await sut.LoginAsync(request);

        Assert.Equal("jwt-token", result.AccessToken);
        Assert.Equal("Bearer", result.TokenType);
        Assert.Equal(3600, result.ExpiresInSeconds);
        Assert.Equal(user.Id, result.User.Id);
        Assert.Equal("admin", result.User.Role);
    }
}
