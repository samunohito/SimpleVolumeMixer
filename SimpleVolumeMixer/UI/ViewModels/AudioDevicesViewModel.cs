using System.Reactive.Disposables;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SimpleVolumeMixer.Core.Contracts.Services;
using SimpleVolumeMixer.UI.ViewModels.Audio;

namespace SimpleVolumeMixer.UI.ViewModels;

public class AudioDevicesViewModel : BindableBase
{
    private readonly CompositeDisposable _disposable;
    private readonly ICoreAudioService _coreAudioService;

    public AudioDevicesViewModel(ICoreAudioService coreAudioService)
    {
        _disposable = new CompositeDisposable();
        _coreAudioService = coreAudioService;
        
        Devices = coreAudioService.Devices
            .ToReadOnlyReactiveCollection(x => new AudioDeviceViewModel(x, coreAudioService))
            .AddTo(_disposable);
    }
    
    ~AudioDevicesViewModel()
    {
        _disposable.Dispose();
    }
    
    public ReadOnlyReactiveCollection<AudioDeviceViewModel> Devices { get; }
}