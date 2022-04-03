using System.Collections.Generic;
using System.Linq;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio.Event;

public class AudioSessionManagerStartedEventArgs : System.EventArgs
{
    internal AudioSessionManagerStartedEventArgs(IEnumerable<AudioSessionAccessor> sessions)
    {
        Sessions = sessions.ToList();
    }

    public IList<AudioSessionAccessor> Sessions { get; }
}