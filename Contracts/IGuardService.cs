using Entities;

namespace Contracts;

public interface IGuardService
{
    public Task<Guard> GetGuardByIdAsync(long id);
    public Task<ICollection<Guard>> GetGuardsAsync(int number);
    public Task CreateGuardAsync(Guard guard);
    public Task RemoveGuardAsync(long id);
    public Task UpdateGuardAsync(Guard guard);
    public Task<Sector> GetGuardSector(long id);
}