using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SimpleVolumeMixer.Core.Helper.CoreAudio;
using SimpleVolumeMixer.Core.Helper.Utils;

namespace SimpleVolumeMixer.Core.Models.Domain.CoreAudio;

public class AudioDevice : NotifyPropertyChangedBase, IDisposable
{
    private readonly CompositeDisposable _disposable;
    private readonly AudioDeviceAccessor _accessor;

    private readonly PropertyHolder<string> _deviceId;
    private readonly PropertyHolder<string> _friendlyName;
    private readonly PropertyHolder<string> _devicePath;
    private readonly PropertyHolder<DeviceStateType> _deviceState;
    private readonly PropertyHolder<DataFlowType> _dataFlow;
    private readonly PropertyHolder<int> _channelCount;
    private readonly PropertyHolder<float> _peekValue;
    private readonly PropertyHolder<int> _meteringChannelCount;

    internal AudioDevice(AudioDeviceAccessor ax)
    {
        _disposable = new CompositeDisposable();
        _accessor = ax.AddTo(_disposable);

        Sessions = ax.Sessions
            .ToReadOnlyReactiveCollection(x => new AudioSession(x))
            .AddTo(_disposable);

        Action<string> act = (propertyName) => OnPropertyChanged(propertyName);

        _deviceId =
            new PropertyHolder<string>(() => ax.DeviceId, nameof(DeviceId), act);
        _friendlyName =
            new PropertyHolder<string>(() => ax.FriendlyName, nameof(FriendlyName), act);
        _devicePath =
            new PropertyHolder<string>(() => ax.DevicePath, nameof(DevicePath), act);
        _deviceState =
            new PropertyHolder<DeviceStateType>(() => ax.DeviceState, nameof(DeviceState), act);
        _dataFlow =
            new PropertyHolder<DataFlowType>(() => ax.DataFlow, nameof(DataFlow), act);
        _channelCount =
            new PropertyHolder<int>(() => ax.ChannelCount, nameof(ChannelCount), act);
        _peekValue =
            new PropertyHolder<float>(() => ax.PeekValue, nameof(PeekValue), act);
        _meteringChannelCount =
            new PropertyHolder<int>(() => ax.MeteringChannelCount, nameof(MeteringChannelCount), act);

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
        };
        foreach (var disposable in disposables)
        {
            disposable.AddTo(_disposable);
        }
    }

    public ReadOnlyReactiveCollection<AudioSession> Sessions { get; }
    public DeviceRole Role => _accessor.Role;
    public IReactiveProperty<string> DeviceId => _deviceId.Holder;
    public IReactiveProperty<string> FriendlyName => _friendlyName.Holder;
    public IReactiveProperty<string> DevicePath => _devicePath.Holder;
    public IReactiveProperty<DeviceStateType> DeviceState => _deviceState.Holder;
    public IReactiveProperty<DataFlowType> DataFlow => _dataFlow.Holder;
    public IReactiveProperty<int> ChannelCount => _channelCount.Holder;
    public IReactiveProperty<float> PeekValue => _peekValue.Holder;
    public IReactiveProperty<int> MeteringChannelCount => _meteringChannelCount.Holder;

    public void Refresh()
    {
        if (_accessor.IsDisposed)
        {
            return;
        }
        
        var monitors = new IPropertyHolder[]
        {
            _deviceId,
            _friendlyName,
            _devicePath,
            _deviceState,
            _dataFlow,
            _channelCount,
            _peekValue,
            _meteringChannelCount,
        };
        foreach (var monitor in monitors)
        {
            monitor.Refresh();
        }

        foreach (var audioSession in Sessions.ToList())
        {
            audioSession.Refresh();
        }
    }

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