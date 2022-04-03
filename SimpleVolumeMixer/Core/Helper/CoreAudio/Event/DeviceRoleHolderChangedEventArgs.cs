using SimpleVolumeMixer.Core.Helper.CoreAudio.Types;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio.Event;

public class DeviceRoleHolderChangedEventArgs : System.EventArgs
{
    internal DeviceRoleHolderChangedEventArgs(AudioDeviceAccessor device, RoleType role, bool oldState, bool newState)
    {
        Device = device;
        Role = role;
        OldState = oldState;
        NewState = newState;
    }

    public AudioDeviceAccessor Device { get; }
    public RoleType Role { get; }
    public bool OldState { get; }
    public bool NewState { get; }
}