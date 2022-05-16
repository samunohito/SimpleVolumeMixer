using System;
using System.Linq;
using CSCore.CoreAudioAPI;
using DisposableComponents;
using Microsoft.Extensions.Logging;
using Reactive.Bindings.Extensions;
using SimpleVolumeMixer.Core.Helper.Component;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Event;
using SimpleVolumeMixer.Core.Helper.CoreAudio.EventAdapter;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Internal;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Types;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio;

/// <summary>
/// 複数の<see cref="AudioDeviceAccessor"/>を管理するためのオブジェクト。
/// 各<see cref="AudioDeviceAccessor"/>の生成・破棄を担うほか、
/// ユーザ操作由来での機能実行及び、CoreAudioAPIからの通知を受けての機能実行を行う。
/// </summary>
public class AudioDeviceAccessorManager : SynchronizedObservableCollectionWrapper<AudioDeviceAccessor>
{
    /// <summary>
    /// このオブジェクトに登録されているいずれかの<see cref="AudioDeviceAccessor"/>が
    /// <see cref="AudioDeviceAccessor.Disposing"/>イベントを発動させた時に連動して発動する。
    /// </summary>
    /// <seealso cref="DisposableComponent.Disposing"/>
    public event EventHandler<AudioDeviceAccessorEventArgs>? DeviceDisposing;

    /// <summary>
    /// このオブジェクトに登録されているいずれかの<see cref="AudioDeviceAccessor"/>が
    /// <see cref="AudioDeviceAccessor.Disposed"/>イベントを発動させた時に連動して発動する。
    /// </summary>
    /// <seealso cref="DisposableComponent.Disposed"/>
    public event EventHandler<AudioDeviceAccessorEventArgs>? DeviceDisposed;

    /// <summary>
    /// このオブジェクトに登録されているいずれかの<see cref="AudioDeviceAccessor"/>が
    /// <see cref="AudioDeviceAccessor.RoleChanged"/>イベントを発動させた時に連動して発動する。
    /// </summary>
    /// <seealso cref="AudioDeviceRole.RoleChanged"/>
    /// <seealso cref="AudioDeviceAccessor.RoleChanged"/>
    public event EventHandler<DeviceAccessorRoleHolderChangedEventArgs>? DeviceRoleChanged;

    private readonly ILogger _logger;
    private readonly MMDeviceEnumerator _deviceEnumerator;
    private readonly MMNotificationClient _notificationClient;
    private readonly NotificationClientEventAdapter _clientEventAdapter;
    private readonly DataFlowType _targetDataFlow;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="dataFlowType"></param>
    /// <param name="logger"></param>
    public AudioDeviceAccessorManager(DataFlowType dataFlowType, ILogger logger)
    {
        _targetDataFlow = dataFlowType;
        _logger = logger;
        _deviceEnumerator = new MMDeviceEnumerator().AddTo(Disposable);
        _notificationClient = new MMNotificationClient(_deviceEnumerator).AddTo(Disposable);
        _clientEventAdapter = new NotificationClientEventAdapter(_notificationClient, logger).AddTo(Disposable);

        _clientEventAdapter.DeviceAdded += OnDeviceAdded;
        _clientEventAdapter.DeviceRemoved += OnDeviceRemoved;
        _clientEventAdapter.DefaultDeviceChanged += OnDefaultDeviceChanged;
        _clientEventAdapter.DeviceStateChanged += OnDeviceStateChanged;
    }

    /// <summary>
    /// 引数のデバイスIDとデータフローを持つデバイスがコレクションにあるかを確認する
    /// </summary>
    /// <param name="deviceId"></param>
    /// <param name="dataFlowType"></param>
    /// <returns></returns>
    public bool Contains(string? deviceId, DataFlowType dataFlowType)
    {
        if (deviceId == null)
        {
            return false;
        }

        lock (Gate)
        {
            return Collection.Any(x => x.DeviceId == deviceId && x.DataFlow == dataFlowType);
        }
    }

    /// <summary>
    /// 引数のデバイスがコレクションにあるかを確認する
    /// </summary>
    /// <param name="device"></param>
    /// <returns></returns>
    public bool Contains(MMDevice device)
    {
        return Contains(device.DeviceID, AccessorHelper.DataFlows[device.DataFlow]);
    }

    /// <summary>
    /// 引数のデバイスIDとデータフローを持つデバイスを取得する
    /// </summary>
    /// <param name="deviceId"></param>
    /// <returns></returns>
    public AudioDeviceAccessor? GetDevice(string? deviceId)
    {
        if (deviceId == null)
        {
            return null;
        }

        lock (Gate)
        {
            return Collection.FirstOrDefault(x => x.DeviceId == deviceId && x.DataFlow == _targetDataFlow);
        }
    }

