using System;
using SimpleVolumeMixer.Core.Helper.Component;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Event;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Types;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio;

public class DeviceRoleHolder : NotifyPropertyChangedBase
{
    internal event EventHandler<DeviceRoleHolderChangedEventArgs>? RoleChanged;

    private readonly AudioDeviceAccessor _device;
    private bool _multimedia;
    private bool _communications;

    internal DeviceRoleHolder(AudioDeviceAccessor device)
    {
        _device = device;
        _multimedia = false;
        _communications = false;
    }

    public bool Multimedia
    {
        get => _multimedia;
        set
        {
            var oldState = _multimedia;
            if (oldState != value)
            {
                SetValue(ref _multimedia, value);
                RaiseDeviceRoleChanged(RoleType.Multimedia, oldState, value);
            }
        }
    }

    public bool Communications
    {
        get => _communications;
        set
        {
            var oldState = _communications;
            if (oldState != value)
            {
                SetValue(ref _communications, value);
                RaiseDeviceRoleChanged(RoleType.Communications, oldState, value);
            }
        }
    }

    private void RaiseDeviceRoleChanged(RoleType roleType, bool oldState, bool newState)
    {
        RoleChanged?.Invoke(this, new DeviceRoleHolderChangedEventArgs(_device, roleType, oldState, newState));
    }
}