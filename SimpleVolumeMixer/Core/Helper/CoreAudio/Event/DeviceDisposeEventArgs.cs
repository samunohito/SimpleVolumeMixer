namespace SimpleVolumeMixer.Core.Helper.CoreAudio.Event;

public class DeviceDisposeEventArgs : System.EventArgs
{
    public DeviceDisposeEventArgs(AudioDeviceAccessor accessor)
    {
        Device = accessor;
    }

    public AudioDeviceAccessor Device { get; }
}