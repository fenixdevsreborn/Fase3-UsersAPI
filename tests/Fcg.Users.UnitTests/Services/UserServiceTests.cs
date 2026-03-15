using Fcg.Users.Application.Exceptions;
using Fcg.Users.Application.Services;
using Fcg.Users.Contracts.Users;
using Fcg.Users.Domain.Entities;
using Fcg.Users.Domain.Enums;
using Fcg.Users.Domain.Paging;
using Fcg.Users.Domain.Repositories;
using NSubstitute;
using Xunit;

namespace Fcg.Users.UnitTests.Services;

public class UserServiceTests
{
    private readonly IUserRepository _userRepo = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();

    [Fact]
    public async Task CreateUserAsync_WhenEmailExists_ThrowsConflict()
    {
        _userRepo.ExistsByUsernameAsync(Arg.Any<string>(), null, Arg.Any<CancellationToken>()).Returns(false);
        _userRepo.ExistsByEmailAsync(Arg.Any<string>(), null, Arg.Any<CancellationToken>()).Returns(true);

        var sut = new UserService(_userRepo, _hasher);
        var request = new CreateUserRequest { Name = "A", Username = "userab", Email = "a@b.com", Password = "password123" };

        await Assert.ThrowsAsync<ConflictException>(() => sut.CreateUserAsync(request));
    }

    [Fact]
    public async Task GetUserByIdAsync_WhenNotFound_ReturnsNull()
    {
        _userRepo.GetByIdAsync(Arg.Any<Guid>(), false, Arg.Any<CancellationToken>()).Returns((User?)null);

        var sut = new UserService(_userRepo, _hasher);

        var result = await sut.GetUserByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserByIdAsync_WhenFound_ReturnsUserResponse()
    {
        var id = Guid.NewGuid();
        var user = new User
        {
            Id = id,
            Name = "Test",
            Username = "testuser",
            Email = "test@test.com",
            Role = UserRole.User,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _userRepo.GetByIdAsync(id, false, Arg.Any<CancellationToken>()).Returns(user);

        var sut = new UserService(_userRepo, _hasher);

        var result = await sut.GetUserByIdAsync(id);

        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("Test", result.Name);
        Assert.Equal("testuser", result.Username);
        Assert.Equal("user", result.Role);
    }
}
