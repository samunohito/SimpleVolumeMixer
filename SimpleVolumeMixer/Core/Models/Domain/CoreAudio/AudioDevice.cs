using System;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SimpleVolumeMixer.Core.Helper.Component;
using SimpleVolumeMixer.Core.Helper.Component.Types;
using SimpleVolumeMixer.Core.Helper.CoreAudio;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Types;

namespace SimpleVolumeMixer.Core.Models.Domain.CoreAudio;

public class AudioDevice : DisposableComponent
{
    private readonly PropertyMonitor<string?> _deviceId;
    private readonly PropertyMonitor<string?> _friendlyName;
    private readonly PropertyMonitor<string?> _devicePath;
    private readonly PropertyMonitor<DeviceStateType> _deviceState;
    private readonly PropertyMonitor<DataFlowType> _dataFlow;
    private readonly PropertyMonitor<int> _channelCount;
    private readonly PropertyMonitor<float> _peakValue;
    private readonly PropertyMonitor<int> _meteringChannelCount;
    private readonly PropertyMonitor<float> _masterVolumeLevel;
    private readonly PropertyMonitor<float> _masterVolumeLevelScalar;
    private readonly PropertyMonitor<bool> _isMuted;

    internal AudioDevice(AudioDeviceAccessor ax)
    {
        Device = ax.AddTo(Disposable);

        Sessions = ax.Sessions
            .ToReadOnlyReactiveCollection(x => new AudioSession(x))
            .AddTo(Disposable);

        _deviceId = new PropertyMonitor<string?>(
            PropertyMonitorIntervalType.Manual,
            () => ax.DeviceId
        );
        _friendlyName = new PropertyMonitor<string?>(
            PropertyMonitorIntervalType.Manual,
            () => ax.FriendlyName
        );
        _devicePath = new PropertyMonitor<string?>(
            PropertyMonitorIntervalType.Manual,
            () => ax.DevicePath
        );
        _deviceState = new PropertyMonitor<DeviceStateType>(
            PropertyMonitorIntervalType.Normal,
            () => ax.DeviceState
        );
        _dataFlow = new PropertyMonitor<DataFlowType>(
            PropertyMonitorIntervalType.Manual,
            () => ax.DataFlow
        );
        _channelCount = new PropertyMonitor<int>(
            PropertyMonitorIntervalType.Manual,
            () => ax.ChannelCount,
            comparer: PropertyMonitor.IntComparer
        );
        _peakValue = new PropertyMonitor<float>(
            PropertyMonitorIntervalType.High,
            () => ax.PeakValue,
            comparer: PropertyMonitor.FloatComparer
        );
        _meteringChannelCount = new PropertyMonitor<int>(
            PropertyMonitorIntervalType.Low,
            () => ax.MeteringChannelCount
        );
        _masterVolumeLevel = new PropertyMonitor<float>(
            PropertyMonitorIntervalType.Manual,
            () => ax.MasterVolumeLevel,
            (x) => ax.MasterVolumeLevel = x,
            PropertyMonitor.FloatComparer
        );
        _masterVolumeLevelScalar = new PropertyMonitor<float>(
            PropertyMonitorIntervalType.Normal,
            () => ax.MasterVolumeLevelScalar,
            (x) => ax.MasterVolumeLevelScalar = x,
            PropertyMonitor.FloatComparer
        );
        _isMuted = new PropertyMonitor<bool>(
            PropertyMonitorIntervalType.Normal,
            () => ax.IsMuted,
            (x) => ax.IsMuted = x,
            PropertyMonitor.BoolComparer
        );

        Role = new DeviceRole(this, ax.Role).AddTo(Disposable);
        DeviceId = _deviceId.ToReactivePropertySlimAsSynchronized(x => x.Value);
        FriendlyName = _friendlyName.ToReactivePropertySlimAsSynchronized(x => x.Value);
        DevicePath = _devicePath.ToReactivePropertySlimAsSynchronized(x => x.Value);
        DeviceState = _deviceState.ToReactivePropertySlimAsSynchronized(x => x.Value);
        DataFlow = _dataFlow.ToReactivePropertySlimAsSynchronized(x => x.Value);
        ChannelCount = _channelCount.ToReactivePropertySlimAsSynchronized(x => x.Value);
        PeakValue = _peakValue.ToReactivePropertySlimAsSynchronized(x => x.Value);
        MeteringChannelCount = _meteringChannelCount.ToReactivePropertySlimAsSynchronized(x => x.Value);
        MasterVolumeLevel = _masterVolumeLevel.ToReactivePropertySlimAsSynchronized(x => x.Value);
        MasterVolumeLevelScalar = _masterVolumeLevelScalar.ToReactivePropertySlimAsSynchronized(x => x.Value);
        IsMuted = _isMuted.ToReactivePropertySlimAsSynchronized(x => x.Value);

        var disposables = new IDisposable[]
        {
            _deviceId,
            _friendlyName,
            _devicePath,
            _deviceState,
            _dataFlow,
            _channelCount,
            _peakValue,
            _meteringChannelCount,
            _masterVolumeLevel,
            _masterVolumeLevelScalar,
            _isMuted,
            DeviceId,
            FriendlyName,
            DevicePath,
            DeviceState,
            DataFlow,
            ChannelCount,
            PeakValue,
            MeteringChannelCount,
            MasterVolumeLevel,
            MasterVolumeLevelScalar,
            IsMuted
        };
        foreach (var disposable in disposables)
        {
            disposable.AddTo(Disposable);
        }

        ax.Disposed += DeviceOnDisposed;
        if (ax.IsDisposed)
        {
            // 無いだろうけど、初期化中にAccessorが破棄された時のために
            Dispose();
        }
    }

    private void DeviceOnDisposed(object? sender, EventArgs e)
    {
        Dispose();
    }

    public AudioDeviceAccessor Device { get; }

    public ReadOnlyReactiveCollection<AudioSession> Sessions { get; }
    public DeviceRole Role { get; }
    public IReadOnlyReactiveProperty<string?> DeviceId { get; }
    public IReadOnlyReactiveProperty<string?> FriendlyName { get; }
    public IReadOnlyReactiveProperty<string?> DevicePath { get; }
    public IReadOnlyReactiveProperty<DeviceStateType> DeviceState { get; }
    public IReadOnlyReactiveProperty<DataFlowType> DataFlow { get; }
    public IReadOnlyReactiveProperty<int> ChannelCount { get; }
    public IReadOnlyReactiveProperty<float> PeakValue { get; }
    public IReadOnlyReactiveProperty<int> MeteringChannelCount { get; }
    public IReactiveProperty<float> MasterVolumeLevel { get; }
    public IReactiveProperty<float> MasterVolumeLevelScalar { get; }
    public IReactiveProperty<bool> IsMuted { get; }

    public Task OpenSession()
    {
        return Device.OpenSession();
    }

    public void CloseSession()
    {
        Device.CloseSession();
    }

    protected override void OnDisposing()
    {
        CloseSession();
        base.OnDisposing();
    }
}