using CSCore.CoreAudioAPI;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio.Types;

public enum AudioSessionStateType
{
    Unknown = int.MinValue,
    AudioSessionStateInactive = AudioSessionState.AudioSessionStateInactive,
    AudioSessionStateActive = AudioSessionState.AudioSessionStateActive,
    AudioSessionStateExpired = AudioSessionState.AudioSessionStateExpired
}