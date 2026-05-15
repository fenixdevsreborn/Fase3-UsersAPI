using Moq;
using ms_users.Events;
using ms_users.Messaging;
using ms_users.Models;
using ms_users.Repositories;
using ms_users.Services;

namespace ms_users.Tests;

public class UserServiceTests
{
    [Fact]
    public async Task Register_WhenUserDoesNotExist_CreatesUserAndPublishesWelcomeNotification()
    {
        var repository = new Mock<IUserRepository>();
        var publisher = new Mock<IMessagePublisher>();
        var jwtService = new Mock<IJwtService>();
        Users? createdUser = null;

        repository
            .Setup(r => r.GetByEmail("player@test.com"))
            .ReturnsAsync((Users?)null);

        repository
            .Setup(r => r.Create(It.IsAny<Users>()))
            .Callback<Users>(user => createdUser = user)
            .ReturnsAsync((Users user) => user);

        var service = new UserService(repository.Object, publisher.Object, jwtService.Object);

        var user = await service.Register(new RegisterRequestUser
        {
            Email = "player@test.com",
            Password = "Password123!",
            Nickname = "player",
            Name = "Player One"
        });

        Assert.NotNull(createdUser);
        Assert.Equal(createdUser, user);
        Assert.Equal("player@test.com", user.Email);
        Assert.Equal("player", user.Nickname);
        Assert.Equal("Player One", user.Name);
        Assert.True(user.Active);
        Assert.NotEqual("Password123!", user.PasswordHash);
        Assert.Equal(36, Convert.FromBase64String(user.PasswordHash!).Length);

        repository.Verify(r => r.Create(It.IsAny<Users>()), Times.Once);
        publisher.Verify(
            p => p.PublishAsync(
                "notification-queue",
                It.Is<EmailNotificationEvent>(e =>
                    e.Recipient == "player@test.com" &&
                    e.Title == "Bem-vindo à Game Store")),
            Times.Once);
    }

    [Fact]
    public async Task Register_WhenEmailAlreadyExists_ThrowsInvalidOperationException()
    {
        var repository = new Mock<IUserRepository>();
        var publisher = new Mock<IMessagePublisher>();
        var jwtService = new Mock<IJwtService>();

        repository
            .Setup(r => r.GetByEmail("player@test.com"))
            .ReturnsAsync(new Users
            {
                Id = "existing-user",
                Email = "player@test.com",
                Nickname = "player",
                Name = "Player One"
            });

        var service = new UserService(repository.Object, publisher.Object, jwtService.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.Register(new RegisterRequestUser
            {
                Email = "player@test.com",
                Password = "Password123!",
                Nickname = "player",
                Name = "Player One"
            }));

        repository.Verify(r => r.Create(It.IsAny<Users>()), Times.Never);
        publisher.Verify(
            p => p.PublishAsync(It.IsAny<string>(), It.IsAny<EmailNotificationEvent>()),
            Times.Never);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokensAndStoresRefreshToken()
    {
        var repository = new Mock<IUserRepository>();
        var publisher = new Mock<IMessagePublisher>();
        var jwtService = new Mock<IJwtService>();
        Users? storedUser = null;

        repository
            .Setup(r => r.GetByEmail("player@test.com"))
            .ReturnsAsync(() => storedUser);

        repository
            .Setup(r => r.Create(It.IsAny<Users>()))
            .Callback<Users>(user => storedUser = user)
            .ReturnsAsync((Users user) => user);

        repository
            .Setup(r => r.Update(It.IsAny<Users>()))
            .Callback<Users>(user => storedUser = user)
            .ReturnsAsync((Users user) => user);

        jwtService
            .Setup(j => j.GenerateToken(It.IsAny<string>(), "player@test.com"))
            .Returns("access-token");

        var service = new UserService(repository.Object, publisher.Object, jwtService.Object);
        await service.Register(new RegisterRequestUser
        {
            Email = "player@test.com",
            Password = "Password123!",
            Nickname = "player",
            Name = "Player One"
        });

        var result = await service.Login("player@test.com", "Password123!");

        Assert.NotNull(result);
        Assert.Equal("access-token", GetPropertyValue<string>(result, "AccessToken"));
        Assert.False(string.IsNullOrWhiteSpace(GetPropertyValue<string>(result, "RefreshToken")));
        Assert.Equal(86400, GetPropertyValue<int>(result, "ExpiresIn"));
        Assert.False(string.IsNullOrWhiteSpace(storedUser?.RefreshToken));
        Assert.NotNull(storedUser?.UpdatedAt);

        repository.Verify(r => r.Update(It.IsAny<Users>()), Times.Once);
        jwtService.Verify(j => j.GenerateToken(storedUser!.Id, "player@test.com"), Times.Once);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ThrowsUnauthorizedAccessException()
    {
        var repository = new Mock<IUserRepository>();
        var publisher = new Mock<IMessagePublisher>();
        var jwtService = new Mock<IJwtService>();
        Users? storedUser = null;

        repository
            .Setup(r => r.GetByEmail("player@test.com"))
            .ReturnsAsync(() => storedUser);

        repository
            .Setup(r => r.Create(It.IsAny<Users>()))
            .Callback<Users>(user => storedUser = user)
            .ReturnsAsync((Users user) => user);

        var service = new UserService(repository.Object, publisher.Object, jwtService.Object);
        await service.Register(new RegisterRequestUser
        {
            Email = "player@test.com",
            Password = "Password123!",
            Nickname = "player",
            Name = "Player One"
        });

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.Login("player@test.com", "wrong-password"));

        repository.Verify(r => r.Update(It.IsAny<Users>()), Times.Never);
        jwtService.Verify(j => j.GenerateToken(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Update_WhenUserExists_UpdatesOnlyProvidedFields()
    {
        var repository = new Mock<IUserRepository>();
        var publisher = new Mock<IMessagePublisher>();
        var jwtService = new Mock<IJwtService>();
        var user = new Users
        {
            Id = "user-123",
            Email = "old@test.com",
            Nickname = "oldnick",
            Name = "Old Name"
        };

        repository.Setup(r => r.GetById("user-123")).ReturnsAsync(user);
        repository.Setup(r => r.Update(It.IsAny<Users>())).ReturnsAsync((Users updated) => updated);

        var service = new UserService(repository.Object, publisher.Object, jwtService.Object);

        var updatedUser = await service.Update("user-123", new UpdateUserRequest
        {
            Name = "New Name",
            Email = "new@test.com"
        });

        Assert.NotNull(updatedUser);
        Assert.Equal("New Name", updatedUser.Name);
        Assert.Equal("new@test.com", updatedUser.Email);
        Assert.Equal("oldnick", updatedUser.Nickname);
        Assert.NotNull(updatedUser.UpdatedAt);

        repository.Verify(r => r.Update(user), Times.Once);
    }

    private static T GetPropertyValue<T>(object source, string propertyName)
    {
        var value = source.GetType().GetProperty(propertyName)?.GetValue(source);
        return Assert.IsType<T>(value);
    }
}
