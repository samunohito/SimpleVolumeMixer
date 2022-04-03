using System.Linq;
using System.Reactive.Disposables;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SimpleVolumeMixer.Core.Contracts.Models.Repository;
using SimpleVolumeMixer.Core.Helper.CoreAudio;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Event;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Types;

namespace SimpleVolumeMixer.Core.Models.Repository;

public class CoreAudioRepository : ICoreAudioRepository
{
    private readonly CompositeDisposable _disposable;
    private readonly CoreAudioAccessor _accessor;

    public CoreAudioRepository()
    {
        _disposable = new CompositeDisposable();
        _accessor = new CoreAudioAccessor().AddTo(_disposable);
        _accessor.DeviceRoleChanged += OnDeviceRoleChanged;

        CommunicationRoleDevice = new ReactivePropertySlim<AudioDeviceAccessor?>().AddTo(_disposable);
        MultimediaRoleDevice = new ReactivePropertySlim<AudioDeviceAccessor?>().AddTo(_disposable);

        _accessor.RefreshDevices();
    }

    public ReadOnlyReactiveCollection<AudioDeviceAccessor> AudioDevices => _accessor.AudioDevices;
    public IReactiveProperty<AudioDeviceAccessor?> CommunicationRoleDevice { get; }
    public IReactiveProperty<AudioDeviceAccessor?> MultimediaRoleDevice { get; }

    private void OnDeviceRoleChanged(object? sender, DeviceRoleHolderChangedEventArgs e)
    {
        if (e.Role == RoleType.Communications)
        {
            if (e.NewState)
            {
                CommunicationRoleDevice.Value = e.Device;
            }
            else
            {
                if (AudioDevices.All(x => !x.Role.Communications))
                {
                    CommunicationRoleDevice.Value = null;
                }
            }
        }

        if (e.Role == RoleType.Multimedia)
        {
            if (e.NewState)
            {
                MultimediaRoleDevice.Value = e.Device;
            }
            else
            {
                if (AudioDevices.All(x => !x.Role.Multimedia))
                {
                    MultimediaRoleDevice.Value = null;
                }
            }
        }
    }

    public void Dispose()
    {
        _accessor.DeviceRoleChanged -= OnDeviceRoleChanged;
        _disposable.Dispose();
    }
}