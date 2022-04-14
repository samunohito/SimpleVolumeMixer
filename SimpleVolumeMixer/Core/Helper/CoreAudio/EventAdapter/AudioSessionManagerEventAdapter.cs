using System;
using CSCore.CoreAudioAPI;
using Microsoft.Extensions.Logging;
using SimpleVolumeMixer.Core.Helper.Component;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio.EventAdapter;

/// <summary>
/// Notify of audio session creation, system ducking, and system undocking events.
/// Since we do not want to block CoreAudioAPI in the app-side processing,
/// notifications from CoreAudioAPI are stored once in <see cref="QueueProcessor{TP,TR}"/> and are notified to the app side asynchronously and sequentially.
/// </summary>
/// <remarks>
/// We use information from MSDN and CSCore functions, so please refer to their documentation as well.
/// The document text for some functions is taken from MSDN.
/// <see cref="AudioSessionNotification"/>
/// <see cref="AudioVolumeDuckNotification"/>
/// https://docs.microsoft.com/ja-jp/windows/win32/api/audiopolicy/nn-audiopolicy-iaudiosessionnotification?redirectedfrom=MSDN
/// https://docs.microsoft.com/en-us/windows/win32/api/audiopolicy/nn-audiopolicy-iaudiovolumeducknotification
/// </remarks>
public class AudioSessionManagerEventAdapter : DisposableComponent
{
    /// <summary>
    /// The OnSessionCreated method notifies the registered processes that the audio session has been created.
    /// </summary>
    public event EventHandler<SessionCreatedEventArgs>? SessionCreated;

    /// <summary>
    /// The OnVolumeDuckNotification method sends a notification about a pending system ducking event.
    /// </summary>
    public event EventHandler<VolumeDuckNotificationEventArgs>? VolumeDuckNotification;

    /// <summary>
    /// The OnVolumeUnduckNotification method sends a notification about a pending system unducking event.
    /// </summary>
    public event EventHandler<VolumeDuckNotificationEventArgs>? VolumeUnDuckNotification;

    private readonly AudioSessionManager2 _sessionManager;
    private readonly ILogger _logger;
    private readonly QueueProcessor<object?, object?> _processor;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="sessionManager">Object to subscribe to events from CoreAudioAPI</param>
    /// <param name="logger">Used to log event details from CoreAudioAPI</param>
    public AudioSessionManagerEventAdapter(AudioSessionManager2 sessionManager, ILogger logger)
    {
        _sessionManager = sessionManager;
        _sessionManager.SessionCreated += OnSessionCreated;
        _sessionManager.VolumeDuckNotification += OnVolumeDuckNotification;
        _sessionManager.VolumeUnDuckNotification += OnVolumeUnDuckNotification;
        _logger = logger;
        _processor = new QueueProcessor<object?, object?>();
        _processor.StartRequest();
    }

    private void OnSessionCreated(object? sender, SessionCreatedEventArgs e)
    {
        _logger.LogDebug(
            "sender:{sender}, " +
            "args:[ {session} ]",
            sender,
            e.NewSession
        );


        Push(() => SessionCreated?.Invoke(this, e));
    }

    private void OnVolumeDuckNotification(object? sender, VolumeDuckNotificationEventArgs e)
    {
        _logger.LogDebug(
            "sender:{sender}, " +
            "args:[ countCommunicationSessions:{countCommunicationSessions}, sessionId:{sessionId} ]",
            sender,
            e.CountCommunicationSessions,
            e.SessionID
        );

        Push(() => VolumeDuckNotification?.Invoke(this, e));
    }

    private void OnVolumeUnDuckNotification(object? sender, VolumeDuckNotificationEventArgs e)
    {
        _logger.LogDebug(
            "sender:{sender}, " +
            "args:[ countCommunicationSessions:{countCommunicationSessions}, sessionId:{sessionId} ]",
            sender,
            e.CountCommunicationSessions,
            e.SessionID
        );

        Push(() => VolumeUnDuckNotification?.Invoke(this, e));
    }

    private void Push(Action action)
    {
        _processor.Push(QueueProcessorItem.OfAction(action));
    }

    protected override void OnDisposing()
    {
        _logger.LogDebug("disposing...");

        _sessionManager.SessionCreated -= OnSessionCreated;
        _sessionManager.VolumeDuckNotification -= OnVolumeDuckNotification;
        _sessionManager.VolumeUnDuckNotification -= OnVolumeUnDuckNotification;
        _processor.StopRequest();

        base.OnDisposing();
    }
}