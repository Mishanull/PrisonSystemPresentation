using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Contracts;
using Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQClient;

public class GuardClient : IGuardService
{
    private readonly IConnection connection;
    private readonly IModel channel;
    private readonly EventingBasicConsumer consumer;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> callbackMapper = new();
    private readonly string replyQueueName;

    public GuardClient()
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

        channel.BasicConsume(
            consumer: consumer,
            queue: replyQueueName,
            autoAck: true);
        
           
    }

    public async Task<Guard> GetGuard(long id)
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = replyQueueName;
        var messageBytes = Encoding.UTF8.GetBytes(id.ToString());
        var tcs = new TaskCompletionSource<string>();
        callbackMapper.TryAdd(correlationId, tcs);                
        channel.BasicPublish(exchange: "sep3.prison", routingKey: "guard.get", basicProperties: props,
            body: messageBytes);
    
        Console.WriteLine("message published");
        cancellationToken.Register(() => callbackMapper.TryRemove(correlationId, out var tmp));
        String response =  tcs.Task.Result;
        Console.WriteLine(response);
        Guard g = JsonSerializer.Deserialize<Guard>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        return g;
    }

    public async Task<ICollection<Guard>> GetGuards(int number)
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = replyQueueName;
        var messageBytes = Encoding.UTF8.GetBytes(number.ToString());
        var tcs = new TaskCompletionSource<string>();
        callbackMapper.TryAdd(correlationId, tcs);                
        channel.BasicPublish(exchange: "sep3.prison", routingKey: "guard.all", basicProperties: props,
            body: messageBytes);
    
        Console.WriteLine("message published");
        cancellationToken.Register(() => callbackMapper.TryRemove(correlationId, out var tmp));
        String response =  tcs.Task.Result;
        Console.WriteLine(response);
        ICollection<Guard> g = JsonSerializer.Deserialize<ICollection<Guard>>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        return g;
    }

    public async Task<string> CreateGuard(Guard guard)
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = replyQueueName;
        guard.Id = 2;
        guard.Email = "test@email.com";
        guard.PhoneNumber = "123123213";
        guard.Password = "213123asad";
        guard.Username = "facjsad";
        String guardToSend = JsonSerializer.Serialize(guard,new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        Console.WriteLine(guardToSend);
        var messageBytes = Encoding.UTF8.GetBytes(guardToSend);
        var tcs = new TaskCompletionSource<string>();
        callbackMapper.TryAdd(correlationId, tcs);                
        channel.BasicPublish(exchange: "guard.exchange", routingKey: "guard.add", basicProperties: props,
            body: messageBytes);
    
        Console.WriteLine("message published guard/add");
        cancellationToken.Register(() => callbackMapper.TryRemove(correlationId, out var tmp));
        String response =  tcs.Task.Result;
        Guard confirm = JsonSerializer.Deserialize<Guard>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        Console.WriteLine(confirm+" guard/add");

        return "confirmed";
    }

    public async Task<string> DeleteGuard(long id)
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = replyQueueName;
        var messageBytes = Encoding.UTF8.GetBytes(id.ToString());
        var tcs = new TaskCompletionSource<string>();
        callbackMapper.TryAdd(correlationId, tcs);                
        channel.BasicPublish(exchange: "sep3.prison", routingKey: "guard.remove", basicProperties: props,
            body: messageBytes);
    
        Console.WriteLine("message published");
        cancellationToken.Register(() => callbackMapper.TryRemove(correlationId, out var tmp));
        String response =  tcs.Task.Result;
        Console.WriteLine(response);
        string confirm = JsonSerializer.Deserialize<string>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        return confirm;
    }

    public async Task<string> UpdateGuard(Guard guard)
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = replyQueueName;
        string guardToSend = JsonSerializer.Serialize(guard);
        var messageBytes = Encoding.UTF8.GetBytes(guardToSend);
        var tcs = new TaskCompletionSource<string>();
        callbackMapper.TryAdd(correlationId, tcs);                
        channel.BasicPublish(exchange: "sep3.prison", routingKey: "guard.update", basicProperties: props,
            body: messageBytes);
    
        Console.WriteLine("message published");
        cancellationToken.Register(() => callbackMapper.TryRemove(correlationId, out var tmp));
        String response =  tcs.Task.Result;
        Console.WriteLine(response);
        string confirm = JsonSerializer.Deserialize<string>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        return confirm;
    }

 
}