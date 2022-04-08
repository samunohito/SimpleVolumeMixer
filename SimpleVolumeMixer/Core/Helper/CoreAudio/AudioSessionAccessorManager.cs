using System;
using System.Linq;
using System.Threading.Tasks;
using CSCore.CoreAudioAPI;
using Microsoft.Extensions.Logging;
using SimpleVolumeMixer.Core.Helper.Component;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Event;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Types;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio;

public class AudioSessionAccessorManager : SynchronizedReactiveCollectionWrapper<AudioSessionAccessor>
{
    public event EventHandler<AudioSessionAccessorEventArgs>? SessionDisposing;
    public event EventHandler<AudioSessionAccessorEventArgs>? SessionDisposed;

    private readonly ILogger _logger;
    private readonly AudioSessionManagerAccessor _sessionManager;

    public AudioSessionAccessorManager(AudioDeviceAccessor device, ILogger logger)
    {
        _logger = logger;
        _sessionManager = new AudioSessionManagerAccessor(device, logger);
        _sessionManager.SessionManagerOpened += OnSessionManagerOpened;
        _sessionManager.SessionManagerClosed += OnSessionManagerClosed;
    }
    
    public bool Contains(int? procId)
    {
        if (procId == null)
        {
            return false;
        }

        lock (Gate)
        {
            return this.Any(x => x.Process?.Id == procId);
        }
    }

    public AudioSessionAccessor? GetSession(int? procId)
    {
        if (procId == null)
        {
            return null;
        }

        lock (Gate)
        {
            return this.FirstOrDefault(x => x.Process?.Id == procId);
        }
    }

    public void Add(AudioSessionControl session)
    {
        var session2 = session.QueryInterface<AudioSessionControl2>();
        if (Contains(session2.Process?.Id))
        {
            return;
        }

        var ax = new AudioSessionAccessor(session, _logger);
        ax.StateChanged += OnStateChanged;
        ax.SessionDisconnected += OnSessionDisconnected;
        ax.Disposing += OnSessionDisposing;
        ax.Disposed += OnSessionDisposed;

        if (ax.IsSystemSoundSession)
        {
            // SystemSoundの場合は先頭に置きたい
            Insert(0, ax);
        }
        else
        {
            Add(ax);
        }
    }

    public void Remove(int? procId)
    {
        if (procId == null)
        {
            return;
        }

        lock (Gate)
        {
            this.Where(x => x.Process?.Id == procId)
                .ToList()
                .ForEach(x => Remove(x));
        }
    }

    public void Remove(AudioSessionControl session)
    {
        var session2 = session.QueryInterface<AudioSessionControl2>();
        var id = session2.Process?.Id;
        if (id == null)
        {
            return;
        }

        Remove(id);
    }

    public Task OpenSession()
    {
        return _sessionManager.OpenSessionManager();
    }

    public void CloseSession()
    {
        _sessionManager.CloseSessionManager();
    }
    
    private void OnSessionManagerOpened(object? sender, EventArgs e)
    {
        using var enumerator = _sessionManager.GetEnumerator();
        if (enumerator == null)
        {
            return;
        }

        foreach (var session in enumerator)
        {
            Add(session);
        }
    }

    private void OnSessionManagerClosed(object? sender, EventArgs e)
    {
        lock (Gate)
        {
            Clear();
        }
    }

    private void OnSessionDisconnected(object? sender, AudioSessionAccessorDisconnectedEventArgs e)
    {
        if (!Contains(e.Session))
        {
            Remove(e.Session);
        }
    }

    private void OnStateChanged(object? sender, AudioSessionAccessorStateChangedEventArgs e)
    {
        switch (e.NewState)
        {
            case AudioSessionStateType.AudioSessionStateActive:
            case AudioSessionStateType.AudioSessionStateInactive:
                break;
            case AudioSessionStateType.AudioSessionStateExpired:
                Remove(e.Session);
                break;
        }
    }

    private void OnSessionDisposing(object? sender, EventArgs e)
    {
        if (sender is AudioSessionAccessor ax)
        {
            SessionDisposing?.Invoke(this, new AudioSessionAccessorEventArgs(ax));

            ax.StateChanged -= OnStateChanged;
            ax.SessionDisconnected -= OnSessionDisconnected;
            ax.Disposing -= OnSessionDisposing;
            Remove(ax);
        }
    }

    private void OnSessionDisposed(object? sender, EventArgs e)
    {
        if (sender is AudioSessionAccessor ax)
        {
            ax.Disposed -= OnSessionDisposed;

            SessionDisposed?.Invoke(this, new AudioSessionAccessorEventArgs(ax));
        }
    }

    protected override void OnDisposing()
    {
        _logger.LogDebug("disposing...");
        
        _sessionManager.SessionManagerOpened -= OnSessionManagerOpened;
        _sessionManager.SessionManagerClosed -= OnSessionManagerClosed;

        lock (Gate)
        {
            var disposes = this.ToList();
            
            Clear();
            
            foreach (var session in disposes)
            {
                // AudioSessionAccessorとこのクラスの結びつきを解除するのはOnSessionDisposingで
                session.Dispose();
            }
        }

        base.OnDisposing();
    }
}