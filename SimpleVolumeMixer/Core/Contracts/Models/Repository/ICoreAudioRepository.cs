using Reactive.Bindings;
using SimpleVolumeMixer.Core.Helper.CoreAudio;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Types;

namespace SimpleVolumeMixer.Core.Contracts.Models.Repository;

public interface ICoreAudioRepository
{
    ReadOnlyReactiveCollection<AudioDeviceAccessor> AudioDevices { get; }
    IReactiveProperty<AudioDeviceAccessor?> CommunicationRoleDevice { get; }
    IReactiveProperty<AudioDeviceAccessor?> MultimediaRoleDevice { get; }
    void SetDefaultDevice(AudioDeviceAccessor accessor, RoleType roleType);
}