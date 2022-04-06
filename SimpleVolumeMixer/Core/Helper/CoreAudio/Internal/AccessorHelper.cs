using System.Collections.Generic;
using System.Linq;
using CSCore.CoreAudioAPI;
using SimpleVolumeMixer.Core.Helper.CoreAudio.Types;
using SimpleVolumeMixer.Core.Models.Domain.CoreAudio;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio.Internal;

internal static class AccessorHelper
{
    public static readonly IReadOnlyDictionary<DataFlow, DataFlowType> DataFlows =
        new Dictionary<DataFlow, DataFlowType>
        {
            { DataFlow.Render, DataFlowType.Render },
            { DataFlow.Capture, DataFlowType.Capture },
            { DataFlow.All, DataFlowType.All }
        };

    public static readonly IReadOnlyDictionary<DataFlowType, DataFlow> DataFlowsRev =
        DataFlows.ToDictionary(x => x.Value, x => x.Key);

    public static readonly IReadOnlyDictionary<DeviceState, DeviceStateType> DeviceStates =
        new Dictionary<DeviceState, DeviceStateType>
        {
            { DeviceState.Active, DeviceStateType.Active },
            { DeviceState.Disabled, DeviceStateType.Disabled },
            { DeviceState.NotPresent, DeviceStateType.NotPresent },
            { DeviceState.UnPlugged, DeviceStateType.UnPlugged },
            { DeviceState.All, DeviceStateType.All }
        };

    public static readonly IReadOnlyDictionary<DeviceStateType, DeviceState> DeviceStatesRev =
        DeviceStates.ToDictionary(x => x.Value, x => x.Key);

    public static readonly IReadOnlyDictionary<AudioSessionState, AudioSessionStateType> SessionStates =
        new Dictionary<AudioSessionState, AudioSessionStateType>
        {
            { AudioSessionState.AudioSessionStateInactive, AudioSessionStateType.AudioSessionStateInactive },
            { AudioSessionState.AudioSessionStateActive, AudioSessionStateType.AudioSessionStateActive },
            { AudioSessionState.AudioSessionStateExpired, AudioSessionStateType.AudioSessionStateExpired }
        };

    public static readonly IReadOnlyDictionary<AudioSessionStateType, AudioSessionState> SessionStatesRev =
        SessionStates.ToDictionary(x => x.Value, x => x.Key);

    public static readonly IReadOnlyDictionary<Role, RoleType> Roles =
        new Dictionary<Role, RoleType>
        {
            { Role.Console, RoleType.Console },
            { Role.Multimedia, RoleType.Multimedia },
            { Role.Communications, RoleType.Communications }
        };

    public static readonly IReadOnlyDictionary<RoleType, Role> RolesRev =
        Roles.ToDictionary(x => x.Value, x => x.Key);

    public static readonly IReadOnlyDictionary<AudioSessionDisconnectReason, AudioSessionDisconnectReasonType>
        SessionDisconnectReasons =
            new Dictionary<AudioSessionDisconnectReason, AudioSessionDisconnectReasonType>
            {
                {
                    AudioSessionDisconnectReason.DisconnectReasonDeviceRemoval,
                    AudioSessionDisconnectReasonType.DisconnectReasonDeviceRemoval
                },
                {
                    AudioSessionDisconnectReason.DisconnectReasonServerShutdown,
                    AudioSessionDisconnectReasonType.DisconnectReasonServerShutdown
                },
                {
                    AudioSessionDisconnectReason.DisconnectReasonFormatChanged,
                    AudioSessionDisconnectReasonType.DisconnectReasonFormatChanged
                },
                {
                    AudioSessionDisconnectReason.DisconnectReasonSessionLogoff,
                    AudioSessionDisconnectReasonType.DisconnectReasonSessionLogoff
                },
                {
                    AudioSessionDisconnectReason.DisconnectReasonSessionDisconnected,
                    AudioSessionDisconnectReasonType.DisconnectReasonSessionDisconnected
                },
                {
                    AudioSessionDisconnectReason.DisconnectReasonExclusiveModeOverride,
                    AudioSessionDisconnectReasonType.DisconnectReasonExclusiveModeOverride
                }
            };

    public static readonly IReadOnlyDictionary<AudioSessionDisconnectReasonType, AudioSessionDisconnectReason>
        SessionDisconnectReasonsRev =
            SessionDisconnectReasons.ToDictionary(x => x.Value, x => x.Key);
}