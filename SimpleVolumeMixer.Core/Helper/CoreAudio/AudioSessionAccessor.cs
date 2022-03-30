using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Disposables;
using CSCore.CoreAudioAPI;
using Reactive.Bindings.Extensions;
using SimpleVolumeMixer.Core.Models.Domain.CoreAudio;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio;

public class AudioSessionAccessor : IDisposable
{
    public event EventHandler<AudioSessionStateChangedEventArgs>? StateChanged;
    public event EventHandler<EventArgs>? Disposed; 
    
    private readonly CompositeDisposable _disposable;
    private readonly AudioSessionControl _session;
    private readonly AudioMeterInformation _meterInformation;
    private readonly SimpleAudioVolume _audioVolume;
    private readonly AudioSessionControl2 _sessionControl2;
    internal AudioSessionAccessor(AudioSessionControl audioSessionControl)
    {
        _disposable = new CompositeDisposable();
        _session = audioSessionControl.AddTo(_disposable);
        _session.DisplayNameChanged += SessionOnDisplayNameChanged;
        _session.IconPathChanged += SessionOnIconPathChanged;
        _session.SimpleVolumeChanged += SessionOnSimpleVolumeChanged;
        _session.ChannelVolumeChanged += SessionOnChannelVolumeChanged;
        _session.GroupingParamChanged += SessionOnGroupingParamChanged;
        _session.StateChanged += SessionOnStateChanged;
        _session.SessionDisconnected += SessionOnSessionDisconnected;

        _meterInformation = _session.QueryInterface<AudioMeterInformation>().AddTo(_disposable);
        _audioVolume = _session.QueryInterface<SimpleAudioVolume>().AddTo(_disposable);
        _sessionControl2 = _session.QueryInterface<AudioSessionControl2>().AddTo(_disposable);
    }

    public Process Process => _sessionControl2.Process;
    public AudioSessionStateType SessionState => AccessorHelper.SessionStates[_session.SessionState];
    public float PeekValue => _meterInformation.PeakValue;
    public int MeteringChannelCount => _meterInformation.MeteringChannelCount;
    
    public string? DisplayName
    {
        get => _session.DisplayName;
        set => _session.DisplayName = value;
    }

    public string? IconPath
    {
        get => _session.IconPath;
        set => _session.IconPath = value;
    }

    public Guid GroupingParam
    {
        get => _session.GroupingParam;
        set => _session.GroupingParam = value;
    }

    public float MasterVolume
    {
        get => _audioVolume.MasterVolume;
        set => _audioVolume.MasterVolume = value;
    }

    public bool IsMuted
    {
        get => _audioVolume.IsMuted;
        set => _audioVolume.IsMuted = value;
    }

    public bool IsDisposed => _disposable.IsDisposed;

    private void SessionOnDisplayNameChanged(object sender, AudioSessionDisplayNameChangedEventArgs e)
    {
        Debug.WriteLine("SessionOnDisplayNameChanged");
    }

    private void SessionOnIconPathChanged(object sender, AudioSessionIconPathChangedEventArgs e)
    {
        Debug.WriteLine("SessionOnIconPathChanged");
    }

    private void SessionOnSimpleVolumeChanged(object sender, AudioSessionSimpleVolumeChangedEventArgs e)
    {
        Debug.WriteLine("SessionOnSimpleVolumeChanged");
    }

    private void SessionOnChannelVolumeChanged(object sender, AudioSessionChannelVolumeChangedEventArgs e)
    {
        Debug.WriteLine("SessionOnChannelVolumeChanged");
    }

    private void SessionOnGroupingParamChanged(object sender, AudioSessionGroupingParamChangedEventArgs e)
    {
        Debug.WriteLine("SessionOnGroupingParamChanged");
    }

    private void SessionOnStateChanged(object sender, CSCore.CoreAudioAPI.AudioSessionStateChangedEventArgs e)
    {
        Debug.WriteLine("SessionOnStateChanged");
        StateChanged?.Invoke(this, new AudioSessionStateChangedEventArgs(AccessorHelper.SessionStates[e.NewState]));
    }

    private void SessionOnSessionDisconnected(object sender, AudioSessionDisconnectedEventArgs e)
    {
        Debug.WriteLine("SessionOnSessionDisconnected");
    }

    public void Dispose()
    {
        _session.DisplayNameChanged -= SessionOnDisplayNameChanged;
        _session.IconPathChanged -= SessionOnIconPathChanged;
        _session.SimpleVolumeChanged -= SessionOnSimpleVolumeChanged;
        _session.ChannelVolumeChanged -= SessionOnChannelVolumeChanged;
        _session.GroupingParamChanged -= SessionOnGroupingParamChanged;
        _session.StateChanged -= SessionOnStateChanged;
        _session.SessionDisconnected -= SessionOnSessionDisconnected;

        _disposable.Dispose();
        
        Disposed?.Invoke(this, EventArgs.Empty);
    }
}