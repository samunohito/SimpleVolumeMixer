using System.Collections;
using System.Collections.Generic;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace SimpleVolumeMixer.Core.Helper.Component;

public class SynchronizedReactiveCollectionWrapper<T> : DisposableComponent, ICollection<T> where T : class
{
    private readonly object _gate = new();
    private readonly ReactiveCollection<T> _collection;

    public SynchronizedReactiveCollectionWrapper()
    {
        _collection = new ReactiveCollection<T>().AddTo(Disposable);
        ReadOnlyCollection = _collection.ToReadOnlyReactiveCollection(UIDispatcherScheduler.Default, false);
    }

    public ReadOnlyReactiveCollection<T> ReadOnlyCollection { get; }

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