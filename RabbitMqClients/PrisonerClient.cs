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
    
    
    public async Task CreatePrisonerAsync(Prisoner prisoner)
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
        cancellationToken.Register(() => callbackMapper.TryRemove(correlationId, out var tmp));
        
        String response =  tcs.Task.Result;
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to create prisoner");
        }
    }

    public async Task RemovePrisonerAsync(long id)
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = replyQueueName;
        
        var messageBytes = Encoding.UTF8.GetBytes(id.ToString());
        var tcs = new TaskCompletionSource<string>();
        callbackMapper.TryAdd(correlationId, tcs);                
        channel.BasicPublish(exchange: Exchange, routingKey: "prisoner.remove", basicProperties: props, body: messageBytes);
        cancellationToken.Register(() => callbackMapper.TryRemove(correlationId, out var tmp));
        
        String response =  tcs.Task.Result;
        if (response.Equals("fail"))
        {
            throw new Exception($"Failed to remove prisoner n.-{id}");
        }
    }

    public async Task<Prisoner> GetPrisonerByIdAsync(long prisonerId)
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
        cancellationToken.Register(() => callbackMapper.TryRemove(correlationId, out var tmp));
        
        String response =  tcs.Task.Result;
        if (response.Equals("fail"))
        {
            throw new Exception($"Failed to load prisoner n.-{prisonerId}");
        }
        Prisoner p = JsonSerializer.Deserialize<Prisoner>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        return p;
    }

    public async Task<Prisoner> GetPrisonerBySsn(string prisonerSSN)
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        
        props.CorrelationId = correlationId;
        props.ReplyTo = replyQueueName;
        var messageBytes = Encoding.UTF8.GetBytes(prisonerSSN);
        var tcs = new TaskCompletionSource<string>();
        callbackMapper.TryAdd(correlationId, tcs);                
        channel.BasicPublish(exchange: Exchange, routingKey: "prisoner.getBySSN", basicProperties: props, body: messageBytes);
        cancellationToken.Register(() => callbackMapper.TryRemove(correlationId, out var tmp));
        
        String response =  tcs.Task.Result;
        if (response.Equals("fail"))
        {
            throw new Exception($"Failed to load prisoner n.{prisonerSSN}");
        }
        Prisoner p = JsonSerializer.Deserialize<Prisoner>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        return p;
    }

    // public async Task<ICollection<Prisoner>> GetPrisonersAsync()
    // {
    //     CancellationToken cancellationToken = default;
    //     IBasicProperties props = channel.CreateBasicProperties();
    //     var correlationId = Guid.NewGuid().ToString();
    //     
    //     props.CorrelationId = correlationId;
    //     props.ReplyTo = replyQueueName;
    //     
    //     var messageBytes = Encoding.UTF8.GetBytes("");
    //     var tcs = new TaskCompletionSource<string>();
    //     callbackMapper.TryAdd(correlationId, tcs);                
    //     channel.BasicPublish(exchange: Exchange, routingKey: "prisoners.get", basicProperties: props, body: messageBytes);
    //     cancellationToken.Register(() => callbackMapper.TryRemove(correlationId, out var tmp));
    //     
    //     String response =  tcs.Task.Result;
    //     if (response.Equals("fail"))
    //     {
    //         throw new Exception("Failed to load prisoners");
    //     }
    //
    //     ICollection<Prisoner> ps = JsonSerializer.Deserialize<ICollection<Prisoner>>(response, new JsonSerializerOptions
    //     {
    //         PropertyNameCaseInsensitive = true
    //     })!;
    //     return ps;
    // }

    public async Task UpdatePrisonerAsync(Prisoner newPrisoner)
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
        cancellationToken.Register(() => callbackMapper.TryRemove(correlationId, out var tmp));
        
        String response =  tcs.Task.Result;
        if (response.Equals("fail"))
        {
            throw new Exception($"Failed to update prisoner n.-{newPrisoner.Id}");
        }
    }

    public async Task<ICollection<Prisoner>?> GetPrisonersAsync(int pageNumber, int pageSize)
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        
        props.CorrelationId = correlationId;
        props.ReplyTo = replyQueueName;

        String[] array = new[] {pageNumber.ToString(), pageSize.ToString()};
        var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(array));
        var tcs = new TaskCompletionSource<string>();
        callbackMapper.TryAdd(correlationId, tcs);                
        channel.BasicPublish(exchange: Exchange, routingKey: "prisoners.get", basicProperties: props, body: messageBytes);
        cancellationToken.Register(() => callbackMapper.TryRemove(correlationId, out var tmp));
        
        String response =  tcs.Task.Result;
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to load prisoners");
        }
    
        ICollection<Prisoner> ps = JsonSerializer.Deserialize<ICollection<Prisoner>>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        return ps;
    }

    public async Task<int> GetPrisonerCount()
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        
        props.CorrelationId = correlationId;
        props.ReplyTo = replyQueueName;

        var messageBytes = Encoding.UTF8.GetBytes("");
        var tcs = new TaskCompletionSource<string>();
        callbackMapper.TryAdd(correlationId, tcs);                
        channel.BasicPublish(exchange: Exchange, routingKey: "prisoners.count", basicProperties: props, body: messageBytes);
        cancellationToken.Register(() => callbackMapper.TryRemove(correlationId, out var tmp));
        
        String response =  tcs.Task.Result;
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to load prisoners. Please try again ");
        }
    
        int count = Int32.Parse(response);
        return count;
    }

    public async Task<ICollection<Prisoner>?> GetPrisonersBySectorAsync(int pageNumber, int pageSize, int sectorId)
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        
        props.CorrelationId = correlationId;
        props.ReplyTo = replyQueueName;

        String[] array = new[] {pageNumber.ToString(), pageSize.ToString(),sectorId.ToString()};
        var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(array));
        var tcs = new TaskCompletionSource<string>();
        callbackMapper.TryAdd(correlationId, tcs);                
        channel.BasicPublish(exchange: Exchange, routingKey: "prisoners.getBySector", basicProperties: props, body: messageBytes);
        cancellationToken.Register(() => callbackMapper.TryRemove(correlationId, out var tmp));
        String response =  tcs.Task.Result;
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to load prisoners");
        }
    
        ICollection<Prisoner> ps = JsonSerializer.Deserialize<ICollection<Prisoner>>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        return ps;
    }

    public async Task AddPointsToPrisonerAsync(long prisonerId, int points)
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = replyQueueName;
        
        String[] array = {prisonerId.ToString(), points.ToString()};
        var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(array));
        var tcs = new TaskCompletionSource<string>();
        callbackMapper.TryAdd(correlationId, tcs); 
        
        channel.BasicPublish(exchange: Exchange, routingKey: "prisoner.addPoints", basicProperties: props, body: messageBytes);
        cancellationToken.Register(() => callbackMapper.TryRemove(correlationId, out var tmp));
        
        String response =  tcs.Task.Result;
        if (response.Equals("fail"))
        {
            throw new Exception($"Failed to add points prisoner n.-{prisonerId}");
        }
    }

    public Task<int[]> GetNumPrisPerSectAsync()
    {
        throw new NotImplementedException();
    }
}