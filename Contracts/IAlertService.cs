using Entities;

namespace Contracts;

public interface IAlertService
{
    public Task SendAlert(Alert alert);
    Task<ICollection<Alert>> GetAlertsAsync(int pageNumber, int pageSize);
}