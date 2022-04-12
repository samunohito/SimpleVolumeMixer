using SimpleVolumeMixer.Core.Helper.CoreAudio.Types;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio.Event;

public class DeviceAccessorRoleHolderChangedEventArgs : AudioDeviceAccessorEventArgs
{
    internal DeviceAccessorRoleHolderChangedEventArgs(AudioDeviceAccessor device, RoleType role, bool oldState,
        bool newState) :
        base(device)
    {
        Role = role;
        OldState = oldState;
        NewState = newState;
    }

    public RoleType Role { get; }
    public bool OldState { get; }
    public bool NewState { get; }
}