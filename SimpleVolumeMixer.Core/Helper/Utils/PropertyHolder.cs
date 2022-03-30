using System;
using System.Reactive.Disposables;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace SimpleVolumeMixer.Core.Helper.Utils;

public class PropertyHolder<T> : NotifyPropertyChangedBase, IPropertyHolder, IDisposable
{
    private readonly CompositeDisposable _disposable;

    private readonly Func<T> _newValueGetter;
    private readonly string _propertyName;
    private readonly Action<string> _notifier;

    private T _value;

    public T Value
    {
        get => _value;
        set => _value = value;
    }

    public PropertyHolder(Func<T> newValueGetter, string propertyName, Action<string> notifier,
        Action<T>? writer = null)
    {
        _disposable = new CompositeDisposable();

        _newValueGetter = newValueGetter;
        _propertyName = propertyName;
        _notifier = notifier;

        Holder = new ReactivePropertySlim<T>(
            newValueGetter(),
            ReactivePropertyMode.DistinctUntilChanged
        ).AddTo(_disposable);
        
        Holder
            .Subscribe(x =>
            {
                writer?.Invoke(x);
                _notifier(_propertyName);
            })
            .AddTo(_disposable);
    }

    public void Refresh()
    {
        Holder.Value = _newValueGetter();
    }

    public void Dispose()
    {
        _disposable.Dispose();
    }
}