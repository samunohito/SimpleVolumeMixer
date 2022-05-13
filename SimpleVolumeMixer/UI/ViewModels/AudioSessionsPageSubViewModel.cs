using System.Reactive.Linq;
using DisposableComponents;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SimpleVolumeMixer.Core.Helper.Component;
using SimpleVolumeMixer.UI.Models.UseCase;
using SimpleVolumeMixer.UI.ViewModels.Audio;

namespace SimpleVolumeMixer.UI.ViewModels;

public class AudioSessionsPageSubViewModel : DisposableComponent
{
    public AudioSessionsPageSubViewModel(AudioSessionsPageUseCase useCase)
    {
        // 現在選択されているデバイスを監視し、そのデバイスが持っているセッション一覧を参照する.
        // デバイスが変更された場合、変更後のデバイスが持っているセッションを見るようにプロパティの中身を切り替えるようにしておく
        Sessions = useCase.SelectedDevice
            .Select(x => x?.Sessions.ToReadOnlyReactiveCollection(i => new AudioSessionViewModel(i)))
            .ToReadOnlyReactivePropertySlim()
            .AddTo(Disposable);
    }

    /// <summary>
    /// 現在選択されているデバイスのセッションを常に見続けるプロパティ
    /// </summary>
    public IReadOnlyReactiveProperty<ReadOnlyReactiveCollection<AudioSessionViewModel>?> Sessions { get; }
}