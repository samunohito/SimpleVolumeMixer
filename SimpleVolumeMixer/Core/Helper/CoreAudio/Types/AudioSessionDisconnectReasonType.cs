using CSCore.CoreAudioAPI;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio.Types;

public enum AudioSessionDisconnectReasonType
{
    DisconnectReasonDeviceRemoval = AudioSessionDisconnectReason.DisconnectReasonDeviceRemoval,
    DisconnectReasonServerShutdown = AudioSessionDisconnectReason.DisconnectReasonServerShutdown,
    DisconnectReasonFormatChanged = AudioSessionDisconnectReason.DisconnectReasonFormatChanged,
    DisconnectReasonSessionLogoff = AudioSessionDisconnectReason.DisconnectReasonSessionLogoff,
    DisconnectReasonSessionDisconnected = AudioSessionDisconnectReason.DisconnectReasonSessionDisconnected,
    DisconnectReasonExclusiveModeOverride = AudioSessionDisconnectReason.DisconnectReasonExclusiveModeOverride,
}