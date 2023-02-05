using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Contracts;
using Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMqClients;

public class AlertClient : RabbitMQClient,IAlertService
{
  
    private const string Exchange = "alert.exchange";
    public AlertClient() : base()
    {
        
        
           
    }

    public async Task<string> SendAlertAsync(Alert alert)
    {
        var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(alert, new JsonSerializerOptions
        {
            PropertyNamingPolicy= JsonNamingPolicy.CamelCase
        }));
        var routingKey = "alert.broadcast";
        
        var response = await SendMessageWithRoutingKey(routingKey,messageBytes,Exchange);
        Console.WriteLine(response);
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to send alert.");
        }
        return "success";
    }
    

    public async Task<ICollection<Alert>> GetAlertsAsync(int pageNumber, int pageSize)
    {
        var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new[] {pageNumber.ToString(), pageSize.ToString()}));
        var routingKey = "alert.get";
        
        var response =  await  SendMessageWithRoutingKey(routingKey,messageBytes: messageBytes,exchange: Exchange);
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
        var messageBytes = Encoding.UTF8.GetBytes("");
        var routingKey = "alert.getNum";
        
        var response =  await SendMessageWithRoutingKey(routingKey,messageBytes,Exchange);
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