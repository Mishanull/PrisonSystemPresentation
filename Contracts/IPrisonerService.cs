using Entities;

namespace Contracts;

public interface IPrisonerService
{
    public Prisoner AddPrisonerAsync(Prisoner newPrisoner);
    public string RemovePrisonerAsync(Prisoner releasedPrisoner);
    public Task<Prisoner> GetPrisonerByIdAsync(long prisonerId);
    Task<ICollection<Prisoner>?> GetPrisonersAsync();
}