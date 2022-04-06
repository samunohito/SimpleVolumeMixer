using System;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio.Event;

public class AudioDeviceAccessorEventArgs : EventArgs
{
    public AudioDeviceAccessorEventArgs(AudioDeviceAccessor device)
    {
        Device = device;
    }
    
    public AudioDeviceAccessor Device { get; }
}