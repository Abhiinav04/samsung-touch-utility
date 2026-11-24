using System;
using System.Drawing;
using System.Windows.Forms;
using SamsungTouchUtility.Models;

namespace SamsungTouchUtility.Tray
{
    public interface ITrayIconManager : IDisposable
    {
        void UpdateState(DeviceStatus status);
        void ShowNotification(string title, string message, ToolTipIcon icon);
        event Action? Clicked;
    }

    public sealed class TrayIconManager : ITrayIconManager
    {
        private readonly NotifyIcon _notifyIcon;
        private Icon? _currentIcon;

        public event Action? Clicked;

        public TrayIconManager()
        {
            _notifyIcon = new NotifyIcon
            {
                Visible = true,
                Text = "TOUCH STATUS UNKNOWN",
                ContextMenuStrip = null // Strictly NO context menu as required
            };

            // Single left-click toggle
            _notifyIcon.MouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    Clicked?.Invoke();
                }
            };

            UpdateState(DeviceStatus.Unknown);
        }

        public void UpdateState(DeviceStatus status)
        {
            string tooltip = status switch
            {
                DeviceStatus.Enabled => "TOUCH ON",
                DeviceStatus.Disabled => "TOUCH OFF",
                DeviceStatus.RebootPending => "TOUCH STATUS: REBOOT PENDING",
                _ => "TOUCH STATUS UNKNOWN"
            };

            _notifyIcon.Text = tooltip;

            var oldIcon = _currentIcon;
            _currentIcon = IconProvider.CreateStatusIcon(status);
            _notifyIcon.Icon = _currentIcon;
            oldIcon?.Dispose();
        }

        public void ShowNotification(string title, string message, ToolTipIcon icon)
        {
            try
            {
                _notifyIcon.ShowBalloonTip(2000, title, message, icon);
            }
            catch
            {
            }
        }

        public void Dispose()
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _currentIcon?.Dispose();
        }
    }
}
