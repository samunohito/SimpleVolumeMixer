using System;
using System.Diagnostics;
using CSCore.CoreAudioAPI;
using DisposableComponents;
using Microsoft.Extensions.Logging;
using Reactive.Bindings.Extensions;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Event;
using SimpleVolumeMixer.Core.Helper.CoreAudio.EventAdapter;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Internal;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Types;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio;

/// <summary>
/// <see cref="AudioSessionControl"/>と及び周辺インターフェース・関連情報を総括するラッパーオブジェクト。
/// </summary>
/// <seealso cref="AudioSessionControl"/>
/// <seealso cref="AudioSessionControl2"/>
/// <seealso cref="AudioMeterInformation"/>
/// <seealso cref="SimpleAudioVolume"/>
/// <seealso cref="AudioSessionEventAdapter"/>
public class AudioSessionAccessor : SafetyAccessComponent
{
    /// <summary>
    /// セッションが持つ値に変化が生じた際に発生するイベントハンドラ。
    /// CoreAudioAPIから<see cref="AudioSessionEventAdapter.DisplayNameChanged"/>経由で通知があった際に発動する。
    /// </summary>
    public event EventHandler<AudioSessionAccessorDisplayNameChangedEventArgs>? DisplayNameChanged;

    /// <summary>
    /// セッションが持つ値に変化が生じた際に発生するイベントハンドラ。
    /// CoreAudioAPIから<see cref="AudioSessionEventAdapter.IconPathChanged"/>経由で通知があった際に発動する。
    /// </summary>
    public event EventHandler<AudioSessionAccessorIconPathChangedEventArgs>? IconPathChanged;

    /// <summary>
    /// セッションが持つ値に変化が生じた際に発生するイベントハンドラ。
    /// CoreAudioAPIから<see cref="AudioSessionEventAdapter.SimpleVolumeChanged"/>経由で通知があった際に発動する。
    /// </summary>
    public event EventHandler<AudioSessionAccessorSimpleVolumeChangedEventArgs>? SimpleVolumeChanged;

    /// <summary>
    /// セッションが持つ値に変化が生じた際に発生するイベントハンドラ。
    /// CoreAudioAPIから<see cref="AudioSessionEventAdapter.ChannelVolumeChanged"/>経由で通知があった際に発動する。
    /// </summary>
    public event EventHandler<AudioSessionAccessorChannelVolumeChangedEventArgs>? ChannelVolumeChanged;

    /// <summary>
    /// セッションが持つ値に変化が生じた際に発生するイベントハンドラ。
    /// CoreAudioAPIから<see cref="AudioSessionEventAdapter.GroupingParamChanged"/>経由で通知があった際に発動する。
    /// </summary>
    public event EventHandler<AudioSessionAccessorGroupingParamChangedEventArgs>? GroupingParamChanged;

    /// <summary>
    /// セッションが持つ値に変化が生じた際に発生するイベントハンドラ。
    /// CoreAudioAPIから<see cref="AudioSessionEventAdapter.StateChanged"/>経由で通知があった際に発動する。
    /// </summary>
    public event EventHandler<AudioSessionAccessorStateChangedEventArgs>? StateChanged;

    /// <summary>
    /// セッションが持つ値に変化が生じた際に発生するイベントハンドラ。
    /// CoreAudioAPIから<see cref="AudioSessionEventAdapter.SessionDisconnected"/>経由で通知があった際に発動する。
    /// </summary>
    public event EventHandler<AudioSessionAccessorDisconnectedEventArgs>? SessionDisconnected;

    private readonly ILogger _logger;
    private readonly AudioSessionControl _session;
    private readonly AudioSessionControl2 _sessionControl2;
    private readonly AudioMeterInformation _meterInformation;
    private readonly SimpleAudioVolume _audioVolume;
    private readonly AudioSessionEventAdapter _eventAdapter;

    public AudioSessionAccessor(AudioSessionControl audioSessionControl, ILogger logger)
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
        () => _session.GetStateNative(out var value) >= 0
            ? AccessorHelper.SessionStates[value]
            : AudioSessionStateType.Unknown,
        AudioSessionStateType.Unknown);

    public float PeakValue => SafeRead(
        () => _meterInformation.GetPeakValueNative(out var value) >= 0
            ? value
            : 0.0f,
        0.0f);

    public int MeteringChannelCount => SafeRead(
        () => _meterInformation.GetMeteringChannelCountNative(out var value) >= 0
            ? value
            : 0,
        0);

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