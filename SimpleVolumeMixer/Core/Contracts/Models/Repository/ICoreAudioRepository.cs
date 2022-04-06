using Reactive.Bindings;
using SimpleVolumeMixer.Core.Helper.CoreAudio;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Types;

namespace SimpleVolumeMixer.Core.Contracts.Models.Repository;

public interface ICoreAudioRepository
{
    ReadOnlyReactiveCollection<AudioDeviceAccessor> AudioDevices { get; }
    IReadOnlyReactiveProperty<AudioDeviceAccessor?> CommunicationRoleDevice { get; }
    IReadOnlyReactiveProperty<AudioDeviceAccessor?> MultimediaRoleDevice { get; }
    void SetDefaultDevice(AudioDeviceAccessor accessor, DataFlowType dataFlowType, RoleType roleType);
}