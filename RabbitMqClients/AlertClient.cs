using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Contracts;
using Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMqClients;

public class AlertClient : IAlertService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly EventingBasicConsumer _consumer;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _callbackMapper = new();
    private readonly string _replyQueueName;

    public AlertClient()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _replyQueueName = _channel.QueueDeclare(queue: "").QueueName;
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

    public async Task<string> SendAlertAsync(Alert alert)
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = _channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = _replyQueueName;
        var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(alert, new JsonSerializerOptions
        {
            PropertyNamingPolicy= JsonNamingPolicy.CamelCase
        }));
        var tcs = new TaskCompletionSource<string>();
        _callbackMapper.TryAdd(correlationId, tcs);                
        _channel.BasicPublish(exchange: "alert.exchange", routingKey: "alert.broadcast" , basicProperties: props,
            body: messageBytes);
    
        cancellationToken.Register(() => _callbackMapper.TryRemove(correlationId, out var tmp));
        String response = await tcs.Task;
        Console.WriteLine(response);
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to send alert.");
        }
        return "success";
    }
    

    public async Task<ICollection<Alert>> GetAlertsAsync(int pageNumber, int pageSize)
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
        _channel.BasicPublish(exchange: "alert.exchange", routingKey: "alert.get", basicProperties: props, body: messageBytes);
        cancellationToken.Register(() => _callbackMapper.TryRemove(correlationId, out var tmp));
        String response =  tcs.Task.Result;
        if (response.Equals("fail") || response==null)
        {
            throw new Exception("Failed to load alerts");
        }
    
        ICollection<Alert> alerts = JsonSerializer.Deserialize<ICollection<Alert>>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        return alerts;
    }

    public async Task<ICollection<Alert>> GetAlertsTodayAsync()
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = _channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        
        props.CorrelationId = correlationId;
        props.ReplyTo = _replyQueueName;
        
        var messageBytes = Encoding.UTF8.GetBytes("");
        var tcs = new TaskCompletionSource<string>();
        _callbackMapper.TryAdd(correlationId, tcs);                
        _channel.BasicPublish(exchange: "alert.exchange", routingKey: "alert.getNum", basicProperties: props, body: messageBytes);
        cancellationToken.Register(() => _callbackMapper.TryRemove(correlationId, out var tmp));
        String response =  tcs.Task.Result;
        if (response.Equals("fail") || response==null)
        {
            throw new Exception("Failed to load alerts");
        }
    
        ICollection<Alert> alerts = JsonSerializer.Deserialize<ICollection<Alert>>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        return alerts;
    }
}