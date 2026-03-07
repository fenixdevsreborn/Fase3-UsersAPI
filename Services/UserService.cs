using ms_users.Repositories;
using ms_users.Messaging;
using ms_users.Events;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.CognitoIdentityProvider;

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

  public async Task<Users> Register(string email, string password)
  {
    var client = new AmazonCognitoIdentityProviderClient();

    var signUpRequest = new SignUpRequest
    {
      ClientId = Environment.GetEnvironmentVariable("COGNITO_CLIENT_ID"),
      Username = email,
      Password = password
    };

    var response = await client.SignUpAsync(signUpRequest);

    var cognitoSub = response.UserSub;

    var user = new Users
    {
      Id = cognitoSub,
      Email = email
    };

    await _repository.Create(user);

    // =============================
    // EVENTO DE USUÁRIO CRIADO
    // =============================

    var userEvent = new UserRegisteredEvent
    {
      UserId = user.Id,
      Email = user.Email
    };

    // =============================
    // EVENTO DE EMAIL
    // =============================

    var emailEvent = new EmailNotificationEvent
    {
      Title = "Bem-vindo à Game Store",
      Subtitle = "Sua conta foi criada com sucesso",
      Body = "Agora você pode comprar e jogar seus games favoritos.",
      Recipient = email
    };

    var notificationQueue =
        Environment.GetEnvironmentVariable("NOTIFICATION_QUEUE_URL");

    await _publisher.PublishAsync(notificationQueue, userEvent);

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
}