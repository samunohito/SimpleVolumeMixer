using SimpleVolumeMixer.Core.Helper.CoreAudio.Types;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio.Event;

public class AudioSessionAccessorStateChangedEventArgs : AudioSessionAccessorEventArgs
{
    public AudioSessionAccessorStateChangedEventArgs(AudioSessionAccessor session, AudioSessionStateType stateType) :
        base(session)
    {
        NewState = stateType;
    }

    public AudioSessionStateType NewState { get; }
}