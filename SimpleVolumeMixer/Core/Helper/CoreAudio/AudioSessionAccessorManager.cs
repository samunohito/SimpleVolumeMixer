using System;
using System.Linq;
using System.Threading.Tasks;
using CSCore.CoreAudioAPI;
using Microsoft.Extensions.Logging;
using Reactive.Bindings.Extensions;
using SimpleVolumeMixer.Core.Helper.Component;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Event;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Types;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio;

/// <summary>
/// 複数の<see cref="AudioSessionAccessor"/>を管理するためのオブジェクト。
/// 各<see cref="AudioSessionAccessor"/>の生成・破棄を担うほか、
/// ユーザ操作由来での機能実行及び、CoreAudioAPIからの通知を受けての機能実行を行う。
/// </summary>
public class AudioSessionAccessorManager : SynchronizedObservableCollectionWrapper<AudioSessionAccessor>
{
    /// <summary>
    /// このオブジェクトに登録されているいずれかの<see cref="AudioSessionAccessor"/>が
    /// <see cref="AudioSessionAccessor.Disposing"/>イベントを発動させた時に連動して発動する。
    /// </summary>
    /// <seealso cref="DisposableComponent.Disposing"/>
    public event EventHandler<AudioSessionAccessorEventArgs>? SessionDisposing;

    /// <summary>
    /// このオブジェクトに登録されているいずれかの<see cref="AudioSessionAccessor"/>が
    /// <see cref="AudioSessionAccessor.Disposed"/>イベントを発動させた時に連動して発動する。
    /// </summary>
    /// <seealso cref="DisposableComponent.Disposed"/>
    public event EventHandler<AudioSessionAccessorEventArgs>? SessionDisposed;

    private readonly ILogger _logger;
    private readonly AudioSessionManagerAccessor _sessionManager;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="device"></param>
    /// <param name="logger"></param>
    public AudioSessionAccessorManager(AudioDeviceAccessor device, ILogger logger)
    {
        _logger = logger;
        _sessionManager = new AudioSessionManagerAccessor(device, logger).AddTo(Disposable);
        _sessionManager.SessionManagerOpened += OnSessionManagerOpened;
        _sessionManager.SessionManagerClosed += OnSessionManagerClosed;
        _sessionManager.SessionCreated += OnSessionCreated;
    }

    /// <summary>
    /// 引数のPIDを持つセッションがコレクションにあるかを確認する
    /// </summary>
    /// <param name="procId"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 引数のPIDを持つセッションを取得する
    /// </summary>
    /// <param name="procId"></param>
    /// <returns></returns>
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

    /// <summary>
    /// CoreAudioAPIから取得したAudioSessionControlをラッピングし、コレクションに追加する
    /// </summary>
    /// <param name="session"></param>
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

    /// <summary>
    /// セッションの破棄処理を呼び出し、コレクションから削除する
    /// </summary>
    /// <param name="ax"></param>
    public new void Remove(AudioSessionAccessor ax)
    {
        // Disposing/DisposedはSessionDisposing/SessionDisposedが通知されてからそれぞれ解除する
        ax.StateChanged -= OnStateChanged;
        ax.SessionDisconnected -= OnSessionDisconnected;
        ax.Dispose();
        base.Remove(ax);
    }

    /// <summary>
    /// 引数のPIDを持つセッションを全て破棄し、削除する。
    /// </summary>
    /// <param name="procId"></param>
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

    /// <summary>
    /// 引数の<see cref="AudioSessionControl"/>を持つセッションを破棄し、全て削除する。
    /// </summary>
    /// <param name="session"></param>
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

    /// <summary>
    /// コレクションをクリアし、かつ実行時点で保持していたセッションを破棄する
    /// </summary>
    public new void Clear()
    {
        var collections = this.ToList();
        base.Clear();

        foreach (var ax in collections)
        {
            // Disposing/DisposedはSessionDisposing/SessionDisposedが通知されてからそれぞれ解除する
            ax.StateChanged -= OnStateChanged;
            ax.SessionDisconnected -= OnSessionDisconnected;
            ax.Dispose();
        }
    }

