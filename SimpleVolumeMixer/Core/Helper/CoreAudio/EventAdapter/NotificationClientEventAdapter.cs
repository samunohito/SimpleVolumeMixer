using System;
using CSCore.CoreAudioAPI;
using Microsoft.Extensions.Logging;
using SimpleVolumeMixer.Core.Helper.Component;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio.EventAdapter;

public class NotificationClientEventAdapter : DisposableComponent
{
    public event EventHandler<DeviceNotificationEventArgs>? DeviceAdded;
    public event EventHandler<DeviceNotificationEventArgs>? DeviceRemoved;
    public event EventHandler<DevicePropertyChangedEventArgs>? DevicePropertyChanged;
    public event EventHandler<DeviceStateChangedEventArgs>? DeviceStateChanged;
    public event EventHandler<DefaultDeviceChangedEventArgs>? DefaultDeviceChanged;

    private readonly MMNotificationClient _client;
    private readonly ILogger _logger;
    private readonly QueueProcessor<object?, object?> _processor;

    public NotificationClientEventAdapter(MMNotificationClient client, ILogger logger)
    {
        _client = client;
        _client.DeviceAdded += OnDeviceAdded;
        _client.DeviceRemoved += OnDeviceRemoved;
        _client.DevicePropertyChanged += OnDevicePropertyChanged;
        _client.DeviceStateChanged += OnDeviceStateChanged;
        _client.DefaultDeviceChanged += OnDefaultDeviceChanged;
        _logger = logger;
        _processor = new QueueProcessor<object?, object?>(int.MaxValue);
        _processor.StartRequest();
    }

    private void OnDeviceAdded(object? sender, DeviceNotificationEventArgs e)
    {
        _logger.LogDebug(
            "sender:{sender}, " +
            "args:[ deviceId:{deviceId} ]",
            sender,
            e.DeviceId
        );

        Push(() => DeviceAdded?.Invoke(this, e));
    }

    private void OnDeviceRemoved(object? sender, DeviceNotificationEventArgs e)
    {
        _logger.LogDebug(
            "sender:{sender}, " +
            "args:[ deviceId:{deviceId} ]",
            sender,
            e.DeviceId
        );

        Push(() => DeviceRemoved?.Invoke(this, e));
    }

    private void OnDevicePropertyChanged(object? sender, DevicePropertyChangedEventArgs e)
    {
        _logger.LogDebug(
            "sender:{sender}, " +
            "args:[ deviceId:{deviceId}, propertyKey:{key} ]",
            sender,
            e.DeviceId,
            e.PropertyKey
        );

        Push(() => DevicePropertyChanged?.Invoke(this, e));
    }

    private void OnDeviceStateChanged(object? sender, DeviceStateChangedEventArgs e)
    {
        _logger.LogDebug(
            "sender:{sender}, " +
            "args:[ deviceId:{deviceId}, state:{state} ]",
            sender,
            e.DeviceId,
            e.DeviceState
        );

        Push(() => DeviceStateChanged?.Invoke(this, e));
    }

    private void OnDefaultDeviceChanged(object? sender, DefaultDeviceChangedEventArgs e)
    {
        _logger.LogDebug(
            "sender:{sender}, " +
            "args:[ deviceId:{deviceId}, dataFlow:{dataFlow}, role:{role} ]",
            sender,
            e.DeviceId,
            e.DataFlow,
            e.Role
        );

        Push(() => DefaultDeviceChanged?.Invoke(this, e));
    }

    private void Push(Action action)
    {
        _processor.Push(QueueProcessorHandle.OfAction(action));
    }

    protected override void OnDisposing()
    {
        _logger.LogDebug("disposing...");
        
        _client.DeviceAdded -= OnDeviceAdded;
        _client.DeviceRemoved -= OnDeviceRemoved;
        _client.DevicePropertyChanged -= OnDevicePropertyChanged;
        _client.DeviceStateChanged -= OnDeviceStateChanged;
        _client.DefaultDeviceChanged -= OnDefaultDeviceChanged;

        base.OnDisposing();
    }
}