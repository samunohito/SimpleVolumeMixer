using System;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Types;

namespace SimpleVolumeMixer.UI.ViewModels.Audio.Event;

public class DeviceRoleChangeRequestEventArgs : EventArgs
{
    public DeviceRoleChangeRequestEventArgs(AudioDeviceViewModel device, DataFlowType dataFlow, RoleType role)
    {
        Device = device;
        DataFlow = dataFlow;
        Role = role;
    }

    public AudioDeviceViewModel Device { get; }
    public DataFlowType DataFlow { get; }
    public RoleType Role { get; }
}