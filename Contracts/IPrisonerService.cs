﻿using Entities;

namespace Contracts;

public interface IPrisonerService
{
    public Task CreatePrisonerAsync(Prisoner newPrisoner);
    public Task RemovePrisonerAsync(long id);
    public Task<Prisoner> GetPrisonerByIdAsync(long prisonerId);
    public Task<Prisoner> GetPrisonerBySsn(string prisonerSsn);
    public Task UpdatePrisonerAsync(Prisoner newPrisoner);
    public Task<ICollection<Prisoner>?> GetPrisonersAsync(int pageNumber, int pageSize);
    public Task<int> GetPrisonerCount();
    public Task<ICollection<Prisoner>?> GetPrisonersBySectorAsync(int selectedPage, int pageSize, int sectorId);
    public Task AddPointsToPrisonerAsync(long prisonerId, int points);
    public Task<int[]> GetNumPrisPerSectAsync();
}