using Entities;

namespace StateContainer;

public class AlertStateContainer
{
    private Alert? _savedAlert;
    public event Action OnChange;
    public Alert Property
    {
        get => _savedAlert ?? new Alert();
        set
        {
            _savedAlert = value;
            Console.WriteLine(value);
            NotifyStateChanged();
        }
    }
    
    private void NotifyStateChanged() => OnChange?.Invoke();
}