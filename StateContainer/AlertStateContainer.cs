using Entities;

namespace StateContainer;

public class AlertStateContainer
{
    private Alert? savedAlert;
    public event Action OnChange;
    public Alert Property
    {
        get => savedAlert ?? new Alert();
        set
        {
            savedAlert = value;
            Console.WriteLine(value);
            NotifyStateChanged();
        }
    }

    
    
    private void NotifyStateChanged() => OnChange?.Invoke();
}