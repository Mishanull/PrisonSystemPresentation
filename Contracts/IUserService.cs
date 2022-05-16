using Entities;

namespace Contracts;

public interface IUserService
{
    public Task<User> GetUserAsync(string username);
    public Task SendLogInConfirmation(long id);
    public Task SendLogOutConfirmation(long id);
}