    /// <summary>
    /// <see cref="RoleType"/>が割り当てられているデバイスを取得する
    /// </summary>
    /// <param name="roleType"></param>
    /// <returns></returns>
    /// <exception cref="ApplicationException"></exception>
    public AudioDeviceAccessor? GetDefaultDevice(RoleType roleType)
    {
        lock (Gate)
        {
            if (Count <= 0 || roleType == RoleType.Unknown)
            {
                return null;
            }

            using var defaultDevice = _deviceEnumerator.GetDefaultAudioEndpoint(
                AccessorHelper.DataFlowsRev[_targetDataFlow],
                AccessorHelper.RolesRev[roleType]);
            if (defaultDevice == null)
            {
                return null;
            }

            if (!Contains(defaultDevice))
            {
                throw new ApplicationException("unknown device : " + defaultDevice);
            }

            return GetDevice(defaultDevice.DeviceID);
        }
    }

    /// <summary>
    /// CoreAudioAPIから取得したMMDeviceをラッピングし、コレクションに追加する
    /// </summary>
    /// <param name="device"></param>
    public void Add(MMDevice device)
    {
        if (Contains(device))
        {
            return;
        }

        var ax = new AudioDeviceAccessor(device, _logger);
        ax.RoleChanged += OnDeviceRoleChanged;
        ax.Disposing += OnDeviceDisposing;
        ax.Disposed += OnDeviceDisposed;
        Add(ax);
    }

    /// <summary>
    /// デバイスの破棄処理を呼び出し、コレクションから削除する
    /// </summary>
    /// <param name="ax"></param>
    public new void Remove(AudioDeviceAccessor ax)
    {
        // Disposing/DisposedはSessionDisposing/SessionDisposedが通知されてからそれぞれ解除する
        ax.RoleChanged -= OnDeviceRoleChanged;
        ax.Dispose();
        base.Remove(ax);
    }

    /// <summary>
    /// 引数のデバイスIDを持つデバイスを全て削除する。
    /// </summary>
    /// <param name="deviceId"></param>
    public void Remove(string deviceId)
    {
        lock (Gate)
        {
            Collection.Where(x => x.DeviceId == deviceId)
                .ToList()
                .ForEach(x => Remove(x));
        }
    }

    /// <summary>
    /// 引数の<see cref="MMDevice"/>が持つデバイスIDと<see cref="DataFlowType"/>に一致するものを削除する
    /// </summary>
    /// <param name="device"></param>
    public void Remove(MMDevice device)
    {
        var target = GetDevice(device.DeviceID);
        if (target == null)
        {
            return;
        }

        Remove(target);
    }

    /// <summary>
    /// コレクションをクリアし、かつ実行時点で保持していたデバイスをすべて破棄する
    /// </summary>
    public new void Clear()
    {
        var collections = Collection.ToList();
        base.Clear();

        foreach (var ax in collections)
        {
            // Disposing/DisposedはSessionDisposing/SessionDisposedが通知されてからそれぞれ解除する
            ax.RoleChanged -= OnDeviceRoleChanged;
            ax.Dispose();
        }
    }

    /// <summary>
    /// CoreAudioAPIから使用可能なデバイス一覧を取得し、コレクションに順次登録する。
    /// </summary>
    public void CollectAudioEndpoints()
    {
        lock (Gate)
        {
            var dataFlow = AccessorHelper.DataFlowsRev[_targetDataFlow];
            using var devices = _deviceEnumerator.EnumAudioEndpoints(dataFlow, DeviceState.Active);
            if (devices == null)
            {
                throw new InvalidOperationException("通常はありえない");
            }

            foreach (var device in devices)
            {
                Add(device);
            }

            if (Collection.Any(x => x.DataFlow == _targetDataFlow))
            {
                // deviceが1つもない状態でGetDefaultAudioEndpointを呼ぶとエラー落ちする
                using var mulDevice = _deviceEnumerator.GetDefaultAudioEndpoint(dataFlow, Role.Multimedia);
                using var comDevice = _deviceEnumerator.GetDefaultAudioEndpoint(dataFlow, Role.Communications);
                DeviceRoleSync(mulDevice.DeviceID, _targetDataFlow, RoleType.Multimedia);
                DeviceRoleSync(comDevice.DeviceID, _targetDataFlow, RoleType.Communications);
            }
        }
    }

