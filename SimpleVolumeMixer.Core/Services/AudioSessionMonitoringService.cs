using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Timers;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SimpleVolumeMixer.Core.Contracts.Models.Repository;
using SimpleVolumeMixer.Core.Contracts.Services;
using SimpleVolumeMixer.Core.Models.Domain.CoreAudio;

namespace SimpleVolumeMixer.Core.Services;

public class AudioSessionMonitoringService : IDisposable, IAudioSessionMonitoringService
{
    private const int RefreshInterval = 5;

    private readonly CompositeDisposable _disposable;
    private readonly ICoreAudioRepository _coreAudioRepository;
    private readonly Timer _timer;

    public AudioSessionMonitoringService(ICoreAudioRepository coreAudioRepository)
    {
        _disposable = new CompositeDisposable();
        _coreAudioRepository = coreAudioRepository;

        _timer = new Timer().AddTo(_disposable);
        _timer.Interval = RefreshInterval;
        _timer.Elapsed += TimerOnElapsed;

        Devices = coreAudioRepository.AudioDevices
            .ToReadOnlyReactiveCollection(x => new AudioDevice(x))
            .AddTo(_disposable);

        CurrentDevice = new ReactivePropertySlim<AudioDevice>().AddTo(_disposable);

        CurrentDevice.Zip(CurrentDevice.Skip(1), (x, y) => new { OldValue = x, NewValue = y })
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

        _timer.Start();
    }

    public ReadOnlyReactiveCollection<AudioDevice> Devices { get; }

    public IReactiveProperty<AudioDevice> CurrentDevice { get; }

    public double MonitoringInterval
    {
        get => _timer.Interval;
        set => _timer.Interval = value;
    }

    private void TimerOnElapsed(object sender, ElapsedEventArgs e)
    {
        foreach (var audioDevice in Devices.ToList())
        {
            audioDevice.Refresh();
        }
    }

    public void RefreshAudioDevices()
    {
        _coreAudioRepository.RefreshAudioDevices();
    }

    public void Dispose()
    {
        _timer.Stop();
        _timer.Elapsed -= TimerOnElapsed;

        _disposable.Dispose();
    }
}