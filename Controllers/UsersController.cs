using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
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
  public async Task<IActionResult> Register([FromBody] RegisterRequest request)
  {
    var user = await _service.Register(
        request.Email,
        request.Password
    );

    return Created("", user);
  }
}