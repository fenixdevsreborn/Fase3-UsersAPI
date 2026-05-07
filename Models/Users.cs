using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ms_users.Models;

[Table("Users")]
public class Users
{
    [Key]
    [Column("Id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Column("Email")]
    [Required]
    [StringLength(255)]
    public string Email { get; set; }

    [Column("Nickname")]
    [Required]
    [StringLength(100)]
    public string Nickname { get; set; }

    [Column("Name")]
    [Required]
    [StringLength(255)]
    public string Name { get; set; }

    [Column("PasswordHash")]
    [StringLength(500)]
    public string? PasswordHash { get; set; }

    [Column("RefreshToken")]
    [StringLength(500)]
    public string? RefreshToken { get; set; }

    [Column("Active")]
    public bool Active { get; set; } = true;

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("UpdatedAt")]
    public DateTime? UpdatedAt { get; set; }

    // Navigation property - não será serializado
    [JsonIgnore]
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}