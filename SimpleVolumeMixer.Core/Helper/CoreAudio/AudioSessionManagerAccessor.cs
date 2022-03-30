using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading;
using CSCore.CoreAudioAPI;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SimpleVolumeMixer.Core.Models.Domain.CoreAudio;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio;

public class AudioSessionManagerAccessor : IDisposable
{
    public event EventHandler<AudioSessionManagerStartedEventArgs>? SessionManagerOpened;
    public event EventHandler<EventArgs>? SessionManagerClosed;

    private readonly object _sessionLock = new object();
    private readonly CompositeDisposable _disposable;
    private readonly AudioDeviceAccessor _device;
    private readonly ReactiveCollection<AudioSessionAccessor> _sessions;

    private AudioSessionManager2? _sessionManager;

    internal AudioSessionManagerAccessor(AudioDeviceAccessor device)
    {
        _disposable = new CompositeDisposable();
        _device = device;
        _sessions = new ReactiveCollection<AudioSessionAccessor>().AddTo(_disposable);
        Sessions = _sessions.ToReadOnlyReactiveCollection().AddTo(_disposable);
    }

    public ReadOnlyReactiveCollection<AudioSessionAccessor> Sessions { get; }
    public bool IsDisposed => _disposable.IsDisposed;

    private void SessionManagerOnSessionCreated(object sender, SessionCreatedEventArgs e)
    {
        Debug.WriteLine("SessionManagerOnSessionCreated");

        lock (_sessionLock)
        {
            if (_sessionManager != null)
            {
                AppendSession(e.NewSession);
            }
        }
    }

    private void SessionManagerOnVolumeDuckNotification(object sender, VolumeDuckNotificationEventArgs e)
    {
        Debug.WriteLine("SessionManagerOnVolumeDuckNotification");
    }

    private void SessionManagerOnVolumeUnDuckNotification(object sender, VolumeDuckNotificationEventArgs e)
    {
        Debug.WriteLine("SessionManagerOnVolumeUnDuckNotification");
    }

    private void AudioSessionOnStateChanged(object sender, AudioSessionStateChangedEventArgs e)
    {
        Debug.WriteLine("AudioSessionOnStateChanged");
        
        switch (e.NewState)
        {
            case AudioSessionStateType.AudioSessionStateActive:
                break;
            case AudioSessionStateType.AudioSessionStateExpired:
            case AudioSessionStateType.AudioSessionStateInactive:
                DisposeSession((AudioSessionAccessor)sender);
                break;
        }
    }

    private void AccessorOnDisposed(object sender, EventArgs e)
    {
        Debug.WriteLine("AccessorOnDisposed");
        DisposeSession((AudioSessionAccessor)sender);
    }

    public void OpenSessionManager()
    {
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
                _sessionManager.SessionCreated += SessionManagerOnSessionCreated;
                _sessionManager.VolumeDuckNotification += SessionManagerOnVolumeDuckNotification;
                _sessionManager.VolumeUnDuckNotification += SessionManagerOnVolumeUnDuckNotification;

                RefreshSessions();
                SessionManagerOpened?.Invoke(this, new AudioSessionManagerStartedEventArgs(Sessions));
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
                using var sessionEnumerator = _sessionManager.GetSessionEnumerator();
                foreach (var session in sessionEnumerator)
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
        _sessions.Add(accessor);
    }

    private void DisposeSession(AudioSessionAccessor accessor)
    {
        _sessions.Remove(accessor);
        accessor.StateChanged -= AudioSessionOnStateChanged;
        accessor.Disposed -= AccessorOnDisposed;
        accessor.Dispose();
    }

    public void Dispose()
    {
        CloseSessionManager();
        _disposable.Dispose();
    }
}