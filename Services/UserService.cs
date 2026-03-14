using ms_users.Repositories;
using ms_users.Messaging;
using ms_users.Events;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.CognitoIdentityProvider;
using ms_users.Models;

namespace ms_users.Services;

public class UserService
{
  private readonly UserRepository _repository;
  private readonly EventPublisher _publisher;

  public UserService(UserRepository repository, EventPublisher publisher)
  {
    _repository = repository;
    _publisher = publisher;
  }

  public async Task<Users> Register(RegisterRequestUser request)
  {
    var client = new AmazonCognitoIdentityProviderClient();

    var signUpRequest = new SignUpRequest
    {
      ClientId = Environment.GetEnvironmentVariable("COGNITO_CLIENT_ID"),
      Username = request.Nickname,
      Password = request.Password,
      UserAttributes = new List<AttributeType>
        {
            new AttributeType
            {
                Name = "email",
                Value = request.Email
            },
            new AttributeType
            {
                Name = "nickname",
                Value = request.Nickname
            },
            new AttributeType
            {
                Name = "name",
                Value = request.Name
            }
        }
    };

    var response = await client.SignUpAsync(signUpRequest);

    await client.AdminConfirmSignUpAsync(new AdminConfirmSignUpRequest
    {
      UserPoolId = Environment.GetEnvironmentVariable("COGNITO_USER_POOL_ID"),
      Username = request.Nickname
    });

    await client.AdminUpdateUserAttributesAsync(new AdminUpdateUserAttributesRequest
    {
      UserPoolId = Environment.GetEnvironmentVariable("COGNITO_USER_POOL_ID"),
      Username = request.Nickname,
      UserAttributes = new List<AttributeType>
      {
          new AttributeType
          {
              Name = "email_verified",
              Value = "true"
          }
      }
    });

    var cognitoSub = response.UserSub;

    var user = new Users
    {
      Id = cognitoSub,
      Email = request.Email,
      Nickname = request.Nickname,
      Name = request.Name,
    };

    await _repository.Create(user);

    var emailEvent = new EmailNotificationEvent
    {
      Title = "Bem-vindo à Game Store",
      Subtitle = "Sua conta foi criada com sucesso",
      Body = "Agora você pode comprar e jogar seus games favoritos.",
      Recipient = request.Email
    };

    var notificationQueue =
        Environment.GetEnvironmentVariable("NOTIFICATION_QUEUE_URL");

    await _publisher.PublishAsync(notificationQueue, emailEvent);

    return user;
  }

  public async Task<object> Login(string email, string password)
  {
    var client = new AmazonCognitoIdentityProviderClient();

    var request = new InitiateAuthRequest
    {
      AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
      ClientId = Environment.GetEnvironmentVariable("COGNITO_CLIENT_ID"),
      AuthParameters = new Dictionary<string, string>
        {
            { "USERNAME", email },
            { "PASSWORD", password }
        }
    };

    var response = await client.InitiateAuthAsync(request);

    return new
    {
      IdToken = response.AuthenticationResult.IdToken,
      AccessToken = response.AuthenticationResult.AccessToken,
      RefreshToken = response.AuthenticationResult.RefreshToken
    };
  }

  public async Task<Users?> GetById(string id)
  {
    return await _repository.GetById(id);
  }

  public async Task<Users?> Update(string id, UpdateUserRequest request)
  {
    var user = await _repository.GetById(id);

    if (user == null)
      return null;

    user.Name = request.Name;
    user.Nickname = request.Nickname;
    user.UpdatedAt = DateTime.UtcNow;

    await _repository.Update(user);

    return user;
  }

  public async Task Disable(string id)
  {
    await _repository.Disable(id);
  }
}