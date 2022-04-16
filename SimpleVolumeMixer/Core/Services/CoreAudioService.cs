using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SimpleVolumeMixer.Core.Contracts.Models.Repository;
using SimpleVolumeMixer.Core.Contracts.Services;
using SimpleVolumeMixer.Core.Helper.Component;
using SimpleVolumeMixer.Core.Helper.CoreAudio;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Types;
using SimpleVolumeMixer.Core.Models.Domain.CoreAudio;

namespace SimpleVolumeMixer.Core.Services;

public class CoreAudioService : IDisposable, ICoreAudioService
{
    private readonly CompositeDisposable _disposable;
    private readonly ICoreAudioRepository _coreAudioRepository;

    public CoreAudioService(ICoreAudioRepository coreAudioRepository)
    {
        _disposable = new CompositeDisposable();
        _coreAudioRepository = coreAudioRepository;
        
        // AudioDeviceAccessorに対し1-1でAudioDeviceのインスタンスを結びつけたい
        var instanceManager = new KeyValueInstanceManager<AudioDeviceAccessor, AudioDevice>(x => new AudioDevice(x));
        
        Devices = _coreAudioRepository.AudioDevices
            .ToReadOnlyReactiveCollection(x => instanceManager.Obtain(x))
            .AddTo(_disposable);
        
        // Repositoryにあわせて、各ロールのデバイスはDevicesにも登録されているものであるようにする
        CommunicationRoleDevice = _coreAudioRepository.CommunicationRoleDevice
            .Select(x => x != null ? instanceManager.Obtain(x) : null)
            .ToReadOnlyReactivePropertySlim()
            .AddTo(_disposable);
        MultimediaRoleDevice = _coreAudioRepository.MultimediaRoleDevice
            .Select(x => x != null ? instanceManager.Obtain(x) : null)
            .ToReadOnlyReactivePropertySlim()
            .AddTo(_disposable);
    }

    public ReadOnlyReactiveCollection<AudioDevice> Devices { get; }
    public IReadOnlyReactiveProperty<AudioDevice?> CommunicationRoleDevice { get; }
    public IReadOnlyReactiveProperty<AudioDevice?> MultimediaRoleDevice { get; }

    public void SetDefaultDevice(AudioDevice device, DataFlowType dataFlowType, RoleType roleType)
    {
        _coreAudioRepository.SetDefaultDevice(device.Device, dataFlowType, roleType);
    }

    public void Dispose()
    {
        _disposable.Dispose();
    }
}