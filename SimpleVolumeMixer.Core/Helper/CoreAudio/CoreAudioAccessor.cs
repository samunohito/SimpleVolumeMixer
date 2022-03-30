using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using CSCore.CoreAudioAPI;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SimpleVolumeMixer.Core.Helper.Utils;
using SimpleVolumeMixer.Core.Models.Domain.CoreAudio;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio;

public class CoreAudioAccessor : IDisposable
{
    private readonly CompositeDisposable _disposable;
    private readonly MMDeviceEnumerator _deviceEnumerator;
    private readonly MMNotificationClient _notificationClient;
    private readonly ReactiveCollection<AudioDeviceAccessor> _devices;

    public CoreAudioAccessor()
    {
        _disposable = new CompositeDisposable();
        _deviceEnumerator = new MMDeviceEnumerator().AddTo(_disposable);

        _notificationClient = new MMNotificationClient(_deviceEnumerator).AddTo(_disposable);
        _notificationClient.DeviceAdded += NotificationClientOnDeviceAdded;
        _notificationClient.DeviceRemoved += NotificationClientOnDeviceRemoved;
        _notificationClient.DevicePropertyChanged += NotificationClientOnDevicePropertyChanged;
        _notificationClient.DeviceStateChanged += NotificationClientOnDeviceStateChanged;
        _notificationClient.DefaultDeviceChanged += NotificationClientOnDefaultDeviceChanged;

        _devices = new ReactiveCollection<AudioDeviceAccessor>().AddTo(_disposable);
        AudioDevices = _devices.ToReadOnlyReactiveCollection().AddTo(_disposable);
    }

    public ReadOnlyReactiveCollection<AudioDeviceAccessor> AudioDevices { get; }

    private void NotificationClientOnDevicePropertyChanged(object sender, DevicePropertyChangedEventArgs e)
    {
        Debug.WriteLine("NotificationClientOnDevicePropertyChanged " + e);
    }

    private void NotificationClientOnDeviceRemoved(object sender, DeviceNotificationEventArgs e)
    {
        Debug.WriteLine("NotificationClientOnDeviceRemoved " + e);
        _devices.FirstOrDefault(x => x.DeviceId == e.DeviceId).IfPresent(target => DisposeDevice(target));
    }

    private void NotificationClientOnDeviceAdded(object sender, DeviceNotificationEventArgs e)
    {
        Debug.WriteLine("NotificationClientOnDeviceAdded " + e);
        if (_devices.All(x => x.DeviceId != e.DeviceId) && e.TryGetDevice(out var newDevice))
        {
            AppendDevice(newDevice);
        }
    }

    private void NotificationClientOnDefaultDeviceChanged(object sender, DefaultDeviceChangedEventArgs e)
    {
        Debug.WriteLine("NotificationClientOnDefaultDeviceChanged " + e);

        var newRoleType = AccessorHelper.Roles[e.Role];
        var dataFlowType = AccessorHelper.DataFlows[e.DataFlow];
        DeviceRoleSync(e.DeviceId, dataFlowType, newRoleType);
    }

    private void DeviceRoleSync(string deviceId, DataFlowType dataFlowType, RoleType roleType)
    {
        // Communications/Multimediaはシステム上で1つだけなので、すでに存在するものはフラグをOFFにする
        switch (roleType)
        {
            case RoleType.Communications:
                _devices.FirstOrDefault(x => x.Role.Communications).IfPresent(oldCommunicationsDevice =>
                {
                    oldCommunicationsDevice.Role.Communications = false;
                });
                break;
            case RoleType.Multimedia:
                _devices.FirstOrDefault(x => x.Role.Multimedia).IfPresent(oldMultimediaDevice =>
                {
                    oldMultimediaDevice.Role.Multimedia = false;
                });
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

    private void NotificationClientOnDeviceStateChanged(object sender, DeviceStateChangedEventArgs e)
    {
        Debug.WriteLine("NotificationClientOnDeviceStateChanged " + e);
        // TODO:イベント発行
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
        foreach (var audioDevice in AudioDevices.ToList())
        {
            DisposeDevice(audioDevice);
        }

        _devices.Clear();
    }

    private void AppendDevice(MMDevice device)
    {
        var accessor = new AudioDeviceAccessor(device);
        _devices.Add(accessor);
    }

    private void DisposeDevice(AudioDeviceAccessor accessor)
    {
        _devices.Remove(accessor);
        accessor.Dispose();
    }

    public void Dispose()
    {
        _notificationClient.DeviceAdded -= NotificationClientOnDeviceAdded;
        _notificationClient.DeviceRemoved -= NotificationClientOnDeviceRemoved;
        _notificationClient.DevicePropertyChanged -= NotificationClientOnDevicePropertyChanged;
        _notificationClient.DeviceStateChanged -= NotificationClientOnDeviceStateChanged;
        _notificationClient.DefaultDeviceChanged -= NotificationClientOnDefaultDeviceChanged;

        _disposable.Dispose();
    }
}