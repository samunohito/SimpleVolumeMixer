using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SimpleVolumeMixer.Core.Helper.InteropServices;

/**
 * Copied By https://github.com/SomeProgrammerGuy/Powershell-Default-Audio-Device-Changer/blob/master/Set-DefaultAudioDevice.ps1
 * 
 * MIT License
 * Copyright (c) 2019 William Humphreys
 * https://github.com/SomeProgrammerGuy/Powershell-Default-Audio-Device-Changer/blob/master/LICENSE
 */
public class PolicyConfigClient
{
    public static int SetDefaultDevice(string deviceId)
    {
        return SetDefaultDevice(deviceId, new[]
        {
            ERole.Console,
            ERole.Multimedia,
            ERole.Communications,
        });
    }

    public static int SetDefaultDevice(string deviceId, ERole role)
    {
        return SetDefaultDevice(deviceId, new[] { role });
    }

    public static int SetDefaultDevice(string deviceId, IEnumerable<ERole> roles)
    {
        // ReSharper disable once SuspiciousTypeConversion.Global
        var policyConfigClient = (new _CPolicyConfigClient() as IPolicyConfig);
        if (policyConfigClient == null)
        {
            return 1;
        }

        try
        {
            foreach (var role in roles)
            {
                Marshal.ThrowExceptionForHR(policyConfigClient.SetDefaultEndpoint(deviceId, role));
            }

            return 0;
        }
        catch
        {
            return 1;
        }
    }
}