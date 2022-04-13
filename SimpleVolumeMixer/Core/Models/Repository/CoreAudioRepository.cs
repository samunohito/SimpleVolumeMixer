using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using Microsoft.Extensions.Logging;
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

    private readonly ILogger _logger;
    private readonly CompositeDisposable _disposable;
    private readonly CoreAudioAccessor _accessor;
    private readonly IReactiveProperty<AudioDeviceAccessor?> _communicationRoleDevice;
    private readonly IReactiveProperty<AudioDeviceAccessor?> _multimediaRoleDevice;

    public CoreAudioRepository(ILogger logger)
    {
        _logger = logger;
        _disposable = new CompositeDisposable();
        _accessor = new CoreAudioAccessor(logger).AddTo(_disposable);
        _accessor.DeviceRoleChanged += OnDeviceRoleChanged;
        _accessor.DeviceDisposed += OnDeviceDisposed;

        _communicationRoleDevice = new ReactivePropertySlim<AudioDeviceAccessor?>(_accessor.GetDefaultDevice(
                DataFlowType.Render,
                RoleType.Communications)
            )
            .AddTo(_disposable);
        _multimediaRoleDevice = new ReactivePropertySlim<AudioDeviceAccessor?>(_accessor.GetDefaultDevice(
                DataFlowType.Render,
                RoleType.Multimedia)
            )
            .AddTo(_disposable);
    }

    public ReadOnlyObservableCollection<AudioDeviceAccessor> AudioDevices => _accessor.AudioDevices;
    public IReadOnlyReactiveProperty<AudioDeviceAccessor?> CommunicationRoleDevice => _communicationRoleDevice;
    public IReadOnlyReactiveProperty<AudioDeviceAccessor?> MultimediaRoleDevice => _multimediaRoleDevice;

    private void OnDeviceRoleChanged(object? sender, DeviceAccessorRoleHolderChangedEventArgs e)
    {
        if (e.Role == RoleType.Communications && e.NewState)
        {
            _communicationRoleDevice.Value = e.Device;
        }

        if (e.Role == RoleType.Multimedia && e.NewState)
        {
            _multimediaRoleDevice.Value = e.Device;
        }
    }

    private void OnDeviceDisposed(object? sender, AudioDeviceAccessorEventArgs e)
    {
        if (e.Device == CommunicationRoleDevice.Value)
        {
            _communicationRoleDevice.Value = null;
        }

        if (e.Device == MultimediaRoleDevice.Value)
        {
            _multimediaRoleDevice.Value = null;
        }
    }

    public void SetDefaultDevice(AudioDeviceAccessor accessor, DataFlowType dataFlowType, RoleType roleType)
    {
        var deviceId = accessor.DeviceId;
        if (accessor.IsDisposed || deviceId == null)
        {
            return;
        }

        AudioDeviceAccessor? currentDevice;
        switch (roleType)
        {
            case RoleType.Communications:
                currentDevice = _accessor.GetDefaultDevice(dataFlowType, RoleType.Communications);
                break;
            case RoleType.Multimedia:
                currentDevice = _accessor.GetDefaultDevice(dataFlowType, RoleType.Multimedia);
                break;
            default:
                return;
        }

        if (currentDevice?.DeviceId == accessor.DeviceId)
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