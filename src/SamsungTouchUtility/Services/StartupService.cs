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
        private const string TaskName = "SamsungTouchUtility";
        private const string RunRegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string AppRegistryValueName = "SamsungTouchUtility";

        public bool IsStartupEnabled()
        {
            try
            {
                // Check Task Scheduler first (preferred for elevated PnP utilities)
                using var p = Process.Start(new ProcessStartInfo
                {
                    FileName = "schtasks.exe",
                    Arguments = $"/query /tn \"{TaskName}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                });
                p?.WaitForExit();
                if (p?.ExitCode == 0)
                    return true;

                // Fallback to registry check
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
            string exePath = Process.GetCurrentProcess().MainModule?.FileName 
                ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SamsungTouchUtility.exe");

            try
            {
                if (enable)
                {
                    // Create Task Scheduler entry with HighestAvailable privilege so it launches at logon without UAC prompts
                    string createArgs = $"/create /tn \"{TaskName}\" /tr \"\\\"{exePath}\\\"\" /sc onlogon /rl highest /f";
                    using var p = Process.Start(new ProcessStartInfo
                    {
                        FileName = "schtasks.exe",
                        Arguments = createArgs,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });
                    p?.WaitForExit();

                    // Also set registry Run key for redundancy
                    using var key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, true);
                    key?.SetValue(AppRegistryValueName, $"\"{exePath}\"");
                    return true;
                }
                else
                {
                    // Delete Task Scheduler entry
                    using var p = Process.Start(new ProcessStartInfo
                    {
                        FileName = "schtasks.exe",
                        Arguments = $"/delete /tn \"{TaskName}\" /f",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });
                    p?.WaitForExit();

                    // Delete registry Run key
                    using var key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, true);
                    key?.DeleteValue(AppRegistryValueName, false);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
