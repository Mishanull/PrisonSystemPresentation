using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using Contracts;
using Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQClient;

public class UserClient : IUserService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly EventingBasicConsumer _consumer;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _callbackMapper = new();
    private readonly string _replyQueueName;

    public UserClient()
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

    public async Task<User> GetUserAsync(string username)
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = _channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = _replyQueueName;
        var messageBytes = Encoding.UTF8.GetBytes(username);
        var tcs = new TaskCompletionSource<string>();
        _callbackMapper.TryAdd(correlationId, tcs);                
        _channel.BasicPublish(exchange: "sep3.prison", routingKey: "prison.users", basicProperties: props, body: messageBytes);
        cancellationToken.Register(() => _callbackMapper.TryRemove(correlationId, out var tmp));
        String response =  tcs.Task.Result;
        Console.WriteLine(response);
        if (response.Equals("fail"))
        {
            throw new Exception("Something went wrong, please try again or contact the support team.");
        }
        User u = JsonSerializer.Deserialize<User>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
         return u;


    }

    public async Task SendLogInConfirmationAsync(long id)
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = _channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = _replyQueueName;
        var messageBytes = Encoding.UTF8.GetBytes(id.ToString());
        var tcs = new TaskCompletionSource<string>();
        _callbackMapper.TryAdd(correlationId, tcs);                
        _channel.BasicPublish(exchange: "sep3.prison", routingKey: "login.confirm", basicProperties: props, body: messageBytes);
        Console.WriteLine("message published login");
        cancellationToken.Register(() => _callbackMapper.TryRemove(correlationId, out var tmp));
    }

    public Task SendLogOutConfirmationAsync(long id)
    {
        throw new NotImplementedException();
    }
}