using System.ComponentModel.DataAnnotations;

namespace Fcg.Users.Contracts.Users;

public class UpdateMeRequest
{
    [MinLength(1), MaxLength(200)]
    public string? Name { get; set; }

    [MaxLength(2000)]
    public string? AvatarUrl { get; set; }

    [MaxLength(2000)]
    public string? Bio { get; set; }
}
