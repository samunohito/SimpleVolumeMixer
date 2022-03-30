using System;
using SimpleVolumeMixer.Core.Models.Domain.CoreAudio;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio;

public class AudioSessionStateChangedEventArgs : EventArgs
{
    public AudioSessionStateChangedEventArgs(AudioSessionStateType stateType)
    {
        NewState = stateType;
    }
    
    public AudioSessionStateType NewState { get; }
}