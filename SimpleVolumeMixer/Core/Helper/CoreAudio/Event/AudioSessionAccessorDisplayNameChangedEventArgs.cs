namespace SimpleVolumeMixer.Core.Helper.CoreAudio.Event;

/// <summary>
/// 
/// </summary>
public class AudioSessionAccessorDisplayNameChangedEventArgs : AudioSessionAccessorEventArgs
{
    public AudioSessionAccessorDisplayNameChangedEventArgs(
        AudioSessionAccessor session,
        string newDisplayName
    ) : base(session)
    {
        NewDisplayName = newDisplayName;
    }

    public string NewDisplayName { get; }
}