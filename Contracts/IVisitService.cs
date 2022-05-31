using Entities;

namespace Contracts;

public interface IVisitService
{
    public Task CreateVisitAsync(Visit visit);
    public Task<Visit> GetAccessCodeConfirmationAsync(string code);
    public Task UpdateVisitStatusAsync(Visit v);
    public Task<ICollection<Visit>> GetVisitsAsync(int pageNumber, int pageSize);
    public Task<ICollection<Visit>> GetVisitsTodayAsync();
    public Task<ICollection<Visit>> GetVisitsPendingAsync();
}