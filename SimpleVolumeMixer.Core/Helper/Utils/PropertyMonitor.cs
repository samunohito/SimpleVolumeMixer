using System;
using System.Reactive.Disposables;
using System.Timers;
using Reactive.Bindings.Extensions;

namespace SimpleVolumeMixer.Core.Helper.Utils;

public static class PropertyMonitor
{
    public static readonly Func<int, int, bool> IntComparer = (x, y) => x == y;
    public static readonly Func<float, float, bool> FloatComparer = (x, y) => x.Equals(y);
    public static readonly Func<bool, bool, bool> BoolComparer = (x, y) => x == y;
}

public class PropertyMonitor<T> : NotifyPropertyChangedBase, IPropertyHolder, IDisposable
{
    private static readonly Func<T, T, bool> DefaultComparer = (x, y) => Equals(x, y);

    private static readonly Action<T> DefaultWriter = (_) => { };
    private readonly CompositeDisposable _disposable;
    private readonly Func<T> _reader;
    private readonly Action<T> _writer;
    private readonly Func<T, T, bool> _comparer;
    private readonly Timer _timer;
    private PropertyMonitorIntervalType _intervalType;
    private T _value;

    public PropertyMonitor(
        PropertyMonitorIntervalType intervalType,
        Func<T> reader,
        Action<T>? writer = null,
        Func<T, T, bool>? comparer = null
    )
    {
        _disposable = new CompositeDisposable();
        _intervalType = intervalType;
        _reader = reader;
        _writer = writer != null ? writer : DefaultWriter;
        _comparer = comparer != null ? comparer : DefaultComparer;

        _timer = new Timer().AddTo(_disposable);
        _timer.Enabled = true;
        _timer.Elapsed += TimerOnElapsed;
        TimerSetting();

        _value = reader();
        OnPropertyChanged(nameof(Value));
    }

    public T Value
    {
        get => _value;
        set
        {
            _writer(value);
            _value = value;
        }
    }

    public PropertyMonitorIntervalType IntervalType
    {
        get => _intervalType;
        set
        {
            _intervalType = value;
            TimerSetting();
        }
    }

    private void TimerOnElapsed(object sender, ElapsedEventArgs e)
    {
        Refresh();
    }

    private void TimerSetting()
    {
        if (_intervalType == PropertyMonitorIntervalType.Manual)
        {
            _timer.Stop();
        }
        else
        {
            _timer.Interval = (double)_intervalType;
            _timer.Start();
        }
    }

    public void Refresh()
    {
        var newValue = _reader();
        if (!_comparer(_value, newValue))
        {
            _value = newValue;
            OnPropertyChanged(nameof(Value));
        }
    }

    public void Dispose()
    {
        _timer.Elapsed -= TimerOnElapsed;
        _timer.Stop();

        _disposable.Dispose();
    }
}