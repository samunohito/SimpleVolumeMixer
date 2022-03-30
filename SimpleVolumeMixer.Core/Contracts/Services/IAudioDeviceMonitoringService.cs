using System;
using Reactive.Bindings;
using SimpleVolumeMixer.Core.Models.Domain.CoreAudio;

namespace SimpleVolumeMixer.Core.Contracts.Services;

public interface IAudioDeviceMonitoringService
{
    ReadOnlyReactiveCollection<AudioDevice> Devices { get; }
    
    double MonitoringInterval { get; set; }

    void RefreshAudioDevices();
}