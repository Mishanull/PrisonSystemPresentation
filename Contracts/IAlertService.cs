using Entities;

namespace Contracts;

public interface IAlertService
{
    public Task SendAlert(Alert alert, long[] sectors);
    Task<ICollection<Alert>> GetAlertsAsync(int pageNumber, int pageSize);
}