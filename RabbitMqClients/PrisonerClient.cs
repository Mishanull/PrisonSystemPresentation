using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Contracts;
using Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMqClients;

public class PrisonerClient : RabbitMQClient,IPrisonerService
{
    
  

    private const string Exchange = "prisoner.exchange";

    public PrisonerClient(): base()
    {
    }
    
    
    public async Task CreatePrisonerAsync(Prisoner prisoner)
    {
        var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(prisoner));
        var routingKey = "prisoner.add";
        
        
        var response =  await SendMessageWithRoutingKey(routingKey, messageBytes, Exchange);
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to create prisoner");
        }
    }

    public async Task RemovePrisonerAsync(long id)
    {
        var messageBytes = Encoding.UTF8.GetBytes(id.ToString());
        var routingKey = "prisoner.remove";

        var response =  await SendMessageWithRoutingKey(routingKey,messageBytes,Exchange);
        if (response.Equals("fail"))
        {
            throw new Exception($"Failed to remove prisoner n.-{id}");
        }
    }

    public async Task<Prisoner> GetPrisonerByIdAsync(long prisonerId)
    {
        var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(prisonerId));
        var routingKey = "prisoner.getById";
        var response = await SendMessageWithRoutingKey(routingKey, messageBytes, Exchange);
        if (response.Equals("fail"))
        {
            throw new Exception($"Failed to load prisoner n.-{prisonerId}");
        }
        var p = JsonSerializer.Deserialize<Prisoner>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        return p;
    }

    public async Task<Prisoner> GetPrisonerBySsnAsync(string prisonerSsn)
    {
        var messageBytes = Encoding.UTF8.GetBytes(prisonerSsn);
        var routingKey = "prisoner.getBySSN";
        var response = await SendMessageWithRoutingKey(routingKey, messageBytes, Exchange);
        if (response.Equals("fail"))
        {
            throw new Exception($"Failed to load prisoner n.{prisonerSsn}");
        }
        var p = JsonSerializer.Deserialize<Prisoner>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        return p;
    }
    

    public async Task UpdatePrisonerAsync(Prisoner newPrisoner)
    {
        var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(newPrisoner));        
        var routingKey = "prisoner.update";
        var response =  await SendMessageWithRoutingKey(routingKey,messageBytes,Exchange) ;
        if (response.Equals("fail"))
        {
            throw new Exception($"Failed to update prisoner n.-{newPrisoner.Id}");
        }
    }

    public async Task<ICollection<Prisoner>?> GetPrisonersAsync(int pageNumber, int pageSize)
    {
        var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new[] {pageNumber.ToString(), pageSize.ToString()}));
        var routingKey = "prisoners.get";
        
        var response =  await SendMessageWithRoutingKey(routingKey,messageBytes,Exchange);
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to load prisoners");
        }
    
        var ps = JsonSerializer.Deserialize<ICollection<Prisoner>>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        return ps;
    }

    public async Task<int> GetPrisonerCountAsync()
    {
        var messageBytes = Encoding.UTF8.GetBytes("");
        var routingKey = "prisoners.count";
        
        
        var response =  await SendMessageWithRoutingKey(routingKey,messageBytes,Exchange);
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to load prisoners. Please try again ");
        }
    
        var count = Int32.Parse(response);
        return count;
    }

    public async Task<ICollection<Prisoner>?> GetPrisonersBySectorAsync(int pageNumber, int pageSize, int sectorId)
    {
        var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new[] {pageNumber.ToString(), pageSize.ToString(),sectorId.ToString()}));
        var routingKey = "prisoners.getBySector";
        
        var response =  await SendMessageWithRoutingKey(routingKey,messageBytes,Exchange);
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to load prisoners");
        }
    
        var ps = JsonSerializer.Deserialize<ICollection<Prisoner>>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        return ps;
    }

    public async Task AddPointsToPrisonerAsync(long prisonerId, int points)
    {
        var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new []{prisonerId.ToString(), points.ToString()}));
        var routingKey = "prisoner.addPoints";
        
        var response =  await SendMessageWithRoutingKey(routingKey,messageBytes,Exchange);
        if (response.Equals("fail"))
        {
            throw new Exception($"Failed to add points prisoner n.-{prisonerId}");
        }
    }

    public async Task<List<int>> GetNumberOfPrisonersPerSectorAsync()
    {
        var messageBytes = Encoding.UTF8.GetBytes("");
        var routingKey = "prisoner.getNumPerSector";
        var response =  await SendMessageWithRoutingKey(routingKey,messageBytes,Exchange);
        Console.WriteLine(response);
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to fetch prisoners number per sector.");
        }
        var g = JsonSerializer.Deserialize<List<int>>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        return g;
    }

    public async Task<ICollection<Prisoner>> GetPrisonersWithLowBehaviourAsync()
    {
        var messageBytes = Encoding.UTF8.GetBytes("");
        var routingKey = "prisoner.getLowBehaviour";
        
        var response =  await SendMessageWithRoutingKey(routingKey,messageBytes,Exchange);
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to load prisoners");
        }
    
        var ps = JsonSerializer.Deserialize<ICollection<Prisoner>>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        return ps;
    }
}