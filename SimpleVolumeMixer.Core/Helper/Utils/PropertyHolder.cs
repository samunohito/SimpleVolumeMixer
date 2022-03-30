using System;
using System.Reactive.Disposables;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace SimpleVolumeMixer.Core.Helper.Utils;

public class PropertyHolder<T> : IPropertyHolder, IDisposable
{
    private readonly CompositeDisposable _disposable;
    
    private readonly Func<T> _newValueGetter;
    private readonly string _propertyName;
    private readonly Action<string> _notifier;

    public IReactiveProperty<T> Holder { get; }

    public PropertyHolder(Func<T> newValueGetter, string propertyName, Action<string> notifier, Action<T>? writer = null)
    {
        _disposable = new CompositeDisposable();
        
        _newValueGetter = newValueGetter;
        _propertyName = propertyName;
        _notifier = notifier;

        Holder = new ReactivePropertySlim<T>().AddTo(_disposable);
        Holder
            .Subscribe(x => writer?.Invoke(x))
            .AddTo(_disposable);
    }

    public void Refresh()
    {
        var oldValue = Holder.Value;
        var newValue = _newValueGetter();
        if (Equals(oldValue, newValue))
        {
            return;
        }

        Holder.Value = newValue;
        _notifier(_propertyName);
    }

    public void Dispose()
    {
        _disposable.Dispose();
    }
}