using System;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Types;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio.Event;

/// <summary>
/// <see cref="EventArgs"/> used when the state of <see cref="AudioSessionAccessor"/> changes.
/// </summary>
public class AudioSessionAccessorStateChangedEventArgs : AudioSessionAccessorEventArgs
{
    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="session"><see cref="AudioSessionAccessor"/> where the event occurred</param>
    /// <param name="stateType">New state</param>
    public AudioSessionAccessorStateChangedEventArgs(
        AudioSessionAccessor session,
        AudioSessionStateType stateType
    ) : base(session)
    {
        NewState = stateType;
    }

    /// <summary>
    /// New state
    /// </summary>
    public AudioSessionStateType NewState { get; }
}