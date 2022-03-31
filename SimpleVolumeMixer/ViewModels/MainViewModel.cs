using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SimpleVolumeMixer.Core.Contracts.Services;
using SimpleVolumeMixer.ViewModels.Main;
using SimpleVolumeMixer.Views.Controls;

namespace SimpleVolumeMixer.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private readonly CompositeDisposable _disposable;
        private readonly ICoreAudioService _coreAudioService;
        private ISoundBarHandler? _soundBarHandler;

        public MainViewModel(ICoreAudioService coreAudioService)
        {
            _disposable = new CompositeDisposable();
            _coreAudioService = coreAudioService;
            _soundBarHandler = null;

            Devices = coreAudioService.Devices
                .ToReadOnlyReactiveCollection(x => new AudioDeviceViewModel(x))
                .AddTo(_disposable);
            SelectedDevice = new ReactiveProperty<AudioDeviceViewModel?>().AddTo(_disposable);

            SelectedDevice.Zip(SelectedDevice.Skip(1), (x, y) => new { OldValue = x, NewValue = y })
                .Subscribe(x =>
                {
                    if (Equals(x?.OldValue, x?.NewValue))
                    {
                        return;
                    }

                    x?.OldValue?.CloseSession();
                    x?.NewValue?.OpenSession();
                })
                .AddTo(_disposable);

            SelectedDevice
                .Where(x => x != null)
                .Select(x => x!)
                .Subscribe(x => { x.SoundBarHandler = _soundBarHandler; })
                .AddTo(_disposable);

            OnLoadedCommand = new DelegateCommand(OnLoaded);
            SoundBarReadyCommand = new DelegateCommand<SoundBarReadyEventArgs>(OnSoundBarReady);
        }

        ~MainViewModel()
        {
            _disposable.Dispose();
        }

        public ReadOnlyReactiveCollection<AudioDeviceViewModel> Devices { get; }
        public IReactiveProperty<AudioDeviceViewModel?> SelectedDevice { get; }

        public ICommand OnLoadedCommand { get; }
        public ICommand SoundBarReadyCommand { get; }

        private void OnLoaded()
        {
            _coreAudioService.RefreshAudioDevices();
            SelectedDevice.Value = Devices.FirstOrDefault(i => i.Role.Multimedia);
        }

        private void OnSoundBarReady(SoundBarReadyEventArgs e)
        {
            _soundBarHandler = e.Handler;
        }
    }
}