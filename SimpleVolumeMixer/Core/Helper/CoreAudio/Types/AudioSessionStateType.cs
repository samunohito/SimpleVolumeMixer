using CSCore.CoreAudioAPI;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio.Types;

/// <summary>
/// The AudioSessionState enumeration defines constants that indicate the current state of an audio session.
/// </summary>
/// <remarks>
/// We use information from MSDN and CSCore functions, so please refer to their documentation as well.
/// The document text for some functions is taken from MSDN.
/// <see cref="AudioSessionState"/>
/// https://docs.microsoft.com/en-us/windows/win32/api/audiosessiontypes/ne-audiosessiontypes-audiosessionstate
/// </remarks>
public enum AudioSessionStateType
{
    /// <summary>
    /// It is an unknown state that does not belong to any of the states.
    /// This is a value provided for the convenience of the application; the Windows API will not be notified of this value.
    /// </summary>
    Unknown = int.MinValue,

    /// <summary>
    /// The audio session is inactive. (It contains at least one stream, but none of the streams in the session is currently running.)
    /// </summary>
    AudioSessionStateInactive = AudioSessionState.AudioSessionStateInactive,

    /// <summary>
    /// The audio session is active. (At least one of the streams in the session is running.)
    /// </summary>
    AudioSessionStateActive = AudioSessionState.AudioSessionStateActive,

    /// <summary>
    /// The audio session has expired. (It contains no streams.)
    /// </summary>
    AudioSessionStateExpired = AudioSessionState.AudioSessionStateExpired
}