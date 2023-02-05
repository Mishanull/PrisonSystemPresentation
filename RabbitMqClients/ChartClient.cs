using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Contracts;
using Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMqClients;

public class ChartClient : RabbitMQClient, IChartService
{
    
    private const string Exchange = "chart.exchange";
    public ChartClient() : base()
    {
        
    }
    public async Task<Dictionary<string, int>> GetChartData()
    {
        var messageBytes = Encoding.UTF8.GetBytes("");
        var routingKey = "chart.get";
        var response = await SendMessageWithRoutingKey(routingKey,messageBytes,Exchange);
        if (response.Equals("fail") || response==null)
        {
            throw new Exception("Failed to load alerts");
        }
        Dictionary<string,int> chartData = JsonSerializer.Deserialize<Dictionary<string,int>>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        return chartData;
    }
}