using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Contracts;
using Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMqClients;

public class WorkShiftClient : IWorkShiftService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly EventingBasicConsumer _consumer;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _callbackMapper = new();
    private readonly string _replyQueueName;
    
    private const string Exchange = "workShift.exchange";

    public WorkShiftClient()
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
    public async Task<ICollection<WorkShift>> GetWorkShiftsAsync()
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = _channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        
        props.CorrelationId = correlationId;
        props.ReplyTo = _replyQueueName;
        
        var messageBytes = Encoding.UTF8.GetBytes("");
        var tcs = new TaskCompletionSource<string>();
        _callbackMapper.TryAdd(correlationId, tcs);                
        _channel.BasicPublish(exchange: Exchange, routingKey: "workShift.get", basicProperties: props, body: messageBytes);
        cancellationToken.Register(() => _callbackMapper.TryRemove(correlationId, out var tmp));
        
        String response =  tcs.Task.Result;
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to load workshifts");
        }
    
        ICollection<WorkShift> ws = JsonSerializer.Deserialize<ICollection<WorkShift>>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        return ws;
    }

    public async Task CreateWorkShiftAsync(WorkShift workShift)
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = _channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = _replyQueueName;
        
        var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(workShift));
        var tcs = new TaskCompletionSource<string>();
        _callbackMapper.TryAdd(correlationId, tcs);                
        _channel.BasicPublish(exchange: Exchange, routingKey: "workShift.add", basicProperties: props, body: messageBytes);
        cancellationToken.Register(() => _callbackMapper.TryRemove(correlationId, out var tmp));
        Console.WriteLine("message published workshift/add");
        String response =  tcs.Task.Result;
        Console.WriteLine(response);
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to create a workshift");
        }
        
    }

    public async Task RemoveWorkShiftAsync(long id)
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = _channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = _replyQueueName;
        
        var messageBytes = Encoding.UTF8.GetBytes(id.ToString());
        var tcs = new TaskCompletionSource<string>();
        _callbackMapper.TryAdd(correlationId, tcs);                
        _channel.BasicPublish(exchange: Exchange, routingKey: "workShift.remove", basicProperties: props, body: messageBytes);
        cancellationToken.Register(() => _callbackMapper.TryRemove(correlationId, out var tmp));
        
        String response =  tcs.Task.Result;
        if (response.Equals("fail"))
        {
            throw new Exception($"Failed to remove workshift n.-{id}");
        }
    }

    public async Task UpdateWorkShiftAsync(WorkShift workShift)
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = _channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = _replyQueueName;
        
        var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(workShift));
        var tcs = new TaskCompletionSource<string>();
        _callbackMapper.TryAdd(correlationId, tcs); 
        
        _channel.BasicPublish(exchange: Exchange, routingKey: "workShift.update", basicProperties: props, body: messageBytes);
        cancellationToken.Register(() => _callbackMapper.TryRemove(correlationId, out var tmp));
        
        String response =  tcs.Task.Result;
        if (response.Equals("fail"))
        {
            throw new Exception($"Failed to update workshift n.-{workShift.Id}");
        }
    }

    public async Task<WorkShift> GetWorkShiftByIdAsync(long? id)
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = _channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        
        props.CorrelationId = correlationId;
        props.ReplyTo = _replyQueueName;
        
        var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(id));
        var tcs = new TaskCompletionSource<string>();
        _callbackMapper.TryAdd(correlationId, tcs);                
        _channel.BasicPublish(exchange: Exchange, routingKey: "workShift.getById", basicProperties: props, body: messageBytes);
        cancellationToken.Register(() => _callbackMapper.TryRemove(correlationId, out var tmp));
        
        String response =  tcs.Task.Result;
        if (response.Equals("fail"))
        {
            throw new Exception($"Failed to load workshift n.-{id}");
        }
        WorkShift w = JsonSerializer.Deserialize<WorkShift>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        return w;    
    }


    public async Task AddGuardToWorkShiftAsync(string guardId, string shiftId)
    {
        string[] idArray = {guardId, shiftId};
        CancellationToken cancellationToken = default;
        IBasicProperties props = _channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = _replyQueueName;
        
        var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(idArray));
        var tcs = new TaskCompletionSource<string>();
        _callbackMapper.TryAdd(correlationId, tcs); 
        
        _channel.BasicPublish(exchange: Exchange, routingKey: "workShift.addGuard", basicProperties: props, body: messageBytes);
        cancellationToken.Register(() => _callbackMapper.TryRemove(correlationId, out var tmp));
        
        String response =  tcs.Task.Result;
        if (response.Equals("fail"))
        {
            throw new Exception($"Failed to add guard n.-{guardId} to workshift n.-{shiftId}");
        }
    }

    public async Task RemoveGuardFromWorkShiftAsync(string guardId, string shiftId)
    {
        string ids = $"{guardId}{shiftId}";
        Console.WriteLine(ids);
        CancellationToken cancellationToken = default;
        IBasicProperties props = _channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = _replyQueueName;
        
        var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(ids));
        var tcs = new TaskCompletionSource<string>();
        _callbackMapper.TryAdd(correlationId, tcs); 
        
        _channel.BasicPublish(exchange: Exchange, routingKey: "workShift.removeGuard", basicProperties: props, body: messageBytes);
        cancellationToken.Register(() => _callbackMapper.TryRemove(correlationId, out var tmp));
        
        String response =  tcs.Task.Result;
        if (response.Equals("fail"))
        {
            throw new Exception($"Failed to remove guard n.-{guardId} from workshift n.-{shiftId}");
        }
    }

    public async Task<WorkShift> GetWorkShiftByGuardAsync(long guardId)
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = _channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        
        props.CorrelationId = correlationId;
        props.ReplyTo = _replyQueueName;
        
        var messageBytes = Encoding.UTF8.GetBytes(guardId.ToString());
        var tcs = new TaskCompletionSource<string>();
        _callbackMapper.TryAdd(correlationId, tcs);                
        _channel.BasicPublish(exchange: Exchange, routingKey: "workShift.getByGuardId", basicProperties: props, body: messageBytes);
        cancellationToken.Register(() => _callbackMapper.TryRemove(correlationId, out var tmp));
        
        String response =  tcs.Task.Result;
        if (response.Equals("fail"))
        {
            throw new Exception($"Failed to load workshift from guard n.-{guardId}");
        }
        WorkShift w = JsonSerializer.Deserialize<WorkShift>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        return w;    
    }
}