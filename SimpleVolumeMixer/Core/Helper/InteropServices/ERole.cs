namespace SimpleVolumeMixer.Core.Helper.InteropServices;

/**
 * Copied By https://github.com/SomeProgrammerGuy/Powershell-Default-Audio-Device-Changer/blob/master/Set-DefaultAudioDevice.ps1
 * 
 * MIT License
 * Copyright (c) 2019 William Humphreys
 * https://github.com/SomeProgrammerGuy/Powershell-Default-Audio-Device-Changer/blob/master/LICENSE
 */
public enum ERole : uint
{
    Console = 0,
    Multimedia = 1,
    Communications = 2
}