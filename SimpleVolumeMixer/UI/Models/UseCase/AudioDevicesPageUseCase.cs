using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using DisposableComponents;
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
        
        SelectedDataFlowType = new ReactivePropertySlim<DataFlowType>(Core.Helper.CoreAudio.Types.DataFlowType.Render);
        Devices = SelectedDataFlowType
            .Select(x => x == DataFlowType.Render
                ? coreAudioService.RenderDevices
                : coreAudioService.CaptureDevices)
            .ToReadOnlyReactivePropertySlim()
            .AddTo(Disposable);
    }

    /// <summary>
    /// 一覧に表示するデバイスの種別.
    /// </summary>
    public IReactiveProperty<DataFlowType> SelectedDataFlowType { get; }

    /// <summary>
    /// 使用可能なデバイス一覧.
    /// <see cref="SelectedDataFlowType"/>にて選択されている種別のデバイスが表示される.
    /// </summary>
    public IReadOnlyReactiveProperty<ReadOnlyObservableCollection<AudioDevice>?> Devices { get; }

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