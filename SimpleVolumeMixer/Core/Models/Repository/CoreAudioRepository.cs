using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SimpleVolumeMixer.Core.Contracts.Models.Repository;
using SimpleVolumeMixer.Core.Helper.CoreAudio;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Event;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Types;
using SimpleVolumeMixer.Core.Helper.InteropServices;

namespace SimpleVolumeMixer.Core.Models.Repository;

public class CoreAudioRepository : ICoreAudioRepository
{
    private static readonly IDictionary<RoleType, ERole> Roles = new Dictionary<RoleType, ERole>()
    {
        { RoleType.Console, ERole.Console },
        { RoleType.Communications, ERole.Communications },
        { RoleType.Multimedia, ERole.Multimedia },
    };

    private readonly CompositeDisposable _disposable;
    private readonly CoreAudioAccessor _accessor;

    public CoreAudioRepository()
    {
        _disposable = new CompositeDisposable();
        _accessor = new CoreAudioAccessor().AddTo(_disposable);
        _accessor.DeviceRoleChanged += OnDeviceRoleChanged;
        _accessor.DeviceDisposed += OnDeviceDisposed;

        // RoleChangedは断続的に発生するため、簡易的な仕組みのReactivePropertySlimだと不整合が起こってしまう。
        // ReactivePropertyを使用し、イベントを通知された順に処理する。
        CommunicationRoleDevice = new ReactiveProperty<AudioDeviceAccessor?>().AddTo(_disposable);
        MultimediaRoleDevice = new ReactiveProperty<AudioDeviceAccessor?>().AddTo(_disposable);

        _accessor.RefreshDevices();
    }

    public ReadOnlyReactiveCollection<AudioDeviceAccessor> AudioDevices => _accessor.AudioDevices;
    public IReactiveProperty<AudioDeviceAccessor?> CommunicationRoleDevice { get; }
    public IReactiveProperty<AudioDeviceAccessor?> MultimediaRoleDevice { get; }

    private void OnDeviceRoleChanged(object? sender, DeviceRoleHolderChangedEventArgs e)
    {
        if (e.Role == RoleType.Communications && e.NewState)
        {
            CommunicationRoleDevice.Value = e.Device;
        }

        if (e.Role == RoleType.Multimedia && e.NewState)
        {
            MultimediaRoleDevice.Value = e.Device;
        }
    }
    
    private void OnDeviceDisposed(object? sender, DeviceDisposeEventArgs e)
    {
        if (e.Device == CommunicationRoleDevice.Value)
        {
            // CommunicationRoleDevice.Value = null;
        }

        if (e.Device == MultimediaRoleDevice.Value)
        {
            // MultimediaRoleDevice.Value = null;
        }
    }

    public void SetDefaultDevice(AudioDeviceAccessor accessor, RoleType roleType)
    {
        var deviceId = accessor.DeviceId;
        if (accessor.IsDisposed || deviceId == null || roleType == RoleType.Unknown)
        {
            return;
        }

        if (roleType == RoleType.Communications && deviceId == CommunicationRoleDevice.Value?.DeviceId)
        {
            return;
        } 
        
        if (roleType == RoleType.Multimedia && deviceId == MultimediaRoleDevice.Value?.DeviceId)
        {
            return;
        } 

        PolicyConfigClient.SetDefaultDevice(deviceId, Roles[roleType]);
    }

    public void Dispose()
    {
        _accessor.DeviceRoleChanged -= OnDeviceRoleChanged;
        _disposable.Dispose();
    }
}