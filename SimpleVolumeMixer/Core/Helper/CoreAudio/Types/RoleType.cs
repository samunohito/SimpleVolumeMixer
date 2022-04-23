using CSCore.CoreAudioAPI;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio.Types;

/// <summary>
/// The ERole enumeration defines constants that indicate the role that the system has assigned to an audio endpoint device.
/// </summary>
/// <remarks>
/// We use information from MSDN and CSCore functions, so please refer to their documentation as well.
/// The document text for some functions is taken from MSDN.
/// <see cref="Role"/>
/// https://docs.microsoft.com/en-us/windows/win32/api/Mmdeviceapi/ne-mmdeviceapi-erole
/// </remarks>
public enum RoleType
{
    /// <summary>
    /// It is an unknown state that does not belong to any of the states.
    /// This is a value provided for the convenience of the application; the Windows API will not be notified of this value.
    /// </summary>
    Unknown = int.MinValue,

    /// <summary>
    /// Games, system notification sounds, and voice commands.
    /// </summary>
    Console = Role.Console,

    /// <summary>
    /// Music, movies, narration, and live music recording.
    /// </summary>
    Multimedia = Role.Multimedia,

    /// <summary>
    /// Voice communications (talking to another person).
    /// </summary>
    Communications = Role.Communications
}