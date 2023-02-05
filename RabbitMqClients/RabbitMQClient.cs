using System.Collections.Concurrent;
using System.Text;
using Microsoft.VisualBasic;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMqClients;

public class RabbitMQClient
{
    protected  IConnection? _connection { get; set; }
    protected  IModel? _channel { get; set; }
    protected  EventingBasicConsumer _consumer { get; set; }

    protected  ConcurrentDictionary<string, TaskCompletionSource<string>> _callbackMapper
    {
        get;
    } = new();

    protected  string _replyQueueName { get; set; }

    protected RabbitMQClient()
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

    protected  Task<string> SendMessageWithRoutingKey(string routingKey, byte[] messageBytes, string exchange)
    {
        String response = "";
        CancellationToken cancellationToken = default;
        IBasicProperties props = _channel!.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = _replyQueueName;
        var tcs = new TaskCompletionSource<string>(); 
        _callbackMapper.TryAdd(correlationId, tcs);
        _channel.BasicPublish(exchange: exchange, routingKey: routingKey, basicProperties: props,
            body: messageBytes);

        Console.WriteLine("message published");
        cancellationToken.Register(() => _callbackMapper.TryRemove(correlationId, out var tmp));
        response = tcs.Task.Result;
            
        Console.WriteLine(response);
        return Task.FromResult(response);
    }
}
