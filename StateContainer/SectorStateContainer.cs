using Entities;

namespace StateContainer;

public class SectorStateContainer
{
    private Sector? savedSector;
    public event Action OnChange;
    public Sector Property
    {
        get => savedSector ?? new Sector();
        set
        {
            savedSector = value;
            Console.WriteLine(value);
            NotifyStateChanged();
        }
    }

    
    
    private void NotifyStateChanged() => OnChange?.Invoke();
}