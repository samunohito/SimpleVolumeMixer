using System;
using System.Linq;
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
    public class AudioDeviceViewModel : BindableBase, IDisposable
    {
        private readonly AudioDevice _device;
        private readonly CompositeDisposable _disposable;
        public AudioDeviceViewModel(AudioDevice device)
        {
            _disposable = new CompositeDisposable();
            _device = device;

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
                .Subscribe(x => SoundBarHandler?.NotifyValue(x))
                .AddTo(_disposable);
        }

        public ReadOnlyReactiveCollection<AudioSessionViewModel> Sessions { get; }
        public DeviceRole Role => _device.Role;
        public IReadOnlyReactiveProperty<string> DeviceId { get; }
        public IReadOnlyReactiveProperty<string> FriendlyName { get; }
        public IReadOnlyReactiveProperty<string> DevicePath { get; }
        public IReadOnlyReactiveProperty<DeviceStateType> DeviceState { get; }
        public IReadOnlyReactiveProperty<DataFlowType> DataFlow { get; }
        public IReadOnlyReactiveProperty<int> ChannelCount { get; }
        public IReadOnlyReactiveProperty<float> PeakValue { get; }
        public IReadOnlyReactiveProperty<int> MeteringChannelCount { get; }
        public IReactiveProperty<float> MasterVolume { get; }
        public IReactiveProperty<bool> IsMuted { get; }
        public ISoundBarHandler? SoundBarHandler { get; set; }

        public void OpenSession()
        {
            _device.OpenSession();
        }

        public void CloseSession()
        {
            _device.CloseSession();
        }

        public void Dispose()
        {
            foreach (var session in Sessions.ToList())
            {
                session.Dispose();
            }

            _disposable.Dispose();
        }
    }
}