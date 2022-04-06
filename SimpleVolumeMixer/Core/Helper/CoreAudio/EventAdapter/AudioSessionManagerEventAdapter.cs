using System;
using CSCore.CoreAudioAPI;
using Microsoft.Extensions.Logging;
using SimpleVolumeMixer.Core.Helper.Component;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio.EventAdapter;

public class AudioSessionManagerEventAdapter : DisposableComponent
{
    public event EventHandler<SessionCreatedEventArgs>? SessionCreated;
    public event EventHandler<VolumeDuckNotificationEventArgs>? VolumeDuckNotification;
    public event EventHandler<VolumeDuckNotificationEventArgs>? VolumeUnDuckNotification;

    private readonly AudioSessionManager2 _sessionManager;
    private readonly ILogger _logger;
    private readonly QueueProcessor<object?, object?> _processor;

    public AudioSessionManagerEventAdapter(AudioSessionManager2 sessionManager, ILogger logger)
    {
        _sessionManager = sessionManager;
        _sessionManager.SessionCreated += OnSessionCreated;
        _sessionManager.VolumeDuckNotification += OnVolumeDuckNotification;
        _sessionManager.VolumeUnDuckNotification += OnVolumeUnDuckNotification;
        _logger = logger;
        _processor = new QueueProcessor<object?, object?>(int.MaxValue);
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
        _processor.Push(QueueProcessorHandle.OfAction(action));
    }

    protected override void OnDisposing()
    {
        _logger.LogDebug("disposing...");

        _sessionManager.SessionCreated -= OnSessionCreated;
        _sessionManager.VolumeDuckNotification -= OnVolumeDuckNotification;
        _sessionManager.VolumeUnDuckNotification -= OnVolumeUnDuckNotification;

        base.OnDisposing();
    }
}