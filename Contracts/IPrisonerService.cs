using Entities;

namespace Contracts;

public interface IPrisonerService
{
    public Task CreatePrisonerAsync(Prisoner? newPrisoner);
    public Task RemovePrisonerAsync(long id);
    public Task<Prisoner?> GetPrisonerByIdAsync(long prisonerId);
    public Task<ICollection<Prisoner>?> GetPrisonersAsync();
    public Task UpdatePrisonerAsync(Prisoner newPrisoner);
}