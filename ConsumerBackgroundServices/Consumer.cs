using System.Text;
using System.Text.Json;
using Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ConsumerBackgroundServices;

public class Consumer: BackgroundService
{
    private IServiceProvider _sp;
    private ConnectionFactory _factory;
    private IConnection _connection;
    private IModel _channel;
    private string replyQueueName;

    public Consumer(IServiceProvider sp)
    {
        Console.WriteLine("Service started");
        _sp = sp;
        _factory = new ConnectionFactory() {HostName = "localhost", UserName = "guest", Password = "guest"};
        _connection = _factory.CreateConnection();
        _channel = _connection.CreateModel();
        replyQueueName=_channel.QueueDeclare("").QueueName;
        _channel.QueueBind(replyQueueName,"guard.listen","");
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("ExecuteAsync");
        if (stoppingToken.IsCancellationRequested)
        {
            _channel.Dispose();
            _connection.Dispose();
            Console.WriteLine("CancellationRequested");

        }
        var sectorState = _sp.CreateScope().ServiceProvider.GetService < StateContainer.SectorStateContainer>();
        sectorState!.OnChange += () =>
        {
            switch (sectorState.Property.Id)
            {
                case 1:     _channel.QueueBind(replyQueueName,"guard.listen.sector1","");
                    break;
                case 2: _channel.QueueBind(replyQueueName,"guard.listen.sector2","");
                    break;
                case 3: _channel.QueueBind(replyQueueName,"guard.listen.sector3","");
                    break;
            }
        };
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received +=   (model, ea) =>
        {
            Console.WriteLine("received something");
            var body = ea.Body.ToArray();
            String result = Encoding.UTF8.GetString(body,0,body.Length);
            Console.WriteLine(result+" received");
            Alert a = JsonSerializer.Deserialize<Alert>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })!;
            var message = Encoding.UTF8.GetString(body);
            using (var scope = _sp.CreateScope())
            {
                
                var state = scope.ServiceProvider.GetService<StateContainer.AlertStateContainer>();
                state!.Property = a;
                
            }
            Console.WriteLine(" [x] Received {0}", message);
        };
        _channel.BasicConsume(queue:replyQueueName, autoAck: true, consumer: consumer);
    }
}