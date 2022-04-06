using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SimpleVolumeMixer.Core.Contracts.Services;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Types;
using SimpleVolumeMixer.Core.Models.Domain.CoreAudio;
using SimpleVolumeMixer.UI.Views.Controls;

namespace SimpleVolumeMixer.UI.ViewModels.Audio;

public class AudioDeviceViewModel : BindableBase, IDisposable, IAudioSessionCard
{
    private readonly ICoreAudioService _coreAudioService;
    private readonly CompositeDisposable _disposable;
    private ISoundBarHandler? _soundBarHandler;

    public AudioDeviceViewModel(AudioDevice device, ICoreAudioService coreAudioService)
    {
        _disposable = new CompositeDisposable();
        _soundBarHandler = null;
        _coreAudioService = coreAudioService;
        Device = device;

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

        PeakBarReadyCommand = new DelegateCommand<SoundBarReadyEventArgs>(OnSoundBarReady);
        MuteStateChangeCommand = new DelegateCommand(OnMuteStateChange);
        CommunicationRoleApplyCommand = new DelegateCommand(OnCommunicationRoleApply);
        MultimediaRoleApplyCommand = new DelegateCommand(OnMultimediaRoleApply);
        
        // for IAudioSessionCard
        IconSource = new ReactivePropertySlim<ImageSource?>().AddTo(_disposable);
        DisplayName = new ReactivePropertySlim<string?>("Master").AddTo(_disposable);
    }

    public AudioDevice Device { get; }
    public ReadOnlyReactiveCollection<AudioSessionViewModel> Sessions { get; }
    public DeviceRole Role => Device.Role;
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
    public ICommand CommunicationRoleApplyCommand { get; }
    public ICommand MultimediaRoleApplyCommand { get; }

    public void Dispose()
    {
        foreach (var session in Sessions.ToList())
        {
            session.Dispose();
        }

        _disposable.Dispose();
    }

    private void OnSoundBarReady(SoundBarReadyEventArgs e)
    {
        _soundBarHandler = e.Handler;
    }

    private void OnMuteStateChange()
    {
        IsMuted.Value = !IsMuted.Value;
    }

    private void OnCommunicationRoleApply()
    {
        _coreAudioService.SetDefaultDevice(Device, DataFlowType.Render, RoleType.Communications);
    }

    private void OnMultimediaRoleApply()
    {
        _coreAudioService.SetDefaultDevice(Device, DataFlowType.Render, RoleType.Multimedia);
    }

    public Task OpenSession()
    {
        return Device.OpenSession();
    }

    public void CloseSession()
    {
        Device.CloseSession();
    }
}