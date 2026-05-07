namespace ms_users.Models.DTOs;

public class UserDto
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string Nickname { get; set; }
    public string Name { get; set; }
    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}