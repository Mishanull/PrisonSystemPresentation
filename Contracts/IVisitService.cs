using Entities;

namespace Contracts;

public interface IVisitService
{
    Task CreateVisitAsync(Visit visit);
    Task<ICollection<Visit>> GetVisitsAsync();
    Task<Visit> GetAccessCodeConfirmation(string code);
    Task UpdateVisitStatusAsync(long id, Status status);
}