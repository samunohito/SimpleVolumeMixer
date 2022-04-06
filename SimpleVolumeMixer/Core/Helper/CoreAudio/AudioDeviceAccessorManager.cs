using System;
using System.Linq;
using CSCore.CoreAudioAPI;
using Microsoft.Extensions.Logging;
using Reactive.Bindings.Extensions;
using SimpleVolumeMixer.Core.Helper.Component;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Event;
using SimpleVolumeMixer.Core.Helper.CoreAudio.EventAdapter;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Internal;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Types;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio;

public class AudioDeviceAccessorManager : SynchronizedReactiveCollectionWrapper<AudioDeviceAccessor>
{
    public event EventHandler<AudioDeviceAccessorEventArgs>? DeviceDisposing;
    public event EventHandler<AudioDeviceAccessorEventArgs>? DeviceDisposed;
    public event EventHandler<DeviceAccessorRoleHolderChangedEventArgs>? DeviceRoleChanged;

    private readonly ILogger _logger;
    private readonly MMDeviceEnumerator _deviceEnumerator;
    private readonly MMNotificationClient _notificationClient;
    private readonly NotificationClientEventAdapter _clientEventAdapter;

    public AudioDeviceAccessorManager(ILogger logger)
    {
        _logger = logger;
        _deviceEnumerator = new MMDeviceEnumerator().AddTo(Disposable);
        _notificationClient = new MMNotificationClient(_deviceEnumerator).AddTo(Disposable);
        _clientEventAdapter = new NotificationClientEventAdapter(_notificationClient, logger).AddTo(Disposable);

        _clientEventAdapter.DeviceAdded += OnDeviceAdded;
        _clientEventAdapter.DeviceRemoved += OnDeviceRemoved;
        _clientEventAdapter.DefaultDeviceChanged += OnDefaultDeviceChanged;
        _clientEventAdapter.DeviceStateChanged += OnDeviceStateChanged;
    }

    public bool Contains(string? deviceId, DataFlowType dataFlowType)
    {
        if (deviceId == null)
        {
            return false;
        }

        lock (Gate)
        {
            return this.Any(x => x.DeviceId == deviceId && x.DataFlow == dataFlowType);
        }
    }

    public bool Contains(MMDevice device)
    {
        return Contains(device.DeviceID, AccessorHelper.DataFlows[device.DataFlow]);
    }

    public AudioDeviceAccessor? GetDevice(string? deviceId, DataFlowType dataFlowType)
    {
        if (deviceId == null)
        {
            return null;
        }

        lock (Gate)
        {
            return this.FirstOrDefault(x => x.DeviceId == deviceId && x.DataFlow == dataFlowType);
        }
    }

    public AudioDeviceAccessor? GetDefaultDevice(DataFlowType dataFlowType, RoleType roleType)
    {
        lock (Gate)
        {
            if (Count <= 0 || dataFlowType == DataFlowType.Unknown || roleType == RoleType.Unknown)
            {
                return null;
            }

            using var defaultDevice = _deviceEnumerator.GetDefaultAudioEndpoint(
                AccessorHelper.DataFlowsRev[dataFlowType],
                AccessorHelper.RolesRev[roleType]);
            if (defaultDevice == null)
            {
                return null;
            }

            if (!Contains(defaultDevice))
            {
                throw new ApplicationException("unknown device : " + defaultDevice);
            }

            return GetDevice(defaultDevice.DeviceID, dataFlowType);
        }
    }

    public void Add(MMDevice device)
    {
        if (Contains(device))
        {
            return;
        }

        var ax = new AudioDeviceAccessor(device, _logger);
        ax.RoleChanged += OnDeviceRoleChanged;
        ax.Disposing += OnDeviceDisposing;
        ax.Disposed += OnDeviceDisposed;
        Add(ax);
    }

    public void Remove(string deviceId)
    {
        lock (Gate)
        {
            this.Where(x => x.DeviceId == deviceId)
                .ToList()
                .ForEach(x => Remove(x));
        }
    }

    public void Remove(MMDevice device)
    {
        Remove(device.DeviceID);
    }

    public void CollectAudioEndpoints()
    {
        lock (Gate)
        {
            using var devices = _deviceEnumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active);
            if (devices == null)
            {
                throw new ApplicationException("Failed to acquire DeviceCollection.");
            }

            foreach (var device in devices)
            {
                Add(device);
            }

            if (this.Any(x => x.DataFlow == DataFlowType.Render))
            {
                // deviceが1つもない状態でGetDefaultAudioEndpointを呼ぶとエラー落ちする
                using var mulDevice = _deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                using var comDevice = _deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Communications);
                DeviceRoleSync(mulDevice.DeviceID, DataFlowType.Render, RoleType.Multimedia);
                DeviceRoleSync(comDevice.DeviceID, DataFlowType.Render, RoleType.Communications);
            }

