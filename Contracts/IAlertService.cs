using Entities;

namespace Contracts;

public interface IAlertService
{
    public Task SendAlert(Alert alert);
}