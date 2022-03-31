using System;
using System.Reactive.Disposables;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SimpleVolumeMixer.Core.Contracts.Models.Repository;
using SimpleVolumeMixer.Core.Contracts.Services;
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

        Devices = coreAudioRepository.AudioDevices
            .ToReadOnlyReactiveCollection(x => new AudioDevice(x))
            .AddTo(_disposable);
    }

    public ReadOnlyReactiveCollection<AudioDevice> Devices { get; }


    public void RefreshAudioDevices()
    {
        _coreAudioRepository.RefreshAudioDevices();
    }

    public void Dispose()
    {
        _disposable.Dispose();
    }
}