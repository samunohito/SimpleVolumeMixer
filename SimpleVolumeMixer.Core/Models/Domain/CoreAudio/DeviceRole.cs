namespace SimpleVolumeMixer.Core.Models.Domain.CoreAudio;

public class DeviceRole
{
    internal DeviceRole()
    {
        Multimedia = false;
        Communications = false;
    }
    
    public bool Multimedia { get; internal set; }
    public bool Communications { get; internal set; }
}