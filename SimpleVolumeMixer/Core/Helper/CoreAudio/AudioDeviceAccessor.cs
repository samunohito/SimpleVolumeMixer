using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
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
/// <see cref="MMDevice"/>と及び周辺インターフェース・関連情報を総括するラッパーオブジェクト。
/// </summary>
/// <seealso cref="MMDevice"/>
/// <seealso cref="AudioEndpointVolume"/>
/// <seealso cref="AudioMeterInformation"/>
/// <seealso cref="AudioSessionAccessorManager"/>
public class AudioDeviceAccessor : SafetyAccessComponent
{
    /// <summary>
    /// デバイスが持つロールに変化が発生した際に発動するイベントハンドラ。
    /// このオブジェクトが<see cref="AudioDeviceAccessorManager"/>により生成されて間もなくか、
    /// CoreAudioAPIから<see cref="NotificationClientEventAdapter.DefaultDeviceChanged"/>経由で通知があった際に発動する。
    /// </summary>
    /// <seealso cref="AudioDeviceRole"/>
    /// <seealso cref="NotificationClientEventAdapter.DefaultDeviceChanged"/>
    /// <seealso cref="AudioDeviceAccessorManager"/>
    public event EventHandler<DeviceAccessorRoleHolderChangedEventArgs>? RoleChanged
    {
        add => Role.RoleChanged += value;
        remove => Role.RoleChanged -= value;
    }

    private readonly ILogger _logger;
    private readonly AudioEndpointVolume _endpointVolume;
    private readonly AudioMeterInformation _meterInformation;
    private readonly AudioSessionAccessorManager _accessorManager;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="device"></param>
    /// <param name="logger"></param>
    public AudioDeviceAccessor(MMDevice device, ILogger logger)
    {
        Device = device.AddTo(Disposable);

        _logger = logger;
        _endpointVolume = AudioEndpointVolume.FromDevice(Device).AddTo(Disposable);
        _meterInformation = AudioMeterInformation.FromDevice(Device).AddTo(Disposable);
        _accessorManager = new AudioSessionAccessorManager(this, logger);

        Role = new AudioDeviceRole(this);
    }

    /// <summary>
    /// 生の<see cref="MMDevice"/>
    /// </summary>
    public MMDevice Device { get; }

    /// <summary>
    /// このデバイスが持つロールを管理する
    /// </summary>
    /// <seealso cref="AudioDeviceRole"/>
    public AudioDeviceRole Role { get; }

    /// <summary>
    /// このデバイスに紐付けられた<see cref="AudioSessionAccessor"/>の一覧を取得する。
    /// セッションの一覧にアクセスするには、<see cref="OpenSession"/>をあらかじめ呼び出しておく必要がある。
    /// <see cref="CloseSession"/>が呼び出された後はセッションにアクセスすることができなくなる。
    /// </summary>
    public ReadOnlyObservableCollection<AudioSessionAccessor> Sessions => _accessorManager.ReadOnlyCollection;

    public string? DeviceId => SafeRead(() => Device.DeviceID, null);
    public string? FriendlyName => SafeRead(() => Device.FriendlyName, null);
    public string? DevicePath => SafeRead(() => Device.DevicePath, null);

    public DeviceStateType DeviceState => SafeRead(
        () => Device.GetStateNative(out var value) >= 0
            ? AccessorHelper.DeviceStates[value]
            : DeviceStateType.Unknown,
        DeviceStateType.Unknown
    );

    public DataFlowType DataFlow => SafeRead(
        () => AccessorHelper.DataFlows[Device.DataFlow],
        DataFlowType.Unknown);

    public int ChannelCount => SafeRead(_endpointVolume.GetChannelCount, 0);

    public float PeakValue => SafeRead(() => _meterInformation.GetPeakValueNative(out var value) >= 0
            ? value
            : 0.0f,
        0.0f
    );

    public float[]? ChannelsPeakValues
    {
        get
        {
            if (_meterInformation.GetMeteringChannelCountNative(out var count) < 0)
            {
                return null;
            }

            if (_meterInformation.GetChannelsPeakValuesNative(count, out var result) < 0)
            {
                return null;
            }

            return result;
        }
    }

    public int MeteringChannelCount => SafeRead(_meterInformation.GetMeteringChannelCount, 0);

    public float MasterVolumeLevel
    {
        get => SafeRead(_endpointVolume.GetMasterVolumeLevel, 0.0f);
        set => SafeWrite(v => _endpointVolume.MasterVolumeLevel = v, value);
    }

    public float MasterVolumeLevelScalar
    {
        get => SafeRead(
            () => _endpointVolume.GetMasterVolumeLevelScalarNative(out var value) >= 0
                ? value
                : 0.0f,
            0.0f
        );
        set => SafeWrite(v => _endpointVolume.MasterVolumeLevelScalar = v, value);
    }

    public bool IsMuted
    {
        get
        {
            TrySafeRead(_endpointVolume.GetMute, false, out var result);
            return result;
        }
        set => SafeWrite(v => _endpointVolume.IsMuted = v, value);
    }

    public Task OpenSession()
    {
        _logger.LogInformation("open session in {}", FriendlyName);
        return _accessorManager.OpenSession();
    }

    public void CloseSession()
    {
        _logger.LogInformation("close session in {}", FriendlyName);
        _accessorManager.CloseSession();
    }

    protected override void OnDisposing()
    {
        _logger.LogInformation("disposing device... {}", FriendlyName);

        CloseSession();
        base.OnDisposing();
    }
}