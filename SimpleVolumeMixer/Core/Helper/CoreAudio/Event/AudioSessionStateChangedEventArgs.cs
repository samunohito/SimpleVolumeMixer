using SimpleVolumeMixer.Core.Helper.CoreAudio.Types;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio.Event;

public class AudioSessionStateChangedEventArgs : System.EventArgs
{
    public AudioSessionStateChangedEventArgs(AudioSessionStateType stateType)
    {
        NewState = stateType;
    }

    public AudioSessionStateType NewState { get; }
}