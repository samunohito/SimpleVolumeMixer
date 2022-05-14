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
public class SynchronizedObservableCollectionWrapper<T> : DisposableComponent where T : class
{
    private readonly object _gate = new();

    public SynchronizedObservableCollectionWrapper()
    {
        Collection = new ReactiveCollection<T>().AddTo(Disposable);
        ReadOnlyCollection = Collection
            .ToReadOnlyReactiveCollection(disposeElement: false)
            .AddTo(Disposable);
    }
    
    /// <summary>
    /// Managed ObservableCollection
    /// </summary>
    protected ObservableCollection<T> Collection { get; }

    /// <summary>
    /// This is a read-only collection, linked to the update of the wrapped <see cref="ObservableCollection{T}"/>.
    /// </summary>
    public ReadOnlyObservableCollection<T> ReadOnlyCollection { get; }

    /// <summary>
    /// A lock object used in the exclusion process. It is made into a property for use in inheritance.
    /// </summary>
    protected object Gate => _gate;

    public void Add(T item)
    {
        lock (_gate)
        {
            Collection.Add(item);
        }
    }

    public void Insert(int idx, T item)
    {
        lock (_gate)
        {
            Collection.Insert(idx, item);
        }
    }

    public void Clear()
    {
        lock (_gate)
        {
            Collection.Clear();
        }
    }

    public bool Contains(T item)
    {
        lock (_gate)
        {
            return Collection.Contains(item);
        }
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        lock (_gate)
        {
            Collection.CopyTo(array, arrayIndex);
        }
    }

    public bool Remove(T item)
    {
        lock (_gate)
        {
            return Collection.Remove(item);
        }
    }

    public int Count
    {
        get
        {
            lock (_gate)
            {
                return Collection.Count;
            }
        }
    }

    public bool IsReadOnly => false;
}