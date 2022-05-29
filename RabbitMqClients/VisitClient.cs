using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Contracts;
using Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMqClients;

public class VisitClient : IVisitService
{
    private readonly IConnection connection;
    private readonly IModel channel;
    private readonly EventingBasicConsumer consumer;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> callbackMapper = new();
    private readonly string replyQueueName;
    
    private const string Exchange = "visit.exchange";

    public VisitClient()
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

    public async Task CreateVisitAsync(Visit visit)
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        
        props.CorrelationId = correlationId;
        props.ReplyTo = replyQueueName;
        
        var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(visit, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
        var tcs = new TaskCompletionSource<string>();
        callbackMapper.TryAdd(correlationId, tcs);                
        channel.BasicPublish(exchange: Exchange, routingKey: "visit.add", basicProperties: props, body: messageBytes);
        cancellationToken.Register(() => callbackMapper.TryRemove(correlationId, out var tmp));
        
        String response =  tcs.Task.Result;
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to request a visit");
        }
    }
    

    public async Task<Visit> GetAccessCodeConfirmation(string code)
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        
        props.CorrelationId = correlationId;
        props.ReplyTo = replyQueueName;
        
        var messageBytes = Encoding.UTF8.GetBytes(code);
        var tcs = new TaskCompletionSource<string>();
        callbackMapper.TryAdd(correlationId, tcs);                
        channel.BasicPublish(exchange: Exchange, routingKey: "visit.getByCode", basicProperties: props, body: messageBytes);
        cancellationToken.Register(() => callbackMapper.TryRemove(correlationId, out var tmp));
        String response =  tcs.Task.Result;
        Visit visit = new Visit();
        if (response.Equals("fail"))
        {
            throw new Exception($"Failed to load visit code: {code}");
        }

        if (response.Equals("no"))
        {
            throw new Exception("Invalid");
        }

        if (response.Equals("fulfilled"))
        {
            throw new Exception("Visit has already been completed.");
        }
        visit = JsonSerializer.Deserialize<Visit>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
         
         if (visit.VisitDate.Date == DateTime.Today.Date && visit.VisitDate.TimeOfDay > DateTime.Now.TimeOfDay)
         {
             throw new Exception("Visitor is too early. Please try again at " + visit.VisitDate.Hour + ":" +
                                 visit.VisitDate.Minute);
         }
         if (visit.VisitDate.Date != DateTime.Today)
         {
             throw new Exception("This visit is not booked for today.");
         }

         if (visit.VisitDate.Date == DateTime.Today.Date &&
             visit.VisitDate.TimeOfDay.Add(new TimeSpan(0, 30, 0)) < DateTime.Now.TimeOfDay)
         {
             throw new Exception("The visitor is 30 min late. Access is denied.");
         }
         
        return visit;
    }

    public async Task UpdateVisitStatusAsync(Visit v)
    {
        CancellationToken cancellationToken = default;
        IBasicProperties props = channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        
        props.CorrelationId = correlationId;
        props.ReplyTo = replyQueueName;
       
        var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(v, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
        var tcs = new TaskCompletionSource<string>();
        callbackMapper.TryAdd(correlationId, tcs);                
        channel.BasicPublish(exchange: Exchange, routingKey: "visit.update", basicProperties: props, body: messageBytes);
        cancellationToken.Register(() => callbackMapper.TryRemove(correlationId, out var tmp));
        
        String response =  tcs.Task.Result;
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to update the visit");
        }
    }

    public async Task<ICollection<Visit>> GetVisitsAsync(int pageNumber, int pageSize)
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
        channel.BasicPublish(exchange: Exchange, routingKey: "visit.get", basicProperties: props, body: messageBytes);
        cancellationToken.Register(() => callbackMapper.TryRemove(correlationId, out var tmp));
        
        String response =  tcs.Task.Result;
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to load visits");
        }
    
        ICollection<Visit> visits = JsonSerializer.Deserialize<ICollection<Visit>>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        return visits;
    }

    public Task<int> GetNumOfVisitsToday()
    {
        throw new NotImplementedException();
    }
}