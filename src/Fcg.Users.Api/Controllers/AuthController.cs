using Fcg.Users.Contracts.Auth;
using Fcg.Users.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Fcg.Users.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>Authenticate and receive a JWT Bearer token.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var response = await _authService.LoginAsync(request, cancellationToken);
            _logger.LogInformation("User logged in: {Login}", request.Login);
            return Ok(response);
        }
        catch (Fcg.Users.Application.Exceptions.UnauthorizedException)
        {
            _logger.LogWarning("Failed login attempt for {Login}", request.Login);
            return Unauthorized(new { message = "Invalid email/username or password." });
        }
    }
}
