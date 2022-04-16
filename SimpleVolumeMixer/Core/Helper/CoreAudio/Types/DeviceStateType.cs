using CSCore.CoreAudioAPI;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio.Types;

/// <summary>
/// The DEVICE_STATE_XXX constants indicate the current state of an audio endpoint device.
/// </summary>
/// <remarks>
/// We use information from MSDN and CSCore functions, so please refer to their documentation as well.
/// The document text for some functions is taken from MSDN.
/// <see cref="DeviceState"/>
/// https://docs.microsoft.com/en-us/windows/win32/coreaudio/device-state-xxx-constants
/// </remarks>
public enum DeviceStateType
{
    /// <summary>
    /// It is an unknown state that does not belong to any of the states.
    /// This is a value provided for the convenience of the application; the Windows API will not be notified of this value.
    /// </summary>
    Unknown = int.MinValue,

    /// <summary>
    /// The audio endpoint device is active. That is, the audio adapter that connects to the endpoint device is present and enabled. In addition,
    /// if the endpoint device plugs into a jack on the adapter, then the endpoint device is plugged in.
    /// </summary>
    Active = DeviceState.Active,

    /// <summary>
    /// The audio endpoint device is disabled. The user has disabled the device in the Windows multimedia control panel,
    /// Mmsys.cpl. For more information, see Remarks.
    /// </summary>
    Disabled = DeviceState.Disabled,

    /// <summary>
    /// The audio endpoint device is not present because the audio adapter that connects to the endpoint device has been removed from the system,
    /// or the user has disabled the adapter device in Device Manager.
    /// </summary>
    NotPresent = DeviceState.NotPresent,

    /// <summary>
    /// The audio endpoint device is unplugged. The audio adapter that contains the jack for the endpoint device is present and enabled,
    /// but the endpoint device is not plugged into the jack. Only a device with jack-presence detection can be in this state.
    /// For more information about jack-presence detection, see Audio Endpoint Devices.
    /// </summary>
    UnPlugged = DeviceState.UnPlugged,

    /// <summary>
    /// Includes audio endpoint devices in all states active, disabled, not present, and unplugged.
    /// </summary>
    All = DeviceState.All
}