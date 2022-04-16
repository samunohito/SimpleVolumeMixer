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

/// <summary>
/// <see cref="AudioDeviceAccessor"/>を監視し、値の変更があったら<see cref="ReactiveProperty"/>経由で通知及び最新値の配信を行う。
/// <see cref="AudioDeviceAccessor"/>の存在が前提となるため、<see cref="DisposableComponent"/>の仕組みを利用して破棄されたことを検知し、
/// それに合わせて監視の終了やこのクラスの破棄を行う仕組みも実装する。
/// </summary>
public class AudioDevice : DisposableComponent
{
    private readonly PollingMonitor<string?> _deviceId;
    private readonly PollingMonitor<string?> _friendlyName;
    private readonly PollingMonitor<string?> _devicePath;
    private readonly PollingMonitor<DeviceStateType> _deviceState;
    private readonly PollingMonitor<DataFlowType> _dataFlow;
    private readonly PollingMonitor<int> _channelCount;
    private readonly PollingMonitor<float> _peakValue;
    private readonly PollingMonitor<int> _meteringChannelCount;
    private readonly PollingMonitor<float> _masterVolumeLevel;
    private readonly PollingMonitor<float> _masterVolumeLevelScalar;
    private readonly PollingMonitor<bool> _isMuted;

    internal AudioDevice(AudioDeviceAccessor ax)
    {
        Device = ax.AddTo(Disposable);

        Sessions = ax.Sessions
            .ToReadOnlyReactiveCollection(x => new AudioSession(x))
            .AddTo(Disposable);

        _deviceId = new PollingMonitor<string?>(
            PollingMonitorIntervalType.Manual,
            () => ax.DeviceId
        );
        _friendlyName = new PollingMonitor<string?>(
            PollingMonitorIntervalType.Manual,
            () => ax.FriendlyName
        );
        _devicePath = new PollingMonitor<string?>(
            PollingMonitorIntervalType.Manual,
            () => ax.DevicePath
        );
        _deviceState = new PollingMonitor<DeviceStateType>(
            PollingMonitorIntervalType.Normal,
            () => ax.DeviceState
        );
        _dataFlow = new PollingMonitor<DataFlowType>(
            PollingMonitorIntervalType.Manual,
            () => ax.DataFlow
        );
        _channelCount = new PollingMonitor<int>(
            PollingMonitorIntervalType.Manual,
            () => ax.ChannelCount,
            comparer: PollingMonitor.IntComparer
        );
        _peakValue = new PollingMonitor<float>(
            PollingMonitorIntervalType.High,
            () => ax.PeakValue,
            comparer: PollingMonitor.FloatComparer
        );
        _meteringChannelCount = new PollingMonitor<int>(
            PollingMonitorIntervalType.Low,
            () => ax.MeteringChannelCount
        );
        _masterVolumeLevel = new PollingMonitor<float>(
            PollingMonitorIntervalType.Manual,
            () => ax.MasterVolumeLevel,
            (x) => ax.MasterVolumeLevel = x,
            PollingMonitor.FloatComparer
        );
        _masterVolumeLevelScalar = new PollingMonitor<float>(
            PollingMonitorIntervalType.Normal,
            () => ax.MasterVolumeLevelScalar,
            (x) => ax.MasterVolumeLevelScalar = x,
            PollingMonitor.FloatComparer
        );
        _isMuted = new PollingMonitor<bool>(
            PollingMonitorIntervalType.Normal,
            () => ax.IsMuted,
            (x) => ax.IsMuted = x,
            PollingMonitor.BoolComparer
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