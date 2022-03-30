using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using CSCore.CoreAudioAPI;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio;

public class CoreAudioAccessor : IDisposable
{
    private CompositeDisposable _disposable;
    private MMDeviceEnumerator _deviceEnumerator;
    private MMNotificationClient _notificationClient;

    public CoreAudioAccessor()
    {
        _disposable = new CompositeDisposable();
        _deviceEnumerator = new MMDeviceEnumerator().AddTo(_disposable);

        _notificationClient = new MMNotificationClient(_deviceEnumerator).AddTo(_disposable);
        _notificationClient.DeviceAdded += NotificationClientOnDeviceAdded;
        _notificationClient.DeviceRemoved += NotificationClientOnDeviceRemoved;
        _notificationClient.DevicePropertyChanged += NotificationClientOnDevicePropertyChanged;

        AudioDevices = new ReactiveCollection<AudioDeviceAccessor>().AddTo(_disposable);
    }

    public ReactiveCollection<AudioDeviceAccessor> AudioDevices { get; }

    private void DisposeAudioDevices()
    {
        foreach (var audioDevice in AudioDevices.ToList())
        {
            audioDevice.Dispose();
        }
    }

    public void RefreshAudioDevices()
    {
        DisposeAudioDevices();

        using var devices = _deviceEnumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active);
        AudioDevices.Clear();
        foreach (var device in devices)
        {
            AudioDevices.Add(new AudioDeviceAccessor(device));
        }
    }

    private void NotificationClientOnDevicePropertyChanged(object sender, DevicePropertyChangedEventArgs e)
    {
        Debug.WriteLine("NotificationClientOnDevicePropertyChanged");
        RefreshAudioDevices();
    }

    private void NotificationClientOnDeviceRemoved(object sender, DeviceNotificationEventArgs e)
    {
        Debug.WriteLine("NotificationClientOnDeviceRemoved");
        RefreshAudioDevices();
    }

    private void NotificationClientOnDeviceAdded(object sender, DeviceNotificationEventArgs e)
    {
        Debug.WriteLine("NotificationClientOnDeviceAdded");
        RefreshAudioDevices();
    }

    public void Dispose()
    {
        _notificationClient.DeviceAdded -= NotificationClientOnDeviceAdded;
        _notificationClient.DeviceRemoved -= NotificationClientOnDeviceRemoved;
        _notificationClient.DevicePropertyChanged -= NotificationClientOnDevicePropertyChanged;

        _disposable.Dispose();
    }
}