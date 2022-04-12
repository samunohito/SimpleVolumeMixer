using System;
using System.Reactive.Disposables;
using System.Windows.Controls;
using System.Windows.Input;
using Prism.Commands;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SimpleVolumeMixer.Core.Helper.Component;
using SimpleVolumeMixer.Core.Models.Domain.CoreAudio;
using SimpleVolumeMixer.UI.Constants;
using SimpleVolumeMixer.UI.Models.UseCase;
using SimpleVolumeMixer.UI.ViewModels.Audio;

namespace SimpleVolumeMixer.UI.ViewModels;

/// <summary>
/// セッション一覧親ページ向け.
/// ViewModelへの変換処理、Viewからのイベント処理、子ページの制御以外はUseCase側に実装したい.
/// </summary>
public class AudioSessionsPageViewModel : DisposableComponent, INavigationAware
{
    /// <summary>
    /// OnNavigatedTo ～ OnNavigatedFromの間だけ使うIDisposable向けのやつ
    /// </summary>
    private CompositeDisposable? _disposableNavigationAware = null;

    public AudioSessionsPageViewModel(IRegionManager regionManager, AudioSessionsPageUseCase useCase)
    {
        RegionManager = regionManager;

        // アプリの構造上このViewModelが破棄されることは無いと思われるが、
        // UseCaseの寿命はこのViewModelと揃えたい（子ページ側との整合性を保ちたい）ので、
        // このViewModelがDispose()されたら一緒にDispose()されるようにする
        useCase.AddTo(Disposable);

        Orientation = new ReactivePropertySlim<Orientation>(System.Windows.Controls.Orientation.Vertical);
        SelectedDeviceForPanel = new ReactiveCollection<AudioDeviceViewModel?>();
        OnLoadedCommand = new DelegateCommand(OnLoaded);
        OnOrientationChangeCommand = new DelegateCommand(OnOrientationChange);

        // KeyValueInstanceManagerを使い、AudioDeviceとそのViewModelが必ず対応するようにする.
        // あるAudioDeviceに対し異なるViewModelのインスタンスがあると、xaml側のSelectedItemが動作しないため.
        var instanceManager = new KeyValueInstanceManager<AudioDevice, AudioDeviceViewModel>(
            x => new AudioDeviceViewModel(x)
        );
        Devices = useCase.Devices
            .ToReadOnlyReactiveCollection(x => instanceManager.Obtain(x))
            .AddTo(Disposable);
        SelectedDevice = useCase.SelectedDevice
            .ToReactivePropertySlimAsSynchronized(
                (x) => x.Value,
                x => x != null ? instanceManager.Obtain(x) : null,
                x => x?.Device)
            .AddTo(Disposable);

        // SelectedDeviceForPanelの中身とSelectedDeviceを同期させるための処理.
        SelectedDevice
            .ObserveOnUIDispatcher()
            .Subscribe(x =>
            {
                SelectedDeviceForPanel.Clear();
                if (x != null)
                {
                    SelectedDeviceForPanel.Add(x);
                }
            })
            .AddTo(Disposable);
    }

    /// <summary>
    /// DIコンテナ経由で取得したRegionManager.
    /// 子ページ用の領域登録を行うのにあたりWPF側のコードビハインドにわたす必要があるので外出ししている.
    /// </summary>
    public IRegionManager RegionManager { get; }

    /// <summary>
    /// 選択可能なオーディオデバイス一覧
    /// </summary>
    public ReadOnlyReactiveCollection<AudioDeviceViewModel> Devices { get; }

    /// <summary>
    /// 現在、メインの表示対象として設定されているデバイス.
    /// この選択値はUseCaseを通じて子ページ側のセッション一覧にも影響する.
    /// </summary>
    public IReactiveProperty<AudioDeviceViewModel?> SelectedDevice { get; }

    /// <summary>
    /// 現在、メインの表示対象として設定されているデバイス.
    /// 内容はSelectedDeviceの値が1つだけで、複数入ることはない.
    /// あくまでxamlとデータバインディングの都合上用意されているもの.
    /// </summary>
    public ReactiveCollection<AudioDeviceViewModel?> SelectedDeviceForPanel { get; }

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
            ? PageKeys.AudioSessionsSubHorizontal
            : PageKeys.AudioSessionsSubVertical;
        RegionManager.RequestNavigate(Regions.AudioSessionSubRegion, key);
    }

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

    protected override void OnDisposing()
    {
        _disposableNavigationAware?.Dispose();

        base.OnDisposing();
    }
}