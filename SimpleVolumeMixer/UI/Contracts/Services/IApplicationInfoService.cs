using System;

namespace SimpleVolumeMixer.UI.Contracts.Services;

public interface IApplicationInfoService
{
    string GetAssemblyProductVersion();
}