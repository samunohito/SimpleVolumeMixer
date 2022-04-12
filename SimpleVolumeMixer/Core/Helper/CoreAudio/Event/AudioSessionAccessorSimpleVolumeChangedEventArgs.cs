namespace SimpleVolumeMixer.Core.Helper.CoreAudio.Event;

public class AudioSessionAccessorSimpleVolumeChangedEventArgs : AudioSessionAccessorEventArgs
{
    public AudioSessionAccessorSimpleVolumeChangedEventArgs(AudioSessionAccessor session, float newVolume, bool isMuted)
        :
        base(session)
    {
        NewVolume = newVolume;
        IsMuted = isMuted;
    }

    public float NewVolume { get; }
    public bool IsMuted { get; }
}