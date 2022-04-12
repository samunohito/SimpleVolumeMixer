using System;
using System.Collections.Generic;
using System.Reactive.Disposables;

namespace SimpleVolumeMixer.Core.Helper.Component;

/// <summary>
/// Concrete class for IDisposableComponent. See Interface for detailed behavior.
/// </summary>
/// <seealso cref="IDisposableComponent"/>
public abstract class DisposableComponent : IDisposableComponent
{
    /// <inheritdoc cref="IDisposableComponent.Disposing"/>
    public event EventHandler<EventArgs>? Disposing;

    /// <inheritdoc cref="IDisposableComponent.Disposed"/>
    public event EventHandler<EventArgs>? Disposed;

    /// <summary>
    /// An external component that delegates the entire destruction process.
    /// </summary>
    private readonly CompositeDisposable _disposable;

    /// <summary>
    /// ctor.
    /// </summary>
    public DisposableComponent()
    {
        _disposable = new CompositeDisposable();
    }

    /// <inheritdoc cref="IDisposableComponent.Disposable"/>
    public ICollection<IDisposable> Disposable => _disposable;

    /// <inheritdoc cref="IDisposableComponent.IsDisposed"/>
    public bool IsDisposed => _disposable.IsDisposed;

    /// <summary>
    /// Called before the object is destroyed by a call to <code>Dispose()</code>.
    /// </summary>
    protected virtual void OnDisposing()
    {
        Disposing?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Called after an object has been destroyed by a call to <code>Dispose()</code>.
    /// </summary>
    protected virtual void OnDisposed()
    {
        Disposed?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc cref="IDisposableComponent.Dispose"/>
    public void Dispose()
    {
        if (_disposable.IsDisposed)
        {
            return;
        }

        OnDisposing();
        _disposable.Dispose();
        OnDisposed();
    }

    /// <inheritdoc cref="IDisposableComponent.Dispose"/>
    void IDisposable.Dispose()
    {
        Dispose();
    }
}