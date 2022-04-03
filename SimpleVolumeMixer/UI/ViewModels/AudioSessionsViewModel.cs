﻿using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SimpleVolumeMixer.Core.Contracts.Services;
using SimpleVolumeMixer.Core.Helper.Component;
using SimpleVolumeMixer.Core.Helper.Utils;
using SimpleVolumeMixer.Core.Models.Domain.CoreAudio;
using SimpleVolumeMixer.UI.ViewModels.Audio;

namespace SimpleVolumeMixer.UI.ViewModels;

public class AudioSessionsViewModel : BindableBase
{
    private readonly CompositeDisposable _disposable;
    private readonly KeyValueInstanceManager<AudioDevice, AudioDeviceViewModel> _instanceManager;
    private readonly ICoreAudioService _coreAudioService;

    public AudioSessionsViewModel(ICoreAudioService coreAudioService)
    {
        _disposable = new CompositeDisposable();
        _instanceManager =
            new KeyValueInstanceManager<AudioDevice, AudioDeviceViewModel>(x => new AudioDeviceViewModel(x));
        _coreAudioService = coreAudioService;

        Devices = coreAudioService.Devices
            .ToReadOnlyReactiveCollection(x => _instanceManager.Obtain(x))
            .AddTo(_disposable);
        SelectedDeviceForMaster = new ReactiveCollection<AudioDeviceViewModel>();

        SelectedDevice = new ReactiveProperty<AudioDeviceViewModel?>().AddTo(_disposable);
        SelectedDevice.Zip(SelectedDevice.Skip(1), (x, y) => new { OldValue = x, NewValue = y })
            .Subscribe(x =>
            {
                if (Equals(x?.OldValue, x?.NewValue)) return;

                x?.OldValue?.CloseSession();
                x?.NewValue?.OpenSession();
            })
            .AddTo(_disposable);
        SelectedDevice
            .Subscribe(x =>
            {
                SelectedDeviceForMaster.Clear();
                x.IfPresent(i => SelectedDeviceForMaster.Add(i));
            })
            .AddTo(_disposable);

        _coreAudioService.MultimediaRoleDevice
            .Subscribe(x => { SelectedDevice.Value = x != null ? _instanceManager.Obtain(x) : null; })
            .AddTo(_disposable);

        OnLoadedCommand = new DelegateCommand(OnLoaded);
    }

    public ReadOnlyReactiveCollection<AudioDeviceViewModel> Devices { get; }
    public ReactiveCollection<AudioDeviceViewModel> SelectedDeviceForMaster { get; }
    public IReactiveProperty<AudioDeviceViewModel?> SelectedDevice { get; }

    public ICommand OnLoadedCommand { get; }

    ~AudioSessionsViewModel()
    {
        _disposable.Dispose();
    }

    private void OnLoaded()
    {
    }
}