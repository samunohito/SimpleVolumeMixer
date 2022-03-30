﻿using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
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

        public AudioSessionViewModel(AudioSession session)
        {
            _disposable = new CompositeDisposable();

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

            SoundBarHandler = new ReactiveProperty<ISoundBarHandler>().AddTo(_disposable);

            PeekValue
                .Subscribe(x => SoundBarHandler.Value?.NotifyValue(x))
                .AddTo(_disposable);
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

        public ReactiveProperty<ISoundBarHandler> SoundBarHandler { get; }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}