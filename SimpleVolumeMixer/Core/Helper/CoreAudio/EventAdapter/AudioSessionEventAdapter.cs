using System;
using CSCore.CoreAudioAPI;
using Microsoft.Extensions.Logging;
using SimpleVolumeMixer.Core.Helper.Component;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio.EventAdapter;

public class AudioSessionEventAdapter : DisposableComponent
{
    public event EventHandler<AudioSessionDisplayNameChangedEventArgs>? DisplayNameChanged;
    public event EventHandler<AudioSessionIconPathChangedEventArgs>? IconPathChanged;
    public event EventHandler<AudioSessionSimpleVolumeChangedEventArgs>? SimpleVolumeChanged;
    public event EventHandler<AudioSessionChannelVolumeChangedEventArgs>? ChannelVolumeChanged;
    public event EventHandler<AudioSessionGroupingParamChangedEventArgs>? GroupingParamChanged;
    public event EventHandler<AudioSessionStateChangedEventArgs>? StateChanged;
    public event EventHandler<AudioSessionDisconnectedEventArgs>? SessionDisconnected;

    private readonly AudioSessionControl _session;
    private readonly ILogger _logger;
    private readonly QueueProcessor<object?, object?> _processor;

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
        _processor = new QueueProcessor<object?, object?>(int.MaxValue);
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
        _processor.Push(QueueProcessorHandle.OfAction(action));
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