using System;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio.Event;

/// <summary>
/// <see cref="EventArgs"/> to be used when an event related to <see cref="AudioSessionAccessor"/> occurs.
/// </summary>
public class AudioSessionAccessorEventArgs : EventArgs
{
    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="session"><see cref="AudioSessionAccessor"/> where the event occurred</param>
    public AudioSessionAccessorEventArgs(AudioSessionAccessor session)
    {
        Session = session;
    }

    /// <summary>
    /// <see cref="AudioSessionAccessor"/> where the event occurred.
    /// </summary>
    public AudioSessionAccessor Session { get; }
}