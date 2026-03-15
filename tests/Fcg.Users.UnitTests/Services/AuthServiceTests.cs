using Fcg.Users.Application.Exceptions;
using Fcg.Users.Application.Services;
using Fcg.Users.Contracts.Auth;
using Fcg.Users.Domain.Entities;
using Fcg.Users.Domain.Enums;
using Fcg.Users.Domain.Repositories;
using NSubstitute;
using Xunit;

namespace Fcg.Users.UnitTests.Services;

public class AuthServiceTests
{
    private readonly IUserRepository _userRepo = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();

    [Fact]
    public async Task LoginAsync_WhenUserNotFound_ThrowsUnauthorized()
    {
        _userRepo.GetByEmailOrUsernameAsync(Arg.Any<string>(), false, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var sut = new AuthService(_userRepo, _hasher, _tokenService);
        var request = new LoginRequest { Login = "nobody@test.com", Password = "any" };

        await Assert.ThrowsAsync<UnauthorizedException>(() => sut.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordInvalid_ThrowsUnauthorized()
    {
        var user = new User { Id = Guid.NewGuid(), Username = "user1", Email = "u@t.com", PasswordHash = "hash", IsActive = true, Role = UserRole.User };
        _userRepo.GetByEmailOrUsernameAsync("u@t.com", false, Arg.Any<CancellationToken>()).Returns(user);
        _hasher.Verify("wrong", "hash").Returns(false);

        var sut = new AuthService(_userRepo, _hasher, _tokenService);
        var request = new LoginRequest { Login = "u@t.com", Password = "wrong" };

        await Assert.ThrowsAsync<UnauthorizedException>(() => sut.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_WhenValid_ReturnsLoginResponse()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "admin",
            Email = "admin@test.com",
            Name = "Admin",
            PasswordHash = "hash",
            IsActive = true,
            Role = UserRole.Admin
        };
        _userRepo.GetByEmailOrUsernameAsync("admin@test.com", false, Arg.Any<CancellationToken>()).Returns(user);
        _hasher.Verify("secret", "hash").Returns(true);
        _tokenService.GenerateAccessToken(user).Returns("jwt-token");
        _tokenService.GetExpirationSeconds().Returns(3600);

        var sut = new AuthService(_userRepo, _hasher, _tokenService);
        var request = new LoginRequest { Login = "admin@test.com", Password = "secret" };

        var result = await sut.LoginAsync(request);

        Assert.Equal("jwt-token", result.AccessToken);
        Assert.Equal("Bearer", result.TokenType);
        Assert.Equal(3600, result.ExpiresInSeconds);
        Assert.Equal(user.Id, result.User.Id);
        Assert.Equal("admin", result.User.Username);
        Assert.Equal("admin", result.User.Role);
    }
}
