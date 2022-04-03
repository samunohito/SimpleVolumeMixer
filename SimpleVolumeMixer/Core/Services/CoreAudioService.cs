using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SimpleVolumeMixer.Core.Contracts.Models.Repository;
using SimpleVolumeMixer.Core.Contracts.Services;
using SimpleVolumeMixer.Core.Helper.Component;
using SimpleVolumeMixer.Core.Helper.CoreAudio;
using SimpleVolumeMixer.Core.Models.Domain.CoreAudio;

namespace SimpleVolumeMixer.Core.Services;

public class CoreAudioService : IDisposable, ICoreAudioService
{
    private readonly CompositeDisposable _disposable;
    private readonly ICoreAudioRepository _coreAudioRepository;
    private readonly KeyValueInstanceManager<AudioDeviceAccessor, AudioDevice> _instanceManager;

    public CoreAudioService(ICoreAudioRepository coreAudioRepository)
    {
        _disposable = new CompositeDisposable();
        _instanceManager = new KeyValueInstanceManager<AudioDeviceAccessor, AudioDevice>(x => new AudioDevice(x));
        _coreAudioRepository = coreAudioRepository;

        Devices = _coreAudioRepository.AudioDevices
            .ToReadOnlyReactiveCollection(x => _instanceManager.Obtain(x))
            .AddTo(_disposable);
        CommunicationRoleDevice = _coreAudioRepository.CommunicationRoleDevice
            .Select(x => x != null ? _instanceManager.Obtain(x) : null)
            .ToReadOnlyReactivePropertySlim()
            .AddTo(_disposable);
        MultimediaRoleDevice = _coreAudioRepository.MultimediaRoleDevice
            .Select(x => x != null ? _instanceManager.Obtain(x) : null)
            .ToReadOnlyReactivePropertySlim()
            .AddTo(_disposable);
    }

    public ReadOnlyReactiveCollection<AudioDevice> Devices { get; }
    public IReadOnlyReactiveProperty<AudioDevice?> CommunicationRoleDevice { get; }
    public IReadOnlyReactiveProperty<AudioDevice?> MultimediaRoleDevice { get; }

    public void Dispose()
    {
        _disposable.Dispose();
    }
}