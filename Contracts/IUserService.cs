using Entities;

namespace Contracts;

public interface IUserService
{
    public Task<User> GetUserAsync(string username);
    public Task SendLogInConfirmationAsync(long id);
    public Task SendLogOutConfirmationAsync(long id);
}