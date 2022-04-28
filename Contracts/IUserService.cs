using Entities;

namespace Contracts;

public interface IUserService
{
    public Task<User> GetUserAsync(string username);
}