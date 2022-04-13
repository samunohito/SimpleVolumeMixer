using System;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SimpleVolumeMixer.Core.Helper.Component;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Event;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Types;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio;

public class CoreAudioAccessor : SafetyAccessorComponent
{
    public event EventHandler<AudioDeviceAccessorEventArgs>? DeviceDisposing
    {
        add => _deviceManager.DeviceDisposing += value;
        remove => _deviceManager.DeviceDisposing -= value;
    }

    public event EventHandler<AudioDeviceAccessorEventArgs>? DeviceDisposed
    {
        add => _deviceManager.DeviceDisposed += value;
        remove => _deviceManager.DeviceDisposed -= value;
    }

    public event EventHandler<DeviceAccessorRoleHolderChangedEventArgs>? DeviceRoleChanged
    {
        add => _deviceManager.DeviceRoleChanged += value;
        remove => _deviceManager.DeviceRoleChanged -= value;
    }

    private readonly ILogger _logger;
    private readonly AudioDeviceAccessorManager _deviceManager;

    public CoreAudioAccessor(ILogger logger)
    {
        _logger = logger;
        _deviceManager = new AudioDeviceAccessorManager(logger).AddTo(Disposable);
        _deviceManager.CollectAudioEndpoints();
    }

    public ReadOnlyObservableCollection<AudioDeviceAccessor> AudioDevices => _deviceManager.ReadOnlyCollection;

    public AudioDeviceAccessor? GetDefaultDevice(DataFlowType dataFlowType, RoleType roleType)
    {
        return _deviceManager.GetDefaultDevice(dataFlowType, roleType);
    }

    protected override void OnDisposing()
    {
        _logger.LogDebug("disposing...");
        base.OnDisposing();
    }
}