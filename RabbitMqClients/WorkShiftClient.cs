using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Contracts;
using Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMqClients;

public class WorkShiftClient : RabbitMQClient, IWorkShiftService
{
    
    
    private const string Exchange = "workShift.exchange";

    public WorkShiftClient() : base()
    {
    }
    public async Task<ICollection<WorkShift>> GetWorkShiftsAsync()
    {
        var routingKey = "workShift.get";
        
        var messageBytes = Encoding.UTF8.GetBytes("");
 
        
        var response =  await SendMessageWithRoutingKey(routingKey,messageBytes,Exchange);
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to load workshifts");
        }
    
        ICollection<WorkShift> ws = JsonSerializer.Deserialize<ICollection<WorkShift>>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        return ws;
    }

    public async Task CreateWorkShiftAsync(WorkShift workShift)
    {
        var routingKey = "workShift.add";
        var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(workShift));
        
        var response =  await SendMessageWithRoutingKey(routingKey,messageBytes,Exchange);
        Console.WriteLine(response);
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to create a workshift");
        }
        
    }

    public async Task RemoveWorkShiftAsync(long id)
    {
        var routingKey = "workShift.remove";
        var messageBytes = Encoding.UTF8.GetBytes(id.ToString());
        
        
        var response =  await SendMessageWithRoutingKey(routingKey,messageBytes,Exchange);
        if (response.Equals("fail"))
        {
            throw new Exception($"Failed to remove workshift n.-{id}");
        }
    }

    public async Task UpdateWorkShiftAsync(WorkShift workShift)
    {

        var routingKey = "workShift.update";
        var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(workShift));
        
        
        var response =  await SendMessageWithRoutingKey(routingKey,messageBytes,Exchange);
        if (response.Equals("fail"))
        {
            throw new Exception($"Failed to update workshift n.-{workShift.Id}");
        }
    }

    public async Task<WorkShift> GetWorkShiftByIdAsync(long? id)
    {
        var routingKey = "workShift.getById";
        var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(id));
        
        var response =  await SendMessageWithRoutingKey(routingKey,messageBytes,Exchange);
        if (response.Equals("fail"))
        {
            throw new Exception($"Failed to load workshift n.-{id}");
        }
        WorkShift w = JsonSerializer.Deserialize<WorkShift>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        return w;    
    }


    public async Task AddGuardToWorkShiftAsync(string guardId, string shiftId)
    {
        var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new[]{guardId, shiftId}));
        var routingKey = "workShift.addGuard";

        var response =  await SendMessageWithRoutingKey(routingKey,messageBytes,Exchange);
        if (response.Equals("fail"))
        {
            throw new Exception($"Failed to add guard n.-{guardId} to workshift n.-{shiftId}");
        }
    }

    public async Task RemoveGuardFromWorkShiftAsync(string guardId, string shiftId)
    {
        var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize($"{guardId}{shiftId}"));
        var routingKey = "workShift.removeGuard";
        var response =  await SendMessageWithRoutingKey(routingKey,messageBytes,Exchange);
        if (response.Equals("fail"))
        {
            throw new Exception($"Failed to remove guard n.-{guardId} from workshift n.-{shiftId}");
        }
    }

    public async Task<WorkShift> GetWorkShiftByGuardAsync(long guardId)
    {
        var routingKey = "workShift.getByGuardId";
        
        var messageBytes = Encoding.UTF8.GetBytes(guardId.ToString());
        var response =  await SendMessageWithRoutingKey(routingKey,messageBytes,Exchange);
        if (response.Equals("fail"))
        {
            throw new Exception($"Failed to load workshift from guard n.-{guardId}");
        }
        WorkShift w = JsonSerializer.Deserialize<WorkShift>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        return w;    
    }
}