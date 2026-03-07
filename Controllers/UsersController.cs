using Microsoft.AspNetCore.Authorization;
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
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var user = await _service.Register(request.Email, request.Password);
            return Created("", new { user.Id, user.Email, user.Name, user.CreatedAt });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var token = await _service.Login(request.Email, request.Password);

        if (string.IsNullOrEmpty(token))
            return Unauthorized(new { message = "E-mail ou senha inválidos." });

        return Ok(new { token });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProfile(string id)
    {
        var profile = await _service.GetProfile(id);

        if (profile == null)
            return NotFound(new { message = "Usuário não encontrado." });

        return Ok(new { profile.Id, profile.Email, profile.Name });
    }

    [Authorize] 
    [HttpPut("{id}/credentials")]
    public async Task<IActionResult> UpdateCredentials(string id, [FromBody] Users request)
    {
        try
        {
            var updatedProfile = await _service.UpdateCredentials(id, request);

            if (updatedProfile == null)
                return NotFound(new { message = "Usuário não encontrado." });

            return Ok(new
            {
                updatedProfile.Id,
                updatedProfile.Email,
                updatedProfile.Name
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}