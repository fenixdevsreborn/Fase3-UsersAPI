using Amazon.DynamoDBv2.DataModel;

namespace ms_users.Models
{
  [DynamoDBTable("AuditLogs")]
  public class AuditLog
  {
    [DynamoDBHashKey]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string TableName { get; set; }
    public string Operation { get; set; }
    public string UserId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
  }
}
