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
    
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly EventingBasicConsumer _consumer;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _callbackMapper = new();
    private readonly string _replyQueueName;

    private const string Exchange = "prisoner.exchange";

    public PrisonerClient()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _replyQueueName = _channel.QueueDeclare(queue: "").QueueName;
        Console.WriteLine(_replyQueueName);
        _consumer = new EventingBasicConsumer(_channel);
        _consumer.Received += (model, ea) =>
        {
            if (!_callbackMapper.TryRemove(ea.BasicProperties.CorrelationId, out TaskCompletionSource<string>? tcs))
                return;
            var body = ea.Body.ToArray();
            var response = Encoding.UTF8.GetString(body);
            tcs.TrySetResult(response);
        };

        _channel.BasicConsume(consumer: _consumer, queue: _replyQueueName, autoAck: true);
    }
    
    
    public async Task CreatePrisonerAsync(Prisoner prisoner)
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = _channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = _replyQueueName;
        
        var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(prisoner));
        var tcs = new TaskCompletionSource<string>();
        _callbackMapper.TryAdd(correlationId, tcs);                
        _channel.BasicPublish(exchange: Exchange, routingKey: "prisoner.add", basicProperties: props, body: messageBytes);
        cancellationToken.Register(() => _callbackMapper.TryRemove(correlationId, out var tmp));
        
        String response =  tcs.Task.Result;
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to create prisoner");
        }
    }

    public async Task RemovePrisonerAsync(long id)
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = _channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = _replyQueueName;
        
        var messageBytes = Encoding.UTF8.GetBytes(id.ToString());
        var tcs = new TaskCompletionSource<string>();
        _callbackMapper.TryAdd(correlationId, tcs);                
        _channel.BasicPublish(exchange: Exchange, routingKey: "prisoner.remove", basicProperties: props, body: messageBytes);
        cancellationToken.Register(() => _callbackMapper.TryRemove(correlationId, out var tmp));
        
        String response =  tcs.Task.Result;
        if (response.Equals("fail"))
        {
            throw new Exception($"Failed to remove prisoner n.-{id}");
        }
    }

    public async Task<Prisoner> GetPrisonerByIdAsync(long prisonerId)
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = _channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        
        props.CorrelationId = correlationId;
        props.ReplyTo = _replyQueueName;
        
        var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(prisonerId));
        var tcs = new TaskCompletionSource<string>();
        _callbackMapper.TryAdd(correlationId, tcs);                
        _channel.BasicPublish(exchange: Exchange, routingKey: "prisoner.getById", basicProperties: props, body: messageBytes);
        cancellationToken.Register(() => _callbackMapper.TryRemove(correlationId, out var tmp));
        
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

    public async Task<Prisoner> GetPrisonerBySsnAsync(string prisonerSSN)
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = _channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        
        props.CorrelationId = correlationId;
        props.ReplyTo = _replyQueueName;
        var messageBytes = Encoding.UTF8.GetBytes(prisonerSSN);
        var tcs = new TaskCompletionSource<string>();
        _callbackMapper.TryAdd(correlationId, tcs);                
        _channel.BasicPublish(exchange: Exchange, routingKey: "prisoner.getBySSN", basicProperties: props, body: messageBytes);
        cancellationToken.Register(() => _callbackMapper.TryRemove(correlationId, out var tmp));
        
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
        IBasicProperties props = _channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = _replyQueueName;
        
        var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(newPrisoner));
        var tcs = new TaskCompletionSource<string>();
        _callbackMapper.TryAdd(correlationId, tcs); 
        
        _channel.BasicPublish(exchange: Exchange, routingKey: "prisoner.update", basicProperties: props, body: messageBytes);
        cancellationToken.Register(() => _callbackMapper.TryRemove(correlationId, out var tmp));
        
        String response =  tcs.Task.Result;
        if (response.Equals("fail"))
        {
            throw new Exception($"Failed to update prisoner n.-{newPrisoner.Id}");
        }
    }

    public async Task<ICollection<Prisoner>?> GetPrisonersAsync(int pageNumber, int pageSize)
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = _channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        
        props.CorrelationId = correlationId;
        props.ReplyTo = _replyQueueName;

        String[] array = new[] {pageNumber.ToString(), pageSize.ToString()};
        var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(array));
        var tcs = new TaskCompletionSource<string>();
        _callbackMapper.TryAdd(correlationId, tcs);                
        _channel.BasicPublish(exchange: Exchange, routingKey: "prisoners.get", basicProperties: props, body: messageBytes);
        cancellationToken.Register(() => _callbackMapper.TryRemove(correlationId, out var tmp));
        
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

    public async Task<int> GetPrisonerCountAsync()
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = _channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        
        props.CorrelationId = correlationId;
        props.ReplyTo = _replyQueueName;

        var messageBytes = Encoding.UTF8.GetBytes("");
        var tcs = new TaskCompletionSource<string>();
        _callbackMapper.TryAdd(correlationId, tcs);                
        _channel.BasicPublish(exchange: Exchange, routingKey: "prisoners.count", basicProperties: props, body: messageBytes);
        cancellationToken.Register(() => _callbackMapper.TryRemove(correlationId, out var tmp));
        
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
        IBasicProperties props = _channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        
        props.CorrelationId = correlationId;
        props.ReplyTo = _replyQueueName;

        String[] array = new[] {pageNumber.ToString(), pageSize.ToString(),sectorId.ToString()};
        var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(array));
        var tcs = new TaskCompletionSource<string>();
        _callbackMapper.TryAdd(correlationId, tcs);                
        _channel.BasicPublish(exchange: Exchange, routingKey: "prisoners.getBySector", basicProperties: props, body: messageBytes);
        cancellationToken.Register(() => _callbackMapper.TryRemove(correlationId, out var tmp));
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
        IBasicProperties props = _channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = _replyQueueName;
        
        String[] array = {prisonerId.ToString(), points.ToString()};
        var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(array));
        var tcs = new TaskCompletionSource<string>();
        _callbackMapper.TryAdd(correlationId, tcs); 
        
        _channel.BasicPublish(exchange: Exchange, routingKey: "prisoner.addPoints", basicProperties: props, body: messageBytes);
        cancellationToken.Register(() => _callbackMapper.TryRemove(correlationId, out var tmp));
        
        String response =  tcs.Task.Result;
        if (response.Equals("fail"))
        {
            throw new Exception($"Failed to add points prisoner n.-{prisonerId}");
        }
    }

    public async Task<List<int>> GetNumberOfPrisonersPerSectorAsync()
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = _channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = _replyQueueName;
        var messageBytes = Encoding.UTF8.GetBytes("");
        var tcs = new TaskCompletionSource<string>();
        _callbackMapper.TryAdd(correlationId, tcs);                
        _channel.BasicPublish(exchange: Exchange, routingKey: "prisoner.getNumPerSector", basicProperties: props,
            body: messageBytes);
    
        Console.WriteLine("message published");
        cancellationToken.Register(() => _callbackMapper.TryRemove(correlationId, out var tmp));
        String response =  tcs.Task.Result;
        Console.WriteLine(response);
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to fetch prisoners number per sector.");
        }
        List<int> g = JsonSerializer.Deserialize<List<int>>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        return g;
    }

    public async Task<ICollection<Prisoner>> GetPrisonersWithLowBehaviourAsync()
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = _channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        
        props.CorrelationId = correlationId;
        props.ReplyTo = _replyQueueName;

        var messageBytes = Encoding.UTF8.GetBytes("");
        var tcs = new TaskCompletionSource<string>();
        _callbackMapper.TryAdd(correlationId, tcs);                
        _channel.BasicPublish(exchange: Exchange, routingKey: "prisoner.getLowBehaviour", basicProperties: props, body: messageBytes);
        cancellationToken.Register(() => _callbackMapper.TryRemove(correlationId, out var tmp));
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
}