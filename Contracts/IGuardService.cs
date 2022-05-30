using Entities;

namespace Contracts;

public interface IGuardService
{
    public Task<Guard> GetGuardByIdAsync(long id);
    public Task<ICollection<Guard>> GetGuardsAsync(int number);
    public Task CreateGuardAsync(Guard guard);
    public Task RemoveGuardAsync(long id);
    public Task UpdateGuardAsync(Guard guard);
    public Task<Sector> GetGuardSectorAsync(long id);
    public Task<ICollection<Guard>> GetGuardsBySector(long sectorId);
    public Task<List<int>> GetNoOfGuardsPerSectToday();
    public Task<List<int>> GetNoOfGuardsPerSect();
    public Task<bool> IsGuardAssigned(long guardId);
    public Task<bool> IsGuardWorking(long guardId);
}