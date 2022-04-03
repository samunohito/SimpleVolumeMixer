using System;

namespace SimpleVolumeMixer.Core.Helper.Component;

/// <summary>
/// Extends DisposableComponent to provide a mechanism whereby read/write functions can be executed only when the object is not destroyed.
/// </summary>
/// <seealso cref="DisposableComponent"/>
public abstract class SafetyAccessorComponent : DisposableComponent
{
    /// <summary>
    /// ctor.
    /// </summary>
    public SafetyAccessorComponent() : base()
    {
    }

    /// <summary>
    /// Assists in safely reading values from the reader.
    /// If this object is destroyed, the value of defaultValue is returned.
    /// </summary>
    /// <param name="reader">Function to read the value</param>
    /// <param name="defaultValue">Default value for when the object was destroyed</param>
    /// <typeparam name="T">Type of value to be read</typeparam>
    /// <returns>Value read from reader or default value</returns>
    protected virtual T SafeRead<T>(Func<T> reader, T defaultValue)
    {
        return IsDisposed
            ? defaultValue
            : reader();
    }

    /// <summary>
    /// Assists in safely writing the value of param.
    /// If this object is destroyed, no call to writer is made.
    /// </summary>
    /// <param name="writer">Function for writing values</param>
    /// <param name="param">Value to be written</param>
    /// <typeparam name="T">Type of value to be written</typeparam>
    protected virtual void SafeWrite<T>(Action<T> writer, T param)
    {
        if (!IsDisposed)
        {
            writer(param);
        }
    }
}