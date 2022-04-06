namespace SimpleVolumeMixer.Core.Helper.CoreAudio.Event;

public class AudioSessionAccessorIconPathChangedEventArgs : AudioSessionAccessorEventArgs
{
    public AudioSessionAccessorIconPathChangedEventArgs(AudioSessionAccessor session, string newIconPath) :
        base(session)
    {
        NewIconPath = newIconPath;
    }

    public string NewIconPath { get; }
}