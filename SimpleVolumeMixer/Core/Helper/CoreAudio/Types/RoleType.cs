using CSCore.CoreAudioAPI;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio.Types;

public enum RoleType
{
    Unknown = int.MinValue,
    Console = Role.Console,
    Multimedia = Role.Multimedia,
    Communications = Role.Communications
}