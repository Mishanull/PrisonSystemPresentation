using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Contracts;
using Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMqClients;

public class PrisonerClient : IPrisonerService
{
    
    private readonly IConnection connection;
    private readonly IModel channel;
    private readonly EventingBasicConsumer consumer;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> callbackMapper = new();
    private readonly string replyQueueName;

    private const string Exchange = "prisoner.exchange";

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
    
    
    public async Task<Prisoner> AddPrisonerAsync(Prisoner? prisoner)
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = replyQueueName;
        
        var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(prisoner));
        var tcs = new TaskCompletionSource<string>();
        callbackMapper.TryAdd(correlationId, tcs);                
        channel.BasicPublish(exchange: Exchange, routingKey: "prisoner.add", basicProperties: props, body: messageBytes);
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

    public async Task<string> RemovePrisonerAsync(Prisoner releasedPrisoner)
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = replyQueueName;
        
        var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(releasedPrisoner));
        var tcs = new TaskCompletionSource<string>();
        callbackMapper.TryAdd(correlationId, tcs);                
        channel.BasicPublish(exchange: Exchange, routingKey: "prisoner.remove", basicProperties: props, body: messageBytes);
        Console.WriteLine("message published");
        cancellationToken.Register(() => callbackMapper.TryRemove(correlationId, out var tmp));
        
        String response =  tcs.Task.Result;
        Console.WriteLine(response);
        string msg = JsonSerializer.Deserialize<string>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        
        return msg;
    }

    public async Task<Prisoner?> GetPrisonerByIdAsync(long prisonerId)
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        
        props.CorrelationId = correlationId;
        props.ReplyTo = replyQueueName;
        
        var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(prisonerId));
        var tcs = new TaskCompletionSource<string>();
        callbackMapper.TryAdd(correlationId, tcs);                
        channel.BasicPublish(exchange: Exchange, routingKey: "prisoner.getById", basicProperties: props, body: messageBytes);
    
        Console.WriteLine("message published login");
        cancellationToken.Register(() => callbackMapper.TryRemove(correlationId, out var tmp));
        String response =  tcs.Task.Result;
        Console.WriteLine(response);
        Prisoner? p = JsonSerializer.Deserialize<Prisoner>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        return p;
    }

    public async Task<ICollection<Prisoner>?> GetPrisonersAsync()
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        
        props.CorrelationId = correlationId;
        props.ReplyTo = replyQueueName;
        
        var messageBytes = Encoding.UTF8.GetBytes("");
        var tcs = new TaskCompletionSource<string>();
        callbackMapper.TryAdd(correlationId, tcs);                
        channel.BasicPublish(exchange: Exchange, routingKey: "prisoners.get", basicProperties: props, body: messageBytes);
    
        Console.WriteLine("message published login");
        cancellationToken.Register(() => callbackMapper.TryRemove(correlationId, out var tmp));
        
        String response =  tcs.Task.Result;
        Console.WriteLine(response);
        try
        {
            ICollection<Prisoner>? ps = JsonSerializer.Deserialize<ICollection<Prisoner>>(response, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })!;
            return ps;
        }
        catch (Exception e)
        {
            return null;
        }
    }

    public async Task<Prisoner?> UpdatePrisonerAsync(Prisoner newPrisoner)
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = replyQueueName;
        
        var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(newPrisoner));
        var tcs = new TaskCompletionSource<string>();
        callbackMapper.TryAdd(correlationId, tcs);                
        channel.BasicPublish(exchange: Exchange, routingKey: "prisoner.update", basicProperties: props, body: messageBytes);
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
}