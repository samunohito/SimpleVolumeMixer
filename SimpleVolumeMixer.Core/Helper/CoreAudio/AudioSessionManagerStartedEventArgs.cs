using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio;

public class AudioSessionManagerStartedEventArgs : EventArgs
{
    internal AudioSessionManagerStartedEventArgs(IEnumerable<AudioSessionAccessor> sessions)
    {
        Sessions = sessions.ToList();
    }

    public IList<AudioSessionAccessor> Sessions { get; }
}