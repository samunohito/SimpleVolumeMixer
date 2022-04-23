using System;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio.Event;

/// <summary>
/// <see cref="EventArgs"/> used when there is a change in the IconPath of <see cref="AudioSessionAccessor"/>.
/// </summary>
public class AudioSessionAccessorIconPathChangedEventArgs : AudioSessionAccessorEventArgs
{
    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="session"><see cref="AudioSessionAccessor"/> where the event occurred</param>
    /// <param name="newIconPath">New IconPath</param>
    public AudioSessionAccessorIconPathChangedEventArgs(AudioSessionAccessor session, string newIconPath) :
        base(session)
    {
        NewIconPath = newIconPath;
    }

    /// <summary>
    /// New IconPath
    /// </summary>
    public string NewIconPath { get; }
}