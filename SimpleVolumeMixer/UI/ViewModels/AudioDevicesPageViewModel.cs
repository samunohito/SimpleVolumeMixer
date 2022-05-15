using System;
using System.Reactive.Disposables;
using System.Windows.Controls;
using System.Windows.Input;
using DisposableComponents;
using Prism.Commands;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SimpleVolumeMixer.Core.Helper.Component;
using SimpleVolumeMixer.Core.Models.Domain.CoreAudio;
using SimpleVolumeMixer.UI.Constants;
using SimpleVolumeMixer.UI.Models.UseCase;
using SimpleVolumeMixer.UI.ViewModels.Audio;
using SimpleVolumeMixer.UI.ViewModels.Audio.Event;

namespace SimpleVolumeMixer.UI.ViewModels;

/// <summary>
/// デバイス一覧ページ向け.
/// ViewModelへの変換処理、Viewからのイベント処理、子ページの制御以外はUseCase側に実装したい.
/// </summary>
public class AudioDevicesPageViewModel : DisposableComponent, INavigationAware
{
    private readonly AudioDevicesPageUseCase _useCase;
    
    /// <summary>
    /// OnNavigatedTo ～ OnNavigatedFromの間だけ使うIDisposable向けのやつ
    /// </summary>
    private CompositeDisposable? _disposableNavigationAware = null;

    public AudioDevicesPageViewModel(IRegionManager regionManager, AudioDevicesPageUseCase useCase)
    {
        RegionManager = regionManager;
        _useCase = useCase;

        Orientation = new ReactivePropertySlim<Orientation>(System.Windows.Controls.Orientation.Vertical);

        OnLoadedCommand = new DelegateCommand(OnLoaded);
        OnOrientationChangeCommand = new DelegateCommand(OnOrientationChange);
    }
    
    /// <summary>
    /// DIコンテナ経由で取得したRegionManager.
    /// 子ページ用の領域登録を行うのにあたりWPF側のコードビハインドにわたす必要があるので外出ししている.
    /// </summary>
    public IRegionManager RegionManager { get; }

    /// <summary>
    /// 子ページの種類を決定するプロパティ.
    /// </summary>
    public IReactiveProperty<Orientation> Orientation { get; }
    
    /// <summary>
    /// ページロード時イベント
    /// </summary>
    public ICommand OnLoadedCommand { get; }

    /// <summary>
    /// 子ページの切り替えボタンが押されたときのイベント処理
    /// </summary>
    public ICommand OnOrientationChangeCommand { get; }
    
    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        _disposableNavigationAware?.Dispose();
        _disposableNavigationAware = new CompositeDisposable();
    }

    public bool IsNavigationTarget(NavigationContext navigationContext)
    {
        return true;
    }

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
        _disposableNavigationAware?.Dispose();
        _disposableNavigationAware = null;
    }
    
    /// <summary>
    /// ページロード時イベント
    /// </summary>
    private void OnLoaded()
    {
        UpdateAudioSessionView();
        Orientation
            .Subscribe(x => UpdateAudioSessionView())
            .AddTo(_disposableNavigationAware);
    }

    /// <summary>
    /// 子ページの切り替えボタンが押されたときのイベント処理
    /// </summary>
    private void OnOrientationChange()
    {
        Orientation.Value = Orientation.Value == System.Windows.Controls.Orientation.Horizontal
            ? System.Windows.Controls.Orientation.Vertical
            : System.Windows.Controls.Orientation.Horizontal;
    }
    
    /// <summary>
    /// Orientationにあわせて表示する子ページを選択する.
    /// </summary>
    private void UpdateAudioSessionView()
    {
        var key = Orientation.Value == System.Windows.Controls.Orientation.Horizontal
            ? PageKeys.AudioDevicesSubHorizontal
            : PageKeys.AudioDevicesSubVertical;
        RegionManager.RequestNavigate(Regions.AudioDeviceSubRegion, key);
    }

    protected override void OnDisposing()
    {
        _disposableNavigationAware?.Dispose();
        base.OnDisposing();
    }
}