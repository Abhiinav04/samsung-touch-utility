using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;

namespace SamsungTouchUtility
{
    internal static class Program
    {
        private const string MutexName = @"Global\SamsungTouchUtility_InstanceMutex_v1";

        [STAThread]
        private static void Main()
        {
            // Verify Administrator privilege required for PnP hardware control
            if (!IsAdministrator())
            {
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = Environment.ProcessPath ?? Application.ExecutablePath,
                        UseShellExecute = true,
                        Verb = "runas"
                    };
                    Process.Start(psi);
                }
                catch
                {
                    MessageBox.Show(
                        "Administrator permissions are required to toggle the touchscreen hardware.\nPlease right-click the application and select 'Run as administrator'.",
                        "Samsung Touch Utility",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
                return;
            }

            using var mutex = new Mutex(true, MutexName, out bool createdNew);
            if (!createdNew)
            {
                // Single-instance enforcement: Another instance is already active
                return;
            }

            ApplicationConfiguration.Initialize();
            Application.Run(new TouchApplicationContext());
        }

        private static bool IsAdministrator()
        {
            try
            {
                using var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }
    }
}
