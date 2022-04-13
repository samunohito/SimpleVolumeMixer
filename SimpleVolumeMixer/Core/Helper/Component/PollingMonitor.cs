using System;
using System.Reactive.Disposables;
using System.Timers;
using Reactive.Bindings.Extensions;
using SimpleVolumeMixer.Core.Helper.Component.Types;

namespace SimpleVolumeMixer.Core.Helper.Component;

public static class PollingMonitor
{
    /// <summary>
    /// Comparison function for int type only.
    /// </summary>
    public static Func<int, int, bool> IntComparer = (x, y) => x == y;

    /// <summary>
    /// Comparison function for float type only.
    /// </summary>
    public static Func<float, float, bool> FloatComparer = (x, y) => x.Equals(y);

    /// <summary>
    /// Comparison function for bool type only.
    /// </summary>
    public static Func<bool, bool, bool> BoolComparer = (x, y) => x == y;

    /// <summary>
    /// Comparison processing using object.Equals;
    /// if boxing occurs, as in the case of ints and floats, it is more efficient to have a dedicated comparison function.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Func<T, T, bool> GetDefaultComparer<T>() => (x, y) => Equals(x, y);
}

public class PollingMonitor<T> : NotifyPropertyChangedBase, IDisposable, IPollingMonitor<T>
{
    private static readonly Action<T> DefaultWriter = _ => { };
    private readonly Func<T, T, bool> _comparer;

    private readonly CompositeDisposable _disposable;
    private readonly Func<T> _reader;
    private readonly Timer _timer;
    private readonly Action<T> _writer;
    private PollingMonitorIntervalType _intervalType;
    private T _value;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="intervalType">Polling interval</param>
    /// <param name="reader">Function making the polling target call</param>
    /// <param name="writer">This function delegates write processing when the Value property is written from the outside.</param>
    /// <param name="comparer">Comparison process to detect that a value has been rewritten</param>
    public PollingMonitor(
        PollingMonitorIntervalType intervalType,
        Func<T> reader,
        Action<T>? writer = null,
        Func<T, T, bool>? comparer = null
    )
    {
        _disposable = new CompositeDisposable();
        _reader = reader;
        _writer = writer != null ? writer : DefaultWriter;
        _comparer = comparer != null ? comparer : PollingMonitor.GetDefaultComparer<T>();

        _value = reader();
        RaisePropertyChanged(nameof(Value));

        _timer = new Timer().AddTo(_disposable);
        _timer.Enabled = false;
        _timer.Elapsed += TimerOnElapsed;

        IntervalType = intervalType;
    }

    /// <summary>
    /// Extend IPollingMonitor.Value to also support write processing.
    /// The actual writing process is performed through the function object obtained at class initialization.
    /// Also, to avoid changing the interface behavior (or rather, to avoid notification loops),
    /// the <see cref="IPollingMonitor.PropertyChanged"/> event is not triggered when writing to this property.
    /// </summary>
    /// <seealso cref="IPollingMonitor.PropertyChanged"/>
    public T Value
    {
        get => _value;
        set
        {
            _writer(value);
            _value = value;
        }
    }

    /// <inheritdoc cref="IPollingMonitor.Value"/>
    object IPollingMonitor.Value
    {
#pragma warning disable CS8603
        get => Value;
#pragma warning restore CS8603
    }

    /// <inheritdoc cref="IPollingMonitor.IntervalType"/>
    public PollingMonitorIntervalType IntervalType
    {
        get => _intervalType;
        set
        {
            _intervalType = value;

            if (_intervalType == PollingMonitorIntervalType.Manual)
            {
                _timer.Stop();
            }
            else
            {
                _timer.Interval = (double)_intervalType;
                _timer.Start();
            }
        }
    }

    private void TimerOnElapsed(object sender, ElapsedEventArgs e)
    {
        Refresh();
    }

    /// <inheritdoc cref="IPollingMonitor.Start"/>
    public void Start()
    {
        if (_intervalType == PollingMonitorIntervalType.Manual)
        {
            return;
        }

        if (_timer.Enabled)
        {
            return;
        }

        _timer.Enabled = true;
        _timer.Start();
    }

    /// <inheritdoc cref="IPollingMonitor.Stop"/>
    public void Stop()
    {
        _timer.Enabled = false;
        _timer.Stop();
    }

    /// <inheritdoc cref="IPollingMonitor.Refresh"/>
    public void Refresh()
    {
        var newValue = _reader();
        if (!_comparer(_value, newValue))
        {
            _value = newValue;
            RaisePropertyChanged(nameof(Value));
        }
    }

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        _timer.Stop();
        _timer.Elapsed -= TimerOnElapsed;

        _disposable.Dispose();
    }
}