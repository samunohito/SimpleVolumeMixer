using System;
using System.Collections.ObjectModel;
using DisposableComponents;
using Microsoft.Extensions.Logging;
using Reactive.Bindings.Extensions;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Event;
using SimpleVolumeMixer.Core.Helper.CoreAudio.EventAdapter;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Types;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio;

public class CoreAudioAccessor : DisposableComponent
{
    /// <summary>
    /// CoreAudioAPIからデバイスの破棄通知があり、<see cref="AudioDeviceAccessorManager"/>がそれを受けてデバイスの破棄を開始した時に呼び出される。
    /// </summary>
    /// <seealso cref="AudioDeviceAccessorManager.Remove(AudioDeviceAccessor)"/>
    /// <seealso cref="NotificationClientEventAdapter.DeviceRemoved"/>
    public event EventHandler<AudioDeviceAccessorEventArgs>? RenderDeviceDisposing
    {
        add => _renderDeviceManager.DeviceDisposing += value;
        remove => _renderDeviceManager.DeviceDisposing -= value;
    }

    /// <summary>
    /// CoreAudioAPIからデバイスの破棄通知があり、<see cref="AudioDeviceAccessorManager"/>がそれを受けてデバイスの破棄を開始した時に呼び出される。
    /// </summary>
    /// <seealso cref="AudioDeviceAccessorManager.Remove(AudioDeviceAccessor)"/>
    /// <seealso cref="NotificationClientEventAdapter.DeviceRemoved"/>
    public event EventHandler<AudioDeviceAccessorEventArgs>? CaptureDeviceDisposing
    {
        add => _captureDeviceManager.DeviceDisposing += value;
        remove => _captureDeviceManager.DeviceDisposing -= value;
    }

    /// <summary>
    /// CoreAudioAPIからデバイスの破棄通知があり、<see cref="AudioDeviceAccessorManager"/>がそれを受けてデバイスの破棄を完了した時に呼び出される。
    /// </summary>
    /// <seealso cref="AudioDeviceAccessorManager.Remove(AudioDeviceAccessor)"/>
    /// <seealso cref="NotificationClientEventAdapter.DeviceRemoved"/>
    public event EventHandler<AudioDeviceAccessorEventArgs>? RenderDeviceDisposed
    {
        add => _renderDeviceManager.DeviceDisposed += value;
        remove => _renderDeviceManager.DeviceDisposed -= value;
    }

    /// <summary>
    /// CoreAudioAPIからデバイスの破棄通知があり、<see cref="AudioDeviceAccessorManager"/>がそれを受けてデバイスの破棄を完了した時に呼び出される。
    /// </summary>
    /// <seealso cref="AudioDeviceAccessorManager.Remove(AudioDeviceAccessor)"/>
    /// <seealso cref="NotificationClientEventAdapter.DeviceRemoved"/>
    public event EventHandler<AudioDeviceAccessorEventArgs>? CaptureDeviceDisposed
    {
        add => _captureDeviceManager.DeviceDisposed += value;
        remove => _captureDeviceManager.DeviceDisposed -= value;
    }

    /// <summary>
    /// CoreAudioAPIからデバイスの破棄通知があり、<see cref="AudioDeviceAccessorManager"/>がそれを受けてデバイスのロールを最新に更新した際に呼び出される。
    /// </summary>
    /// <seealso cref="AudioDeviceAccessorManager"/>
    /// <seealso cref="NotificationClientEventAdapter.DefaultDeviceChanged"/>
    /// <seealso cref="AudioDeviceRole"/>
    public event EventHandler<DeviceAccessorRoleHolderChangedEventArgs>? RenderDeviceRoleChanged
    {
        add => _renderDeviceManager.DeviceRoleChanged += value;
        remove => _renderDeviceManager.DeviceRoleChanged -= value;
    }

    /// <summary>
    /// CoreAudioAPIからデバイスの破棄通知があり、<see cref="AudioDeviceAccessorManager"/>がそれを受けてデバイスのロールを最新に更新した際に呼び出される。
    /// </summary>
    /// <seealso cref="AudioDeviceAccessorManager"/>
    /// <seealso cref="NotificationClientEventAdapter.DefaultDeviceChanged"/>
    /// <seealso cref="AudioDeviceRole"/>
    public event EventHandler<DeviceAccessorRoleHolderChangedEventArgs>? CaptureDeviceRoleChanged
    {
        add => _captureDeviceManager.DeviceRoleChanged += value;
        remove => _captureDeviceManager.DeviceRoleChanged -= value;
    }

    private readonly ILogger _logger;
    private readonly AudioDeviceAccessorManager _renderDeviceManager;
    private readonly AudioDeviceAccessorManager _captureDeviceManager;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="logger"></param>
    public CoreAudioAccessor(ILogger logger)
    {
        _logger = logger;
        _renderDeviceManager = new AudioDeviceAccessorManager(DataFlowType.Render, logger).AddTo(Disposable);
        _renderDeviceManager.CollectAudioEndpoints();
        _captureDeviceManager = new AudioDeviceAccessorManager(DataFlowType.Capture, logger).AddTo(Disposable);
        _captureDeviceManager.CollectAudioEndpoints();
    }

    /// <summary>
    /// 現在再生用として使用可能な<see cref="AudioDeviceAccessor"/>の一覧を取得する。
    /// 読み取り専用であり、このオブジェクトからデバイスの増減を行うことは出来ない。
    /// デバイスの増減は<see cref="AudioDeviceAccessorManager.CollectAudioEndpoints"/>によりCoreAudioAPIからデバイス一覧を取り直すか、
    /// <see cref="AudioDeviceAccessorManager"/>がCoreAudioAPIからの通知を受け、その結果デバイスが追加されるかに限る。
    /// </summary>
    public ReadOnlyObservableCollection<AudioDeviceAccessor> RenderAudioDevices =>
        _renderDeviceManager.ReadOnlyCollection;

    /// <summary>
    /// 現在録音用として使用可能な<see cref="AudioDeviceAccessor"/>の一覧を取得する。
    /// 読み取り専用であり、このオブジェクトからデバイスの増減を行うことは出来ない。
    /// デバイスの増減は<see cref="AudioDeviceAccessorManager.CollectAudioEndpoints"/>によりCoreAudioAPIからデバイス一覧を取り直すか、
    /// <see cref="AudioDeviceAccessorManager"/>がCoreAudioAPIからの通知を受け、その結果デバイスが追加されるかに限る。
    /// </summary>
    public ReadOnlyObservableCollection<AudioDeviceAccessor> CaptureAudioDevices =>
        _captureDeviceManager.ReadOnlyCollection;

    /// <summary>
    /// <see cref="RoleType"/>が割り当てられている再生デバイスを取得する
    /// </summary>
    /// <param name="roleType"></param>
    /// <returns></returns>
    /// <seealso cref="AudioDeviceAccessorManager.GetDefaultDevice"/>
    public AudioDeviceAccessor? GetDefaultRenderDevice(RoleType roleType)
    {
        return _renderDeviceManager.GetDefaultDevice(roleType);
    }

    /// <summary>
    /// <see cref="RoleType"/>が割り当てられている録音デバイスを取得する
    /// </summary>
    /// <param name="roleType"></param>
    /// <returns></returns>
    /// <seealso cref="AudioDeviceAccessorManager.GetDefaultDevice"/>
    public AudioDeviceAccessor? GetDefaultCaptureDevice(RoleType roleType)
    {
        return _captureDeviceManager.GetDefaultDevice(roleType);
    }

    protected override void OnDisposing()
    {
        _logger.LogDebug("disposing...");
        base.OnDisposing();
    }
}