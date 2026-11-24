using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace SamsungTouchUtility.Services
{
    public interface IStartupService
    {
        bool IsStartupEnabled();
        bool SetStartup(bool enable);
    }

    public sealed class StartupService : IStartupService
    {
        private const string RunRegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string AppRegistryValueName = "SamsungTouchUtility";

        public bool IsStartupEnabled()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, false);
                return key?.GetValue(AppRegistryValueName) != null;
            }
            catch
            {
                return false;
            }
        }

        public bool SetStartup(bool enable)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, true);
                if (key == null)
                    return false;

                if (enable)
                {
                    string exePath = Process.GetCurrentProcess().MainModule?.FileName 
                        ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SamsungTouchUtility.exe");
                    key.SetValue(AppRegistryValueName, $"\"{exePath}\"");
                }
                else
                {
                    key.DeleteValue(AppRegistryValueName, false);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
