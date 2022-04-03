using System;
using System.Diagnostics;
using CSCore.CoreAudioAPI;
using Reactive.Bindings.Extensions;
using SimpleVolumeMixer.Core.Helper.Component;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Internal;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Types;
using AudioSessionStateChangedEventArgs =
    SimpleVolumeMixer.Core.Helper.CoreAudio.Event.AudioSessionStateChangedEventArgs;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio;

public class AudioSessionAccessor : SafetyAccessorComponent
{
    public event EventHandler<AudioSessionStateChangedEventArgs>? StateChanged;

    private readonly AudioSessionControl _session;
    private readonly AudioSessionControl2 _sessionControl2;
    private readonly AudioMeterInformation _meterInformation;
    private readonly SimpleAudioVolume _audioVolume;

    internal AudioSessionAccessor(AudioSessionControl audioSessionControl)
    {
        _session = audioSessionControl.AddTo(Disposable);
        _session.DisplayNameChanged += SessionOnDisplayNameChanged;
        _session.IconPathChanged += SessionOnIconPathChanged;
        _session.SimpleVolumeChanged += SessionOnSimpleVolumeChanged;
        _session.ChannelVolumeChanged += SessionOnChannelVolumeChanged;
        _session.GroupingParamChanged += SessionOnGroupingParamChanged;
        _session.StateChanged += SessionOnStateChanged;
        _session.SessionDisconnected += SessionOnSessionDisconnected;

        _meterInformation = _session.QueryInterface<AudioMeterInformation>().AddTo(Disposable);
        _audioVolume = _session.QueryInterface<SimpleAudioVolume>().AddTo(Disposable);
        _sessionControl2 = _session.QueryInterface<AudioSessionControl2>().AddTo(Disposable);
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

    private void SessionOnDisplayNameChanged(object? sender, AudioSessionDisplayNameChangedEventArgs e)
    {
        Debug.WriteLine("SessionOnDisplayNameChanged " + e);
    }

    private void SessionOnIconPathChanged(object? sender, AudioSessionIconPathChangedEventArgs e)
    {
        Debug.WriteLine("SessionOnIconPathChanged " + e);
    }

    private void SessionOnSimpleVolumeChanged(object? sender, AudioSessionSimpleVolumeChangedEventArgs e)
    {
        Debug.WriteLine("SessionOnSimpleVolumeChanged " + e);
    }

    private void SessionOnChannelVolumeChanged(object? sender, AudioSessionChannelVolumeChangedEventArgs e)
    {
        Debug.WriteLine("SessionOnChannelVolumeChanged " + e);
    }

    private void SessionOnGroupingParamChanged(object? sender, AudioSessionGroupingParamChangedEventArgs e)
    {
        Debug.WriteLine("SessionOnGroupingParamChanged " + e);
    }

    private void SessionOnStateChanged(object? sender, CSCore.CoreAudioAPI.AudioSessionStateChangedEventArgs e)
    {
        Debug.WriteLine("SessionOnStateChanged " + e);
        StateChanged?.Invoke(this, new AudioSessionStateChangedEventArgs(AccessorHelper.SessionStates[e.NewState]));
    }

    private void SessionOnSessionDisconnected(object? sender, AudioSessionDisconnectedEventArgs e)
    {
        Debug.WriteLine("SessionOnSessionDisconnected " + e);
    }

    protected override void OnDisposing()
    {
        _session.DisplayNameChanged -= SessionOnDisplayNameChanged;
        _session.IconPathChanged -= SessionOnIconPathChanged;
        _session.SimpleVolumeChanged -= SessionOnSimpleVolumeChanged;
        _session.ChannelVolumeChanged -= SessionOnChannelVolumeChanged;
        _session.GroupingParamChanged -= SessionOnGroupingParamChanged;
        _session.StateChanged -= SessionOnStateChanged;
        _session.SessionDisconnected -= SessionOnSessionDisconnected;
    }
}