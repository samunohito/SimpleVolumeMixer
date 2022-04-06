using SimpleVolumeMixer.Core.Helper.CoreAudio.Types;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio.Event;

public class AudioSessionAccessorDisconnectedEventArgs : AudioSessionAccessorEventArgs
{
    public AudioSessionAccessorDisconnectedEventArgs(AudioSessionAccessor session,
        AudioSessionDisconnectReasonType disconnectReason) :
        base(session)
    {
        DisconnectReason = disconnectReason;
    }

    public AudioSessionDisconnectReasonType DisconnectReason { get; }
}