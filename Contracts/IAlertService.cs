using Entities;

namespace Contracts;

public interface IAlertService
{
    public Task<string> SendAlert(Alert alert);
    public Task<ICollection<Alert>> GetAlertsAsync(int pageNumber, int pageSize);
    public Task<ICollection<Alert>> GetAlertsToday();
}