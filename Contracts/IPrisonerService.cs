using Entities;

namespace Contracts;

public interface IPrisonerService
{
    Task<Prisoner> AddPrisonerAsync(Prisoner? newPrisoner);
    Task<string> RemovePrisonerAsync(Prisoner releasedPrisoner);
    Task<Prisoner?> GetPrisonerByIdAsync(long prisonerId);
    Task<ICollection<Prisoner>?> GetPrisonersAsync();
    Task<Prisoner?> UpdatePrisonerAsync(Prisoner newPrisoner);
}