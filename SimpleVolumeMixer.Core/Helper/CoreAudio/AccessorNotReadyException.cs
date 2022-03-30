using System;

namespace SimpleVolumeMixer.Core.Helper.CoreAudio;

public class AccessorNotReadyException : Exception
{
    public AccessorNotReadyException(string msg) : base(msg)
    {
        
    }
}