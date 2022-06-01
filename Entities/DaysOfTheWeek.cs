namespace Entities;

public class DaysOfTheWeek
{
    public ICollection<string> Days { get; } = new[]
        {"Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"};
}