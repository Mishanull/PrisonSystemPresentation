using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Contracts;
using Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMqClients;

public class VisitClient : RabbitMQClient, IVisitService
{

    
    private const string Exchange = "visit.exchange";

    public VisitClient() : base()
    {
    
    }

    public async Task CreateVisitAsync(Visit visit)
    {
        var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(visit, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
        var routingKey = "visit.add";

        var response =  await SendMessageWithRoutingKey(routingKey,messageBytes,Exchange);
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to request a visit");
        }
    }
    
    //TODO move that bit of logic to application tier
    public async Task<Visit> GetAccessCodeConfirmationAsync(string code)
    {
        var routingKey = "visit.getByCode";
        
        var messageBytes = Encoding.UTF8.GetBytes(code);
        
        var response =  await SendMessageWithRoutingKey(routingKey,messageBytes,Exchange);
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
        var visit = JsonSerializer.Deserialize<Visit>(response, new JsonSerializerOptions
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
        var routingKey = "visit.update";
        var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(v, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));

        var response =  await SendMessageWithRoutingKey(routingKey,messageBytes,Exchange);
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to update the visit");
        }
    }

    public async Task<ICollection<Visit>> GetVisitsAsync(int pageNumber, int pageSize)
    {
        var routingKey = "visit.get";
        
        var messageBytes = Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(new[] {pageNumber.ToString(), pageSize.ToString()}));
        
        var response =  await SendMessageWithRoutingKey(routingKey,messageBytes,Exchange);
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to load visits");
        }
    
        ICollection<Visit> visits = JsonSerializer.Deserialize<ICollection<Visit>>
        (response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        return visits;
    }

    public async Task<ICollection<Visit>> GetVisitsTodayAsync()
    {
        var routingKey = "visit.getNumToday";
        var messageBytes = Encoding.UTF8.GetBytes("");
        
        var response =  await SendMessageWithRoutingKey(routingKey,messageBytes,Exchange);
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to load number of visits");
        }

        ICollection<Visit>? resp = JsonSerializer.Deserialize<ICollection<Visit>>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return resp!;
    }

    public async Task<ICollection<Visit>> GetVisitsPendingAsync()
    {
        var routingKey = "visit.getPending";
        
        var messageBytes = Encoding.UTF8.GetBytes("");
        
        
        var response =  await SendMessageWithRoutingKey(routingKey,messageBytes,Exchange);
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to load number of visits");
        }

        ICollection<Visit>? resp = JsonSerializer.Deserialize<ICollection<Visit>>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return resp!;
    }
}