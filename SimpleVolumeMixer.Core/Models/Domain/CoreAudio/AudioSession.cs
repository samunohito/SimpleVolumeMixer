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
    private readonly CompositeDisposable _disposable;
    private readonly AudioSessionAccessor _accessor;
    private readonly bool _isSystemSound;

    private readonly PropertyHolder<AudioSessionStateType> _sessionState;
    private readonly PropertyHolder<float> _peekValue;
    private readonly PropertyHolder<int> _meteringChannelCount;
    private readonly PropertyHolder<string> _displayName;
    private readonly PropertyHolder<string> _iconPath;
    private readonly PropertyHolder<Guid> _groupingParam;
    private readonly PropertyHolder<float> _masterVolume;
    private readonly PropertyHolder<bool> _isMuted;

    internal AudioSession(AudioSessionAccessor ax)
    {
        _disposable = new CompositeDisposable();
        _accessor = ax.AddTo(_disposable);
        ax.Disposed += SessionOnDisposed;

        _isSystemSound = _accessor.DisplayName?.Contains(@"@%SystemRoot%\System32\AudioSrv.Dll") ?? false;

        Action<string> act = (propertyName) => OnPropertyChanged(propertyName);

        _sessionState =
            new PropertyHolder<AudioSessionStateType>(() => ax.SessionState, nameof(SessionState), act);
        _peekValue =
            new PropertyHolder<float>(() => ax.PeekValue, nameof(PeekValue), act);
        _meteringChannelCount =
            new PropertyHolder<int>(() => ax.MeteringChannelCount, nameof(MeteringChannelCount), act);
        _displayName =
            new PropertyHolder<string>(ResolveDisplayName, nameof(DisplayName), act);
        _iconPath =
            new PropertyHolder<string>(() => ax.IconPath, nameof(IconPath), act);
        _groupingParam =
            new PropertyHolder<Guid>(() => ax.GroupingParam, nameof(GroupingParam), act);
        _masterVolume =
            new PropertyHolder<float>(() => ax.MasterVolume, nameof(MasterVolume), act, x => ax.MasterVolume = x);
        _isMuted =
            new PropertyHolder<bool>(() => ax.IsMuted, nameof(IsMuted), act);
        
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
        };
        foreach (var disposable in disposables)
        {
            disposable.AddTo(_disposable);
        }
    }

    public IReactiveProperty<AudioSessionStateType> SessionState => _sessionState.Holder;
    public IReactiveProperty<float> PeekValue => _peekValue.Holder;
    public IReactiveProperty<int> MeteringChannelCount => _meteringChannelCount.Holder;
    public IReactiveProperty<string> DisplayName => _displayName.Holder;
    public IReactiveProperty<string> IconPath => _iconPath.Holder;
    public IReactiveProperty<Guid> GroupingParam => _groupingParam.Holder;
    public IReactiveProperty<float> MasterVolume => _masterVolume.Holder;
    public IReactiveProperty<bool> IsMuted => _isMuted.Holder;
    
    private void SessionOnDisposed(object sender, EventArgs e)
    {
        Dispose();
    }

    private string ResolveDisplayName()
    {
        if (_isSystemSound)
        {
            return "SystemSound";
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

    public void Refresh()
    {
        if (_disposable.IsDisposed || _accessor.IsDisposed)
        {
            return;
        }
        
        var monitors = new IPropertyHolder[]
        {
            _sessionState,
            _peekValue,
            _meteringChannelCount,
            _displayName,
            _iconPath,
            _groupingParam,
            _masterVolume,
            _isMuted,
        };
        foreach (var monitor in monitors)
        {
            monitor.Refresh();
        }
    }

    public void Dispose()
    {
        _disposable.Dispose();
    }
}