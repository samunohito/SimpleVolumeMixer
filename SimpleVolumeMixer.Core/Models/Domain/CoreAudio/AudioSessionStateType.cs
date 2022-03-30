namespace SimpleVolumeMixer.Core.Models.Domain.CoreAudio;

public enum AudioSessionStateType
{
    /// <summary>The session has no active audio streams.</summary>
    AudioSessionStateInactive,
    /// <summary>The session has active audio streams.</summary>
    AudioSessionStateActive,
    /// <summary>The session is dormant.</summary>
    AudioSessionStateExpired,
}