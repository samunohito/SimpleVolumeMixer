using System;
using System.Reactive.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SimpleVolumeMixer.Core.Contracts.Services;
using SimpleVolumeMixer.Core.Helper.Component;
using SimpleVolumeMixer.Core.Models.Domain.CoreAudio;

namespace SimpleVolumeMixer.UI.Models.UseCase;

/// <summary>
/// セッション一覧ページ向けのロジックを定義する
/// </summary>
public class AudioSessionsPageUseCase : DisposableComponent
{
    public AudioSessionsPageUseCase(ICoreAudioService coreAudioService)
    {
        // AudioDeviceの破棄はより上位で行いたいため、ここではReadOnlyReactiveCollectionの破棄処理呼び出しを抑止する設定にする
        Devices = coreAudioService.Devices
            .ToReadOnlyReactiveCollection(disposeElement: false)
            .AddTo(Disposable);
        SelectedDevice = new ReactivePropertySlim<AudioDevice?>().AddTo(Disposable);

        // デバイスの選択変更時にセッションの開け締めを行うため通知を購読する.
        // 前回値が出来るまで待ち合わせるので、下記の購読開始処理を実行しただけでは動作しない.
        SelectedDevice
            .Zip(SelectedDevice.Skip(1), (x, y) => new { OldValue = x, NewValue = y })
            .Subscribe(x => OnSelectedDeviceChanged(x.OldValue, x.NewValue))
            .AddTo(Disposable);

        // マルチメディアロールのデバイスを初期表示にしたい.
        // 起動時点でのマルチメディアロールデバイスさえ取れれば良いので、特別なことはせずにValueで初期値を取る.
        // これにより初期デバイスが取れた場合は、SelectedDeviceの値がnullから変わるのでセッションの開始処理も一緒に呼び出される.
        SelectedDevice.Value = coreAudioService.MultimediaRoleDevice.Value;
    }

    /// <summary>
    /// 参照可能なデバイスの一覧
    /// </summary>
    public ReadOnlyReactiveCollection<AudioDevice> Devices { get; }

    /// <summary>
    /// セッションの列挙対象として選択されているデバイス
    /// </summary>
    public IReactiveProperty<AudioDevice?> SelectedDevice { get; }

    /// <summary>
    /// 選択されていたデバイスが変更された際の処理
    /// </summary>
    /// <param name="oldDevice">選択されていたデバイス</param>
    /// <param name="newDevice">新たに選択されたデバイス</param>
    private async void OnSelectedDeviceChanged(AudioDevice? oldDevice, AudioDevice? newDevice)
    {
        if (Equals(oldDevice, newDevice))
        {
            return;
        }

        oldDevice?.CloseSession();
        if (newDevice != null)
        {
            await newDevice.OpenSession();
        }
    }
}