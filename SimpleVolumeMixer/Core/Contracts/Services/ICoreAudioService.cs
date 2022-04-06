using Reactive.Bindings;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Types;
using SimpleVolumeMixer.Core.Models.Domain.CoreAudio;

namespace SimpleVolumeMixer.Core.Contracts.Services;

public interface ICoreAudioService
{
    ReadOnlyReactiveCollection<AudioDevice> Devices { get; }
    IReadOnlyReactiveProperty<AudioDevice?> CommunicationRoleDevice { get; }
    IReadOnlyReactiveProperty<AudioDevice?> MultimediaRoleDevice { get; }
    void SetDefaultDevice(AudioDevice device, DataFlowType dataFlowType, RoleType roleType);
}