using Fcg.Users.Domain.Entities;

namespace Fcg.Users.Application.Services;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    int GetExpirationSeconds();
}
