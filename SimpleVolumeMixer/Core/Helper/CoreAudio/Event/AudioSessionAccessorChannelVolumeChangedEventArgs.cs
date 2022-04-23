using System;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio.Event;

/// <summary>
/// <see cref="EventArgs"/> to be used when there is a change in the value related to the channel in <see cref="AudioSessionAccessor"/>.
/// </summary>
public class AudioSessionAccessorChannelVolumeChangedEventArgs : AudioSessionAccessorEventArgs
{
    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="session"><see cref="AudioSessionAccessor"/> where the event occurred</param>
    /// <param name="channelCount">Value after change</param>
    /// <param name="channelVolumes">Value after change</param>
    /// <param name="changedChannel">Value after change</param>
    public AudioSessionAccessorChannelVolumeChangedEventArgs(
        AudioSessionAccessor session,
        int channelCount,
        float[] channelVolumes,
        int changedChannel
    ) : base(session)
    {
        ChannelCount = channelCount;
        ChannelVolumes = channelVolumes;
        ChangedChannel = changedChannel;
    }

    /// <summary>
    /// Value after change
    /// </summary>
    public int ChannelCount { get; }

    /// <summary>
    /// Value after change
    /// </summary>
    public float[] ChannelVolumes { get; }

    /// <summary>
    /// Value after change
    /// </summary>
    public int ChangedChannel { get; }
}