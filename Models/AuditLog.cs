using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ms_users.Models;

[Table("AuditLogs")]
public class AuditLog
{
    [Key]
    [Column("Id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Column("TableName")]
    [Required]
    [StringLength(100)]
    public string TableName { get; set; }

    [Column("Operation")]
    [Required]
    [StringLength(50)]
    public string Operation { get; set; }

    [Column("UserId")]
    [StringLength(36)]
    [ForeignKey("User")]
    public string? UserId { get; set; }

    [Column("Timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [Column("OldValues")]
    public string? OldValues { get; set; }

    [Column("NewValues")]
    public string? NewValues { get; set; }

    // Navigation property - não será serializado
    [JsonIgnore]
    public Users? User { get; set; }
}