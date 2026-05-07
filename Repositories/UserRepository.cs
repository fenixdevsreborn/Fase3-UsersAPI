using Microsoft.EntityFrameworkCore;
using ms_users.Infrastructure;
using ms_users.Models;
using System.Text.Json;

namespace ms_users.Repositories;

public interface IUserRepository
{
    Task<Users?> GetById(string id);
    Task<Users?> GetByEmail(string email);
    Task<Users> Create(Users user);
    Task<Users> Update(Users user);
    Task Disable(string id);
    Task<IEnumerable<Users>> GetAll();
}

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditRepository _audit;

    public UserRepository(ApplicationDbContext context, IAuditRepository audit)
    {
        _context = context;
        _audit = audit;
    }

    public async Task<Users?> GetById(string id)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<Users?> GetByEmail(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<Users> Create(Users user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        await _audit.Log(new AuditLog
        {
            TableName = "Users",
            Operation = "INSERT",
            UserId = user.Id,
            NewValues = JsonSerializer.Serialize(user)
        });

        return user;
    }

    public async Task<Users> Update(Users user)
    {
        var oldUser = await GetById(user.Id);

        if (oldUser == null)
            throw new InvalidOperationException($"User with ID {user.Id} not found");

        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        await _audit.Log(new AuditLog
        {
            TableName = "Users",
            Operation = "UPDATE",
            UserId = user.Id,
            OldValues = JsonSerializer.Serialize(oldUser),
            NewValues = JsonSerializer.Serialize(user)
        });

        return user;
    }

    public async Task Disable(string id)
    {
        var user = await GetById(id);

        if (user == null)
            throw new InvalidOperationException($"User with ID {id} not found");

        var oldUser = JsonSerializer.Serialize(user);

        user.Active = false;
        user.UpdatedAt = DateTime.UtcNow;

        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        await _audit.Log(new AuditLog
        {
            TableName = "Users",
            Operation = "UPDATE",
            UserId = user.Id,
            OldValues = oldUser,
            NewValues = JsonSerializer.Serialize(user)
        });
    }

    public async Task<IEnumerable<Users>> GetAll()
    {
        return await _context.Users.ToListAsync();
    }
}