namespace KenmanDesignStudio.Web.Services;

/// <summary>Per-circuit UI state for the light/dark toggle, shared across layouts.</summary>
public class ThemeState
{
    public bool IsDarkMode { get; private set; } = true;

    public event Action? OnChange;

    public void Toggle()
    {
        IsDarkMode = !IsDarkMode;
        OnChange?.Invoke();
    }

    public void Set(bool dark)
    {
        if (dark == IsDarkMode) return;
        IsDarkMode = dark;
        OnChange?.Invoke();
    }
}
