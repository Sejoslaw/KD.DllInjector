using System.Configuration;
using System.IO;

namespace KD.DllInjector
{
    /// <summary>
    /// Holds injector settings.
    /// </summary>
    internal static class Settings
    {
        internal static string PathToDll => GetPathToDll();
        internal static string ProcessName => GetConfigValue("ProcessName");

        private static string GetPathToDll()
        {
            string path = GetConfigValue("PathToDll");
            path = Path.Combine(path, "");
            return path;
        }

        private static string GetConfigValue(string key)
        {
            string value = ConfigurationManager.AppSettings[key];
            return value;
        }
    }
}