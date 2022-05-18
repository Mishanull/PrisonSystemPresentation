using Entities;

namespace Contracts;

public interface IVisitService
{
    Task CreateVisitAsync(Visit visit);
    Task<ICollection<Visit>> GetVisitsAsync();
    Task<Visit> GetVisitByAccessCodeAsync(string code);
    Task UpdateVisitStatusAsync(long id, Visit.Status status);
}