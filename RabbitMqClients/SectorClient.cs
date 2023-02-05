using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Contracts;
using Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMqClients;

public class SectorClient : RabbitMQClient,ISectorService
{


    private const string Exchange = "sector.exchange";

    public SectorClient() : base()
    {
     
    }
    
    public async Task<ICollection<Sector>> GetSectorsAsync()
    {
        var messageBytes = Encoding.UTF8.GetBytes("");
        var routingKey = "sectors.get";
        var response =  await SendMessageWithRoutingKey(routingKey,messageBytes,Exchange);
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to load sectors");
        }
    
        ICollection<Sector> ss = JsonSerializer.Deserialize<ICollection<Sector>>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        return ss;
    }
}