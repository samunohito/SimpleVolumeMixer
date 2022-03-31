using System;
using System.Reactive.Disposables;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SimpleVolumeMixer.Core.Models.Domain.CoreAudio;
using SimpleVolumeMixer.Views.Controls;

namespace SimpleVolumeMixer.ViewModels.Main
{
    public class AudioSessionViewModel : BindableBase, IDisposable
    {
        private readonly CompositeDisposable _disposable;
        private ISoundBarHandler? _soundBarHandler;

        public AudioSessionViewModel(AudioSession session)
        {
            _disposable = new CompositeDisposable();
            _soundBarHandler = null;

            Session = session;
            SessionState = session.SessionState.ToReadOnlyReactivePropertySlim().AddTo(_disposable);
            PeekValue = session.PeekValue.ToReadOnlyReactivePropertySlim().AddTo(_disposable);
            MeteringChannelCount = session.MeteringChannelCount.ToReadOnlyReactivePropertySlim().AddTo(_disposable);
            DisplayName = session.DisplayName.ToReadOnlyReactivePropertySlim().AddTo(_disposable);
            IconPath = session.IconPath.ToReadOnlyReactivePropertySlim().AddTo(_disposable);
            GroupingParam = session.GroupingParam.ToReadOnlyReactivePropertySlim().AddTo(_disposable);
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

            PeekValue
                .Subscribe(x => _soundBarHandler?.NotifyValue(x))
                .AddTo(_disposable);

            SoundBarReadyCommand = new DelegateCommand<SoundBarReadyEventArgs>(OnSoundBarReady);
        }

        public AudioSession Session { get; }
        public IReadOnlyReactiveProperty<AudioSessionStateType> SessionState { get; }
        public IReadOnlyReactiveProperty<float> PeekValue { get; }
        public IReadOnlyReactiveProperty<int> MeteringChannelCount { get; }
        public IReadOnlyReactiveProperty<string> DisplayName { get; }
        public IReadOnlyReactiveProperty<string> IconPath { get; }
        public IReadOnlyReactiveProperty<Guid> GroupingParam { get; }
        public IReactiveProperty<float> MasterVolume { get; }
        public IReactiveProperty<bool> IsMuted { get; }
        public ICommand SoundBarReadyCommand { get; }

        private void OnSoundBarReady(SoundBarReadyEventArgs e)
        {
            _soundBarHandler = e.Handler;
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}