using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SimpleVolumeMixer.Core.Helper.Component;

public class NotifyPropertyChangedBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void SetValue<T>(ref T holder, T newValue, [CallerMemberName] string? callerMemberName = null)
    {
        if (!Equals(holder, newValue))
        {
            holder = newValue;
            RaisePropertyChanged(callerMemberName);
        }
    }

    protected void RaisePropertyChanged(string? propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}