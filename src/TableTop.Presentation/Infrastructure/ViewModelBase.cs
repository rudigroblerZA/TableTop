using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TableTop.Presentation.Infrastructure;

/// <summary>
/// Base class for every shared ViewModel.
///
/// <para>
/// Plain <see cref="INotifyPropertyChanged"/> rather than a framework base
/// type. MAUI's ViewModels previously derived from <c>BindableObject</c>, which
/// is why they could not be shared with WinUI at all — but MAUI binds happily
/// to any <see cref="INotifyPropertyChanged"/> implementation, so the
/// inheritance was buying nothing and costing the entire sharing story.
/// </para>
/// </summary>
public abstract class ViewModelBase : INotifyPropertyChanged
{
    /// <summary>Raised when a bound property changes. A null or empty name means "all properties".</summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Assigns <paramref name="field"/> and raises <see cref="PropertyChanged"/>
    /// only if the value actually differs.
    /// </summary>
    /// <returns>True when the value changed.</returns>
    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    /// <summary>
    /// Raises <see cref="PropertyChanged"/>. Pass null to signal that every
    /// property may have changed, which binding engines treat as "re-read
    /// everything" — used after a bulk reset.
    /// </summary>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
