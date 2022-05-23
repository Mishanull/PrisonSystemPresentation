using System.Text.Json.Serialization;

namespace Entities;

public class Alert
{
    public DateTime DateTime { get; set; }
    public String Text { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Priority Priority { get; set; }
    public int DurationInMinutes { get; set; }
    
}