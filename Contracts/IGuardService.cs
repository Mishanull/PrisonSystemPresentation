using Entities;

namespace Contracts;

public interface IGuardService
{
    public Task<Guard> GetGuard(long id);
    public Task<ICollection<Guard>> GetGuards(int number);
    public Task<string> CreateGuard(Guard guard);
    public Task<string> DeleteGuard(long id);
    public Task<string> UpdateGuard(Guard guard);
}