            if (this.Any(x => x.DataFlow == DataFlowType.Capture))
            {
                // deviceが1つもない状態でGetDefaultAudioEndpointを呼ぶとエラー落ちする
                using var mulDevice = _deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Multimedia);
                using var comDevice = _deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications);
                DeviceRoleSync(mulDevice.DeviceID, DataFlowType.Capture, RoleType.Multimedia);
                DeviceRoleSync(comDevice.DeviceID, DataFlowType.Capture, RoleType.Communications);
            }
        }
    }

    private void DeviceRoleSync(string deviceId, DataFlowType dataFlowType, RoleType roleType)
    {
        lock (Gate)
        {
            // Communications/Multimediaはシステム上で1つだけなので、すでに存在するものはフラグをOFFにする
            switch (roleType)
            {
                case RoleType.Communications:
                    foreach (var device in this)
                    {
                        device.Role.Communications = false;
                    }

                    break;
                case RoleType.Multimedia:
                    foreach (var device in this)
                    {
                        device.Role.Multimedia = false;
                    }

                    break;
            }

            // 今回あたらしくCommunications/Multimediaに設定されたものにフラグを立てる.
            // 1つのデバイスが複数のRoleを持ったとしても、持っているRoleの数だけ通知してくるため、都度確認して対応するRoleのフラグを管理する必要がある
            var target = GetDevice(deviceId, dataFlowType);
            if (target == null)
            {
                return;
            }

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

    private void OnDeviceAdded(object? sender, DeviceNotificationEventArgs e)
    {
        if (e.TryGetDevice(out var newDevice) && !Contains(newDevice))
        {
            Add(newDevice);
        }
    }

    private void OnDeviceRemoved(object? sender, DeviceNotificationEventArgs e)
    {
        Remove(e.DeviceId);
    }

    private void OnDefaultDeviceChanged(object? sender, DefaultDeviceChangedEventArgs e)
    {
        var newRoleType = AccessorHelper.Roles[e.Role];
        var dataFlowType = AccessorHelper.DataFlows[e.DataFlow];
        DeviceRoleSync(e.DeviceId, dataFlowType, newRoleType);
    }

    private void OnDeviceStateChanged(object? sender, DeviceStateChangedEventArgs e)
    {
        switch (e.DeviceState)
        {
            case DeviceState.Active:
                if (e.TryGetDevice(out var device))
                {
                    Add(device);
                }

                break;
            case DeviceState.Disabled:
            case DeviceState.NotPresent:
            case DeviceState.UnPlugged:
                Remove(e.DeviceId);
                break;
        }
    }

    private void OnDeviceRoleChanged(object? sender, DeviceAccessorRoleHolderChangedEventArgs e)
    {
        DeviceRoleChanged?.Invoke(this, e);
    }

    private void OnDeviceDisposing(object? sender, EventArgs e)
    {
        if (sender is AudioDeviceAccessor ax)
        {
            DeviceDisposing?.Invoke(this, new AudioDeviceAccessorEventArgs(ax));

            ax.RoleChanged -= OnDeviceRoleChanged;
            ax.Disposing -= OnDeviceDisposing;
            Remove(ax);
        }
    }

    private void OnDeviceDisposed(object? sender, EventArgs e)
    {
        if (sender is AudioDeviceAccessor ax)
        {
            ax.Disposed -= OnDeviceDisposed;

            DeviceDisposed?.Invoke(this, new AudioDeviceAccessorEventArgs(ax));
        }
    }

    protected override void OnDisposing()
    {
        _logger.LogDebug("disposing...");

        _clientEventAdapter.DeviceAdded -= OnDeviceAdded;
        _clientEventAdapter.DeviceRemoved -= OnDeviceRemoved;
        _clientEventAdapter.DefaultDeviceChanged -= OnDefaultDeviceChanged;
        _clientEventAdapter.DeviceStateChanged -= OnDeviceStateChanged;

        lock (Gate)
        {
            foreach (var device in this)
            {
                // AudioDeviceAccessorとこのクラスの結びつきを解除するのはOnDeviceDisposingで
                device.Dispose();
            }
        }

        base.OnDisposing();
    }
}