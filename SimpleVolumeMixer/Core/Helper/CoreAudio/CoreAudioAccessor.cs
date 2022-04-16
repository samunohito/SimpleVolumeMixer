using System;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SimpleVolumeMixer.Core.Helper.Component;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Event;
using SimpleVolumeMixer.Core.Helper.CoreAudio.EventAdapter;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Types;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio;

public class CoreAudioAccessor : SafetyAccessorComponent
{
    /// <summary>
    /// CoreAudioAPIからデバイスの破棄通知があり、<see cref="AudioDeviceAccessorManager"/>がそれを受けてデバイスの破棄を開始した時に呼び出される。
    /// </summary>
    /// <seealso cref="AudioDeviceAccessorManager.Remove(AudioDeviceAccessor)"/>
    /// <seealso cref="NotificationClientEventAdapter.DeviceRemoved"/>
    public event EventHandler<AudioDeviceAccessorEventArgs>? DeviceDisposing
    {
        add => _deviceManager.DeviceDisposing += value;
        remove => _deviceManager.DeviceDisposing -= value;
    }

    /// <summary>
    /// CoreAudioAPIからデバイスの破棄通知があり、<see cref="AudioDeviceAccessorManager"/>がそれを受けてデバイスの破棄を完了した時に呼び出される。
    /// </summary>
    /// <seealso cref="AudioDeviceAccessorManager.Remove(AudioDeviceAccessor)"/>
    /// <seealso cref="NotificationClientEventAdapter.DeviceRemoved"/>
    public event EventHandler<AudioDeviceAccessorEventArgs>? DeviceDisposed
    {
        add => _deviceManager.DeviceDisposed += value;
        remove => _deviceManager.DeviceDisposed -= value;
    }

    /// <summary>
    /// CoreAudioAPIからデバイスの破棄通知があり、<see cref="AudioDeviceAccessorManager"/>がそれを受けてデバイスのロールを最新に更新した際に呼び出される。
    /// </summary>
    /// <seealso cref="AudioDeviceAccessorManager"/>
    /// <seealso cref="NotificationClientEventAdapter.DefaultDeviceChanged"/>
    /// <seealso cref="AudioDeviceRole"/>
    public event EventHandler<DeviceAccessorRoleHolderChangedEventArgs>? DeviceRoleChanged
    {
        add => _deviceManager.DeviceRoleChanged += value;
        remove => _deviceManager.DeviceRoleChanged -= value;
    }

    private readonly ILogger _logger;
    private readonly AudioDeviceAccessorManager _deviceManager;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="logger"></param>
    public CoreAudioAccessor(ILogger logger)
    {
        _logger = logger;
        _deviceManager = new AudioDeviceAccessorManager(logger).AddTo(Disposable);
        _deviceManager.CollectAudioEndpoints();
    }

    /// <summary>
    /// 現在使用可能な<see cref="AudioDeviceAccessor"/>の一覧を取得する。
    /// 読み取り専用であり、このオブジェクトからデバイスの増減を行うことは出来ない。
    /// デバイスの増減は<see cref="AudioDeviceAccessorManager.CollectAudioEndpoints"/>によりCoreAudioAPIからデバイス一覧を取り直すか、
    /// <see cref="AudioDeviceAccessorManager"/>がCoreAudioAPIからの通知を受け、その結果デバイスが追加されるかに限る。
    /// </summary>
    public ReadOnlyObservableCollection<AudioDeviceAccessor> AudioDevices => _deviceManager.ReadOnlyCollection;

    /// <summary>
    /// 引数の<see cref="DataFlowType"/>と<see cref="RoleType"/>が割り当てられているデバイスを取得する
    /// </summary>
    /// <param name="dataFlowType"></param>
    /// <param name="roleType"></param>
    /// <returns></returns>
    /// <seealso cref="AudioDeviceAccessorManager.GetDefaultDevice"/>
    public AudioDeviceAccessor? GetDefaultDevice(DataFlowType dataFlowType, RoleType roleType)
    {
        return _deviceManager.GetDefaultDevice(dataFlowType, roleType);
    }

    protected override void OnDisposing()
    {
        _logger.LogDebug("disposing...");
        base.OnDisposing();
    }
}