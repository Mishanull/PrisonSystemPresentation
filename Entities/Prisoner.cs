namespace Entities;

public class Prisoner
{
    public long Id { get; set; }
    public string  FirstName { get; set; }
    public string  LastName { get; set; }
    public int Ssn { get; set; }
    public string CrimeCommitted { get; set; }
    public int Points { get; set; }
    public ICollection<Note>? Notes { get; set; }
    public DateTime EntryDate { get; set; }
    public DateTime ReleaseDate { get; set; }
    public Sector? Sector { get; set; }
}