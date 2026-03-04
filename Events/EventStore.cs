using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;

namespace ms_users.Events;

public class EventStore
{
  private readonly ITable _table;

  public EventStore()
  {
    var client = new AmazonDynamoDBClient();
    _table = Table.LoadTable(client, "events-table");
  }

  public async Task SaveEvent(string aggregateId, string type, object payload)
  {
    var doc = new Document
    {
      ["aggregateId"] = aggregateId,
      ["timestamp"] = DateTime.UtcNow.ToString("o"),
      ["type"] = type,
      ["payload"] = payload.ToString()
    };

    await _table.PutItemAsync(doc);
  }
}