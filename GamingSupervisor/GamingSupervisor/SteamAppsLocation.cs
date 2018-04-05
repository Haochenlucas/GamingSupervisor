using Microsoft.Win32;
using System;
using System.IO;

namespace GamingSupervisor
{
    static class SteamAppsLocation
    {
        private static string location = null;

        public static string Get()
        {
            if (location != null)
                return location;

            RegistryKey regKey = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
            if (regKey != null)
            {
                return FindPath(regKey);
            }

            regKey = Registry.CurrentUser.OpenSubKey(@"Software\Wow6432Node\Valve\Steam");
            if (regKey != null)
            {
                return FindPath(regKey);
            }

#if DEBUG
            return "./../../debug";
#endif

            throw new Exception("No registry entry for Steam. Is Steam installed?");
        }

        private static string FindPath(RegistryKey regKey)
        {
            location = regKey.GetValue("SteamPath").ToString();
            foreach (string line in File.ReadLines(Path.Combine(location, "steamapps/libraryfolders.vdf")))
            {
                if (line.Contains("\"1\""))
                {
                    location = line.Replace("\"1\"", "").Trim().Replace("\"", "");
                    break;
                }
            }
            location = Path.Combine(location, "steamapps/common/dota 2 beta/game/dota");
            return location;
        }
    }
}
