using System;
using System.Reactive.Disposables;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SimpleVolumeMixer.Core.Helper.CoreAudio;
using SimpleVolumeMixer.Core.Helper.Utils;

namespace SimpleVolumeMixer.Core.Models.Domain.CoreAudio;

public class AudioDevice : NotifyPropertyChangedBase, IDisposable
{
    private readonly CompositeDisposable _disposable;
    private readonly AudioDeviceAccessor _accessor;

    private readonly PropertyMonitor<string> _deviceId;
    private readonly PropertyMonitor<string> _friendlyName;
    private readonly PropertyMonitor<string> _devicePath;
    private readonly PropertyMonitor<DeviceStateType> _deviceState;
    private readonly PropertyMonitor<DataFlowType> _dataFlow;
    private readonly PropertyMonitor<int> _channelCount;
    private readonly PropertyMonitor<float> _peekValue;
    private readonly PropertyMonitor<int> _meteringChannelCount;

    internal AudioDevice(AudioDeviceAccessor ax)
    {
        _disposable = new CompositeDisposable();
        _accessor = ax.AddTo(_disposable);

        Sessions = ax.Sessions
            .ToReadOnlyReactiveCollection(x => new AudioSession(x))
            .AddTo(_disposable);

        _deviceId = new PropertyMonitor<string>(
            PropertyMonitorIntervalType.Manual,
            () => ax.DeviceId
        );
        _friendlyName = new PropertyMonitor<string>(
            PropertyMonitorIntervalType.Manual,
            () => ax.FriendlyName
        );
        _devicePath = new PropertyMonitor<string>(
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
        _peekValue = new PropertyMonitor<float>(
            PropertyMonitorIntervalType.High,
            () => ax.PeekValue,
            comparer: PropertyMonitor.FloatComparer
        );
        _meteringChannelCount = new PropertyMonitor<int>(
            PropertyMonitorIntervalType.Low,
            () => ax.MeteringChannelCount
        );

        DeviceId = _deviceId.ToReactivePropertySlimAsSynchronized(x => x.Value);
        FriendlyName = _friendlyName.ToReactivePropertySlimAsSynchronized(x => x.Value);
        DevicePath = _devicePath.ToReactivePropertySlimAsSynchronized(x => x.Value);
        DeviceState = _deviceState.ToReactivePropertySlimAsSynchronized(x => x.Value);
        DataFlow = _dataFlow.ToReactivePropertySlimAsSynchronized(x => x.Value);
        ChannelCount = _channelCount.ToReactivePropertySlimAsSynchronized(x => x.Value);
        PeekValue = _peekValue.ToReactivePropertySlimAsSynchronized(x => x.Value);
        MeteringChannelCount = _meteringChannelCount.ToReactivePropertySlimAsSynchronized(x => x.Value);

        var disposables = new IDisposable[]
        {
            _deviceId,
            _friendlyName,
            _devicePath,
            _deviceState,
            _dataFlow,
            _channelCount,
            _peekValue,
            _meteringChannelCount,
            DeviceId,
            FriendlyName,
            DevicePath,
            DeviceState,
            DataFlow,
            ChannelCount,
            PeekValue,
            MeteringChannelCount,
        };
        foreach (var disposable in disposables)
        {
            disposable.AddTo(_disposable);
        }
    }

    public ReadOnlyReactiveCollection<AudioSession> Sessions { get; }
    public DeviceRole Role => _accessor.Role;
    public IReadOnlyReactiveProperty<string> DeviceId { get; }
    public IReadOnlyReactiveProperty<string> FriendlyName { get; }
    public IReadOnlyReactiveProperty<string> DevicePath { get; }
    public IReadOnlyReactiveProperty<DeviceStateType> DeviceState { get; }
    public IReadOnlyReactiveProperty<DataFlowType> DataFlow { get; }
    public IReadOnlyReactiveProperty<int> ChannelCount { get; }
    public IReadOnlyReactiveProperty<float> PeekValue { get; }
    public IReadOnlyReactiveProperty<int> MeteringChannelCount { get; }

    public void OpenSession()
    {
        _accessor.OpenSession();
    }

    public void CloseSession()
    {
        _accessor.CloseSession();
    }

    public void Dispose()
    {
        _disposable.Dispose();
    }
}