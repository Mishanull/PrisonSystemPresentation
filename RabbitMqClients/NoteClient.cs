using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Contracts;
using Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMqClients;

public class NoteClient : RabbitMQClient,INotesService
{

    private const string Exchange = "note.exchange";
    public NoteClient() : base()
    {
        
        
    }
    public async Task AddNoteAsync(long prisonerId, string text)
    {
        var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new[] {prisonerId.ToString(), text}));
        var routingKey = "note.add";
        
        var response =  await SendMessageWithRoutingKey(routingKey, messageBytes, Exchange);
        Console.WriteLine(response);
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to add note.");
        }
    }

    public async Task RemoveNoteAsync(long noteId)
    {
        var messageBytes = Encoding.UTF8.GetBytes(noteId.ToString());
        var routingKey = "note.remove";
        var response =  await SendMessageWithRoutingKey(routingKey,messageBytes,Exchange);
        Console.WriteLine(response);
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to remove note.");
        }
    }

    public async Task UpdateNoteAsync(Note note)
    {
        var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(note));
        var routingKey = "note.update";
        var response =  await SendMessageWithRoutingKey(routingKey,messageBytes,Exchange);
        Console.WriteLine(response);
        if (response.Equals("fail"))
        {
            throw new Exception("Failed to update note.");
        }
    }
}