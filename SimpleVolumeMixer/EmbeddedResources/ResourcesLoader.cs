using System;
using System.IO;
using System.Reflection;

namespace SimpleVolumeMixer.EmbeddedResources;

public class ResourcesLoader
{
    private const string NameSpace = "SimpleVolumeMixer.EmbeddedResources.";

    public static readonly ResourcesLoader MaterialVolumeHigh = new(NameSpace + "Material-VolumeHigh.png");
    public static readonly ResourcesLoader MaterialVolumeOff = new(NameSpace + "Material-VolumeOff.png");
    public static readonly ResourcesLoader MaterialVolumeAudio = new(NameSpace + "Microns-Audio.png");
    public static readonly ResourcesLoader TypiconsDeviceDesktop = new(NameSpace + "Typicons-DeviceDesktop.png");

    private readonly Lazy<byte[]> _lazy;

    private ResourcesLoader(string path)
    {
        _lazy = new Lazy<byte[]>(() => Loader(path));
    }

    public byte[] GetBytes()
    {
        return _lazy.Value;
    }

    public Stream GetStream()
    {
        return new MemoryStream(GetBytes());
    }

    private static byte[] Loader(string path)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path) ??
                           throw new InvalidDataException();
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }
}