using CSCore.SoundIn;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio;

public class SoundSource
{
    public SoundSource(ISoundIn soundIn)
    {
        SoundIn = soundIn;
    }

    public ISoundIn SoundIn;
}