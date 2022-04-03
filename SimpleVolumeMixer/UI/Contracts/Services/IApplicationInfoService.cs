using System;

namespace SimpleVolumeMixer.UI.Contracts.Services;

public interface IApplicationInfoService
{
    Version GetVersion();
}