using Entities;

namespace Contracts;

public interface ISectorService
{
    public Task<ICollection<Sector>> GetSectorsAsync();
}