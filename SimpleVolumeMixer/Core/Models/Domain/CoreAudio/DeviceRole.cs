using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SimpleVolumeMixer.Core.Helper.CoreAudio;

namespace SimpleVolumeMixer.Core.Models.Domain.CoreAudio;

public class DeviceRole : IDisposable
{
    private readonly CompositeDisposable _disposable;

    public DeviceRole(AudioDevice device, DeviceRoleHolder holder)
    {
        _disposable = new CompositeDisposable();

        Device = device;
        Multimedia = holder.ToReactivePropertySlimAsSynchronized(x => x.Multimedia).AddTo(_disposable);
        Communications = holder.ToReactivePropertySlimAsSynchronized(x => x.Communications).AddTo(_disposable);
    }

    public AudioDevice Device { get; }
    public IReadOnlyReactiveProperty<bool> Multimedia { get; }
    public IReadOnlyReactiveProperty<bool> Communications { get; }

    public void Dispose()
    {
        _disposable.Dispose();
    }
}