using CSCore.CoreAudioAPI;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio.Types;

public enum DeviceStateType
{
    Unknown = int.MinValue,
    Active = DeviceState.Active,
    Disabled = DeviceState.Disabled,
    NotPresent = DeviceState.NotPresent,
    UnPlugged = DeviceState.UnPlugged,
    All = DeviceState.All
}