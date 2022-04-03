using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Input;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Types;
using SimpleVolumeMixer.Core.Models.Domain.CoreAudio;
using SimpleVolumeMixer.UI.Views.Controls;

namespace SimpleVolumeMixer.UI.ViewModels.Audio;

public class AudioSessionViewModel : BindableBase, IDisposable, IAudioSessionCard
{
    private readonly CompositeDisposable _disposable;
    private ISoundBarHandler? _soundBarHandler;

    public AudioSessionViewModel(AudioSession session)
    {
        _disposable = new CompositeDisposable();
        _soundBarHandler = null;

        Session = session;
        SessionState = session.SessionState
            .ToReadOnlyReactivePropertySlim()
            .AddTo(_disposable);
        PeakValue = session.PeakValue
            .ToReadOnlyReactivePropertySlim()
            .AddTo(_disposable);
        MeteringChannelCount = session.MeteringChannelCount
            .ToReadOnlyReactivePropertySlim()
            .AddTo(_disposable);
        DisplayName = session.DisplayName
            .Select(x => session.IsSystemSound ? "SystemSound" : x)
            .ToReadOnlyReactivePropertySlim()
            .AddTo(_disposable);
        IconSource = session.IconSource
            .ToReadOnlyReactivePropertySlim()
            .AddTo(_disposable);
        GroupingParam = session.GroupingParam
            .ToReadOnlyReactivePropertySlim()
            .AddTo(_disposable);
        MasterVolume = session.MasterVolume
            .ToReactivePropertyAsSynchronized(
                x => x.Value,
                i => i * 1000.0f,
                i => i / 1000.0f
            )
            .AddTo(_disposable);
        IsMuted = session.IsMuted
            .ToReactivePropertyAsSynchronized(x => x.Value)
            .AddTo(_disposable);

        PeakValue
            .Subscribe(x => _soundBarHandler?.NotifyValue(x))
            .AddTo(_disposable);

        PeakBarReadyCommand = new DelegateCommand<SoundBarReadyEventArgs>(OnSoundBarReady);
        MuteStateChangeCommand = new DelegateCommand(OnOnMuteStateChange);
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

    public void Dispose()
    {
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
}