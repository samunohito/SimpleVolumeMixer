namespace SimpleVolumeMixer.Core.Helper.CoreAudio.Event;

public class AudioSessionAccessorChannelVolumeChangedEventArgs : AudioSessionAccessorEventArgs
{
    public AudioSessionAccessorChannelVolumeChangedEventArgs(AudioSessionAccessor session, int channelCount,
        float[] channelVolumes, int changedChannel) :
        base(session)
    {
        ChannelCount = channelCount;
        ChannelVolumes = channelVolumes;
        ChangedChannel = changedChannel;
    }

    public int ChannelCount { get; }
    public float[] ChannelVolumes { get; }
    public int ChangedChannel { get; }
}