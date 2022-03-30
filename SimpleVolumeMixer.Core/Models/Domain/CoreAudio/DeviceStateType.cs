namespace SimpleVolumeMixer.Core.Models.Domain.CoreAudio
{
    public enum DeviceStateType
    {
        Unknown = -1,
        /// <summary>
        /// The audio endpoint device is active. That is, the audio adapter that connects to the endpoint device is present and enabled. In addition, if the endpoint device plugs into a jack on the adapter, then the endpoint device is plugged in.
        /// </summary>
        Active = 1,
        /// <summary>
        /// The audio endpoint device is disabled. The user has disabled the device in the Windows multimedia control panel, Mmsys.cpl. For more information, see Remarks.
        /// </summary>
        Disabled = 2,
        /// <summary>
        /// he audio endpoint device is not present because the audio adapter that connects to the endpoint device has been removed from the system, or the user has disabled the adapter device in Device Manager.
        /// </summary>
        NotPresent = 4,
        /// <summary>
        /// The audio endpoint device is unplugged. The audio adapter that contains the jack for the endpoint device is present and enabled, but the endpoint device is not plugged into the jack. Only a device with jack-presence detection can be in this state.
        /// </summary>
        UnPlugged = 8,
        /// <summary>
        /// Includes audio endpoint devices in all states—active, disabled, not present, and unplugged.
        /// </summary>
        All = UnPlugged | NotPresent | Disabled | Active, // 0x0000000F
    }
}