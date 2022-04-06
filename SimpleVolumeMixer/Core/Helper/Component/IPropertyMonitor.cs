using SimpleVolumeMixer.Core.Helper.Component.Types;

namespace SimpleVolumeMixer.Core.Helper.Component;

public interface IPropertyMonitor
{
    object Value { get; set; }
    PropertyMonitorIntervalType IntervalType { get; set; }
    void Start();
    void Stop();
    void Refresh();
}

public interface IPropertyMonitor<T> : IPropertyMonitor
{
    new T Value { get; set; }
}