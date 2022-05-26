using Entities;

namespace Contracts;

public interface IPrisonerService
{
    public Task CreatePrisonerAsync(Prisoner newPrisoner);
    public Task RemovePrisonerAsync(long id);
    public Task<Prisoner> GetPrisonerByIdAsync(long prisonerId);
    public Task<Prisoner> GetPrisonerBySSN(string prisonerSSN);
    // public Task<ICollection<Prisoner>> GetPrisonersAsync();
    public Task UpdatePrisonerAsync(Prisoner newPrisoner);
    public Task<ICollection<Prisoner>?> GetPrisonersAsync(int pageNumber, int pageSize);
    public Task<int> GetPrisonerCount();
    public Task<ICollection<Prisoner>?> GetPrisonersBySectorAsync(int selectedPage, int pageSize, int sectorId);
}