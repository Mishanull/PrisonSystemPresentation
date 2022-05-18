using Entities;

namespace Contracts;

public interface ISectorService
{
    Task<ICollection<Sector>> GetSectorsAsync();
}