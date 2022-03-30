using Reactive.Bindings;
using SimpleVolumeMixer.Core.Helper.CoreAudio;

namespace SimpleVolumeMixer.Core.Contracts.Models.Repository;

public interface ICoreAudioRepository
{
    ReadOnlyReactiveCollection<AudioDeviceAccessor> AudioDevices { get; }

    void RefreshAudioDevices();
}