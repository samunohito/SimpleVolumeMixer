using System;
using System.Diagnostics;
using CSCore.CoreAudioAPI;
using Microsoft.Extensions.Logging;
using Reactive.Bindings.Extensions;
using SimpleVolumeMixer.Core.Helper.Component;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Event;
using SimpleVolumeMixer.Core.Helper.CoreAudio.EventAdapter;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Internal;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Types;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio;

public class AudioSessionAccessor : SafetyAccessorComponent
{
    public event EventHandler<AudioSessionAccessorDisplayNameChangedEventArgs>? DisplayNameChanged;
    public event EventHandler<AudioSessionAccessorIconPathChangedEventArgs>? IconPathChanged;
    public event EventHandler<AudioSessionAccessorSimpleVolumeChangedEventArgs>? SimpleVolumeChanged;
    public event EventHandler<AudioSessionAccessorChannelVolumeChangedEventArgs>? ChannelVolumeChanged;
    public event EventHandler<AudioSessionAccessorGroupingParamChangedEventArgs>? GroupingParamChanged;
    public event EventHandler<AudioSessionAccessorStateChangedEventArgs>? StateChanged;
    public event EventHandler<AudioSessionAccessorDisconnectedEventArgs>? SessionDisconnected;

    private readonly ILogger _logger;
    private readonly AudioSessionControl _session;
    private readonly AudioSessionControl2 _sessionControl2;
    private readonly AudioMeterInformation _meterInformation;
    private readonly SimpleAudioVolume _audioVolume;
    private readonly AudioSessionEventAdapter _eventAdapter;

    internal AudioSessionAccessor(AudioSessionControl audioSessionControl, ILogger logger)
    {
        _logger = logger;
        _session = audioSessionControl.AddTo(Disposable);

        _meterInformation = _session.QueryInterface<AudioMeterInformation>().AddTo(Disposable);
        _audioVolume = _session.QueryInterface<SimpleAudioVolume>().AddTo(Disposable);

        // AudioSessionControlと同一ポインタのようなのでDisposableへの追加は不要。
        // 逆に追加すると多重に開放してしまいハングアップする
        _sessionControl2 = _session.QueryInterface<AudioSessionControl2>();

        _eventAdapter = new AudioSessionEventAdapter(_session, logger).AddTo(Disposable);
        _eventAdapter.DisplayNameChanged += OnDisplayNameChanged;
        _eventAdapter.IconPathChanged += OnIconPathChanged;
        _eventAdapter.SimpleVolumeChanged += OnSimpleVolumeChanged;
        _eventAdapter.ChannelVolumeChanged += OnChannelVolumeChanged;
        _eventAdapter.GroupingParamChanged += OnGroupingParamChanged;
        _eventAdapter.StateChanged += OnStateChanged;
        _eventAdapter.SessionDisconnected += OnSessionDisconnected;
    }

    public Process? Process => SafeRead(() => _sessionControl2.Process, null);

    public bool IsSystemSoundSession => SafeRead(() => _sessionControl2.IsSystemSoundSession, false);

    public AudioSessionStateType SessionState => SafeRead(
        () => AccessorHelper.SessionStates[_session.SessionState],
        AudioSessionStateType.Unknown);

    public float PeakValue => SafeRead(_meterInformation.GetPeakValue, 0.0f);
    public int MeteringChannelCount => SafeRead(_meterInformation.GetMeteringChannelCount, 0);

    public string? DisplayName
    {
        get => SafeRead(() => _session.DisplayName, null);
        set => SafeWrite(v => _session.DisplayName = v, value);
    }

    public string? IconPath
    {
        get => SafeRead(() => _session.IconPath, null);
        set => SafeWrite(v => _session.IconPath = v, value);
    }

    public Guid GroupingParam
    {
        get => SafeRead(() => _session.GroupingParam, Guid.Empty);
        set => SafeWrite(v => _session.GroupingParam = v, value);
    }

    public float MasterVolume
    {
        get => SafeRead(() => _audioVolume.MasterVolume, 0.0f);
        set => SafeWrite(v => _audioVolume.MasterVolume = v, value);
    }

    public bool IsMuted
    {
        get => SafeRead(() => _audioVolume.IsMuted, false);
        set => SafeWrite(v => _audioVolume.IsMuted = v, value);
    }

    private void OnDisplayNameChanged(object? sender, AudioSessionDisplayNameChangedEventArgs e)
    {
        DisplayNameChanged?.Invoke(
            this,
            new AudioSessionAccessorDisplayNameChangedEventArgs(this, e.NewDisplayName)
        );
    }

    private void OnIconPathChanged(object? sender, AudioSessionIconPathChangedEventArgs e)
    {
        IconPathChanged?.Invoke(
            this,
            new AudioSessionAccessorIconPathChangedEventArgs(this, e.NewIconPath)
        );
    }

    private void OnSimpleVolumeChanged(object? sender, AudioSessionSimpleVolumeChangedEventArgs e)
    {
        SimpleVolumeChanged?.Invoke(
            this,
            new AudioSessionAccessorSimpleVolumeChangedEventArgs(this, e.NewVolume, e.IsMuted)
        );
    }

    private void OnChannelVolumeChanged(object? sender, AudioSessionChannelVolumeChangedEventArgs e)
    {
        ChannelVolumeChanged?.Invoke(
            this,
            new AudioSessionAccessorChannelVolumeChangedEventArgs(
                this,
                e.ChangedChannel,
                e.ChannelVolumes,
                e.ChangedChannel)
        );
    }

    private void OnGroupingParamChanged(object? sender, AudioSessionGroupingParamChangedEventArgs e)
    {
        GroupingParamChanged?.Invoke(
            this,
            new AudioSessionAccessorGroupingParamChangedEventArgs(
                this,
                e.NewGroupingParam)
        );
    }

    private void OnStateChanged(object? sender, AudioSessionStateChangedEventArgs e)
    {
        StateChanged?.Invoke(
            this,
            new AudioSessionAccessorStateChangedEventArgs(
                this,
                AccessorHelper.SessionStates[e.NewState])
        );
    }

    private void OnSessionDisconnected(object? sender, AudioSessionDisconnectedEventArgs e)
    {
        SessionDisconnected?.Invoke(
            this,
            new AudioSessionAccessorDisconnectedEventArgs(
                this,
                AccessorHelper.SessionDisconnectReasons[e.DisconnectReason])
        );
    }

    protected override void OnDisposing()
    {
        _eventAdapter.DisplayNameChanged -= OnDisplayNameChanged;
        _eventAdapter.IconPathChanged -= OnIconPathChanged;
        _eventAdapter.SimpleVolumeChanged -= OnSimpleVolumeChanged;
        _eventAdapter.ChannelVolumeChanged -= OnChannelVolumeChanged;
        _eventAdapter.GroupingParamChanged -= OnGroupingParamChanged;
        _eventAdapter.StateChanged -= OnStateChanged;
        _eventAdapter.SessionDisconnected -= OnSessionDisconnected;

        base.OnDisposing();
    }
}