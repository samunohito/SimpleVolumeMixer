using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using System.Threading;
using DisposableComponents;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace SimpleVolumeMixer.Core.Helper.Component;

/// <summary>
/// Wrapper for exclusive handling of <see cref="ObservableCollection{T}"/>.
/// </summary>
/// <typeparam name="T">element type</typeparam>
public class SynchronizedObservableCollectionWrapper<T> : DisposableComponent, ICollection<T> where T : class
{
    private readonly object _gate = new();
    private readonly ObservableCollection<T> _collection;

    public SynchronizedObservableCollectionWrapper()
    {
        _collection = new ReactiveCollection<T>().AddTo(Disposable);
        ReadOnlyCollection = _collection
            .ToReadOnlyReactiveCollection(UIDispatcherScheduler.Default, false)
            .AddTo(Disposable);
    }

    /// <summary>
    /// This is a read-only collection, linked to the update of the wrapped <see cref="ObservableCollection{T}"/>.
    /// Change notifications are sent to the UI thread.
    /// </summary>
    public ReadOnlyObservableCollection<T> ReadOnlyCollection { get; }

    /// <summary>
    /// A lock object used in the exclusion process. It is made into a property for use in inheritance.
    /// </summary>
    protected object Gate => _gate;

    public IEnumerator<T> GetEnumerator()
    {
        lock (_gate)
        {
            return _collection.GetEnumerator();
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }


    public void Add(T item)
    {
        lock (_gate)
        {
            _collection.Add(item);
        }
    }

    public void Insert(int idx, T item)
    {
        lock (_gate)
        {
            _collection.Insert(idx, item);
        }
    }

    public void Clear()
    {
        lock (_gate)
        {
            _collection.Clear();
        }
    }

    public bool Contains(T item)
    {
        lock (_gate)
        {
            return _collection.Contains(item);
        }
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        lock (_gate)
        {
            _collection.CopyTo(array, arrayIndex);
        }
    }

    public bool Remove(T item)
    {
        lock (_gate)
        {
            return _collection.Remove(item);
        }
    }

    public int Count
    {
        get
        {
            lock (_gate)
            {
                return _collection.Count;
            }
        }
    }

    public bool IsReadOnly => false;
}