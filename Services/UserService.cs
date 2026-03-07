using ms_users.Models;
using ms_users.Repositories;
using ms_users.Events;
using ms_users.Messaging;

namespace ms_users.Services;

public class UserService
{
    private readonly UserRepository _repository;
    private readonly PaymentPublisher _publisher;

    public UserService(UserRepository repository, PaymentPublisher publisher)
    {
        _repository = repository;
        _publisher = publisher;
    }

    public async Task<Users> Register(string email, string password)
    {
        var existingUser = await _repository.GetByEmailAsync(email);
        if (existingUser != null) throw new Exception("E-mail já cadastrado.");

        var user = new Users
        {
            Id = Guid.NewGuid().ToString(),
            Email = email,
            Password = BCrypt.Net.BCrypt.HashPassword(password)
        };

        await _repository.Create(user);

        var evt = new UserRegisteredEvent
        {
            UserId = user.Id,
            Email = user.Email
        };

        await _publisher.PublishAsync(evt);

        return user;
    }

    public async Task<string?> Login(string email, string password)
    {
        var user = await _repository.GetByEmailAsync(email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
            return null;

        return $"mock-jwt-token-{user.Id}"; // Substituir pelo JWT
    }

    public async Task<Users?> GetProfile(string id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<Users?> UpdateCredentials(string id, Users request)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user == null) return null;

        if (!string.IsNullOrWhiteSpace(request.Email) && user.Email != request.Email)
        {
            var existingUser = await _repository.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                throw new Exception("Este e-mail já está em uso por outra conta.");
            }
            user.Email = request.Email;
        }

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
        }

        await _repository.UpdateAsync(id, user);

        return user;
    }
}