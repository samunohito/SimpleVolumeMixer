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
        var renderInstanceManager = new KeyValueInstanceManager<AudioDeviceAccessor, AudioDevice>(x => new AudioDevice(x));
        var captureInstanceManager = new KeyValueInstanceManager<AudioDeviceAccessor, AudioDevice>(x => new AudioDevice(x));

        RenderDevices = _coreAudioRepository.RenderAudioDevices
            .ToReadOnlyReactiveCollection(x => renderInstanceManager.Obtain(x))
            .AddTo(_disposable);
        CaptureDevices = _coreAudioRepository.CaptureAudioDevices
            .ToReadOnlyReactiveCollection(x => captureInstanceManager.Obtain(x))
            .AddTo(_disposable);
        
        // Repositoryにあわせて、各ロールのデバイスはDevicesにも登録されているものであるようにする
        CommunicationRoleRenderDevice = _coreAudioRepository.CommunicationRoleRenderDevice
            .Select(x => x != null ? renderInstanceManager.Obtain(x) : null)
            .ToReadOnlyReactivePropertySlim()
            .AddTo(_disposable);
        MultimediaRoleRenderDevice = _coreAudioRepository.MultimediaRoleRenderDevice
            .Select(x => x != null ? renderInstanceManager.Obtain(x) : null)
            .ToReadOnlyReactivePropertySlim()
            .AddTo(_disposable);
        CommunicationRoleCaptureDevice = _coreAudioRepository.CommunicationRoleCaptureDevice
            .Select(x => x != null ? captureInstanceManager.Obtain(x) : null)
            .ToReadOnlyReactivePropertySlim()
            .AddTo(_disposable);
        MultimediaRoleCaptureDevice = _coreAudioRepository.MultimediaRoleCaptureDevice
            .Select(x => x != null ? captureInstanceManager.Obtain(x) : null)
            .ToReadOnlyReactivePropertySlim()
            .AddTo(_disposable);
    }

    public ReadOnlyReactiveCollection<AudioDevice> RenderDevices { get; }
    public ReadOnlyReactiveCollection<AudioDevice> CaptureDevices { get; }
    public IReadOnlyReactiveProperty<AudioDevice?> CommunicationRoleRenderDevice { get; }
    public IReadOnlyReactiveProperty<AudioDevice?> MultimediaRoleRenderDevice { get; }
    public IReadOnlyReactiveProperty<AudioDevice?> CommunicationRoleCaptureDevice { get; }
    public IReadOnlyReactiveProperty<AudioDevice?> MultimediaRoleCaptureDevice { get; }

    public void SetDefaultDevice(AudioDevice device, DataFlowType dataFlowType, RoleType roleType)
    {
        _coreAudioRepository.SetDefaultDevice(device.Device, dataFlowType, roleType);
    }

    public void Dispose()
    {
        _disposable.Dispose();
    }
}