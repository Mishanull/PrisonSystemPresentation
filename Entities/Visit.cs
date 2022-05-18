namespace Entities;

public class Visit
{
    public long Id { get; set; }
    public DateTime DateTime { get; set; }
    public Status Status0 { get; set; }
    public string AccessCode { get; set; }
    public string  FirstName { get; set; }
    public string  LastName { get; set; }
    public string Email { get; set; }
    public int PrisonerSsn { get; set; }
    
    public enum Status
    {
        Waiting,
        Denied,
        Approved
    }
}