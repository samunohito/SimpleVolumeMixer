using CSCore.CoreAudioAPI;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio.Types;

/// <summary>
/// The reason that the audio session was disconnected. The caller sets this parameter to one of the AudioSessionDisconnectReason enumeration values shown in the following table.
/// </summary>
/// <remarks>
/// We use information from MSDN and CSCore functions, so please refer to their documentation as well.
/// The document text for some functions is taken from MSDN.
/// <see cref="AudioSessionDisconnectReason"/>
/// https://docs.microsoft.com/ja-jp/windows/win32/api/audiopolicy/nf-audiopolicy-iaudiosessionevents-onsessiondisconnected?redirectedfrom=MSDN
/// </remarks>
public enum AudioSessionDisconnectReasonType
{
    /// <summary>
    /// It is an unknown state that does not belong to any of the states.
    /// This is a value provided for the convenience of the application; the Windows API will not be notified of this value.
    /// </summary>
    Unknown = int.MinValue,

    /// <summary>
    /// The user removed the audio endpoint device.
    /// </summary>
    DisconnectReasonDeviceRemoval = AudioSessionDisconnectReason.DisconnectReasonDeviceRemoval,

    /// <summary>
    /// The Windows audio service has stopped.
    /// </summary>
    DisconnectReasonServerShutdown = AudioSessionDisconnectReason.DisconnectReasonServerShutdown,

    /// <summary>
    /// The stream format changed for the device that the audio session is connected to.
    /// </summary>
    DisconnectReasonFormatChanged = AudioSessionDisconnectReason.DisconnectReasonFormatChanged,

    /// <summary>
    /// The user logged off the Windows Terminal Services (WTS) session that the audio session was running in.
    /// </summary>
    DisconnectReasonSessionLogoff = AudioSessionDisconnectReason.DisconnectReasonSessionLogoff,

    /// <summary>
    /// The WTS session that the audio session was running in was disconnected.
    /// </summary>
    DisconnectReasonSessionDisconnected = AudioSessionDisconnectReason.DisconnectReasonSessionDisconnected,

    /// <summary>
    /// The (shared-mode) audio session was disconnected to make the audio endpoint device available for an exclusive-mode connection.
    /// </summary>
    DisconnectReasonExclusiveModeOverride = AudioSessionDisconnectReason.DisconnectReasonExclusiveModeOverride,
}