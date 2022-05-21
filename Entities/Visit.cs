namespace Entities;

public class Visit
{
    public long Id { get; set; }
    public DateTime VisitDate { get; set; }
    public Status Status { get; set; }
    public string AccessCode { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public int PrisonerSsn { get; set; }

}