using Entities;

namespace Contracts;

public interface IVisitService
{
    public Task CreateVisitAsync(Visit visit);
    public Task<Visit> GetAccessCodeConfirmation(string code);
    public Task UpdateVisitStatusAsync(Visit v);
    public Task<ICollection<Visit>> GetVisitsAsync(int pageNumber, int pageSize);
}