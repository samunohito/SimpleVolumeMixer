using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using DisposableComponents;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SimpleVolumeMixer.Core.Models.Domain.CoreAudio;
using SimpleVolumeMixer.UI.Models.UseCase;
using SimpleVolumeMixer.UI.ViewModels.Audio;
using SimpleVolumeMixer.UI.ViewModels.Audio.Event;

namespace SimpleVolumeMixer.UI.ViewModels;

public class AudioDevicesPageSubViewModel : DisposableComponent
{
    private readonly AudioDevicesPageUseCase _useCase;
    
    public AudioDevicesPageSubViewModel(AudioDevicesPageUseCase useCase)
    {
        _useCase = useCase;
        
        // 現在のデバイス一覧
        Devices = useCase.Devices
            .Select(x => x?.ToReadOnlyReactiveCollection(CreateViewModel))
            .ToReadOnlyReactivePropertySlim()
            .AddTo(Disposable);
    }

    /// <summary>
    /// 現在システムに認識されているデバイス一覧
    /// </summary>
    public IReadOnlyReactiveProperty<ReadOnlyObservableCollection<AudioDeviceViewModel>?> Devices { get; }
    
    /// <summary>
    /// ModelからViewModelを作る.
    /// デバイスロールの変更要求や破棄時のイベントを購読するための設定をするためファクトリメソッド化
    /// </summary>
    /// <param name="device"></param>
    /// <returns></returns>
    private AudioDeviceViewModel CreateViewModel(AudioDevice device)
    {
        var ret = new AudioDeviceViewModel(device);
        ret.DeviceChangeRequest += OnDeviceChangeRequest;
        ret.Disposing += OnDisposing;
        return ret;
    }
    
    /// <summary>
    /// 各ViewModelからデバイスロール変更要求が来た時に呼ばれる
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnDeviceChangeRequest(object? sender, DeviceRoleChangeRequestEventArgs e)
    {
        _useCase.SetDefaultDevice(e.Device.Device, e.DataFlow, e.Role);
    }
    
    /// <summary>
    /// ViewModel破棄時イベント.
    /// Devices（ReadOnlyReactiveCollection）から項目が消える際、勝手に呼び出される.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnDisposing(object? sender, EventArgs e)
    {
        if (sender is AudioDeviceViewModel vm)
        {
            vm.DeviceChangeRequest -= OnDeviceChangeRequest;
            vm.Disposing -= OnDisposing;
        }
    }
}