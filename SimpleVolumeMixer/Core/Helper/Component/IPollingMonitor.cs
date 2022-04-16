using System.ComponentModel;
using SimpleVolumeMixer.Core.Helper.Component.Types;

namespace SimpleVolumeMixer.Core.Helper.Component;

/// <summary>
/// Provides an interface to a function that monitors certain values at regular intervals.
/// </summary>
public interface IPollingMonitor : INotifyPropertyChanged
{
    /// <summary>
    /// Occurs when the result of polling by this interface differs from the previous value.
    /// </summary>
    /// <seealso cref="INotifyPropertyChanged.PropertyChanged"/>
    new event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Holds the latest value obtained during polling.
    /// If the value is rewritten by the polling process, the <see cref="PropertyChanged"/> event is fired.
    /// </summary>
    object Value { get; }

    /// <summary>
    /// Gets or sets the interval for polling. The interval can be set in milliseconds.
    /// See the enum definition for detailed seconds.
    /// If <see cref="PollingMonitorIntervalType.Manual"/> is set,
    /// no polling is performed and the value is updated only when the <see cref="Refresh()"/> method is called externally.
    /// If this property is changed while the polling process is running,
    /// the polling process execution interval is adjusted; if <see cref="PollingMonitorIntervalType.Manual"/> is set, the polling process is stopped.
    /// </summary>
    /// <seealso cref="PollingMonitorIntervalType"/>
    PollingMonitorIntervalType IntervalType { get; set; }

    /// <summary>
    /// Start polling. However, this method only starts the process and does not block the thread.
    /// If this method is called when <see cref="IntervalType"/> is set to <see cref="PollingMonitorIntervalType.Manual"/> or when a polling operation is already running,
    /// no new polling operation will be started and nothing will happen.
    /// </summary>
    void Start();

    /// <summary>
    /// Stops polling, even if called when polling is not running,
    /// including when <see cref="IntervalType"/> is <see cref="PollingMonitorIntervalType.Manual"/>, no exception is made.
    /// </summary>
    void Stop();

    /// <summary>
    /// The process of obtaining the latest value of the polling process is implemented.
    /// When the value is updated, the <see cref="Value"/> property is rewritten and the fact that the value has been updated is notified externally by <see cref="INotifyPropertyChanged.PropertyChanged"/>.
    /// </summary>
    /// <seealso cref="PropertyChanged"/>
    /// <seealso cref="INotifyPropertyChanged.PropertyChanged"/>
    void Refresh();
}

/// <summary>
/// An interface that applies a generic to the <see cref="IPollingMonitor.Value"/> property of the <see cref="IPollingMonitor"/> interface.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IPollingMonitor<T> : IPollingMonitor
{
    /// <summary>
    /// It has the same functionality as the inherited source, the only difference being that the type is generic.
    /// See the documentation of the inherited source for detailed functionality.
    /// </summary>
    /// <seealso cref="IPollingMonitor.Value"/>
    new T Value { get; }
}