    /// <summary>
    /// 引数のDataFlowとRoleをもとに、引数のデバイスIDを持つデバイスのロール情報を更新する
    /// </summary>
    /// <param name="deviceId"></param>
    /// <param name="dataFlowType"></param>
    /// <param name="roleType"></param>
    private void DeviceRoleSync(string deviceId, DataFlowType dataFlowType, RoleType roleType)
    {
        lock (Gate)
        {
            // Communications/Multimediaはシステム上で1つだけなので、すでに存在するものはフラグをOFFにする
            switch (roleType)
            {
                case RoleType.Communications:
                    foreach (var device in Collection)
                    {
                        device.Role.Communications = false;
                    }

                    break;
                case RoleType.Multimedia:
                    foreach (var device in Collection)
                    {
                        device.Role.Multimedia = false;
                    }

                    break;
            }

            // 今回あたらしくCommunications/Multimediaに設定されたものにフラグを立てる.
            // 1つのデバイスが複数のRoleを持ったとしても、持っているRoleの数だけ通知してくるため、都度確認して対応するRoleのフラグを管理する必要がある
            var target = GetDevice(deviceId);
            if (target == null)
            {
                return;
            }

            switch (roleType)
            {
                case RoleType.Communications:
                    target.Role.Communications = true;
                    break;
                case RoleType.Multimedia:
                    target.Role.Multimedia = true;
                    break;
            }
        }
    }

    /// <summary>
    /// CoreAudioAPIからデバイスが追加された旨の通知が届いた際に呼び出される
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnDeviceAdded(object? sender, DeviceNotificationEventArgs e)
    {
        var targetDataFlow = AccessorHelper.DataFlowsRev[_targetDataFlow];
        if (e.TryGetDevice(out var newDevice) && newDevice.DataFlow == targetDataFlow)
        {
            Add(newDevice);
        }
    }

    /// <summary>
    /// CoreAudioAPIからデバイスが削除された旨の通知が届いた際に呼び出される
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnDeviceRemoved(object? sender, DeviceNotificationEventArgs e)
    {
        var targetDataFlow = AccessorHelper.DataFlowsRev[_targetDataFlow];
        if (e.TryGetDevice(out var removedDevice) && removedDevice.DataFlow == targetDataFlow)
        {
            Remove(e.DeviceId);
        }
    }

    /// <summary>
    /// CoreAudioAPIからデフォルトのサウンドデバイスが変更された旨の通知が届いた際に呼び出される
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnDefaultDeviceChanged(object? sender, DefaultDeviceChangedEventArgs e)
    {
        var newRoleType = AccessorHelper.Roles[e.Role];
        var dataFlowType = AccessorHelper.DataFlows[e.DataFlow];
        DeviceRoleSync(e.DeviceId, dataFlowType, newRoleType);
    }

    /// <summary>
    /// CoreAudioAPIからデバイスの状態が変化した旨の通知が届いた際に呼び出される
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnDeviceStateChanged(object? sender, DeviceStateChangedEventArgs e)
    {
        var targetDataFlow = AccessorHelper.DataFlowsRev[_targetDataFlow];
        if (e.TryGetDevice(out var device) && device.DataFlow == targetDataFlow)
        {
            switch (e.DeviceState)
            {
                case DeviceState.Active:
                    Add(device);
                    break;
                case DeviceState.Disabled:
                case DeviceState.NotPresent:
                case DeviceState.UnPlugged:
                    Remove(e.DeviceId);
                    break;
            }
        }
    }

    /// <summary>
    /// コレクションに保持しているいずれかのデバイスのロール情報が変更された際に呼び出される
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnDeviceRoleChanged(object? sender, DeviceAccessorRoleHolderChangedEventArgs e)
    {
        DeviceRoleChanged?.Invoke(this, e);
    }

    /// <summary>
    /// コレクションに保持しているデバイスが破棄される際に呼び出される
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnDeviceDisposing(object? sender, EventArgs e)
    {
        if (sender is AudioDeviceAccessor ax)
        {
            ax.Disposing -= OnDeviceDisposing;
            DeviceDisposing?.Invoke(this, new AudioDeviceAccessorEventArgs(ax));

            if (Contains(ax))
            {
                // このクラス内からデバイスを消す際はDispose()を呼んでコレクションから削除しているが、
                // 外的要因でデバイスのDispose()が呼び出された際は破棄された状態でコレクションに残ってしまう。
                // 上記のケースに対応できるよう、破棄処理の呼び出し時にもコレクションからの削除処理を置いておく。
                Remove(ax);
            }
        }
    }

    /// <summary>
    /// コレクションに保持しているデバイスが破棄された際に呼び出される
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnDeviceDisposed(object? sender, EventArgs e)
    {
        if (sender is AudioDeviceAccessor ax)
        {
            ax.Disposed -= OnDeviceDisposed;
            DeviceDisposed?.Invoke(this, new AudioDeviceAccessorEventArgs(ax));
        }
    }

    protected override void OnDisposing()
    {
        _logger.LogDebug("disposing...");

        _clientEventAdapter.DeviceAdded -= OnDeviceAdded;
        _clientEventAdapter.DeviceRemoved -= OnDeviceRemoved;
        _clientEventAdapter.DefaultDeviceChanged -= OnDefaultDeviceChanged;
        _clientEventAdapter.DeviceStateChanged -= OnDeviceStateChanged;

        Clear();

        base.OnDisposing();
    }
}