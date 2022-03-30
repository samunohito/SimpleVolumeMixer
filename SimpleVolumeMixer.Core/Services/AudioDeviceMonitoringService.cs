using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Timers;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SimpleVolumeMixer.Core.Contracts.Models.Repository;
using SimpleVolumeMixer.Core.Contracts.Services;
using SimpleVolumeMixer.Core.Helper.Utils;
using SimpleVolumeMixer.Core.Models.Domain.CoreAudio;

namespace SimpleVolumeMixer.Core.Services;

public class AudioDeviceMonitoringService : IDisposable, IAudioDeviceMonitoringService
{
    private const int RefreshInterval = 1000;

    private readonly CompositeDisposable _disposable;
    private readonly ICoreAudioRepository _coreAudioRepository;
    private readonly Timer _timer;

    public AudioDeviceMonitoringService(ICoreAudioRepository coreAudioRepository)
    {
        _disposable = new CompositeDisposable();
        _coreAudioRepository = coreAudioRepository;

        _timer = new Timer().AddTo(_disposable);
        _timer.Interval = RefreshInterval;
        _timer.Elapsed += TimerOnElapsed;
        
        Devices = coreAudioRepository.AudioDevices
            .ToReadOnlyReactiveCollection(x => new AudioDevice(x))
            .AddTo(_disposable);

        _timer.Start();
    }

    public ReadOnlyReactiveCollection<AudioDevice> Devices { get; }

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