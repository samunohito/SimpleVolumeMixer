using System;
using System.Reactive.Disposables;
using System.Timers;
using Reactive.Bindings.Extensions;
using SimpleVolumeMixer.Core.Helper.Component.Types;

namespace SimpleVolumeMixer.Core.Helper.Component;

public static class PropertyMonitor
{
    public static readonly Func<int, int, bool> IntComparer = (x, y) => x == y;
    public static readonly Func<float, float, bool> FloatComparer = (x, y) => x.Equals(y);
    public static readonly Func<bool, bool, bool> BoolComparer = (x, y) => x == y;
}

public class PropertyMonitor<T> : NotifyPropertyChangedBase, IDisposable, IPropertyMonitor<T>
{
    private static readonly Func<T, T, bool> DefaultComparer = (x, y) => Equals(x, y);
    private static readonly Action<T> DefaultWriter = _ => { };
    private readonly Func<T, T, bool> _comparer;

    private readonly CompositeDisposable _disposable;
    private readonly Func<T> _reader;
    private readonly Timer _timer;
    private readonly Action<T> _writer;
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
        RaisePropertyChanged(nameof(Value));
    }

    public T Value
    {
        get => _value;
        set
        {
            WriteValue(value);
            _value = value;
        }
    }

    object IPropertyMonitor.Value
    {
#pragma warning disable CS8603
        get => Value;
#pragma warning restore CS8603
        set => Value = (T)value;
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

    public void Start()
    {
        if (_intervalType == PropertyMonitorIntervalType.Manual)
        {
            throw new InvalidOperationException("state is manual.");
        }

        _timer.Start();
    }

    public void Stop()
    {
        _timer.Stop();
    }

    public void Refresh()
    {
        var newValue = ReadValue();
        if (!_comparer(_value, newValue))
        {
            _value = newValue;
            RaisePropertyChanged(nameof(Value));
        }
    }

    protected virtual T ReadValue()
    {
        return _reader();
    }

    protected virtual void WriteValue(T value)
    {
        _writer(value);
    }

    public void Dispose()
    {
        _timer.Stop();
        _timer.Elapsed -= TimerOnElapsed;

        _disposable.Dispose();
    }
}