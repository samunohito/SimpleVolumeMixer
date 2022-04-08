using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Windows.Input;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SimpleVolumeMixer.Core.Helper.Component;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Types;
using SimpleVolumeMixer.Core.Models.Domain.CoreAudio;
using SimpleVolumeMixer.UI.Views.Controls;

namespace SimpleVolumeMixer.UI.ViewModels.Audio;

public class AudioSessionViewModel : DisposableComponent, IAudioSessionCard
{
    private IPeakBarHandler? _soundBarHandler;

    public AudioSessionViewModel(AudioSession session)
    {
        _soundBarHandler = null;

        Session = session;
        SessionState = session.SessionState
            .ToReadOnlyReactivePropertySlim()
            .AddTo(Disposable);
        PeakValue = session.PeakValue
            .ToReadOnlyReactivePropertySlim()
            .AddTo(Disposable);
        MeteringChannelCount = session.MeteringChannelCount
            .ToReadOnlyReactivePropertySlim()
            .AddTo(Disposable);
        DisplayName = session.DisplayName
            .Select(x => session.IsSystemSound ? "SystemSound" : x)
            .ToReadOnlyReactivePropertySlim()
            .AddTo(Disposable);
        IconSource = session.IconSource
            .ToReadOnlyReactivePropertySlim()
            .AddTo(Disposable);
        GroupingParam = session.GroupingParam
            .ToReadOnlyReactivePropertySlim()
            .AddTo(Disposable);
        MasterVolume = session.MasterVolume
            .ToReactivePropertyAsSynchronized(
                x => x.Value,
                i => i * 1000.0f,
                i => i / 1000.0f
            )
            .AddTo(Disposable);
        IsMuted = session.IsMuted
            .ToReactivePropertyAsSynchronized(x => x.Value)
            .AddTo(Disposable);

        PeakValue
            .Subscribe(x => _soundBarHandler?.NotifyValue(x))
            .AddTo(Disposable);

        PeakBarReadyCommand = new DelegateCommand<PeakBarReadyEventArgs>(OnPeakBarReady);
        MuteStateChangeCommand = new DelegateCommand(OnMuteStateChange);
    }

    public AudioSession Session { get; }
    public IReadOnlyReactiveProperty<AudioSessionStateType> SessionState { get; }
    public IReadOnlyReactiveProperty<int> MeteringChannelCount { get; }
    public IReadOnlyReactiveProperty<Guid> GroupingParam { get; }

    public bool UsePackIcon => Session.IsSystemSound;
    public PackIconKind? PackIconKind => MaterialDesignThemes.Wpf.PackIconKind.MonitorSpeaker;
    public IReadOnlyReactiveProperty<float> PeakValue { get; }
    public IReadOnlyReactiveProperty<string?> DisplayName { get; }
    public IReadOnlyReactiveProperty<ImageSource?> IconSource { get; }
    public IReactiveProperty<float> MasterVolume { get; }
    public IReactiveProperty<bool> IsMuted { get; }
    public ICommand PeakBarReadyCommand { get; }
    public ICommand MuteStateChangeCommand { get; }

    private void OnPeakBarReady(PeakBarReadyEventArgs e)
    {
        _soundBarHandler = e.Handler;
    }

    private void OnMuteStateChange()
    {
        IsMuted.Value = !IsMuted.Value;
    }
}