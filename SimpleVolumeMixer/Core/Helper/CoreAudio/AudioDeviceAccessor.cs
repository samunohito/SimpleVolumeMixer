using System;
using System.Threading.Tasks;
using CSCore.CoreAudioAPI;
using Microsoft.Extensions.Logging;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SimpleVolumeMixer.Core.Helper.Component;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Event;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Internal;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Types;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio;

public class AudioDeviceAccessor : SafetyAccessorComponent
{
    public event EventHandler<DeviceAccessorRoleHolderChangedEventArgs>? RoleChanged
    {
        add => Role.RoleChanged += value;
        remove => Role.RoleChanged -= value;
    }

    private readonly AudioEndpointVolume _endpointVolume;
    private readonly AudioMeterInformation _meterInformation;
    private readonly AudioSessionAccessorManager _accessorManager;

    public AudioDeviceAccessor(MMDevice device, ILogger logger)
    {
        Device = device.AddTo(Disposable);

        _endpointVolume = AudioEndpointVolume.FromDevice(Device).AddTo(Disposable);
        _meterInformation = AudioMeterInformation.FromDevice(Device).AddTo(Disposable);
        _accessorManager = new AudioSessionAccessorManager(this, logger);

        Role = new DeviceRoleHolder(this);
    }

    public MMDevice Device { get; }
    public DeviceRoleHolder Role { get; }
    public ReadOnlyReactiveCollection<AudioSessionAccessor> Sessions => _accessorManager.ReadOnlyCollection;
    public string? DeviceId => SafeRead(() => Device.DeviceID, null);
    public string? FriendlyName => SafeRead(() => Device.FriendlyName, null);
    public string? DevicePath => SafeRead(() => Device.DevicePath, null);

    public DeviceStateType DeviceState => SafeRead(
        () => AccessorHelper.DeviceStates[Device.DeviceState],
        DeviceStateType.Unknown);

    public DataFlowType DataFlow => SafeRead(
        () => AccessorHelper.DataFlows[Device.DataFlow],
        DataFlowType.Unknown);

    public int ChannelCount => SafeRead(_endpointVolume.GetChannelCount, 0);
    public float PeakValue => SafeRead(_meterInformation.GetPeakValue, 0.0f);
    public float[]? ChannelsPeakValues => SafeRead(_meterInformation.GetChannelsPeakValues, null);
    public int MeteringChannelCount => SafeRead(_meterInformation.GetMeteringChannelCount, 0);

    public float MasterVolumeLevel
    {
        get => SafeRead(_endpointVolume.GetMasterVolumeLevel, 0.0f);
        set => SafeWrite(v => _endpointVolume.MasterVolumeLevel = v, value);
    }

    public float MasterVolumeLevelScalar
    {
        get => SafeRead(_endpointVolume.GetMasterVolumeLevelScalar, 0.0f);
        set => SafeWrite(v => _endpointVolume.MasterVolumeLevelScalar = v, value);
    }

    public bool IsMuted
    {
        get => SafeRead(_endpointVolume.GetMute, false);
        set => SafeWrite(v => _endpointVolume.IsMuted = v, value);
    }

    public Task OpenSession()
    {
        return _accessorManager.OpenSession();
    }

    public void CloseSession()
    {
        _accessorManager.CloseSession();
    }

    protected override void OnDisposing()
    {
        CloseSession();
        base.OnDisposing();
    }
}