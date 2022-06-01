using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Contracts;
using Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMqClients;

public class NoteClient : INotesService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly EventingBasicConsumer _consumer;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _callbackMapper = new();
    private readonly string _replyQueueName;

    public NoteClient()
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
    public async Task AddNoteAsync(long prisonerId, string text)
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = _channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        
        props.CorrelationId = correlationId;
        props.ReplyTo = _replyQueueName;
        
        String[] array = new[] {prisonerId.ToString(), text};
        var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(array));
        var tcs = new TaskCompletionSource<string>();
        _callbackMapper.TryAdd(correlationId, tcs);
        _channel.BasicPublish(exchange: "note.exchange", routingKey: "note.add", basicProperties: props, body: messageBytes);
        Console.WriteLine("message published");
        cancellationToken.Register(() => _callbackMapper.TryRemove(correlationId, out var tmp));
        
        String response =  tcs.Task.Result;
        Console.WriteLine(response);
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to add note.");
        }
    }

    public async Task RemoveNoteAsync(long noteId)
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = _channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = _replyQueueName;
        var messageBytes = Encoding.UTF8.GetBytes(noteId.ToString());
        var tcs = new TaskCompletionSource<string>();
        _callbackMapper.TryAdd(correlationId, tcs);                
        _channel.BasicPublish(exchange: "note.exchange", routingKey: "note.remove", basicProperties: props,
            body: messageBytes);
    
        Console.WriteLine("message published");
        cancellationToken.Register(() => _callbackMapper.TryRemove(correlationId, out var tmp));
        String response =  tcs.Task.Result;
        Console.WriteLine(response);
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to remove note.");
        }
    }

    public async Task UpdateNoteAsync(Note note)
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = _channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = _replyQueueName;
        string noteToSend = JsonSerializer.Serialize(note);
        var messageBytes = Encoding.UTF8.GetBytes(noteToSend);
        var tcs = new TaskCompletionSource<string>();
        _callbackMapper.TryAdd(correlationId, tcs);                
        _channel.BasicPublish(exchange: "note.exchange", routingKey: "note.update", basicProperties: props,
            body: messageBytes);
    
        Console.WriteLine("message published");
        cancellationToken.Register(() => _callbackMapper.TryRemove(correlationId, out var tmp));
        String response =  tcs.Task.Result;
        Console.WriteLine(response);
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to update note.");
        }
    }
}