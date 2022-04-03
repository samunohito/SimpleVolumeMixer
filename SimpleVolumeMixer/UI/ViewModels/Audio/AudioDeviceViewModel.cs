using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Windows.Input;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SimpleVolumeMixer.Core.Helper.CoreAudio;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Types;
using SimpleVolumeMixer.Core.Models.Domain.CoreAudio;
using SimpleVolumeMixer.UI.Views.Controls;

namespace SimpleVolumeMixer.UI.ViewModels.Audio;

public class AudioDeviceViewModel : BindableBase, IDisposable, IAudioSessionCard
{
    private readonly AudioDevice _device;
    private readonly CompositeDisposable _disposable;
    private ISoundBarHandler? _soundBarHandler;

    public AudioDeviceViewModel(AudioDevice device)
    {
        _disposable = new CompositeDisposable();
        _device = device;
        _soundBarHandler = null;

        Sessions = device.Sessions
            .ToReadOnlyReactiveCollection(x => new AudioSessionViewModel(x))
            .AddTo(_disposable);
        DeviceId = device.DeviceId.ToReadOnlyReactivePropertySlim().AddTo(_disposable);
        FriendlyName = device.FriendlyName.ToReadOnlyReactivePropertySlim().AddTo(_disposable);
        DevicePath = device.DevicePath.ToReadOnlyReactivePropertySlim().AddTo(_disposable);
        DataFlow = device.DataFlow.ToReadOnlyReactivePropertySlim().AddTo(_disposable);
        DeviceState = device.DeviceState.ToReadOnlyReactivePropertySlim().AddTo(_disposable);
        ChannelCount = device.ChannelCount.ToReadOnlyReactivePropertySlim().AddTo(_disposable);
        PeakValue = device.PeakValue.ToReadOnlyReactivePropertySlim().AddTo(_disposable);
        MeteringChannelCount = device.MeteringChannelCount.ToReadOnlyReactivePropertySlim().AddTo(_disposable);
        MasterVolume = device.MasterVolumeLevelScalar
            .ToReactivePropertyAsSynchronized(
                x => x.Value,
                i => i * 1000.0f,
                i => i / 1000.0f
            )
            .AddTo(_disposable);
        IsMuted = device.IsMuted
            .ToReactivePropertyAsSynchronized(x => x.Value)
            .AddTo(_disposable);

        PeakValue
            .Subscribe(x => _soundBarHandler?.NotifyValue(x))
            .AddTo(_disposable);

        IconSource = new ReactivePropertySlim<ImageSource?>().AddTo(_disposable);
        DisplayName = new ReactivePropertySlim<string?>("Master").AddTo(_disposable);

        PeakBarReadyCommand = new DelegateCommand<SoundBarReadyEventArgs>(OnSoundBarReady);
        MuteStateChangeCommand = new DelegateCommand(OnOnMuteStateChange);
    }

    public ReadOnlyReactiveCollection<AudioSessionViewModel> Sessions { get; }
    public DeviceRole Role => _device.Role;
    public IReadOnlyReactiveProperty<string?> DeviceId { get; }
    public IReadOnlyReactiveProperty<string?> FriendlyName { get; }
    public IReadOnlyReactiveProperty<string?> DevicePath { get; }
    public IReadOnlyReactiveProperty<DeviceStateType> DeviceState { get; }
    public IReadOnlyReactiveProperty<DataFlowType> DataFlow { get; }
    public IReadOnlyReactiveProperty<int> ChannelCount { get; }
    public IReadOnlyReactiveProperty<int> MeteringChannelCount { get; }

    public bool UsePackIcon => true;
    public PackIconKind? PackIconKind => MaterialDesignThemes.Wpf.PackIconKind.SpeakerMultiple;
    public IReadOnlyReactiveProperty<string?> DisplayName { get; }
    public IReadOnlyReactiveProperty<ImageSource?> IconSource { get; }
    public IReadOnlyReactiveProperty<float> PeakValue { get; }
    public IReactiveProperty<float> MasterVolume { get; }
    public IReactiveProperty<bool> IsMuted { get; }
    public ICommand PeakBarReadyCommand { get; }
    public ICommand MuteStateChangeCommand { get; }

    public void Dispose()
    {
        foreach (var session in Sessions.ToList()) session.Dispose();

        _disposable.Dispose();
    }

    private void OnSoundBarReady(SoundBarReadyEventArgs e)
    {
        _soundBarHandler = e.Handler;
    }

    private void OnOnMuteStateChange()
    {
        IsMuted.Value = !IsMuted.Value;
    }

    public void OpenSession()
    {
        _device.OpenSession();
    }

    public void CloseSession()
    {
        _device.CloseSession();
    }
}