    /// <summary>
    /// セッションマネージャをCoreAudioAPIから取得・セットアップし、使用可能な状態にする
    /// </summary>
    /// <returns></returns>
    public Task OpenSession()
    {
        return _sessionManager.OpenSessionManager();
    }

    /// <summary>
    /// セッションマネージャを破棄し、使用を終了する
    /// </summary>
    public void CloseSession()
    {
        _sessionManager.CloseSessionManager();
    }

    /// <summary>
    /// CoreAudioAPIから新規セッションが作成された旨の通知が届いた際に呼び出される
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void OnSessionCreated(object? sender, SessionCreatedEventArgs e)
    {
        var session2 = e.NewSession.QueryInterface<AudioSessionControl2>();
        if (Contains(session2?.Process?.Id))
        {
            return;
        }

        Add(e.NewSession);
    }

    /// <summary>
    /// セッションマネージャの取得が完了した際に呼び出される。
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnSessionManagerOpened(object? sender, EventArgs e)
    {
        using var enumerator = _sessionManager.GetEnumerator();
        if (enumerator == null)
        {
            return;
        }

        // セッションマネージャからデバイスに紐づくセッション一覧をすべて抜き出し、コレクションに追加していく
        foreach (var session in enumerator)
        {
            Add(session);
        }
    }

    /// <summary>
    /// セッションマネージャが破棄された際に呼び出される
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnSessionManagerClosed(object? sender, EventArgs e)
    {
        // 全セッションの破棄処理を起動したのちコレクションからも全削除し、セッションを使用できないようにする
        Clear();
    }

    /// <summary>
    /// CoreAudioAPIからセッション切断の通知が来た際に呼び出される
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnSessionDisconnected(object? sender, AudioSessionAccessorDisconnectedEventArgs e)
    {
        if (!Contains(e.Session))
        {
            // 以降、切断されたセッションを使用しないよう破棄し、コレクションからも削除する
            Remove(e.Session);
        }
    }

    /// <summary>
    /// CoreAudioAPIからセッションの状態が変わった旨の通知が届いた際に呼び出される。
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnStateChanged(object? sender, AudioSessionAccessorStateChangedEventArgs e)
    {
        switch (e.NewState)
        {
            case AudioSessionStateType.AudioSessionStateActive:
            case AudioSessionStateType.AudioSessionStateInactive:
                // 音の出力を終了するとInactiveになるアプリもあるため（SystemSoundが筆頭）、
                // Inactiveとなっても削除しない
                break;
            case AudioSessionStateType.AudioSessionStateExpired:
                // 期限切れとなったセッションは使用不可能であるため破棄し、コレクションからも削除する
                Remove(e.Session);
                break;
        }
    }

    /// <summary>
    /// コレクションに保持しているセッションが破棄される際に呼び出される
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnSessionDisposing(object? sender, EventArgs e)
    {
        if (sender is AudioSessionAccessor ax)
        {
            ax.Disposing -= OnSessionDisposing;
            SessionDisposing?.Invoke(this, new AudioSessionAccessorEventArgs(ax));

            if (Contains(ax))
            {
                // このクラス内からセッションを消す際はDispose()を呼んでコレクションから削除しているが、
                // 外的要因でDispose()が呼び出された際は破棄された状態でコレクションに残ってしまう。
                // 上記のケースに対応できるよう、破棄処理の呼び出し時にもコレクションからの削除処理を置いておく。
                Remove(ax);
            }
        }
    }

    /// <summary>
    /// コレクションに保持しているセッションが破棄されたら呼び出される
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
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

        _sessionManager.SessionCreated -= OnSessionCreated;
        _sessionManager.SessionManagerOpened -= OnSessionManagerOpened;
        _sessionManager.SessionManagerClosed -= OnSessionManagerClosed;

        Clear();

        base.OnDisposing();
    }
}