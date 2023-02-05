using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Contracts;
using Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQClients;

public class GuardClient : RabbitMqClients.RabbitMQClient,IGuardService
{
    private const string Exchange = "guard.exchange";
    public GuardClient() : base()
    {
        
    }

    public async Task<Guard> GetGuardByIdAsync(long id)
    {
        var messageBytes = Encoding.UTF8.GetBytes(id.ToString());
        var routingKey = "guard.getById";
        var response =  await SendMessageWithRoutingKey(routingKey, messageBytes,exchange:Exchange);
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to fetch guard.");
        }
        var g = JsonSerializer.Deserialize<Guard>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        return g;
        
    }

    

    public async Task<ICollection<Guard>> GetGuardsAsync(int number)
    {
        CancellationToken cancellationToken = default;
        var props = _channel!.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = _replyQueueName;
        var messageBytes = Encoding.UTF8.GetBytes(number.ToString());
        var tcs = new TaskCompletionSource<string>();
        _callbackMapper.TryAdd(correlationId, tcs);                
        _channel.BasicPublish(exchange: Exchange, routingKey: "guards.get", basicProperties: props,
            body: messageBytes);
    
        Console.WriteLine("message published");
        cancellationToken.Register(() => _callbackMapper.TryRemove(correlationId, out var tmp));
        var response =  tcs.Task.Result;
        Console.WriteLine(response);
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to fetch guards.");
        }
        ICollection<Guard> g = JsonSerializer.Deserialize<ICollection<Guard>>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        return g;
    }

    public async Task CreateGuardAsync(Guard guard)
    {
        var guardToSend = JsonSerializer.Serialize(guard, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var messageBytes = Encoding.UTF8.GetBytes(guardToSend);
        
         var routingKey = "guard.add";
         
         var response = await SendMessageWithRoutingKey(routingKey,messageBytes,Exchange);
         if (response.Equals("fail"))
         {
             throw new Exception("Failed to create guard.");
         }
             
    }

    public async Task RemoveGuardAsync(long id)
    {
        var messageBytes = Encoding.UTF8.GetBytes(id.ToString());
        var routingKey = "guard.remove";
        
        var response =  await SendMessageWithRoutingKey(routingKey,messageBytes:messageBytes,Exchange);
        Console.WriteLine(response);
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to remove guard.");
        }
        
    }

    public async Task UpdateGuardAsync(Guard guard)
    {
        var guardToSend = JsonSerializer.Serialize(guard);
        var messageBytes = Encoding.UTF8.GetBytes(guardToSend);
        var routingKey = "guard.update";
        var response =  await SendMessageWithRoutingKey(routingKey,messageBytes,Exchange);
        Console.WriteLine(response);
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to update guard.");
        }
    }

    public async Task<Sector> GetGuardSectorAsync(long guardId)
    {
        var messageBytes = Encoding.UTF8.GetBytes(guardId.ToString());
        var routingKey = "guard.getBySector";
        var response =  await SendMessageWithRoutingKey(routingKey,messageBytes,Exchange);
        Console.WriteLine(response);
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to get guard sector.");
        }
        var sector = JsonSerializer.Deserialize <Sector>(response,new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return sector!;
    }

    public async Task<ICollection<Guard>> GetGuardsBySectorTodayAsync(long sectorId)
    {
        var messageBytes = Encoding.UTF8.GetBytes(sectorId.ToString());
        var routingKey = "guard.getPerSectorToday";
        
        var response =  await SendMessageWithRoutingKey(routingKey,messageBytes,Exchange);
        Console.WriteLine(response);
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to fetch guards.");
        }
        ICollection<Guard> g = JsonSerializer.Deserialize<ICollection<Guard>>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        return g;
    }

    public async Task<ICollection<Guard>> GetGuardsBySectorAsync(long sectorId)
    {
        var messageBytes = Encoding.UTF8.GetBytes(sectorId.ToString());
        var routingKey = "guard.getBySector";
        var response = await SendMessageWithRoutingKey(routingKey, messageBytes, Exchange);
        Console.WriteLine(response);
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to fetch guards.");
        }
        ICollection<Guard> g = JsonSerializer.Deserialize<ICollection<Guard>>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        return g;
    }

    public async Task<List<int>> GetNumberOfGuardsPerSectorTodayAsync()
    {
        var messageBytes = Encoding.UTF8.GetBytes("");
        var routingKey = "guard.getNumPerSectorToday";
        
        var response = await SendMessageWithRoutingKey(routingKey,messageBytes,Exchange);
        Console.WriteLine(response);
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to fetch guards.");
        }
        var g = JsonSerializer.Deserialize<List<int>>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        return g;
    }

    public async Task<List<int>> GetNumberOfGuardsPerSectorAsync()
    {
        var messageBytes = Encoding.UTF8.GetBytes("");
        var routingKey = "guard.getNumPerSector";
        
        var response =await  SendMessageWithRoutingKey(routingKey,messageBytes,Exchange);
        Console.WriteLine(response);
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to fetch guards.");
        }
        var g = JsonSerializer.Deserialize<List<int>>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        return g;
    }

    public async Task<bool> IsGuardAssignedAsync(long guardId)
    {   
        var messageBytes = Encoding.UTF8.GetBytes(guardId.ToString());
        var routingKey = "guard.isAssigned";
        var response = await SendMessageWithRoutingKey(routingKey,messageBytes,Exchange);
        Console.WriteLine(response);
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to fetch guards.");
        }
        
        return bool.Parse(response);
    }

    public async Task<bool> IsGuardWorkingAsync(long guardId)
    {
        var messageBytes = Encoding.UTF8.GetBytes(guardId.ToString());
        var routingKey = "guard.isWorking";
        var response = await SendMessageWithRoutingKey(routingKey,messageBytes,Exchange);
        Console.WriteLine(response);
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to get if guard is working today.");
        }

        return bool.Parse(response);
    }

  
}