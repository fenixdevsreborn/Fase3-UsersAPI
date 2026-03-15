using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Fcg.Users.IntegrationTests;

public class WebAppFixture : WebApplicationFactory<Program>
{
    private static readonly string TestKeysPath;

    static WebAppFixture()
    {
        using var rsa = RSA.Create(2048);
        var pem = rsa.ExportRSAPrivateKeyPem();
        var dir = Path.Combine(Path.GetTempPath(), "FcgUsersTests_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(dir);
        var keyPath = Path.Combine(dir, "private.pem");
        File.WriteAllText(keyPath, pem);
        TestKeysPath = keyPath;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["UseInMemoryDatabase"] = "true",
                ["Jwt:Issuer"] = "https://localhost",
                ["Jwt:Audience"] = "fcg-cloud-platform",
                ["Jwt:ExpirationSeconds"] = "3600",
                ["Jwt:Signing:Provider"] = "File",
                ["Jwt:Signing:CurrentKeyId"] = "test-1",
                ["Jwt:Signing:FilePath"] = TestKeysPath,
                ["Bootstrap:CreateAdminIfNone"] = "true",
                ["Bootstrap:AdminEmail"] = "admin@fcg.local",
                ["Bootstrap:AdminPassword"] = "ChangeMe@123",
                ["Bootstrap:AdminName"] = "System Admin"
            });
        });
    }
}
