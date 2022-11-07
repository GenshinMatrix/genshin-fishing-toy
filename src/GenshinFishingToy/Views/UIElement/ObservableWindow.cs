using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace GenshinFishingToy.Views;

public class ObservableWindow : Window
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void RaisePropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new(propertyName));

    protected void Set<T>(ref T field, T newValue, [CallerMemberName] string? propertyName = null)
    {
        field = newValue;
        RaisePropertyChanged(propertyName!);
    }

    protected void Set2<T>(ref T field, T newValue, [CallerMemberName] string? propertyName = null)
    {
        if (!Equals(field, newValue))
        {
            field = newValue;
            RaisePropertyChanged(propertyName!);
        }
    }
}
