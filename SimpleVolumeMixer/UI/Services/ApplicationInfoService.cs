using System;
using System.Diagnostics;
using System.Reflection;
using SimpleVolumeMixer.UI.Contracts.Services;

namespace SimpleVolumeMixer.UI.Services;

public class ApplicationInfoService : IApplicationInfoService
{
    public Version GetVersion()
    {
        // Set the app version in SimpleVolumeMixer > Properties > Package > PackageVersion
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var version = FileVersionInfo.GetVersionInfo(assemblyLocation).FileVersion;
        return new Version(version);
    }
}