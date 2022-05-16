namespace Entities;

public class Alert
{
    public DateTime dateTime { get; set; }
    public String text { get; set; }

    public Priority priority { get; set; }

    public enum Priority
    {
        Low,
        Medium,
        High
    }
}