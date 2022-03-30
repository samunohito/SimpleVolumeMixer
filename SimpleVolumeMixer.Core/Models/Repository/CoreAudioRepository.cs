using Reactive.Bindings;
using SimpleVolumeMixer.Core.Contracts.Models.Repository;
using SimpleVolumeMixer.Core.Helper.CoreAudio;

namespace SimpleVolumeMixer.Core.Models.Repository;

public class CoreAudioRepository : ICoreAudioRepository
{
    private readonly CoreAudioAccessor _accessor;

    public CoreAudioRepository()
    {
        _accessor = new CoreAudioAccessor();
    }

    public ReadOnlyReactiveCollection<AudioDeviceAccessor> AudioDevices => _accessor.AudioDevices;

    public void RefreshAudioDevices()
    {
        _accessor.RefreshDevices();
    }

    public void Dispose()
    {
        _accessor.Dispose();
    }
}