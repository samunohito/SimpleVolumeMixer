using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using CSCore.CoreAudioAPI;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SimpleVolumeMixer.Core.Helper.Component;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Event;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Types;
using AudioSessionStateChangedEventArgs =
    SimpleVolumeMixer.Core.Helper.CoreAudio.Event.AudioSessionStateChangedEventArgs;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio;

public class AudioSessionManagerAccessor : SafetyAccessorComponent
{
    public event EventHandler<EventArgs>? SessionManagerOpened;
    public event EventHandler<EventArgs>? SessionManagerClosed;

    private readonly object _sessionLock = new();
    private readonly ReactiveCollection<AudioSessionAccessor> _sessions;
    private readonly AudioDeviceAccessor _device;

    private AudioSessionManager2? _sessionManager;

    internal AudioSessionManagerAccessor(AudioDeviceAccessor device)
    {
        _device = device;
        _sessions = new ReactiveCollection<AudioSessionAccessor>().AddTo(Disposable);
        Sessions = _sessions.ToReadOnlyReactiveCollection().AddTo(Disposable);
        
        SessionManagerOpened += OnSessionManagerOpened;
    }
    
    public ReadOnlyReactiveCollection<AudioSessionAccessor> Sessions { get; }

    private void OnSessionManagerOpened(object? sender, EventArgs e)
    {
        lock (_sessionLock)
        {
            if (_sessionManager != null)
            {
                _sessionManager.SessionCreated += SessionManagerOnSessionCreated;
                _sessionManager.VolumeDuckNotification += SessionManagerOnVolumeDuckNotification;
                _sessionManager.VolumeUnDuckNotification += SessionManagerOnVolumeUnDuckNotification;
                
                RefreshSessions();
            }
        }
    }
    
    private void SessionManagerOnSessionCreated(object? sender, SessionCreatedEventArgs e)
    {
        Debug.WriteLine("SessionManagerOnSessionCreated " + e);

        lock (_sessionLock)
        {
            if (_sessionManager != null)
            {
                AppendSession(e.NewSession);
            }
        }
    }

    private void SessionManagerOnVolumeDuckNotification(object? sender, VolumeDuckNotificationEventArgs e)
    {
        Debug.WriteLine("SessionManagerOnVolumeDuckNotification " + e);
    }

    private void SessionManagerOnVolumeUnDuckNotification(object? sender, VolumeDuckNotificationEventArgs e)
    {
        Debug.WriteLine("SessionManagerOnVolumeUnDuckNotification " + e);
    }

    private void AudioSessionOnStateChanged(object? sender, AudioSessionStateChangedEventArgs e)
    {
        Debug.WriteLine("AudioSessionOnStateChanged " + e);

        switch (e.NewState)
        {
            case AudioSessionStateType.AudioSessionStateActive:
            case AudioSessionStateType.AudioSessionStateInactive:
                break;
            case AudioSessionStateType.AudioSessionStateExpired:
                DisposeSession((AudioSessionAccessor)sender!);
                break;
        }
    }

    private void AccessorOnDisposed(object? sender, EventArgs e)
    {
        Debug.WriteLine("AccessorOnDisposed " + e);
        DisposeSession((AudioSessionAccessor)sender!);
    }

    public void OpenSessionManager()
    {
        // AudioSessionManager2はMTAスレッドでしか取得できないので、別スレッドを起動する（WPFはSTAスレッド）
        var thread = new Thread(() =>
        {
            lock (_sessionLock)
            {
                if (_sessionManager != null)
                {
                    return;
                }

                // 個別で破棄するのでDisposableには入れない
                _sessionManager = AudioSessionManager2.FromMMDevice(_device.Device);
                SessionManagerOpened?.Invoke(this, EventArgs.Empty);
            }
        });
        thread.SetApartmentState(ApartmentState.MTA);
        thread.Start();
    }

    public void CloseSessionManager()
    {
        lock (_sessionLock)
        {
            DisposeSessions();
            
            if (_sessionManager != null)
            {
                _sessionManager.VolumeUnDuckNotification -= SessionManagerOnVolumeUnDuckNotification;
                _sessionManager.VolumeDuckNotification -= SessionManagerOnVolumeDuckNotification;
                _sessionManager.SessionCreated -= SessionManagerOnSessionCreated;

                _sessionManager.Dispose();
                _sessionManager = null;

                SessionManagerClosed?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private void RefreshSessions()
    {
        lock (_sessionLock)
        {
            DisposeSessions();
            
            if (_sessionManager != null)
            {
                // タイミングが早すぎると不正な状態のAudioSessionControl（参照先がIntPtr.Zero）が生成されるため、
                // IntPtr.ZeroのAudioSessionControlが無くなるまで何度か取得し直す
                using var sessionEnumerator = _sessionManager.GetSessionEnumerator();
                var sessions = new List<AudioSessionControl>(sessionEnumerator);
                while (true)
                {
                    if (sessions.All(x => x.BasePtr != IntPtr.Zero))
                    {
                        break;
                    }

                    sessions.ForEach(x => x.Dispose());
                    Thread.Sleep(1);

                    sessions.Clear();
                    sessions.AddRange(sessionEnumerator);
                }

                foreach (var session in sessions)
                {
                    AppendSession(session);
                }
            }
        }
    }

    private void DisposeSessions()
    {
        lock (_sessionLock)
        {
            foreach (var audioSession in _sessions.ToList())
            {
                DisposeSession(audioSession);
            }

            _sessions.Clear();
        }
    }

    private void AppendSession(AudioSessionControl sessionControl)
    {
        var accessor = new AudioSessionAccessor(sessionControl);
        accessor.StateChanged += AudioSessionOnStateChanged;
        accessor.Disposed += AccessorOnDisposed;
        if (accessor.IsSystemSoundSession)
        {
            // システムサウンドの場合は先頭にしたい
            _sessions.Insert(0, accessor);
        }
        else
        {
            _sessions.Add(accessor);
        }
    }

    private void DisposeSession(AudioSessionAccessor accessor)
    {
        _sessions.Remove(accessor);
        accessor.StateChanged -= AudioSessionOnStateChanged;
        accessor.Disposed -= AccessorOnDisposed;
        accessor.Dispose();
    }

    protected override void OnDisposing()
    {
        base.OnDisposing();
        SessionManagerOpened -= OnSessionManagerOpened;
        CloseSessionManager();
    }
}