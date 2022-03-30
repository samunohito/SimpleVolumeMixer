using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Threading;
using Prism.Mvvm;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SimpleVolumeMixer.Core.Contracts.Services;
using SimpleVolumeMixer.ViewModels.Main;

namespace SimpleVolumeMixer.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private readonly CompositeDisposable _disposable;
        private readonly IAudioSessionMonitoringService _audioSessionMonitoringService;
        
        public MainViewModel(IAudioSessionMonitoringService audioSessionMonitoringService)
        {
            _disposable = new CompositeDisposable();
            _audioSessionMonitoringService = audioSessionMonitoringService;

            Devices = audioSessionMonitoringService.Devices
                .ToReadOnlyReactiveCollection(x => new AudioDeviceViewModel(x))
                .AddTo(_disposable);
            SelectedDevice = new ReactiveProperty<AudioDeviceViewModel>().AddTo(_disposable);

            SelectedDevice
                .Subscribe(x => _audioSessionMonitoringService.CurrentDevice.Value = x?.Device)
                .AddTo(_disposable);

            OnLoadedCommand = new ReactiveCommand().AddTo(_disposable);
            OnLoadedCommand
                .Subscribe(x => audioSessionMonitoringService.RefreshAudioDevices())
                .AddTo(_disposable);
        }

        ~MainViewModel()
        {
            _disposable.Dispose();
        }
        
        public ReadOnlyReactiveCollection<AudioDeviceViewModel> Devices { get; }
        public ReactiveProperty<AudioDeviceViewModel> SelectedDevice { get; }
        
        public ReactiveCommand OnLoadedCommand { get; }
    }
}
