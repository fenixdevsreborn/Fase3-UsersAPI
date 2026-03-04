using Amazon.SQS;
using Amazon.SQS.Model;
using System.Text.Json;
using ms_users.Events;

namespace ms_users.Messaging;

public class PaymentPublisher
{
  private readonly IAmazonSQS _sqs;
  private readonly string _queueUrl;

  public PaymentPublisher(IAmazonSQS sqs, IConfiguration config)
  {
    _sqs = sqs;
    _queueUrl = Environment.GetEnvironmentVariable("PAYMENT_QUEUE_URL"); ;
  }

  public async Task PublishAsync(UserRegisteredEvent evt)
  {
    var request = new SendMessageRequest
    {
      QueueUrl = _queueUrl,
      MessageBody = JsonSerializer.Serialize(evt)
    };

    await _sqs.SendMessageAsync(request);
  }
}