using Fcg.Users.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fcg.Users.Api.Controllers;

/// <summary>JWKS endpoint for API Gateway JWT authorizer. OIDC discovery is at GET /.well-known/openid-configuration (minimal API).</summary>
[ApiController]
[Route(".well-known")]
[AllowAnonymous]
public sealed class OpenIdDiscoveryController : ControllerBase
{
    private readonly IJwksService _jwksService;

    public OpenIdDiscoveryController(IJwksService jwksService)
    {
        _jwksService = jwksService;
    }

    /// <summary>JSON Web Key Set. Only public keys; used for JWT signature verification (API Gateway, clients).</summary>
    [HttpGet("jwks.json")]
    [Produces("application/json")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
    public IActionResult GetJwks()
    {
        var json = _jwksService.GetJwksJson();
        return Content(json, "application/json");
    }
}
