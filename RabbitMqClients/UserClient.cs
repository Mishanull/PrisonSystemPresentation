using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using Contracts;
using Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQClient;

public class UserClient : RabbitMqClients.RabbitMQClient, IUserService
{
    private const string Exchange = "sep3.prison";
    public UserClient()
    {
       
    }

    public async Task<User> GetUserAsync(string username)
    {
        var messageBytes = Encoding.UTF8.GetBytes(username);
        var routingKey = "prison.users";
        var response =  await SendMessageWithRoutingKey(routingKey,messageBytes,Exchange);
        Console.WriteLine(response);
        if (response.Equals("fail"))
        {
            throw new Exception("Something went wrong, please try again or contact the support team.");
        }
        User u = JsonSerializer.Deserialize<User>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
         return u;


    }

    
}