using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using ms_users.Models;
using System.Text.Json;

namespace ms_users.Repositories;

public class UserRepository
{
  private readonly DynamoDBContext _context;
  private readonly AuditRepository _audit;

  public UserRepository()
  {
    var client = new AmazonDynamoDBClient();
    _context = new DynamoDBContext(client);
    _audit = new AuditRepository();
  }

  public async Task Create(Users users)
  {
    await _context.SaveAsync(users);

    await _audit.Log(new AuditLog
    {
      TableName = "Users",
      Operation = "INSERT",
      UserId = users.Id,
      NewValues = JsonSerializer.Serialize(users)
    });
  }

  public async Task<Users?> GetById(string id)
  {
    return await _context.LoadAsync<Users>(id);
  }

  public async Task Update(Users user)
  {
    var oldUser = await _context.LoadAsync<Users>(user.Id);

    await _context.SaveAsync(user);

    await _audit.Log(new AuditLog
    {
      TableName = "Users",
      Operation = "UPDATE",
      UserId = user.Id,
      OldValues = JsonSerializer.Serialize(oldUser),
      NewValues = JsonSerializer.Serialize(user)
    });
  }

  public async Task Disable(string id)
  {
    var user = await _context.LoadAsync<Users>(id);

    if (user == null)
      return;

    var oldUser = JsonSerializer.Serialize(user);

    user.Active = false;
    user.UpdatedAt = DateTime.UtcNow;

    await _context.SaveAsync(user);

    await _audit.Log(new AuditLog
    {
      TableName = "Users",
      Operation = "UPDATE",
      UserId = user.Id,
      OldValues = oldUser,
      NewValues = JsonSerializer.Serialize(user)
    });
  }
}