using CSCore.CoreAudioAPI;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio.Types;

public enum DataFlowType
{
    Unknown = int.MinValue,
    Render = DataFlow.Render,
    Capture = DataFlow.Capture,
    All = DataFlow.All
}