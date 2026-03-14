using Amazon.DynamoDBv2.DataModel;

[DynamoDBTable("Users")]
public class Users
{
  [DynamoDBHashKey]
  public string Id { get; set; }
  
  public string Email { get; set; }

  public string Nickname { get; set; }
  
  public string Name { get; set; }

  public bool Active { get; set; } = true;

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  public DateTime? UpdatedAt { get; set; }
}