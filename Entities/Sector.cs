namespace Entities;

public class Sector
{
    public long Id { get; set; }
    public int Capacity { get; set; }

    public Sector(long id, int capacity)
    {
        Id = id;
        Capacity = capacity;
    }

    public override string ToString()
    {
        return $"{Id}";
    }
    
}