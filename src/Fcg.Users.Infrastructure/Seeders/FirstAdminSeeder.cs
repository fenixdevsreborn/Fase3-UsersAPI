using Fcg.Users.Application.Services;
using Fcg.Users.Domain.Entities;
using Fcg.Users.Domain.Enums;
using Fcg.Users.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Fcg.Users.Infrastructure.Seeders;

public sealed class FirstAdminSeeder : IDataSeeder
{
    public int Order => 0;

    private const string DefaultUsername = "admin";
    private const string DefaultEmail = "admin@fcg.local";
    private const string DefaultPassword = "Admin@123";
    private const string DefaultName = "System Admin";

    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<FirstAdminSeeder> _logger;

    public FirstAdminSeeder(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ILogger<FirstAdminSeeder> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var adminCount = await _userRepository.CountAdminsAsync(cancellationToken).ConfigureAwait(false);
        if (adminCount > 0)
            return;

        var username = DefaultUsername.Trim().ToLowerInvariant();
        var email = DefaultEmail.Trim().ToLowerInvariant();
        var password = DefaultPassword;
        var name = DefaultName;

        var admin = new User
        {
            Id = Guid.NewGuid(),
            Name = name,
            Username = username,
            Email = email,
            PasswordHash = _passwordHasher.Hash(password),
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(admin, cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Bootstrap: first admin created with username {Username}", username);
    }
}
