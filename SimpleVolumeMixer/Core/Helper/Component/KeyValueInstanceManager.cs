using System;
using System.Collections.Generic;

namespace SimpleVolumeMixer.Core.Helper.Component;

public class KeyValueInstanceManager<TK, TV> : DisposableComponent where TK : IDisposableComponent
{
    private readonly object _gate = new();
    private readonly IDictionary<TK, TV> _instances;
    private readonly Func<TK, TV> _factory;

    public KeyValueInstanceManager(Func<TK, TV> factory)
    {
        _instances = new Dictionary<TK, TV>();
        _factory = factory;
    }

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