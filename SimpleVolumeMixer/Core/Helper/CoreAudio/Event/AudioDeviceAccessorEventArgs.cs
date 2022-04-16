using System;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio.Event;

/// <summary>
/// <see cref="EventArgs"/> to be used when an event related to <see cref="AudioDeviceAccessor"/> occurs.
/// </summary>
public class AudioDeviceAccessorEventArgs : EventArgs
{
    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="device"><see cref="AudioDeviceAccessor"/> where the event occurred</param>
    public AudioDeviceAccessorEventArgs(AudioDeviceAccessor device)
    {
        Device = device;
    }

    /// <summary>
    /// <see cref="AudioDeviceAccessor"/> where the event occurred.
    /// </summary>
    public AudioDeviceAccessor Device { get; }
}