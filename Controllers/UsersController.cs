using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ms_users.Models;
using ms_users.Models.DTOs;
using ms_users.Services;
using System.Security.Claims;

namespace ms_users.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly UserService _service;
    private readonly ILogger<UsersController> _logger;

    public UsersController(UserService service, ILogger<UsersController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestUser request)
    {
        var user = await _service.Register(request);
        var dto = MapToDto(user);
        return Created($"/api/users/{user.Id}", dto);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _service.Login(request.Email, request.Password);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("debug")]
    public IActionResult Debug()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();

        _logger.LogInformation("Debug endpoint called. Claims: {Claims}",
            string.Join("; ", claims.Select(c => $"{c.Type}={c.Value}")));

        return Ok(new
        {
            IsAuthenticated = User.Identity?.IsAuthenticated,
            IdentityName = User.Identity?.Name,
            AuthenticationType = User.Identity?.AuthenticationType,
            Claims = claims,
            ClaimsCount = claims.Count
        });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userId = GetUserIdFromToken();

        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Could not extract userId from token");
            return Unauthorized(new { error = "Invalid token: no user ID found" });
        }

        _logger.LogInformation("Getting user info for userId: {UserId}", userId);

        var user = await _service.GetById(userId);

        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", userId);
            return NotFound();
        }

        return Ok(MapToDto(user));
    }

    [Authorize]
    [HttpPut("me")]
    public async Task<IActionResult> Update([FromBody] UpdateUserRequest request)
    {
        var userId = GetUserIdFromToken();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { error = "Invalid token: no user ID found" });

        var user = await _service.Update(userId, request);

        if (user == null)
            return NotFound();

        return Ok(MapToDto(user));
    }

    [Authorize]
    [HttpDelete("me")]
    public async Task<IActionResult> Delete()
    {
        var userId = GetUserIdFromToken();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { error = "Invalid token: no user ID found" });

        await _service.Disable(userId);

        return NoContent();
    }

    // Helper method to extract userId from token
    private string GetUserIdFromToken()
    {
        var userId = User.FindFirst("sub")?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("user_id")?.Value
            ?? User.FindFirst("uid")?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            _logger.LogInformation("UserId extracted: {UserId}", userId);
            return userId;
        }

        _logger.LogWarning("No userId found in claims. Available claims: {Claims}",
            string.Join(", ", User.Claims.Select(c => c.Type)));

        return null;
    }

    private UserDto MapToDto(Users user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Nickname = user.Nickname,
            Name = user.Name,
            Active = user.Active,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}