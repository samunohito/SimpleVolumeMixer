using System;
using System.Reactive.Linq;
using DisposableComponents;
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

        // マルチメディアロールのデバイスを初期表示にしたい.
        SelectedDevice = coreAudioService.MultimediaRoleDevice
            .ObserveOnUIDispatcher()
            .ToReactiveProperty()
            .AddTo(Disposable);

        // デバイスの選択変更時にセッションの開け締めを行うため通知を購読する.
        // 前回値が出来るまで待ち合わせるので、下記の購読開始処理を実行しただけでは動作しない.
        SelectedDevice
            .Zip(SelectedDevice.Skip(1), (x, y) => new { OldValue = x, NewValue = y })
            .ObserveOnUIDispatcher()
            .Subscribe(x => OnSelectedDeviceChanged(x.OldValue, x.NewValue))
            .AddTo(Disposable);

        // 初期表示のため、明示的にセッション開け締め処理を呼び出す
        OnSelectedDeviceChanged(null, SelectedDevice.Value);
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