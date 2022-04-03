using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SimpleVolumeMixer.Core.Helper.Component;
using SimpleVolumeMixer.Core.Helper.Component.Types;
using SimpleVolumeMixer.Core.Helper.CoreAudio;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Types;

namespace SimpleVolumeMixer.Core.Models.Domain.CoreAudio;

public class AudioSession : DisposableComponent
{
    private static readonly HashSet<string> ImageTypeExt = new(new[] { ".png", ".jpg", ".bmp" });
    private readonly AudioSessionAccessor _accessor;

    private readonly PropertyMonitor<AudioSessionStateType> _sessionState;
    private readonly PropertyMonitor<float> _peakValue;
    private readonly PropertyMonitor<int> _meteringChannelCount;
    private readonly PropertyMonitor<string?> _displayName;
    private readonly PropertyMonitor<ImageSource?> _iconSource;
    private readonly PropertyMonitor<Guid> _groupingParam;
    private readonly PropertyMonitor<float> _masterVolume;
    private readonly PropertyMonitor<bool> _isMuted;

    internal AudioSession(AudioSessionAccessor ax)
    {
        _accessor = ax.AddTo(Disposable);

        IsSystemSound = _accessor.IsSystemSoundSession;

        _sessionState = new PropertyMonitor<AudioSessionStateType>(
            PropertyMonitorIntervalType.Normal,
            () => ax.SessionState
        );
        _peakValue = new PropertyMonitor<float>(
            PropertyMonitorIntervalType.High,
            () => ax.PeakValue,
            comparer: PropertyMonitor.FloatComparer
        );
        _meteringChannelCount = new PropertyMonitor<int>(
            PropertyMonitorIntervalType.LowMiddle,
            () => ax.MeteringChannelCount,
            comparer: PropertyMonitor.IntComparer
        );
        _displayName = new PropertyMonitor<string?>(
            PropertyMonitorIntervalType.LowMiddle,
            ResolveDisplayName
        );
        _iconSource = new PropertyMonitor<ImageSource?>(
            PropertyMonitorIntervalType.Manual,
            ResolveIcon
        );
        _groupingParam = new PropertyMonitor<Guid>(
            PropertyMonitorIntervalType.Low,
            () => ax.GroupingParam
        );
        _masterVolume = new PropertyMonitor<float>(
            PropertyMonitorIntervalType.Normal,
            () => ax.MasterVolume,
            (x) => ax.MasterVolume = x,
            PropertyMonitor.FloatComparer
        );
        _isMuted = new PropertyMonitor<bool>(
            PropertyMonitorIntervalType.Normal,
            () => ax.IsMuted,
            (x) => ax.IsMuted = x,
            PropertyMonitor.BoolComparer
        );

        SessionState = _sessionState.ToReactivePropertySlimAsSynchronized(x => x.Value);
        PeakValue = _peakValue.ToReactivePropertySlimAsSynchronized(x => x.Value);
        MeteringChannelCount = _meteringChannelCount.ToReactivePropertySlimAsSynchronized(x => x.Value);
        DisplayName = _displayName.ToReactivePropertySlimAsSynchronized(x => x.Value);
        IconSource = _iconSource.ToReactivePropertySlimAsSynchronized(x => x.Value);
        GroupingParam = _groupingParam.ToReactivePropertySlimAsSynchronized(x => x.Value);
        MasterVolume = _masterVolume.ToReactivePropertySlimAsSynchronized(x => x.Value);
        IsMuted = _isMuted.ToReactivePropertySlimAsSynchronized(x => x.Value);

        var disposables = new IDisposable[]
        {
            _sessionState,
            _peakValue,
            _meteringChannelCount,
            _displayName,
            _iconSource,
            _groupingParam,
            _masterVolume,
            _isMuted,
            SessionState,
            PeakValue,
            MeteringChannelCount,
            DisplayName,
            IconSource,
            GroupingParam,
            MasterVolume,
            IsMuted
        };
        foreach (var disposable in disposables)
        {
            disposable.AddTo(Disposable);
        }

        ax.Disposed += SessionOnDisposed;
        if (ax.IsDisposed)
        {
            // 無いだろうけど、初期化中にAccessorが破棄された時のために
            Dispose();
        }
    }

    public Process? Process => _accessor.Process;
    public bool IsSystemSound { get; }
    public IReadOnlyReactiveProperty<AudioSessionStateType> SessionState { get; }
    public IReadOnlyReactiveProperty<float> PeakValue { get; }
    public IReadOnlyReactiveProperty<int> MeteringChannelCount { get; }
    public IReadOnlyReactiveProperty<string?> DisplayName { get; }
    public IReadOnlyReactiveProperty<ImageSource?> IconSource { get; }
    public IReadOnlyReactiveProperty<Guid> GroupingParam { get; }
    public IReactiveProperty<float> MasterVolume { get; }
    public IReactiveProperty<bool> IsMuted { get; }

    private void SessionOnDisposed(object? sender, EventArgs e)
    {
        Dispose();
    }

    private string? ResolveDisplayName()
    {
        if (IsSystemSound)
        {
            // xaml側で対応する
            return null;
        }

        var displayName = _accessor.DisplayName;
        if (!string.IsNullOrWhiteSpace(displayName))
        {
            return displayName;
        }

        var proc = _accessor.Process;
        var title = proc?.MainWindowTitle;
        if (!string.IsNullOrWhiteSpace(title))
        {
            return title;
        }

        var procName = proc?.ProcessName;
        if (!string.IsNullOrWhiteSpace(procName))
        {
            return procName;
        }

        var fileName = proc?.StartInfo.FileName;
        if (!string.IsNullOrWhiteSpace(fileName))
        {
            var fileInfo = new FileInfo(fileName);
            if (fileInfo.Exists) return fileInfo.Name;
        }

        return null;
    }

    private ImageSource? ResolveIcon()
    {
        if (IsSystemSound)
        {
            // テーマにより使う画像を分けたいのでxaml側で対応する
            return null;
        }

        if (_accessor.IsDisposed)
        {
            return null;
        }

        var src = string.IsNullOrWhiteSpace(_accessor.IconPath)
            ? _accessor.Process?.MainModule?.FileName
            : _accessor.IconPath;
        if (string.IsNullOrWhiteSpace(src))
        {
            return null;
        }

        var fileInfo = new FileInfo(src);
        if (!fileInfo.Exists)
        {
            return null;
        }

        if (fileInfo.Extension == ".exe")
        {
            using var ms = new MemoryStream();
            using var bmp = Icon.ExtractAssociatedIcon(fileInfo.FullName).ToBitmap();
            bmp.Save(ms, ImageFormat.Png);
            return BytesToImageSource(ms.GetBuffer());
        }

        if (ImageTypeExt.Contains(fileInfo.Extension))
        {
            return BytesToImageSource(File.ReadAllBytes(fileInfo.FullName));
        }

        return null;
    }

    private ImageSource BytesToImageSource(byte[] bytes)
    {
        var ms = new MemoryStream(bytes);
        var biImg = new BitmapImage();
        biImg.BeginInit();
        biImg.StreamSource = ms;
        biImg.EndInit();
        return biImg;
    }
}