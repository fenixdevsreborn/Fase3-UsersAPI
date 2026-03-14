using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

namespace ms_users.Repositories;

public class UserRepository
{
  private readonly DynamoDBContext _context;

  public UserRepository()
  {
    var client = new AmazonDynamoDBClient();
    _context = new DynamoDBContext(client);
  }

  public async Task Create(Users users)
  {
    await _context.SaveAsync(users);
  }

  public async Task<Users?> GetById(string id)
  {
    return await _context.LoadAsync<Users>(id);
  }
  public async Task Update(Users user)
  {
    await _context.SaveAsync(user);
  }

  public async Task Disable(string id)
  {
    var user = await _context.LoadAsync<Users>(id);

    if (user == null)
      return;

    user.Active = false;
    user.UpdatedAt = DateTime.UtcNow;

    await _context.SaveAsync(user);
  }
}