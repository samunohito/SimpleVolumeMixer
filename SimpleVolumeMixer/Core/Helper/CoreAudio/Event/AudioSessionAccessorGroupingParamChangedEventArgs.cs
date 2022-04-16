using System;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio.Event;

public class AudioSessionAccessorGroupingParamChangedEventArgs : AudioSessionAccessorEventArgs
{
    public AudioSessionAccessorGroupingParamChangedEventArgs(
        AudioSessionAccessor session,
        Guid newGroupingParam
    ) : base(session)
    {
        NewGroupingParam = newGroupingParam;
    }

    public Guid NewGroupingParam { get; }
}