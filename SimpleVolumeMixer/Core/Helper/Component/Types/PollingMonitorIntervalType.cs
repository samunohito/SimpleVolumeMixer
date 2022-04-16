namespace SimpleVolumeMixer.Core.Helper.Component.Types;

/// <summary>
/// Represents the <see cref="IPollingMonitor"/> polling interval. The unit of the number set here is milliseconds.
/// </summary>
public enum PollingMonitorIntervalType
{
    /// <summary>
    /// This is the value set when you want to update manually.
    /// </summary>
    Manual = int.MinValue,
    /// <summary>
    /// Check in increments of 500 milliseconds.
    /// </summary>
    Low = 500,
    /// <summary>
    /// Check in increments of 200 milliseconds.
    /// </summary>
    LowMiddle = 200,
    /// <summary>
    /// Check in increments of 100 milliseconds.
    /// </summary>
    Normal = 100,
    /// <summary>
    /// Check in increments of 40 milliseconds.
    /// Note that heavy use will overload the app!
    /// </summary>
    High = 40,
    /// <summary>
    /// Check in increments of 10 milliseconds.
    /// Note that heavy use will overload the app!
    /// </summary>
    Immediate = 10
}