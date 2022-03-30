namespace SimpleVolumeMixer.Core.Models.Domain.CoreAudio
{
    public enum DataFlowType
    {
        /// <summary>
        /// Audio rendering stream. Audio data flows from the application to the audio endpoint device, which renders the stream.
        /// </summary>
        Render,
        /// <summary>
        /// Audio capture stream. Audio data flows from the audio endpoint device that captures the stream, to the application.
        /// </summary>
        Capture,
        /// <summary>
        /// Audio rendering or capture stream. Audio data can flow either from the application to the audio endpoint device, or from the audio endpoint device to the application.
        /// </summary>
        All,
    }
}