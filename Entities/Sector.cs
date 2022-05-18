namespace Entities;

public class Sector
{
    public long Id { get; set; }
    public int Capacity { get; set; }
    public int OccupiedCells { get; set; }
    
    public int FreeCells => Capacity - OccupiedCells;


    public Sector(long id, int capacity)
    {
        Id = id;
        Capacity = capacity;
    }

    public override string ToString()
    {
        return $"S-{Id} [{OccupiedCells}/{Capacity}]";
    }
    
    
}