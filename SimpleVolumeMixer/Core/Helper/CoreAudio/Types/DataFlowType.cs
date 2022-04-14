using CSCore.CoreAudioAPI;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio.Types;

/// <summary>
/// The EDataFlow enumeration defines constants that indicate the direction in which audio data flows between an audio endpoint device and an application.
/// </summary>
/// <remarks>
/// We use information from MSDN and CSCore functions, so please refer to their documentation as well.
/// The document text for some functions is taken from MSDN.
/// <see cref="DataFlow"/>
/// https://docs.microsoft.com/en-us/windows/win32/api/mmdeviceapi/ne-mmdeviceapi-edataflow
/// </remarks>
public enum DataFlowType
{
    /// <summary>
    /// It is an unknown state that does not belong to any of the states.
    /// This is a value provided for the convenience of the application; the Windows API will not be notified of this value.
    /// </summary>
    Unknown = int.MinValue,

    /// <summary>
    /// Audio rendering stream. Audio data flows from the application to the audio endpoint device, which renders the stream.
    /// </summary>
    Render = DataFlow.Render,

    /// <summary>
    /// Audio capture stream. Audio data flows from the audio endpoint device that captures the stream, to the application.
    /// </summary>
    Capture = DataFlow.Capture,

    /// <summary>
    /// Audio rendering or capture stream. Audio data can flow either from the application to the audio endpoint device, or from the audio endpoint device to the application.
    /// </summary>
    All = DataFlow.All
}