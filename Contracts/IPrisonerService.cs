using Entities;

namespace Contracts;

public interface IPrisonerService
{
    public Task<Prisoner> AddPrisonerAsync(Prisoner? newPrisoner);
    public Task<string> RemovePrisonerAsync(Prisoner releasedPrisoner);
    public Task<Prisoner?> GetPrisonerByIdAsync(long prisonerId);
    public Task<ICollection<Prisoner>?> GetPrisonersAsync();
    public Task<Prisoner?> UpdatePrisonerAsync(Prisoner newPrisoner);
}