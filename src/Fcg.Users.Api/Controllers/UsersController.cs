using Fcg.Users.Api.Authorization;
using Fcg.Users.Api.Observability;
using Fcg.Users.Application.Exceptions;
using Fcg.Users.Application.Services;
using Fcg.Users.Contracts.Auth;
using Fcg.Users.Contracts.Paging;
using Fcg.Users.Contracts.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fcg.Users.Api.Controllers;

[ApiController]
[Route("users")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;
    private readonly FcgMeters _meters;

    public UsersController(IUserService userService, ILogger<UsersController> logger, FcgMeters meters)
    {
        _userService = userService;
        _logger = logger;
        _meters = meters;
    }

    // ----- Admin-only endpoints -----

    /// <summary>Create a new user (Admin only).</summary>
    [HttpPost]
    [Authorize(Policy = FcgPolicies.RequireAdmin)]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponse>> CreateUser([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var user = await _userService.CreateUserAsync(request, cancellationToken);
            _meters.RecordUserCreated();
            _logger.LogInformation("User created: {Email} by admin", user.Email);
            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
        }
        catch (ConflictException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>List users with pagination and filters (Admin only).</summary>
    [HttpGet]
    [Authorize(Policy = FcgPolicies.RequireAdmin)]
    [ProducesResponseType(typeof(PagedResponse<UserResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<UserResponse>>> GetUsers([FromQuery] UserListQuery query, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _userService.GetUsersAsync(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>Get user by id (Admin only).</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = FcgPolicies.RequireAdmin)]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetUserById(Guid id, CancellationToken cancellationToken)
    {
        var user = await _userService.GetUserByIdAsync(id, cancellationToken);
        if (user == null)
            return NotFound(new { message = "User not found." });
        return Ok(user);
    }

    /// <summary>Update user by id (Admin only).</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = FcgPolicies.RequireAdmin)]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponse>> UpdateUser(Guid id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var user = await _userService.UpdateUserAsync(id, request, cancellationToken);
            if (user == null)
                return NotFound(new { message = "User not found." });
            _logger.LogInformation("User updated: {Id} by admin", id);
            return Ok(user);
        }
        catch (ConflictException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>Delete user by id (Admin only).</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = FcgPolicies.RequireAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _userService.DeleteUserAsync(id, cancellationToken);
        if (!deleted)
            return NotFound(new { message = "User not found." });
        _meters.RecordUserDeleted();
        _logger.LogInformation("User deleted: {Id} by admin", id);
        return NoContent();
    }

    // ----- Authenticated user (self) endpoints -----

    /// <summary>Get current user profile.</summary>
    [HttpGet("me")]
    [Authorize(Policy = FcgPolicies.RequireAuthenticatedUser)]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetMe(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var user = await _userService.GetMeAsync(userId.Value, cancellationToken);
        if (user == null)
            return NotFound(new { message = "User not found." });
        return Ok(user);
    }

    /// <summary>Update current user profile (name, avatar, bio only).</summary>
    [HttpPut("me")]
    [Authorize(Policy = FcgPolicies.RequireAuthenticatedUser)]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> UpdateMe([FromBody] UpdateMeRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userService.UpdateMeAsync(userId.Value, request, cancellationToken);
        if (user == null)
            return NotFound(new { message = "User not found." });
        _logger.LogInformation("User updated own profile: {Id}", userId);
        return Ok(user);
    }

    /// <summary>Delete current user account (soft delete).</summary>
    [HttpDelete("me")]
    [Authorize(Policy = FcgPolicies.RequireAuthenticatedUser)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMe(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var deleted = await _userService.DeleteMeAsync(userId.Value, cancellationToken);
        if (!deleted)
            return NotFound(new { message = "User not found." });
        _meters.RecordUserDeleted();
        _logger.LogInformation("User deleted own account: {Id}", userId);
        return NoContent();
    }
}
