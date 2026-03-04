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
    var user = new Users
    {
      Id = Guid.NewGuid().ToString(),
      Email = email,
      Password = password
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
}