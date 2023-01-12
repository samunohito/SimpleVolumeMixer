using CSCore.CoreAudioAPI;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio.Types;


public enum AudioClientShareModeType
{
    /// <summary>
    /// The device will be opened in shared mode and use the WAS format.
    /// </summary>
    Shared = AudioClientShareMode.Shared,
    /// <summary>
    /// The device will be opened in exclusive mode and use the application specified format.
    /// </summary>
    Exclusive = AudioClientShareMode.Exclusive,
}