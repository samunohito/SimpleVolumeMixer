﻿using System;
using CSCore.CoreAudioAPI;
using Microsoft.Extensions.Logging;
using SimpleVolumeMixer.Core.Helper.Component;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio.EventAdapter;

/// <summary>
/// Notification of session-related events such as volume level changes, display names, session status, etc.
/// Since we do not want to block CoreAudioAPI in the app-side processing,
/// notifications from CoreAudioAPI are stored once in <see cref="QueueProcessor{TP,TR}"/> and are notified to the app side asynchronously and sequentially.
/// </summary>
/// <remarks>
/// We use information from MSDN and CSCore functions, so please refer to their documentation as well.
/// The document text for some functions is taken from MSDN.
/// <see cref="AudioSessionEvents"/>
/// https://docs.microsoft.com/ja-jp/windows/win32/api/audiopolicy/nn-audiopolicy-iaudiosessionevents?redirectedfrom=MSDN
/// </remarks>
public class AudioSessionEventAdapter : DisposableComponent
{
    /// <summary>
    /// The OnDisplayNameChanged method notifies the client that the display name for the session has changed.
    /// </summary>
    public event EventHandler<AudioSessionDisplayNameChangedEventArgs>? DisplayNameChanged;

    /// <summary>
    /// The OnIconPathChanged method notifies the client that the display icon for the session has changed.
    /// </summary>
    public event EventHandler<AudioSessionIconPathChangedEventArgs>? IconPathChanged;

    /// <summary>
    /// The OnSimpleVolumeChanged method notifies the client that the volume level or muting state of the audio session has changed.
    /// </summary>
    public event EventHandler<AudioSessionSimpleVolumeChangedEventArgs>? SimpleVolumeChanged;

    /// <summary>
    /// The OnChannelVolumeChanged method notifies the client that the volume level of an audio channel in the session submix has changed.
    /// </summary>
    public event EventHandler<AudioSessionChannelVolumeChangedEventArgs>? ChannelVolumeChanged;

    /// <summary>
    /// The OnGroupingParamChanged method notifies the client that the grouping parameter for the session has changed.
    /// </summary>
    public event EventHandler<AudioSessionGroupingParamChangedEventArgs>? GroupingParamChanged;

    /// <summary>
    /// The OnStateChanged method notifies the client that the stream-activity state of the session has changed.
    /// </summary>
    public event EventHandler<AudioSessionStateChangedEventArgs>? StateChanged;

    /// <summary>
    /// The OnSessionDisconnected method notifies the client that the audio session has been disconnected.
    /// </summary>
    public event EventHandler<AudioSessionDisconnectedEventArgs>? SessionDisconnected;

    private readonly AudioSessionControl _session;
    private readonly ILogger _logger;
    private readonly QueueProcessor<object?, object?> _processor;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="session">Object to subscribe to events from CoreAudioAPI</param>
    /// <param name="logger">Used to log event details from CoreAudioAPI</param>
    public AudioSessionEventAdapter(AudioSessionControl session, ILogger logger)
    {
        _session = session;
        _session.DisplayNameChanged += OnDisplayNameChanged;
        _session.IconPathChanged += OnIconPathChanged;
        _session.SimpleVolumeChanged += OnSimpleVolumeChanged;
        _session.ChannelVolumeChanged += OnChannelVolumeChanged;
        _session.GroupingParamChanged += OnGroupingParamChanged;
        _session.StateChanged += OnStateChanged;
        _session.SessionDisconnected += OnSessionDisconnected;
        _logger = logger;
        _processor = new QueueProcessor<object?, object?>();
        _processor.StartRequest();
    }

    private void OnDisplayNameChanged(object? sender, AudioSessionDisplayNameChangedEventArgs e)
    {
        _logger.LogDebug(
            "sender:{sender}, " +
            "args:[ ctx:{ctx}, newDisplayName:{newDisplayName} ]",
            sender,
            e.EventContext,
            e.NewDisplayName
        );

        Push(() => DisplayNameChanged?.Invoke(this, e));
    }

    private void OnIconPathChanged(object? sender, AudioSessionIconPathChangedEventArgs e)
    {
        _logger.LogDebug(
            "sender:{sender}, " +
            "args:[ ctx:{ctx}, newIconPath:{newIconPath} ]",
            sender,
            e.EventContext,
            e.NewIconPath
        );

        Push(() => IconPathChanged?.Invoke(this, e));
    }

    private void OnSimpleVolumeChanged(object? sender, AudioSessionSimpleVolumeChangedEventArgs e)
    {
        _logger.LogDebug(
            "sender:{sender}, " +
            "args:[ ctx:{ctx}, newVolume:{newIconPath}, isMuted:{isMuted} ]",
            sender,
            e.EventContext,
            e.NewVolume,
            e.IsMuted
        );

        Push(() => SimpleVolumeChanged?.Invoke(this, e));
    }

    private void OnChannelVolumeChanged(object? sender, AudioSessionChannelVolumeChangedEventArgs e)
    {
        _logger.LogDebug(
            "sender:{sender}, " +
            "args:[ ctx:{ctx}, changedChannel:{changedChannel}, channelCount:{channelCount}, channelVolumes:{channelVolumes} ]",
            sender,
            e.EventContext,
            e.ChangedChannel,
            e.ChannelCount,
            e.ChannelVolumes
        );

        Push(() => ChannelVolumeChanged?.Invoke(this, e));
    }

    private void OnGroupingParamChanged(object? sender, AudioSessionGroupingParamChangedEventArgs e)
    {
        _logger.LogDebug(
            "sender:{sender}, " +
            "args:[ ctx:{ctx}, newGroupingParam:{newGroupingParam} ]",
            sender,
            e.EventContext,
            e.NewGroupingParam
        );

        Push(() => GroupingParamChanged?.Invoke(this, e));
    }

    private void OnStateChanged(object? sender, AudioSessionStateChangedEventArgs e)
    {
        _logger.LogDebug(
            "sender:{sender}, " +
            "args:[ newState:{newState} ]",
            sender,
            e.NewState
        );

        Push(() => StateChanged?.Invoke(this, e));
    }

    private void OnSessionDisconnected(object? sender, AudioSessionDisconnectedEventArgs e)
    {
        _logger.LogDebug(
            "sender:{sender}, " +
            "args:[ disconnectReason:{disconnectReason} ]",
            sender,
            e.DisconnectReason
        );

        Push(() => SessionDisconnected?.Invoke(this, e));
    }

    private void Push(Action action)
    {
        _processor.Push(QueueProcessorItem.OfAction(action));
    }

    protected override void OnDisposing()
    {
        _logger.LogDebug("disposing...");

        _session.DisplayNameChanged -= OnDisplayNameChanged;
        _session.IconPathChanged -= OnIconPathChanged;
        _session.SimpleVolumeChanged -= OnSimpleVolumeChanged;
        _session.ChannelVolumeChanged -= OnChannelVolumeChanged;
        _session.GroupingParamChanged -= OnGroupingParamChanged;
        _session.StateChanged -= OnStateChanged;
        _session.SessionDisconnected -= OnSessionDisconnected;

        _processor.StopRequest();

        base.OnDisposing();
    }
}