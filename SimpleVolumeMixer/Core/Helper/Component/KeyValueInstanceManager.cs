using System;
using System.Collections.Generic;

namespace SimpleVolumeMixer.Core.Helper.Component;

/// <summary>
/// This component is used to ensure that the TK and TV values are always 1-1.
/// The key must implement <see cref="IDisposableComponent"/> so that the value can also be destroyed when the key is destroyed.
/// </summary>
/// <typeparam name="TK">Key value type</typeparam>
/// <typeparam name="TV">Type corresponding to TK</typeparam>
public class KeyValueInstanceManager<TK, TV> : DisposableComponent where TK : IDisposableComponent
{
    private readonly object _gate = new();
    private readonly IDictionary<TK, TV> _instances;
    private readonly Func<TK, TV> _factory;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="factory">Function to generate the value corresponding to a key</param>
    public KeyValueInstanceManager(Func<TK, TV> factory)
    {
        _instances = new Dictionary<TK, TV>();
        _factory = factory;
    }

    /// <summary>
    /// If the value corresponding to the key already exists, return that value; if not, create and return a new value.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public TV Obtain(TK key)
    {
        lock (_gate)
        {
            if (_instances.ContainsKey(key))
            {
                return _instances[key];
            }

            var value = _factory(key);
            _instances.Add(key, value);

            key.Disposed += KeyOnDisposed;

            return value;
        }
    }

    /// <summary>
    /// Process to erase the value from this instance when the key is destroyed.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void KeyOnDisposed(object? sender, EventArgs e)
    {
        if (sender == null)
        {
            return;
        }

        lock (_gate)
        {
            var key = (TK)sender;
            key.Disposed -= KeyOnDisposed;

            if (_instances.ContainsKey(key))
            {
                _instances.Remove(key);
            }
        }
    }
}