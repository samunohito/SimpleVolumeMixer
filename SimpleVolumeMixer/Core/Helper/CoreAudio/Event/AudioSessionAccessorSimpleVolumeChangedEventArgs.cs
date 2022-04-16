using System;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio.Event;

/// <summary>
/// <see cref="EventArgs"/> used when <see cref="AudioSessionAccessor"/> volume or mute state is changed.
/// </summary>
public class AudioSessionAccessorSimpleVolumeChangedEventArgs : AudioSessionAccessorEventArgs
{
    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="session"><see cref="AudioSessionAccessor"/> where the event occurred</param>
    /// <param name="newVolume">New volume</param>
    /// <param name="isMuted">New mute state</param>
    public AudioSessionAccessorSimpleVolumeChangedEventArgs(
        AudioSessionAccessor session,
        float newVolume,
        bool isMuted
    ) : base(session)
    {
        NewVolume = newVolume;
        IsMuted = isMuted;
    }

    /// <summary>
    /// New volume
    /// </summary>
    public float NewVolume { get; }

    /// <summary>
    /// New mute state
    /// </summary>
    public bool IsMuted { get; }
}