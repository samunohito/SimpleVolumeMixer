using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SimpleVolumeMixer.Core.Helper.Component;
using SimpleVolumeMixer.Core.Helper.CoreAudio;

namespace SimpleVolumeMixer.Core.Models.Domain.CoreAudio;

public class DeviceRole : DisposableComponent
{
    public DeviceRole(AudioDevice device, DeviceRoleHolder holder)
    {
        Device = device;
        Multimedia = holder.ToReactivePropertySlimAsSynchronized(x => x.Multimedia).AddTo(Disposable);
        Communications = holder.ToReactivePropertySlimAsSynchronized(x => x.Communications).AddTo(Disposable);
    }

    public AudioDevice Device { get; }
    public IReadOnlyReactiveProperty<bool> Multimedia { get; }
    public IReadOnlyReactiveProperty<bool> Communications { get; }
}