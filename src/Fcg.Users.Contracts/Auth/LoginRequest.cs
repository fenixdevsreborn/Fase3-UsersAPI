using System.ComponentModel.DataAnnotations;

namespace Fcg.Users.Contracts.Auth;

public class LoginRequest
{
    /// <summary>Email or username.</summary>
    [Required, MinLength(1), MaxLength(256)]
    public string Login { get; set; } = string.Empty;

    [Required, MinLength(1)]
    public string Password { get; set; } = string.Empty;
}
