using System;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio.Event;

public class AudioSessionAccessorEventArgs : EventArgs
{
    public AudioSessionAccessorEventArgs(AudioSessionAccessor session)
    {
        Session = session;
    }

    public AudioSessionAccessor Session { get; }
}