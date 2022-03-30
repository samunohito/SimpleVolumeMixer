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
using Reactive.Bindings.Helpers;
using SimpleVolumeMixer.Core.Contracts.Services;
using SimpleVolumeMixer.Core.Models.Domain.CoreAudio;
using SimpleVolumeMixer.ViewModels.Main;

namespace SimpleVolumeMixer.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private readonly CompositeDisposable _disposable;
        private readonly IAudioDeviceMonitoringService _audioDeviceMonitoringService;
        
        public MainViewModel(IAudioDeviceMonitoringService audioDeviceMonitoringService)
        {
            _disposable = new CompositeDisposable();
            _audioDeviceMonitoringService = audioDeviceMonitoringService;

            Devices = audioDeviceMonitoringService.Devices
                .ToReadOnlyReactiveCollection(x => new AudioDeviceViewModel(x))
                .AddTo(_disposable);
            SelectedDevice = new ReactiveProperty<AudioDeviceViewModel>().AddTo(_disposable);
            
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

            OnLoadedCommand = new ReactiveCommand().AddTo(_disposable);
            OnLoadedCommand
                .Subscribe(x =>
                {
                    audioDeviceMonitoringService.RefreshAudioDevices();
                    SelectedDevice.Value = Devices.FirstOrDefault(i => i.Role.Multimedia);
                })
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
