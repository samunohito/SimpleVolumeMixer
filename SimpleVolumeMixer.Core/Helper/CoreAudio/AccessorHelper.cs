namespace SimpleVolumeMixer.Core.Helper.CoreAudio;

public static class AccessorHelper
{
    public static T Unwrap<T>(T? nullable, string propName)
    {
        return (nullable ?? throw new AccessorNotReadyException(propName));
    }
}