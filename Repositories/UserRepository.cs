using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using ms_users.Models;

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

  public async Task<Users> Get(string id)
  {
    return await _context.LoadAsync<Users>(id);
  }
}