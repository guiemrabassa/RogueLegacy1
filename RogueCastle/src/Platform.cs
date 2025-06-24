using System;
using System.IO;
using MonoGame.Framework.Utilities;

namespace RogueCastle
{
    public static class Platform
    {
        public static readonly bool IsMobile = PlatformInfo.MonoGamePlatform.Equals(MonoGamePlatform.Android) || PlatformInfo.MonoGamePlatform.Equals(MonoGamePlatform.iOS);

        public static readonly string OSDir = GetOSDir();
        private static string GetOSDir()
        {
            if (OperatingSystem.IsAndroid())
            {
                return "."; // This seems to work?
            }
            else if (OperatingSystem.IsLinux() || OperatingSystem.IsFreeBSD())
            {
                string osDir = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
                if (string.IsNullOrEmpty(osDir))
                {
                    osDir = Environment.GetEnvironmentVariable("HOME");
                    if (string.IsNullOrEmpty(osDir))
                    {
                        return "."; // Oh well.
                    }
                    else
                    {
                        return Path.Combine(osDir, ".config", "RogueLegacy");
                    }
                }
                return Path.Combine(osDir, "RogueLegacy");
            }
            else if (OperatingSystem.IsMacOS())
            {
                string osDir = Environment.GetEnvironmentVariable("HOME");
                if (string.IsNullOrEmpty(osDir))
                {
                    return "."; // Oh well.
                }
                return Path.Combine(osDir, "Library/Application Support/RogueLegacy");
            }
            else if (OperatingSystem.IsWindows())
            {
                string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                return Path.Combine(appdata, "Rogue Legacy");
            }
            else
            {
                throw new NotSupportedException("Unhandled platform!");
            }
        }
    }
}

