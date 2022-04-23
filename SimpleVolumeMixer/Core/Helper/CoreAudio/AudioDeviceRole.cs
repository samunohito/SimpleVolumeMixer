using System;
using CSCore.CoreAudioAPI;
using SimpleVolumeMixer.Core.Helper.Component;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Event;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Types;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio;

/// <summary>
/// <see cref="AudioDeviceAccessor"/>が参照する<see cref="MMDevice"/>に割り当てられたロールを保持する。
/// <see cref="MMDevice"/>からはロールを直接取得することは出来ず、各デバイスに割り当てられたロールを同時に全て知ることが難しくなっている。
/// よって、このオブジェクトに<see cref="MMDevice"/>が持つロールの情報を集約して管理し、デバイスが持つロールの情報にアクセスしやすくする。
/// </summary>
public class AudioDeviceRole : NotifyPropertyChangedBase
{
    /// <summary>
    /// ロールの状態が変化した際に発動するイベントハンドラ
    /// </summary>
    public event EventHandler<DeviceAccessorRoleHolderChangedEventArgs>? RoleChanged;

    private readonly AudioDeviceAccessor _device;
    private bool _multimedia;
    private bool _communications;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="device"></param>
    public AudioDeviceRole(AudioDeviceAccessor device)
    {
        _device = device;
        _multimedia = false;
        _communications = false;
    }

    /// <summary>
    /// デバイスが<see cref="RoleType.Multimedia"/>のロールを持っているかを取得・設定する。
    /// </summary>
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

    /// <summary>
    /// デバイスが<see cref="RoleType.Communications"/>のロールを持っているかを取得・設定する。
    /// </summary>
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
        RoleChanged?.Invoke(this, new DeviceAccessorRoleHolderChangedEventArgs(_device, roleType, oldState, newState));
    }
}