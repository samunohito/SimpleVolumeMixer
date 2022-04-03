using System;
using System.Diagnostics;
using System.Linq;
using CSCore.CoreAudioAPI;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SimpleVolumeMixer.Core.Helper.Component;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Event;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Internal;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Types;
using SimpleVolumeMixer.Core.Helper.Utils;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio;

public class CoreAudioAccessor : SafetyAccessorComponent
{
    public event EventHandler<DeviceDisposeEventArgs>? DeviceDisposed;
    public event EventHandler<DeviceRoleHolderChangedEventArgs>? DeviceRoleChanged;

    private readonly ReactiveCollection<AudioDeviceAccessor> _devices;
    private readonly MMDeviceEnumerator _deviceEnumerator;
    private readonly MMNotificationClient _notificationClient;

    public CoreAudioAccessor()
    {
        _deviceEnumerator = new MMDeviceEnumerator().AddTo(Disposable);

        _notificationClient = new MMNotificationClient(_deviceEnumerator).AddTo(Disposable);
        _notificationClient.DeviceAdded += NotificationClientOnDeviceAdded;
        _notificationClient.DeviceRemoved += NotificationClientOnDeviceRemoved;
        _notificationClient.DevicePropertyChanged += NotificationClientOnDevicePropertyChanged;
        _notificationClient.DeviceStateChanged += NotificationClientOnDeviceStateChanged;
        _notificationClient.DefaultDeviceChanged += NotificationClientOnDefaultDeviceChanged;

        _devices = new ReactiveCollection<AudioDeviceAccessor>().AddTo(Disposable);
        AudioDevices = _devices.ToReadOnlyReactiveCollection().AddTo(Disposable);
    }

    public ReadOnlyReactiveCollection<AudioDeviceAccessor> AudioDevices { get; }
    
    private void NotificationClientOnDevicePropertyChanged(object? sender, DevicePropertyChangedEventArgs e)
    {
        Debug.WriteLine("NotificationClientOnDevicePropertyChanged " + e);
    }

    private void NotificationClientOnDeviceRemoved(object? sender, DeviceNotificationEventArgs e)
    {
        Debug.WriteLine("NotificationClientOnDeviceRemoved " + e);
        _devices.FirstOrDefault(x => x.DeviceId == e.DeviceId).IfPresent(target => DisposeDevice(target));
    }

    private void NotificationClientOnDeviceAdded(object? sender, DeviceNotificationEventArgs e)
    {
        Debug.WriteLine("NotificationClientOnDeviceAdded " + e);
        if (_devices.All(x => x.DeviceId != e.DeviceId) && e.TryGetDevice(out var newDevice))
        {
            AppendDevice(newDevice);
        }
    }

    private void NotificationClientOnDefaultDeviceChanged(object? sender, DefaultDeviceChangedEventArgs e)
    {
        Debug.WriteLine("NotificationClientOnDefaultDeviceChanged " + e);

        var newRoleType = AccessorHelper.Roles[e.Role];
        var dataFlowType = AccessorHelper.DataFlows[e.DataFlow];
        DeviceRoleSync(e.DeviceId, dataFlowType, newRoleType);
    }
    
    private void NotificationClientOnDeviceStateChanged(object? sender, DeviceStateChangedEventArgs e)
    {
        Debug.WriteLine("NotificationClientOnDeviceStateChanged " + e);
    }

    private void DeviceRoleSync(string deviceId, DataFlowType dataFlowType, RoleType roleType)
    {
        // Communications/Multimediaはシステム上で1つだけなので、すでに存在するものはフラグをOFFにする
        switch (roleType)
        {
            case RoleType.Communications:
                _devices.ToList().ForEach(x => x.Role.Communications = false);
                break;
            case RoleType.Multimedia:
                _devices.ToList().ForEach(x => x.Role.Multimedia = false);
                break;
        }

        // 今回あたらしくCommunications/Multimediaに設定されたものにフラグを立てる.
        // 1つのデバイスが複数のRoleを持ったとしても、持っているRoleの数だけ通知してくるため、都度確認して対応するRoleのフラグを管理する必要がある
        var target = _devices.FirstOrDefault(x => x.DeviceId == deviceId && x.DataFlow == dataFlowType);
        if (target != null)
        {
            switch (roleType)
            {
                case RoleType.Communications:
                    target.Role.Communications = true;
                    break;
                case RoleType.Multimedia:
                    target.Role.Multimedia = true;
                    break;
            }
        }
    }
    
    public void RefreshDevices()
    {
        DisposeDevices();

        using var devices = _deviceEnumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active);
        foreach (var device in devices)
        {
            AppendDevice(device);
        }

        if (_devices.Any(x => x.DataFlow == DataFlowType.Render))
        {
            // deviceが1つもない状態でGetDefaultAudioEndpointを呼ぶとエラー落ちする
            using var mulDevice = _deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            using var comDevice = _deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Communications);
            DeviceRoleSync(mulDevice.DeviceID, DataFlowType.Render, RoleType.Multimedia);
            DeviceRoleSync(comDevice.DeviceID, DataFlowType.Render, RoleType.Communications);
        }

        if (_devices.Any(x => x.DataFlow == DataFlowType.Capture))
        {
            // deviceが1つもない状態でGetDefaultAudioEndpointを呼ぶとエラー落ちする
            using var mulDevice = _deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Multimedia);
            using var comDevice = _deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications);
            DeviceRoleSync(mulDevice.DeviceID, DataFlowType.Capture, RoleType.Multimedia);
            DeviceRoleSync(comDevice.DeviceID, DataFlowType.Capture, RoleType.Communications);
        }
    }

    private void DisposeDevices()
    {
        foreach (var audioDevice in _devices.ToList())
        {
            DisposeDevice(audioDevice);
        }

        _devices.Clear();
    }

    private void AppendDevice(MMDevice device)
    {
        var accessor = new AudioDeviceAccessor(device);
        accessor.RoleChanged += OnDeviceRoleChanged;
        _devices.Add(accessor);
    }

    private void DisposeDevice(AudioDeviceAccessor accessor)
    {
        _devices.Remove(accessor);
        accessor.RoleChanged -= OnDeviceRoleChanged;
        accessor.Dispose();

        DeviceDisposed?.Invoke(this, new DeviceDisposeEventArgs(accessor));
    }

    private void OnDeviceRoleChanged(object? sender, DeviceRoleHolderChangedEventArgs e)
    {
        DeviceRoleChanged?.Invoke(this, e);
    }
    
    protected override void OnDisposing()
    {
        base.OnDisposing();
        
        _notificationClient.DeviceAdded -= NotificationClientOnDeviceAdded;
        _notificationClient.DeviceRemoved -= NotificationClientOnDeviceRemoved;
        _notificationClient.DevicePropertyChanged -= NotificationClientOnDevicePropertyChanged;
        _notificationClient.DeviceStateChanged -= NotificationClientOnDeviceStateChanged;
        _notificationClient.DefaultDeviceChanged -= NotificationClientOnDefaultDeviceChanged;
        
        DisposeDevices();
    }
}