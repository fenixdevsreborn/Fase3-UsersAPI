using System.ComponentModel.DataAnnotations;

namespace ms_users.Models;

public class Users
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "O formato do e-mail é inválido.")]
    public string? Email { get; set; }

    [MinLength(6, ErrorMessage = "A senha deve ter pelo menos 6 caracteres.")]
    public string? Password { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}