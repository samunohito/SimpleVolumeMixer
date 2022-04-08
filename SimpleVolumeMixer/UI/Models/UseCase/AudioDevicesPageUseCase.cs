using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SimpleVolumeMixer.Core.Contracts.Services;
using SimpleVolumeMixer.Core.Helper.Component;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Types;
using SimpleVolumeMixer.Core.Models.Domain.CoreAudio;

namespace SimpleVolumeMixer.UI.Models.UseCase;

/// <summary>
/// デバイス一覧ページ向けのロジックを定義する.
/// 現状特に必要なクラスではないが、将来的になにかするときのために用意しておく
/// </summary>
public class AudioDevicesPageUseCase : DisposableComponent
{
    private readonly ICoreAudioService _coreAudioService;

    public AudioDevicesPageUseCase(ICoreAudioService coreAudioService)
    {
        _coreAudioService = coreAudioService;

        // AudioDeviceの破棄はより上位で行いたいため、ここではReadOnlyReactiveCollectionの破棄処理呼び出しを抑止する設定にする
        Devices = coreAudioService.Devices
            .ToReadOnlyReactiveCollection(disposeElement: false)
            .AddTo(Disposable);
    }

    /// <summary>
    /// 使用可能なデバイス一覧
    /// </summary>
    public ReadOnlyReactiveCollection<AudioDevice> Devices { get; }

    /// <summary>
    /// デバイスロールの変更要求（委譲処理）
    /// </summary>
    /// <param name="device"></param>
    /// <param name="dataFlowType"></param>
    /// <param name="roleType"></param>
    public void SetDefaultDevice(AudioDevice device, DataFlowType dataFlowType, RoleType roleType)
    {
        _coreAudioService.SetDefaultDevice(device, dataFlowType, roleType);
    }
}