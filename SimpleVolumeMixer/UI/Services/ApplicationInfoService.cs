﻿using System.Diagnostics;
using System.Reflection;
using SimpleVolumeMixer.UI.Contracts.Services;

namespace SimpleVolumeMixer.UI.Services;

public class ApplicationInfoService : IApplicationInfoService
{
    public string GetAssemblyProductVersion()
    {
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var version = FileVersionInfo.GetVersionInfo(assemblyLocation).ProductVersion;
        return version;
    }
}