using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using CSCore.CoreAudioAPI;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SimpleVolumeMixer.Core.Models.Domain.CoreAudio;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio;

public class AudioDeviceAccessor : IDisposable
{
    private static readonly IDictionary<DataFlow, DataFlowType> DataFlows =
        new Dictionary<DataFlow, DataFlowType>()
        {
            { CSCore.CoreAudioAPI.DataFlow.Render, DataFlowType.Render },
            { CSCore.CoreAudioAPI.DataFlow.Capture, DataFlowType.Capture },
            { CSCore.CoreAudioAPI.DataFlow.All, DataFlowType.All },
        };

    private static readonly IDictionary<DeviceState, DeviceStateType> DeviceStates =
        new Dictionary<DeviceState, DeviceStateType>()
        {
            { CSCore.CoreAudioAPI.DeviceState.Active, DeviceStateType.Active },
            { CSCore.CoreAudioAPI.DeviceState.Disabled, DeviceStateType.Disabled },
            { CSCore.CoreAudioAPI.DeviceState.NotPresent, DeviceStateType.NotPresent },
            { CSCore.CoreAudioAPI.DeviceState.UnPlugged, DeviceStateType.UnPlugged },
            { CSCore.CoreAudioAPI.DeviceState.All, DeviceStateType.All },
        };

    private readonly CompositeDisposable _disposable;
    private readonly AudioEndpointVolume _endpointVolume;
    private readonly AudioMeterInformation _meterInformation;
    private readonly AudioSessionManagerAccessor _sessionManager;

    internal AudioDeviceAccessor(MMDevice device)
    {
        _disposable = new CompositeDisposable();
        Device = device.AddTo(_disposable);

        _endpointVolume = AudioEndpointVolume.FromDevice(Device).AddTo(_disposable);
        _meterInformation = AudioMeterInformation.FromDevice(Device).AddTo(_disposable);
        _sessionManager = new AudioSessionManagerAccessor(this);
    }

    internal MMDevice Device { get; }
    public ReadOnlyReactiveCollection<AudioSessionAccessor> Sessions => _sessionManager.Sessions;
    public string DeviceId => Device.DeviceID;
    public string FriendlyName => Device.FriendlyName;
    public string DevicePath => Device.DevicePath;
    public DeviceStateType DeviceState => DeviceStates[Device.DeviceState];
    public DataFlowType DataFlow => DataFlows[Device.DataFlow];
    public int ChannelCount => _endpointVolume.ChannelCount;
    public float PeekValue => _meterInformation.PeakValue;
    public int MeteringChannelCount => _meterInformation.MeteringChannelCount;

    public float MasterVolumeLevel
    {
        get => _endpointVolume.MasterVolumeLevel;
        set => _endpointVolume.MasterVolumeLevel = value;
    }

    public float MasterVolumeLevelScalar
    {
        get => _endpointVolume.MasterVolumeLevelScalar;
        set => _endpointVolume.MasterVolumeLevelScalar = value;
    }

    public bool IsMuted
    {
        get => _endpointVolume.IsMuted;
        set => _endpointVolume.IsMuted = value;
    }

    public bool IsDisposed => _disposable.IsDisposed;

    public void OpenSession()
    {
        _sessionManager.OpenSessionManager();
    }

    public void CloseSession()
    {
        _sessionManager.CloseSessionManager();
    }

    public void Dispose()
    {
        CloseSession();
        _disposable.Dispose();
    }
}