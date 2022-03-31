using System;
using System.IO;
using System.Reactive.Disposables;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SimpleVolumeMixer.Core.Helper.CoreAudio;
using SimpleVolumeMixer.Core.Helper.Utils;

namespace SimpleVolumeMixer.Core.Models.Domain.CoreAudio;

public class AudioSession : NotifyPropertyChangedBase, IDisposable
{
    private static readonly string SystemSoundText = "SystemSound";
    private static readonly string SystemSoundDisplayName = @"@%SystemRoot%\System32\AudioSrv.Dll";

    private readonly CompositeDisposable _disposable;
    private readonly AudioSessionAccessor _accessor;
    private readonly bool _isSystemSound;

    private readonly PropertyMonitor<AudioSessionStateType> _sessionState;
    private readonly PropertyMonitor<float> _peekValue;
    private readonly PropertyMonitor<int> _meteringChannelCount;
    private readonly PropertyMonitor<string> _displayName;
    private readonly PropertyMonitor<string> _iconPath;
    private readonly PropertyMonitor<Guid> _groupingParam;
    private readonly PropertyMonitor<float> _masterVolume;
    private readonly PropertyMonitor<bool> _isMuted;

    internal AudioSession(AudioSessionAccessor ax)
    {
        _disposable = new CompositeDisposable();
        _accessor = ax.AddTo(_disposable);
        ax.Disposed += SessionOnDisposed;

        _isSystemSound = _accessor.DisplayName.Contains(SystemSoundDisplayName);

        _sessionState = new PropertyMonitor<AudioSessionStateType>(
            PropertyMonitorIntervalType.Normal,
            () => ax.SessionState
        );
        _peekValue = new PropertyMonitor<float>(
            PropertyMonitorIntervalType.High,
            () => ax.PeekValue,
            comparer: PropertyMonitor.FloatComparer
        );
        _meteringChannelCount = new PropertyMonitor<int>(
            PropertyMonitorIntervalType.LowMiddle,
            () => ax.MeteringChannelCount,
            comparer: PropertyMonitor.IntComparer
        );
        _displayName = new PropertyMonitor<string>(
            PropertyMonitorIntervalType.LowMiddle,
            ResolveDisplayName
        );
        _iconPath = new PropertyMonitor<string>(
            PropertyMonitorIntervalType.Low,
            () => ax.IconPath
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
        PeekValue = _peekValue.ToReactivePropertySlimAsSynchronized(x => x.Value);
        MeteringChannelCount = _meteringChannelCount.ToReactivePropertySlimAsSynchronized(x => x.Value);
        DisplayName = _displayName.ToReactivePropertySlimAsSynchronized(x => x.Value);
        IconPath = _iconPath.ToReactivePropertySlimAsSynchronized(x => x.Value);
        GroupingParam = _groupingParam.ToReactivePropertySlimAsSynchronized(x => x.Value);
        MasterVolume = _masterVolume.ToReactivePropertySlimAsSynchronized(x => x.Value);
        IsMuted = _isMuted.ToReactivePropertySlimAsSynchronized(x => x.Value);

        var disposables = new IDisposable[]
        {
            _sessionState,
            _peekValue,
            _meteringChannelCount,
            _displayName,
            _iconPath,
            _groupingParam,
            _masterVolume,
            _isMuted,
            SessionState,
            PeekValue,
            MeteringChannelCount,
            DisplayName,
            IconPath,
            GroupingParam,
            MasterVolume,
            IsMuted,
        };
        foreach (var disposable in disposables)
        {
            disposable.AddTo(_disposable);
        }
    }

    public IReadOnlyReactiveProperty<AudioSessionStateType> SessionState { get; }
    public IReadOnlyReactiveProperty<float> PeekValue { get; }
    public IReadOnlyReactiveProperty<int> MeteringChannelCount { get; }
    public IReadOnlyReactiveProperty<string> DisplayName { get; }
    public IReadOnlyReactiveProperty<string> IconPath { get; }
    public IReadOnlyReactiveProperty<Guid> GroupingParam { get; }
    public IReactiveProperty<float> MasterVolume { get; }
    public IReactiveProperty<bool> IsMuted { get; }

    private void SessionOnDisposed(object sender, EventArgs e)
    {
        Dispose();
    }

    private string ResolveDisplayName()
    {
        if (_isSystemSound)
        {
            return SystemSoundText;
        }

        var displayName = _accessor.DisplayName;
        if (!string.IsNullOrWhiteSpace(displayName))
        {
            return displayName;
        }

        var proc = _accessor.Process;
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

        return "";
    }

    public void Dispose()
    {
        _disposable.Dispose();
    }
}