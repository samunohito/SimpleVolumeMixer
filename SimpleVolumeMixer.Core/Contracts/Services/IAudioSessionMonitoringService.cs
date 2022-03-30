using System;
using Reactive.Bindings;
using SimpleVolumeMixer.Core.Models.Domain.CoreAudio;

namespace SimpleVolumeMixer.Core.Contracts.Services;

public interface IAudioSessionMonitoringService
{
    ReadOnlyReactiveCollection<AudioDevice> Devices { get; }
    IReactiveProperty<AudioDevice> CurrentDevice { get; }
    double MonitoringInterval { get; set; }

    void RefreshAudioDevices();
}