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
    private readonly IReactiveProperty<AudioDeviceAccessor?> _communicationRoleRenderDevice;
    private readonly IReactiveProperty<AudioDeviceAccessor?> _multimediaRoleRenderDevice;
    private readonly IReactiveProperty<AudioDeviceAccessor?> _communicationRoleCaptureDevice;
    private readonly IReactiveProperty<AudioDeviceAccessor?> _multimediaRoleCaptureDevice;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="logger"></param>
    public CoreAudioRepository(ILogger logger)
    {
        _logger = logger;
        _disposable = new CompositeDisposable();
        _accessor = new CoreAudioAccessor(logger).AddTo(_disposable);
        _accessor.RenderDeviceRoleChanged += OnRenderDeviceRoleChanged;
        _accessor.RenderDeviceDisposed += OnRenderDeviceDisposed;
        _accessor.CaptureDeviceRoleChanged += OnCaptureDeviceRoleChanged;
        _accessor.CaptureDeviceDisposed += OnCaptureDeviceDisposed;

        _communicationRoleRenderDevice =
            new ReactivePropertySlim<AudioDeviceAccessor?>(_accessor.GetDefaultRenderDevice(RoleType.Communications))
                .AddTo(_disposable);
        _multimediaRoleRenderDevice =
            new ReactivePropertySlim<AudioDeviceAccessor?>(_accessor.GetDefaultRenderDevice(RoleType.Multimedia))
                .AddTo(_disposable);
        _communicationRoleCaptureDevice =
            new ReactivePropertySlim<AudioDeviceAccessor?>(_accessor.GetDefaultCaptureDevice(RoleType.Communications))
                .AddTo(_disposable);
        _multimediaRoleCaptureDevice =
            new ReactivePropertySlim<AudioDeviceAccessor?>(_accessor.GetDefaultCaptureDevice(RoleType.Multimedia))
                .AddTo(_disposable);
    }

    public ReadOnlyObservableCollection<AudioDeviceAccessor> RenderAudioDevices => _accessor.RenderAudioDevices;
    
    public ReadOnlyObservableCollection<AudioDeviceAccessor> CaptureAudioDevices => _accessor.CaptureAudioDevices;

    public IReadOnlyReactiveProperty<AudioDeviceAccessor?> CommunicationRoleRenderDevice =>
        _communicationRoleRenderDevice;

    public IReadOnlyReactiveProperty<AudioDeviceAccessor?> MultimediaRoleRenderDevice => 
        _multimediaRoleRenderDevice;

    public IReadOnlyReactiveProperty<AudioDeviceAccessor?> CommunicationRoleCaptureDevice =>
        _communicationRoleCaptureDevice;

    public IReadOnlyReactiveProperty<AudioDeviceAccessor?> MultimediaRoleCaptureDevice => 
        _multimediaRoleCaptureDevice;

    private void OnRenderDeviceRoleChanged(object? sender, DeviceAccessorRoleHolderChangedEventArgs e)
    {
        if (e.Role == RoleType.Communications && e.NewState)
        {
            _communicationRoleRenderDevice.Value = e.Device;
        }

        if (e.Role == RoleType.Multimedia && e.NewState)
        {
            _multimediaRoleRenderDevice.Value = e.Device;
        }
    }

    private void OnRenderDeviceDisposed(object? sender, AudioDeviceAccessorEventArgs e)
    {
        if (e.Device == _communicationRoleRenderDevice.Value)
        {
            _communicationRoleRenderDevice.Value = null;
        }

        if (e.Device == _multimediaRoleRenderDevice.Value)
        {
            _multimediaRoleRenderDevice.Value = null;
        }
    }

    private void OnCaptureDeviceRoleChanged(object? sender, DeviceAccessorRoleHolderChangedEventArgs e)
    {
        if (e.Role == RoleType.Communications && e.NewState)
        {
            _communicationRoleCaptureDevice.Value = e.Device;
        }

        if (e.Role == RoleType.Multimedia && e.NewState)
        {
            _multimediaRoleCaptureDevice.Value = e.Device;
        }
    }

    private void OnCaptureDeviceDisposed(object? sender, AudioDeviceAccessorEventArgs e)
    {
        if (e.Device == _communicationRoleCaptureDevice.Value)
        {
            _communicationRoleCaptureDevice.Value = null;
        }

        if (e.Device == _multimediaRoleCaptureDevice.Value)
        {
            _multimediaRoleCaptureDevice.Value = null;
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
                currentDevice = dataFlowType == DataFlowType.Render
                    ? _accessor.GetDefaultRenderDevice(RoleType.Communications)
                    : _accessor.GetDefaultCaptureDevice(RoleType.Communications);
                break;
            case RoleType.Multimedia:
                currentDevice = dataFlowType == DataFlowType.Render
                    ? _accessor.GetDefaultRenderDevice(RoleType.Multimedia)
                    : _accessor.GetDefaultCaptureDevice(RoleType.Multimedia);
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
        _accessor.RenderDeviceRoleChanged -= OnRenderDeviceRoleChanged;
        _accessor.CaptureDeviceRoleChanged -= OnCaptureDeviceRoleChanged;
        _accessor.RenderDeviceDisposed -= OnRenderDeviceDisposed;
        _accessor.CaptureDeviceDisposed -= OnCaptureDeviceDisposed;
        _disposable.Dispose();
    }
}