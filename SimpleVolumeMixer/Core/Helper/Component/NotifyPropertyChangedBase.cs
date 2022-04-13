using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SimpleVolumeMixer.Core.Helper.Component;

/// <summary>
/// Abstract class for efficiently preparing implementations of the <see cref="INotifyPropertyChanged"/> interface
/// </summary>
/// <seealso cref="INotifyPropertyChanged"/>
public class NotifyPropertyChangedBase : INotifyPropertyChanged
{
    /// <inheritdoc cref="INotifyPropertyChanged.PropertyChanged"/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Attempts to write the value of <see cref="newValue"/> to the <see cref="holder"/>.
    /// When the value of the <see cref="holder"/> is updated to the value of <see cref="newValue"/>,
    /// a property change notification by <see cref="RaisePropertyChanged"/> is generated.
    /// If the values are the same, the aforementioned process is not performed.
    /// </summary>
    /// <param name="holder">A reference to a variable that holds a value. Note that the variable may be rewritten to the value of <see cref="newValue"/></param>
    /// <param name="newValue">new value</param>
    /// <param name="callerMemberName">Name of the location where this function was called. This is set automatically, so you do not need to do anything explicitly.</param>
    /// <typeparam name="T"></typeparam>
    protected void SetValue<T>(ref T holder, T newValue, [CallerMemberName] string? callerMemberName = null)
    {
        if (!Equals(holder, newValue))
        {
            holder = newValue;
            RaisePropertyChanged(callerMemberName);
        }
    }

    /// <summary>
    /// Raises PropertyChanged event
    /// </summary>
    /// <param name="propertyName">Name of the property where the change occurred</param>
    protected void RaisePropertyChanged(string? propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}