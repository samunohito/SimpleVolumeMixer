namespace SimpleVolumeMixer.Core.Helper.CoreAudio.Event;

public class AudioSessionAccessorDisplayNameChangedEventArgs : AudioSessionAccessorEventArgs
{
    public AudioSessionAccessorDisplayNameChangedEventArgs(AudioSessionAccessor session, string newDisplayName) :
        base(session)
    {
        NewDisplayName = newDisplayName;
    }

    public string NewDisplayName { get; }
}