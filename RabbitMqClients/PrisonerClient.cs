using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Contracts;
using Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQClient;

public class PrisonerClient : IPrisonerService
{
    
    private readonly IConnection connection;
    private readonly IModel channel;
    private readonly EventingBasicConsumer consumer;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> callbackMapper = new();
    private readonly string replyQueueName;

    public PrisonerClient()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };

        connection = factory.CreateConnection();
        channel = connection.CreateModel();
        replyQueueName = channel.QueueDeclare(queue: "").QueueName;
        Console.WriteLine(replyQueueName);
        consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            if (!callbackMapper.TryRemove(ea.BasicProperties.CorrelationId, out TaskCompletionSource<string>? tcs))
                return;
            var body = ea.Body.ToArray();
            var response = Encoding.UTF8.GetString(body);
            tcs.TrySetResult(response);
        };

        channel.BasicConsume(consumer: consumer, queue: replyQueueName, autoAck: true);
    }
    
    
    public Prisoner AddPrisonerAsync(Prisoner prisoner)
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = replyQueueName;
        
        var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(prisoner));
        var tcs = new TaskCompletionSource<string>();
        callbackMapper.TryAdd(correlationId, tcs);                
        channel.BasicPublish(exchange: "prisoner.exchange", routingKey: "prisoner.add", basicProperties: props, body: messageBytes);
        Console.WriteLine("message published");
        cancellationToken.Register(() => callbackMapper.TryRemove(correlationId, out var tmp));
        
        String response =  tcs.Task.Result;
        Console.WriteLine(response);
        Prisoner p = JsonSerializer.Deserialize<Prisoner>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        
        return p;
    }

    public string RemovePrisonerAsync(Prisoner releasedPrisoner)
    {
        throw new NotImplementedException();
    }

    public Prisoner GetPrisonerByIdAsync(long prisonerId)
    {
        throw new NotImplementedException();
    }
}