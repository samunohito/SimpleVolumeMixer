using System;
using Reactive.Bindings;
using SimpleVolumeMixer.Core.Helper.CoreAudio;

namespace SimpleVolumeMixer.Core.Contracts.Models.Repository;

public interface ICoreAudioRepository
{
    ReadOnlyReactiveCollection<AudioDeviceAccessor> AudioDevices { get; }
    IReactiveProperty<AudioDeviceAccessor?> CommunicationRoleDevice { get; }
    IReactiveProperty<AudioDeviceAccessor?> MultimediaRoleDevice { get; }
    
}