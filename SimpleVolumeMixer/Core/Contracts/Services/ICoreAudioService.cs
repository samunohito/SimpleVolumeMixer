using Reactive.Bindings;
using SimpleVolumeMixer.Core.Models.Domain.CoreAudio;

namespace SimpleVolumeMixer.Core.Contracts.Services;

public interface ICoreAudioService
{
    ReadOnlyReactiveCollection<AudioDevice> Devices { get; }
    IReadOnlyReactiveProperty<AudioDevice?> CommunicationRoleDevice { get; }
    IReadOnlyReactiveProperty<AudioDevice?> MultimediaRoleDevice { get; }
    
}