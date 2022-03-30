using System;
using Reactive.Bindings;
using SimpleVolumeMixer.Core.Helper.CoreAudio;

namespace SimpleVolumeMixer.Core.Contracts.Models.Repository;

public interface ICoreAudioRepository
{
    ReactiveCollection<AudioDeviceAccessor> AudioDevices { get; }
    
    void RefreshAudioDevices();
}