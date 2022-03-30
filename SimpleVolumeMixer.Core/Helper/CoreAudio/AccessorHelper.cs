using System.Collections.Generic;
using CSCore.CoreAudioAPI;
using SimpleVolumeMixer.Core.Models.Domain.CoreAudio;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio;

internal static class AccessorHelper
{
    public static readonly IReadOnlyDictionary<DataFlow, DataFlowType> DataFlows =
        new Dictionary<DataFlow, DataFlowType>()
        {
            { DataFlow.Render, DataFlowType.Render },
            { DataFlow.Capture, DataFlowType.Capture },
            { DataFlow.All, DataFlowType.All },
        };

    public static readonly IReadOnlyDictionary<DeviceState, DeviceStateType> DeviceStates =
        new Dictionary<DeviceState, DeviceStateType>()
        {
            { DeviceState.Active, DeviceStateType.Active },
            { DeviceState.Disabled, DeviceStateType.Disabled },
            { DeviceState.NotPresent, DeviceStateType.NotPresent },
            { DeviceState.UnPlugged, DeviceStateType.UnPlugged },
            { DeviceState.All, DeviceStateType.All },
        };

    public static readonly IReadOnlyDictionary<AudioSessionState, AudioSessionStateType> SessionStates =
        new Dictionary<AudioSessionState, AudioSessionStateType>()
        {
            { AudioSessionState.AudioSessionStateInactive, AudioSessionStateType.AudioSessionStateInactive },
            { AudioSessionState.AudioSessionStateActive, AudioSessionStateType.AudioSessionStateActive },
            { AudioSessionState.AudioSessionStateExpired, AudioSessionStateType.AudioSessionStateExpired },
        };

    public static readonly IReadOnlyDictionary<Role, RoleType> Roles =
        new Dictionary<Role, RoleType>()
        {
            { Role.Console, RoleType.Console },
            { Role.Multimedia, RoleType.Multimedia },
            { Role.Communications, RoleType.Communications },
        };

    public static T Unwrap<T>(T? nullable, string propName)
    {
        return (nullable ?? throw new AccessorNotReadyException(propName));
    }
}