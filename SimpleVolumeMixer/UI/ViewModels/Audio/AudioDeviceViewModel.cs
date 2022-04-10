using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SimpleVolumeMixer.Core.Helper.Component;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Types;
using SimpleVolumeMixer.Core.Models.Domain.CoreAudio;
using SimpleVolumeMixer.UI.ViewModels.Audio.Event;
using SimpleVolumeMixer.UI.Views.Controls;

namespace SimpleVolumeMixer.UI.ViewModels.Audio;

public class AudioDeviceViewModel : DisposableComponent, IAudioSessionCard
{
    public event EventHandler<DeviceRoleChangeRequestEventArgs>? DeviceChangeRequest;

    private IPeakBarHandler? _soundBarHandler;

    public AudioDeviceViewModel(AudioDevice device)
    {
        _soundBarHandler = null;
        Device = device;

        DeviceId = device.DeviceId.ToReadOnlyReactivePropertySlim().AddTo(Disposable);
        FriendlyName = device.FriendlyName.ToReadOnlyReactivePropertySlim().AddTo(Disposable);
        DevicePath = device.DevicePath.ToReadOnlyReactivePropertySlim().AddTo(Disposable);
        DataFlow = device.DataFlow.ToReadOnlyReactivePropertySlim().AddTo(Disposable);
        DeviceState = device.DeviceState.ToReadOnlyReactivePropertySlim().AddTo(Disposable);
        ChannelCount = device.ChannelCount.ToReadOnlyReactivePropertySlim().AddTo(Disposable);
        PeakValue = device.PeakValue.ToReadOnlyReactivePropertySlim().AddTo(Disposable);
        MeteringChannelCount = device.MeteringChannelCount.ToReadOnlyReactivePropertySlim().AddTo(Disposable);
        MasterVolume = device.MasterVolumeLevelScalar
            .ToReactivePropertyAsSynchronized(
                x => x.Value,
                i => i * 1000.0f,
                i => i / 1000.0f
            )
            .AddTo(Disposable);
        IsMuted = device.IsMuted
            .ToReactivePropertyAsSynchronized(x => x.Value)
            .AddTo(Disposable);

        PeakValue
            .Subscribe(x => _soundBarHandler?.NotifyValue(x))
            .AddTo(Disposable);

        PeakBarReadyCommand = new DelegateCommand<PeakBarReadyEventArgs>(OnSoundBarReady);
        MuteStateChangeCommand = new DelegateCommand(OnMuteStateChange);
        CommunicationRoleApplyCommand = new DelegateCommand(OnCommunicationRoleApply);
        MultimediaRoleApplyCommand = new DelegateCommand(OnMultimediaRoleApply);

        // for IAudioSessionCard
        IconSource = new ReactivePropertySlim<ImageSource?>().AddTo(Disposable);
        DisplayName = new ReactivePropertySlim<string?>("Master").AddTo(Disposable);
    }

    public AudioDevice Device { get; }
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

    private void OnSoundBarReady(PeakBarReadyEventArgs e)
    {
        _soundBarHandler = e.Handler;
    }

    private void OnMuteStateChange()
    {
        IsMuted.Value = !IsMuted.Value;
    }

    private void OnCommunicationRoleApply()
    {
        DeviceChangeRequest?.Invoke(
            this,
            new DeviceRoleChangeRequestEventArgs(this, DataFlowType.Render, RoleType.Communications)
        );
    }

    private void OnMultimediaRoleApply()
    {
        DeviceChangeRequest?.Invoke(
            this,
            new DeviceRoleChangeRequestEventArgs(this, DataFlowType.Render, RoleType.Multimedia)
        );
    }
}