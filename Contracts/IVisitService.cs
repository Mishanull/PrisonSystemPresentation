using Entities;

namespace Contracts;

public interface IVisitService
{
    Task CreateVisitAsync(Visit visit);
    Task<Visit> GetAccessCodeConfirmationAsync(string code);
    Task UpdateVisitStatusAsync(long id, Status status);
    Task<ICollection<Visit>> GetVisitsAsync(int pageNumber, int pageSize);
}