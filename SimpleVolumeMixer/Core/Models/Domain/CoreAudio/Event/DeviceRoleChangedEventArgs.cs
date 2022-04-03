using System;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Types;

namespace SimpleVolumeMixer.Core.Models.Domain.CoreAudio.Event;

public class DeviceRoleChangedEventArgs : EventArgs
{
    internal DeviceRoleChangedEventArgs(AudioDevice device, RoleType role, bool oldState, bool newState)
    {
        Device = device;
        Role = role;
        OldState = oldState;
        NewState = newState;
    }

    public AudioDevice Device { get; }
    public RoleType Role { get; }
    public bool OldState { get; }
    public bool NewState { get; }
}