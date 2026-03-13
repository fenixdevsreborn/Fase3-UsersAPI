using Fcg.Users.Application.Exceptions;
using Fcg.Users.Application.Services;
using Fcg.Users.Contracts.Users;
using Fcg.Users.Domain.Entities;
using Fcg.Users.Domain.Enums;
using Fcg.Users.Domain.Paging;
using Fcg.Users.Domain.Repositories;
using Moq;
using Xunit;

namespace Fcg.Users.UnitTests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IPasswordHasher> _hasher = new();

    [Fact]
    public async Task CreateUserAsync_WhenEmailExists_ThrowsConflict()
    {
        _userRepo.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var sut = new UserService(_userRepo.Object, _hasher.Object);
        var request = new CreateUserRequest { Name = "A", Email = "a@b.com", Password = "password123" };

        await Assert.ThrowsAsync<ConflictException>(() => sut.CreateUserAsync(request));
    }

    [Fact]
    public async Task GetUserByIdAsync_WhenNotFound_ReturnsNull()
    {
        _userRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), false, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var sut = new UserService(_userRepo.Object, _hasher.Object);

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
            Email = "test@test.com",
            Role = UserRole.User,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _userRepo.Setup(r => r.GetByIdAsync(id, false, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var sut = new UserService(_userRepo.Object, _hasher.Object);

        var result = await sut.GetUserByIdAsync(id);

        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("Test", result.Name);
        Assert.Equal("user", result.Role);
    }
}
