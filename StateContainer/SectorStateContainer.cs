using Entities;

namespace StateContainer;

public class SectorStateContainer
{
    private Sector? _savedSector;
    public event Action OnChange;
    public Sector Property
    {
        get => _savedSector ?? new Sector();
        set
        {
            _savedSector = value;
            Console.WriteLine(value);
            NotifyStateChanged();
        }
    }
    
    private void NotifyStateChanged() => OnChange?.Invoke();
}