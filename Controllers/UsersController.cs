using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using ms_users.Models;
using ms_users.Services;

namespace ms_users.Controllers;

[ApiController]
[Route("users")]
public class UsersController : ControllerBase
{
  private readonly UserService _service;

  public UsersController(UserService service)
  {
    _service = service;
  }

  [HttpPost("register")]
  public async Task<IActionResult> Register([FromBody] RegisterRequestUser request)
  {
    var user = await _service.Register(request);

    return Created("", user);
  }

  [HttpPost("login")]
  public async Task<IActionResult> Login([FromBody] LoginRequest request)
  {
    var result = await _service.Login(request.Email, request.Password);
    return Ok(result);
  }


  [HttpGet("me")]
  public async Task<IActionResult> Me()
  {
    var userId = User.FindFirst("sub")?.Value;

    if (userId == null)
      return Unauthorized();

    var user = await _service.GetById(userId);

    if (user == null)
      return NotFound();

    return Ok(user);
  }
}