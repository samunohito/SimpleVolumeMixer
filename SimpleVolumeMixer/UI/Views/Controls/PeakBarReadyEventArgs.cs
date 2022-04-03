using System;

namespace SimpleVolumeMixer.UI.Views.Controls;

public class SoundBarReadyEventArgs : EventArgs
{
    public SoundBarReadyEventArgs(ISoundBarHandler handler)
    {
        Handler = handler;
    }

    public ISoundBarHandler Handler { get; }
}