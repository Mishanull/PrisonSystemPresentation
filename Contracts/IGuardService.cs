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
    public Task<ICollection<Guard>> GetGuardsBySectorTodayAsync(long sectorId);
    public Task<List<int>> GetNumberOfGuardsPerSectorTodayAsync();
    public Task<List<int>> GetNumberOfGuardsPerSectorAsync();
    public Task<bool> IsGuardAssignedAsync(long guardId);
    public Task<bool> IsGuardWorkingAsync(long guardId);
    public Task ChangePasswordAsync(Guard loggedGuard);
}