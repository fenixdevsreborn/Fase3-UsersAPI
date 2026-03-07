using Amazon.SQS;
using Amazon.SQS.Model;
using System.Text.Json;

namespace ms_users.Messaging;

public class EventPublisher
{
  private readonly IAmazonSQS _sqs;

  public EventPublisher(IAmazonSQS sqs)
  {
    _sqs = sqs;
  }

  public async Task PublishAsync(string queueUrl, object evt)
  {
    var request = new SendMessageRequest
    {
      QueueUrl = queueUrl,
      MessageBody = JsonSerializer.Serialize(evt)
    };

    await _sqs.SendMessageAsync(request);
  }
}