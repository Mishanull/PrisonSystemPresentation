namespace Entities;

public class Prisoner
{
    public long Id { get; set; }
    
    public string  FirstName { get; set; }
    
    public string  LastName { get; set; } 
    
    public int Ssn { get; set; }
    
    public string CrimeCommitted { get; set; }
    
    public int Points { get; set; }
    
    public string Note { get; set; }
    // public DateTime EntryDate { get; set; }
    public String EntryDate { get; set; }
    public int DurationInMonths { get; set; }
    public Sector Sector { get; set; }
}