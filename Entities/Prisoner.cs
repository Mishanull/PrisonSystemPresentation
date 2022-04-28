namespace Entities;

public class Prisoner
{
    public long Id { get; set; }
    
    public string  Firstname { get; set; }
    
    public string  Lastname { get; set; } 
    
    public int Ssn { get; set; }
    
    public string CrimeCommitted { get; set; }
    
    public int Points { get; set; }
    
    public string Note { get; set; }
}