using System;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Types;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio.Event;

/// <summary>
/// <see cref="EventArgs"/> used when disconnection notification is received from <see cref="AudioSessionAccessor"/>.
/// </summary>
public class AudioSessionAccessorDisconnectedEventArgs : AudioSessionAccessorEventArgs
{
    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="session"><see cref="AudioSessionAccessor"/> where the event occurred</param>
    /// <param name="disconnectReason">Reason for disconnection</param>
    public AudioSessionAccessorDisconnectedEventArgs(
        AudioSessionAccessor session,
        AudioSessionDisconnectReasonType disconnectReason
    ) :
        base(session)
    {
        DisconnectReason = disconnectReason;
    }

    /// <summary>
    /// Reason for disconnection
    /// </summary>
    public AudioSessionDisconnectReasonType DisconnectReason { get; }
}