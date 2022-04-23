using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DisposableComponents;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SimpleVolumeMixer.Core.Helper.Component;
using SimpleVolumeMixer.Core.Helper.Component.Types;
using SimpleVolumeMixer.Core.Helper.CoreAudio;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Event;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Types;

namespace SimpleVolumeMixer.Core.Models.Domain.CoreAudio;

/// <summary>
/// <see cref="AudioSessionAccessor"/>を監視し、値の変更があったら<see cref="ReactiveProperty"/>経由で通知及び最新値の配信を行う。
/// <see cref="AudioSessionAccessor"/>の存在が前提となるため、<see cref="DisposableComponent"/>の仕組みを利用して破棄されたことを検知し、
/// それに合わせて監視の終了やこのクラスの破棄を行う仕組みも実装する。
/// </summary>
public class AudioSession : DisposableComponent
{
    private static readonly HashSet<string> ImageTypeExt = new(new[] { ".png", ".jpg", ".bmp" });
    private readonly AudioSessionAccessor _accessor;

    private readonly PollingMonitor<AudioSessionStateType> _sessionState;
    private readonly PollingMonitor<float> _peakValue;
    private readonly PollingMonitor<int> _meteringChannelCount;
    private readonly PollingMonitor<string?> _displayName;
    private readonly PollingMonitor<ImageSource?> _iconSource;
    private readonly PollingMonitor<Guid> _groupingParam;
    private readonly PollingMonitor<float> _masterVolume;
    private readonly PollingMonitor<bool> _isMuted;

    internal AudioSession(AudioSessionAccessor ax)
    {
        _accessor = ax.AddTo(Disposable);

        IsSystemSound = _accessor.IsSystemSoundSession;

        _sessionState = new PollingMonitor<AudioSessionStateType>(
            PollingMonitorIntervalType.Normal,
            () => ax.SessionState
        );
        _peakValue = new PollingMonitor<float>(
            PollingMonitorIntervalType.High,
            () => ax.PeakValue,
            comparer: PollingMonitor.FloatComparer
        );
        _meteringChannelCount = new PollingMonitor<int>(
            PollingMonitorIntervalType.Low,
            () => ax.MeteringChannelCount,
            comparer: PollingMonitor.IntComparer
        );
        _displayName = new PollingMonitor<string?>(
            PollingMonitorIntervalType.Manual,
            ResolveDisplayName
        );
        _iconSource = new PollingMonitor<ImageSource?>(
            PollingMonitorIntervalType.Manual,
            ResolveIcon
        );
        _groupingParam = new PollingMonitor<Guid>(
            PollingMonitorIntervalType.Manual,
            () => ax.GroupingParam
        );
        _masterVolume = new PollingMonitor<float>(
            PollingMonitorIntervalType.Manual,
            () => ax.MasterVolume,
            (x) => ax.MasterVolume = x,
            PollingMonitor.FloatComparer
        );
        _isMuted = new PollingMonitor<bool>(
            PollingMonitorIntervalType.Manual,
            () => ax.IsMuted,
            (x) => ax.IsMuted = x,
            PollingMonitor.BoolComparer
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

        ax.Disposing += OnSessionDisposing;
        ax.DisplayNameChanged += OnDisplayNameChanged;
        ax.IconPathChanged += OnIconPathChanged;
        ax.SimpleVolumeChanged += OnSimpleVolumeChanged;
        ax.GroupingParamChanged += OnGroupingParamChanged;

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

    private void OnSessionDisposing(object? sender, EventArgs e)
    {
        Dispose();
    }

    private void OnDisplayNameChanged(object? sender, AudioSessionAccessorDisplayNameChangedEventArgs e)
    {
        _displayName.Refresh();
    }

    private void OnIconPathChanged(object? sender, AudioSessionAccessorIconPathChangedEventArgs e)
    {
        _iconSource.Refresh();
    }

    private void OnSimpleVolumeChanged(object? sender, AudioSessionAccessorSimpleVolumeChangedEventArgs e)
    {
        _masterVolume.Refresh();
        _isMuted.Refresh();
    }

    private void OnGroupingParamChanged(object? sender, AudioSessionAccessorGroupingParamChangedEventArgs e)
    {
        _groupingParam.Refresh();
    }

    /// <summary>
    /// CoreAudioAPIからセッションの表示名を取れないことがあるので、
    /// プロセスのタイトルバー → プロセス名 → プロセスの実行ファイル名 とフォールバックして表示名の取得を試みる
    /// </summary>
    /// <returns>取得結果。フォールバックしても取得できなければnull</returns>
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
        if (proc?.HasExited == false)
        {
            var title = proc.MainWindowTitle;
            if (!string.IsNullOrWhiteSpace(title))
            {
                return title;
            }

            var procName = proc.ProcessName;
            if (!string.IsNullOrWhiteSpace(procName))
            {
                return procName;
            }

            var fileName = proc.StartInfo.FileName;
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                var fileInfo = new FileInfo(fileName);
                if (fileInfo.Exists)
                {
                    return fileInfo.Name;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// CoreAudioAPIからセッションのアイコン取得する機能があるが、実際はほとんど取得できないので
    /// セッションの実行ファイルから抽出を試みる。
    /// </summary>
    /// <returns>取得結果。フォールバックしても取得できなければnull</returns>
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

    protected override void OnDisposing()
    {
        var monitors = new IPollingMonitor[]
        {
            _sessionState,
            _peakValue,
            _meteringChannelCount,
            _displayName,
            _iconSource,
            _groupingParam,
            _masterVolume,
            _isMuted
        };
        foreach (var monitor in monitors)
        {
            monitor.Stop();
        }

        _accessor.Disposing -= OnSessionDisposing;
        _accessor.DisplayNameChanged -= OnDisplayNameChanged;
        _accessor.IconPathChanged -= OnIconPathChanged;
        _accessor.SimpleVolumeChanged -= OnSimpleVolumeChanged;
        _accessor.GroupingParamChanged -= OnGroupingParamChanged;

        base.OnDisposing();
    }
}