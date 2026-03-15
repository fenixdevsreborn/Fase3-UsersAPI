using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Fcg.Users.Contracts.Auth;
using Fcg.Users.Domain.Entities;
using Fcg.Users.Domain.Enums;
using Fcg.Users.Infrastructure.Authentication;
using Fcg.Users.Infrastructure.Services;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Fcg.Users.UnitTests.Services;

public class JwtTokenServiceTests
{
    private static (IRsaKeyProvider Provider, RSA PublicKey) CreateKeyProvider()
    {
        var rsa = RSA.Create(2048);
        var publicKey = RSA.Create();
        publicKey.ImportRSAPublicKey(rsa.ExportRSAPublicKey(), out _);
        var keyId = "test-kid";
        var provider = Substitute.For<IRsaKeyProvider>();
        provider.CurrentKeyId.Returns(keyId);
        provider.GetPrivateKeyForSigning().Returns(rsa);
        provider.GetPublicKeysByKeyId().Returns(new Dictionary<string, RSA> { [keyId] = publicKey });
        return (provider, publicKey);
    }

    [Fact]
    public void GenerateAccessToken_ProducesValidTokenWithKidAndRS256()
    {
        var (provider, publicKey) = CreateKeyProvider();
        var options = Options.Create(new JwtOptions
        {
            Issuer = "https://test.example.com",
            Audience = "fcg-cloud-platform",
            ExpirationSeconds = 3600
        });
        var sut = new JwtTokenService(options, provider);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "user1",
            Email = "u@test.com",
            Name = "User",
            Role = UserRole.User,
            IsActive = true
        };

        var token = sut.GenerateAccessToken(user);

        Assert.NotEmpty(token);
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        Assert.Equal("test-kid", jwt.Header.Kid);
        Assert.Equal("RS256", jwt.Header.Alg);
        Assert.Equal("https://test.example.com", jwt.Issuer);
        Assert.Equal("fcg-cloud-platform", jwt.Audiences.FirstOrDefault());
    }

    [Fact]
    public void GenerateAccessToken_ContainsRequiredClaims()
    {
        var (provider, _) = CreateKeyProvider();
        var options = Options.Create(new JwtOptions { Issuer = "https://issuer", Audience = "aud", ExpirationSeconds = 60 });
        var sut = new JwtTokenService(options, provider);
        var user = new User
        {
            Id = Guid.Parse("11111111-2222-3333-4444-555555555555"),
            Username = "testadmin",
            Email = "a@b.com",
            Name = "Test",
            Role = UserRole.Admin,
            IsActive = true
        };

        var token = sut.GenerateAccessToken(user);
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        Assert.Equal(user.Id.ToString(), jwt.Claims.First(c => c.Type == "sub").Value);
        Assert.Equal("a@b.com", jwt.Claims.First(c => c.Type == "email").Value);
        Assert.Equal("testadmin", jwt.Claims.First(c => c.Type == "username").Value);
        Assert.Equal("Test", jwt.Claims.First(c => c.Type == "name").Value);
        Assert.Equal("admin", jwt.Claims.First(c => c.Type == "role").Value);
        Assert.NotNull(jwt.Claims.FirstOrDefault(c => c.Type == "scope"));
        Assert.NotNull(jwt.Claims.FirstOrDefault(c => c.Type == "jti"));
    }

    [Fact]
    public void GetExpirationSeconds_ReturnsConfiguredValue()
    {
        var (provider, _) = CreateKeyProvider();
        var options = Options.Create(new JwtOptions { ExpirationSeconds = 7200 });
        var sut = new JwtTokenService(options, provider);
        Assert.Equal(7200, sut.GetExpirationSeconds());
    }
}
