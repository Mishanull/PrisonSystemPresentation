using Entities;

namespace Contracts;

public interface IAlertService
{
    public Task SendAlert(Alert alert);
    public Task<ICollection<Alert>> GetAlerts();
}