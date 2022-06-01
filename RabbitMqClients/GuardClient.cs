using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Contracts;
using Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQClients;

public class GuardClient : IGuardService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly EventingBasicConsumer _consumer;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _callbackMapper = new();
    private readonly string _replyQueueName;

    public GuardClient()
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

        _channel.BasicConsume(
            consumer: _consumer,
            queue: _replyQueueName,
            autoAck: true);
    }

    public async Task<Guard> GetGuardByIdAsync(long id)
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = _channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = _replyQueueName;
        var messageBytes = Encoding.UTF8.GetBytes(id.ToString());
        var tcs = new TaskCompletionSource<string>();
        _callbackMapper.TryAdd(correlationId, tcs);                
        _channel.BasicPublish(exchange: "guard.exchange", routingKey: "guard.getById", basicProperties: props,
            body: messageBytes);
    
        Console.WriteLine("message published");
        cancellationToken.Register(() => _callbackMapper.TryRemove(correlationId, out var tmp));
        String response =  tcs.Task.Result;
        Console.WriteLine(response);
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to fetch guard.");
        }
        Guard g = JsonSerializer.Deserialize<Guard>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        return g;
    }

    public async Task<ICollection<Guard>> GetGuardsAsync(int number)
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = _channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = _replyQueueName;
        var messageBytes = Encoding.UTF8.GetBytes(number.ToString());
        var tcs = new TaskCompletionSource<string>();
        _callbackMapper.TryAdd(correlationId, tcs);                
        _channel.BasicPublish(exchange: "guard.exchange", routingKey: "guards.get", basicProperties: props,
            body: messageBytes);
    
        Console.WriteLine("message published");
        cancellationToken.Register(() => _callbackMapper.TryRemove(correlationId, out var tmp));
        String response =  tcs.Task.Result;
        Console.WriteLine(response);
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to fetch guards.");
        }
        ICollection<Guard> g = JsonSerializer.Deserialize<ICollection<Guard>>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        return g;
    }

    public async Task CreateGuardAsync(Guard guard)
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = _channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = _replyQueueName;
        String guardToSend = JsonSerializer.Serialize(guard,new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        Console.WriteLine(guardToSend);
        var messageBytes = Encoding.UTF8.GetBytes(guardToSend);
        var tcs = new TaskCompletionSource<string>();
        _callbackMapper.TryAdd(correlationId, tcs);                
        _channel.BasicPublish(exchange: "guard.exchange", routingKey: "guard.add", basicProperties: props,
            body: messageBytes);
    
        Console.WriteLine("message published guard/add");
        cancellationToken.Register(() => _callbackMapper.TryRemove(correlationId, out var tmp));
        String response =  tcs.Task.Result;
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to create guard.");
        }
    }

    public async Task RemoveGuardAsync(long id)
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = _channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = _replyQueueName;
        var messageBytes = Encoding.UTF8.GetBytes(id.ToString());
        var tcs = new TaskCompletionSource<string>();
        _callbackMapper.TryAdd(correlationId, tcs);                
        _channel.BasicPublish(exchange: "guard.exchange", routingKey: "guard.remove", basicProperties: props,
            body: messageBytes);
    
        Console.WriteLine("message published");
        cancellationToken.Register(() => _callbackMapper.TryRemove(correlationId, out var tmp));
        String response =  tcs.Task.Result;
        Console.WriteLine(response);
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to remove guard.");
        }
        
    }

    public async Task UpdateGuardAsync(Guard guard)
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = _channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = _replyQueueName;
        string guardToSend = JsonSerializer.Serialize(guard);
        var messageBytes = Encoding.UTF8.GetBytes(guardToSend);
        var tcs = new TaskCompletionSource<string>();
        _callbackMapper.TryAdd(correlationId, tcs);                
        _channel.BasicPublish(exchange: "guard.exchange", routingKey: "guard.update", basicProperties: props,
            body: messageBytes);
    
        Console.WriteLine("message published");
        cancellationToken.Register(() => _callbackMapper.TryRemove(correlationId, out var tmp));
        String response =  tcs.Task.Result;
        Console.WriteLine(response);
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to update guard.");
        }
    }

    public async Task<Sector> GetGuardSectorAsync(long guardId)
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = _channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = _replyQueueName;
        
        var messageBytes = Encoding.UTF8.GetBytes(guardId.ToString());
        var tcs = new TaskCompletionSource<string>();
        _callbackMapper.TryAdd(correlationId, tcs);                
        _channel.BasicPublish(exchange: "guard.exchange", routingKey: "guard.getBySector", basicProperties: props,
            body: messageBytes);
    
        cancellationToken.Register(() => _callbackMapper.TryRemove(correlationId, out var tmp));
        String response =  tcs.Task.Result;
        Console.WriteLine(response);
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to get guard sector.");
        }
        Sector? sector = JsonSerializer.Deserialize <Sector>(response,new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return sector!;
    }

    public async Task<ICollection<Guard>> GetGuardsBySectorTodayAsync(long sectorId)
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = _channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = _replyQueueName;
        var messageBytes = Encoding.UTF8.GetBytes(sectorId.ToString());
        var tcs = new TaskCompletionSource<string>();
        _callbackMapper.TryAdd(correlationId, tcs);
        _channel.BasicPublish(exchange: "guard.exchange", routingKey: "guard.getPerSectorToday", basicProperties: props,
            body: messageBytes);

        Console.WriteLine("message published");
        cancellationToken.Register(() => _callbackMapper.TryRemove(correlationId, out var tmp));
        String response =  tcs.Task.Result;
        Console.WriteLine(response);
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to fetch guards.");
        }
        ICollection<Guard> g = JsonSerializer.Deserialize<ICollection<Guard>>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        return g;
    }

    public async Task<ICollection<Guard>> GetGuardsBySectorAsync(long sectorId)
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = _channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = _replyQueueName;
        var messageBytes = Encoding.UTF8.GetBytes(sectorId.ToString());
        var tcs = new TaskCompletionSource<string>();
        _callbackMapper.TryAdd(correlationId, tcs);                
        _channel.BasicPublish(exchange: "guard.exchange", routingKey: "guard.getPerSectorToday", basicProperties: props,
            body: messageBytes);
    
        Console.WriteLine("message published");
        cancellationToken.Register(() => _callbackMapper.TryRemove(correlationId, out var tmp));
        String response =  tcs.Task.Result;
        Console.WriteLine(response);
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to fetch guards.");
        }
        ICollection<Guard> g = JsonSerializer.Deserialize<ICollection<Guard>>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        return g;
    }

    public async Task<List<int>> GetNumberOfGuardsPerSectorTodayAsync()
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = _channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = _replyQueueName;
        var messageBytes = Encoding.UTF8.GetBytes("");
        var tcs = new TaskCompletionSource<string>();
        _callbackMapper.TryAdd(correlationId, tcs);                
        _channel.BasicPublish(exchange: "guard.exchange", routingKey: "guard.getNumPerSectorToday", basicProperties: props,
            body: messageBytes);
    
        Console.WriteLine("message published");
        cancellationToken.Register(() => _callbackMapper.TryRemove(correlationId, out var tmp));
        String response =  tcs.Task.Result;
        Console.WriteLine(response);
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to fetch guards.");
        }
        List<int> g = JsonSerializer.Deserialize<List<int>>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        return g;
    }

    public async Task<List<int>> GetNumberOfGuardsPerSectorAsync()
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = _channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = _replyQueueName;
        var messageBytes = Encoding.UTF8.GetBytes("");
        var tcs = new TaskCompletionSource<string>();
        _callbackMapper.TryAdd(correlationId, tcs);                
        _channel.BasicPublish(exchange: "guard.exchange", routingKey: "guard.getNumPerSector", basicProperties: props,
            body: messageBytes);
    
        Console.WriteLine("message published");
        cancellationToken.Register(() => _callbackMapper.TryRemove(correlationId, out var tmp));
        String response =  tcs.Task.Result;
        Console.WriteLine(response);
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to fetch guards.");
        }
        List<int> g = JsonSerializer.Deserialize<List<int>>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        return g;
    }

    public async Task<bool> IsGuardAssignedAsync(long guardId)
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = _channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = _replyQueueName;
        var messageBytes = Encoding.UTF8.GetBytes(guardId.ToString());
        var tcs = new TaskCompletionSource<string>();
        _callbackMapper.TryAdd(correlationId, tcs);                
        _channel.BasicPublish(exchange: "guard.exchange", routingKey: "guard.isAssigned", basicProperties: props,
            body: messageBytes);
    
        Console.WriteLine("message published");
        cancellationToken.Register(() => _callbackMapper.TryRemove(correlationId, out var tmp));
        String response =  tcs.Task.Result;
        Console.WriteLine(response);
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to fetch guards.");
        }
        
        return bool.Parse(response);
    }

    public async Task<bool> IsGuardWorkingAsync(long guardId)
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = _channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = _replyQueueName;
        var messageBytes = Encoding.UTF8.GetBytes(guardId.ToString());
        var tcs = new TaskCompletionSource<string>();
        _callbackMapper.TryAdd(correlationId, tcs);                
        _channel.BasicPublish(exchange: "guard.exchange", routingKey: "guard.isWorking", basicProperties: props,
            body: messageBytes);
    
        Console.WriteLine("message published");
        cancellationToken.Register(() => _callbackMapper.TryRemove(correlationId, out var tmp));
        String response =  tcs.Task.Result;
        Console.WriteLine(response);
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to get if guard is working today.");
        }

        return bool.Parse(response);
    }

    public async Task ChangePasswordAsync(Guard loggedGuard)
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = _channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = _replyQueueName;
        string guardToSend = JsonSerializer.Serialize(loggedGuard);
        var messageBytes = Encoding.UTF8.GetBytes(guardToSend);
        var tcs = new TaskCompletionSource<string>();
        _callbackMapper.TryAdd(correlationId, tcs);                
        _channel.BasicPublish(exchange: "guard.exchange", routingKey: "guard.changePassword", basicProperties: props,
            body: messageBytes);
    
        Console.WriteLine("message published");
        cancellationToken.Register(() => _callbackMapper.TryRemove(correlationId, out var tmp));
        String response =  tcs.Task.Result;
        Console.WriteLine(response);
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to change password.");
        }
    }
}