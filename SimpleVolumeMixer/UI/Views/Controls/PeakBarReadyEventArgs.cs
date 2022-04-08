using System;

namespace SimpleVolumeMixer.UI.Views.Controls;

public class PeakBarReadyEventArgs : EventArgs
{
    public PeakBarReadyEventArgs(IPeakBarHandler handler)
    {
        Handler = handler;
    }

    public IPeakBarHandler Handler { get; }
}