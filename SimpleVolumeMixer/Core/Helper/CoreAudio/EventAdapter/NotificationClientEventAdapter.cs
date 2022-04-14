using System;
using CSCore.CoreAudioAPI;
using Microsoft.Extensions.Logging;
using SimpleVolumeMixer.Core.Helper.Component;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio.EventAdapter;

/// <summary>
/// Notifications when an audio endpoint device is added or removed, when the state or properties of an endpoint device change,
/// or when there is a change in the default role assigned to an endpoint device.
/// Since we do not want to block CoreAudioAPI in the app-side processing,
/// notifications from CoreAudioAPI are stored once in <see cref="QueueProcessor{TP,TR}"/> and are notified to the app side asynchronously and sequentially.
/// </summary>
/// <remarks>
/// We use information from MSDN and CSCore functions, so please refer to their documentation as well.
/// The document text for some functions is taken from MSDN.
/// <see cref="MMNotificationClient"/>
/// https://docs.microsoft.com/ja-jp/windows/win32/api/mmdeviceapi/nn-mmdeviceapi-immnotificationclient
/// </remarks>
public class NotificationClientEventAdapter : DisposableComponent
{
    /// <summary>
    /// The OnDeviceAdded method indicates that a new audio endpoint device has been added.
    /// </summary>
    public event EventHandler<DeviceNotificationEventArgs>? DeviceAdded;

    /// <summary>
    /// The OnDeviceRemoved method indicates that an audio endpoint device has been removed.
    /// </summary>
    public event EventHandler<DeviceNotificationEventArgs>? DeviceRemoved;

    /// <summary>
    /// The OnPropertyValueChanged method indicates that the value of a property belonging to an audio endpoint device has changed.
    /// </summary>
    public event EventHandler<DevicePropertyChangedEventArgs>? DevicePropertyChanged;

    /// <summary>
    /// The OnDeviceStateChanged method indicates that the state of an audio endpoint device has changed.
    /// </summary>
    public event EventHandler<DeviceStateChangedEventArgs>? DeviceStateChanged;

    /// <summary>
    /// The OnDefaultDeviceChanged method notifies the client that the default audio endpoint device for a particular device role has changed.
    /// </summary>
    public event EventHandler<DefaultDeviceChangedEventArgs>? DefaultDeviceChanged;

    private readonly MMNotificationClient _client;
    private readonly ILogger _logger;
    private readonly QueueProcessor<object?, object?> _processor;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="client">Object to subscribe to events from CoreAudioAPI</param>
    /// <param name="logger">Used to log event details from CoreAudioAPI</param>
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
        _processor.Push(QueueProcessorItem.OfAction(action));
    }

    protected override void OnDisposing()
    {
        _logger.LogDebug("disposing...");

        _client.DeviceAdded -= OnDeviceAdded;
        _client.DeviceRemoved -= OnDeviceRemoved;
        _client.DevicePropertyChanged -= OnDevicePropertyChanged;
        _client.DeviceStateChanged -= OnDeviceStateChanged;
        _client.DefaultDeviceChanged -= OnDefaultDeviceChanged;

        _processor.Dispose();

        base.OnDisposing();
    }
}