using System;
using System.Collections.Generic;

namespace SimpleVolumeMixer.Core.Helper.Component;

public interface IDisposableComponent : IDisposable
{
    /// <summary>
    /// This event occurs before the destruction process by IDisposable.
    /// </summary>
    event EventHandler<EventArgs>? Disposing;

    /// <summary>
    /// This event occurs after the destruction process by IDisposable.
    /// </summary>
    event EventHandler<EventArgs>? Disposed;

    /// <summary>
    /// Obtains an ICollection that holds IDisposable and allows the IDisposable associated with this object to be destroyed at once.
    /// IDisposable registered with an ICollection that can be retrieved from this property will be destroyed as soon as this object's <code>Dispose()</code> method is called.
    /// </summary>
    ICollection<IDisposable> Disposable { get; }

    /// <summary>
    /// Gets whether the object has been destroyed.
    /// After calling the <code>Dispose()</code> method, this property always returns false.
    /// </summary>
    bool IsDisposed { get; }

    /// <summary>
    /// Calls the object destruction process.
    /// After this function is called, this object cannot be used.
    /// </summary>
    /// <seealso cref="IDisposable.Dispose"/>
    new void Dispose();
}