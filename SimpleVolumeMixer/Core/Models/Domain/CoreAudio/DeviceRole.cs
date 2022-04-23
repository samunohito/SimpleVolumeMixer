using DisposableComponents;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SimpleVolumeMixer.Core.Helper.Component;
using SimpleVolumeMixer.Core.Helper.CoreAudio;

namespace SimpleVolumeMixer.Core.Models.Domain.CoreAudio;

/// <summary>
/// <see cref="AudioDeviceRole"/>を監視し、値の変更があったら<see cref="ReactiveProperty"/>経由で通知及び最新値の配信を行う。
/// </summary>
public class DeviceRole : DisposableComponent
{
    public DeviceRole(AudioDevice device, AudioDeviceRole holder)
    {
        Device = device;
        Multimedia = holder.ToReactivePropertySlimAsSynchronized(x => x.Multimedia).AddTo(Disposable);
        Communications = holder.ToReactivePropertySlimAsSynchronized(x => x.Communications).AddTo(Disposable);
    }

    public AudioDevice Device { get; }
    public IReadOnlyReactiveProperty<bool> Multimedia { get; }
    public IReadOnlyReactiveProperty<bool> Communications { get; }
}