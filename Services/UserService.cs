using ms_users.Repositories;
using ms_users.Messaging;
using ms_users.Events;
using ms_users.Models;
using System.Security.Cryptography;

namespace ms_users.Services;

public class UserService
{
    private readonly IUserRepository _repository;
    private readonly IMessagePublisher _publisher;
    private readonly IJwtService _jwtService;

    public UserService(
        IUserRepository repository,
        IMessagePublisher publisher,
        IJwtService jwtService)
    {
        _repository = repository;
        _publisher = publisher;
        _jwtService = jwtService;
    }

    public async Task<Users> Register(RegisterRequestUser request)
    {
        if (request?.Email == null || request.Password == null)
            throw new ArgumentNullException(nameof(request));

        // Check if user already exists
        var existingUser = await _repository.GetByEmail(request.Email);
        if (existingUser != null)
            throw new InvalidOperationException("User already exists");

        var hashedPassword = HashPassword(request.Password);
        var userId = Guid.NewGuid().ToString();

        var user = new Users
        {
            Id = userId,
            Email = request.Email,
            Nickname = request.Nickname,
            Name = request.Name,
            PasswordHash = hashedPassword,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.Create(user);

        // Publish event to RabbitMQ
        var emailEvent = new EmailNotificationEvent
        {
            Title = "Bem-vindo à Game Store",
            Subtitle = "Sua conta foi criada com sucesso",
            Body = "Agora você pode comprar e jogar seus games favoritos.",
            Recipient = request.Email
        };

        await _publisher.PublishAsync("notification-queue", emailEvent);

        return user;
    }

    public async Task<object> Login(string email, string password)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            throw new ArgumentNullException();

        var user = await _repository.GetByEmail(email);
        if (user == null)
            throw new UnauthorizedAccessException("Invalid credentials");

        if (!VerifyPassword(password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials");

        var accessToken = _jwtService.GenerateToken(user.Id, user.Email);
        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.UpdatedAt = DateTime.UtcNow;
        await _repository.Update(user);

        return new
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = 86400
        };
    }

    public async Task<Users?> GetById(string id)
    {
        return await _repository.GetById(id);
    }

    public async Task<Users?> Update(string userId, UpdateUserRequest request)
    {
        var user = await _repository.GetById(userId);
        if (user == null)
            return null;

        // Update only non-null fields
        if (!string.IsNullOrEmpty(request.Name))
            user.Name = request.Name;

        if (!string.IsNullOrEmpty(request.Nickname))
            user.Nickname = request.Nickname;

        if (!string.IsNullOrEmpty(request.Email))
            user.Email = request.Email;

        user.UpdatedAt = DateTime.UtcNow;

        await _repository.Update(user);

        return user;
    }

    public async Task Disable(string userId)
    {
        await _repository.Disable(userId);
    }

    private string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(20);

            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);

            return Convert.ToBase64String(hashBytes);
        }
    }

    private bool VerifyPassword(string password, string? hash)
    {
        if (string.IsNullOrEmpty(hash))
            return false;

        byte[] hashBytes = Convert.FromBase64String(hash);
        byte[] salt = new byte[16];
        Array.Copy(hashBytes, 0, salt, 0, 16);

        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
        byte[] hash2 = pbkdf2.GetBytes(20);

        for (int i = 0; i < 20; i++)
        {
            if (hashBytes[i + 16] != hash2[i])
                return false;
        }
        return true;
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
        }
        return Convert.ToBase64String(randomNumber);
    }
}