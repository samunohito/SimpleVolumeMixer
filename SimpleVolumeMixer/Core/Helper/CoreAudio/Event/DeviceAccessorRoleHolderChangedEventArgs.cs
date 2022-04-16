using System;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Types;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio.Event;

/// <summary>
/// <see cref="EventArgs"/> fired when the state of <see cref="AudioDeviceRole"/> held by <see cref="AudioDeviceAccessor"/> changes.
/// </summary>
public class DeviceAccessorRoleHolderChangedEventArgs : AudioDeviceAccessorEventArgs
{
    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="device"><see cref="AudioDeviceAccessor"/> where the event occurred</param>
    /// <param name="role">Role in which the change occurred</param>
    /// <param name="oldState">old state</param>
    /// <param name="newState">new state</param>
    public DeviceAccessorRoleHolderChangedEventArgs(
        AudioDeviceAccessor device,
        RoleType role,
        bool oldState,
        bool newState
    ) : base(device)
    {
        Role = role;
        OldState = oldState;
        NewState = newState;
    }

    /// <summary>
    /// Role in which the change occurred
    /// </summary>
    public RoleType Role { get; }

    /// <summary>
    /// old state
    /// </summary>
    public bool OldState { get; }

    /// <summary>
    /// new state
    /// </summary>
    public bool NewState { get; }
}