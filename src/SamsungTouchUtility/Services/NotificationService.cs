using System;
using System.Windows.Forms;

namespace SamsungTouchUtility.Services
{
    public interface INotificationService
    {
        void ShowNotification(string title, string text, ToolTipIcon icon);
    }

    public sealed class NotificationService : INotificationService
    {
        private readonly NotifyIcon? _notifyIcon;

        public NotificationService(NotifyIcon? notifyIcon)
        {
            _notifyIcon = notifyIcon;
        }

        public void ShowNotification(string title, string text, ToolTipIcon icon)
        {
            try
            {
                if (_notifyIcon != null && _notifyIcon.Visible)
                {
                    _notifyIcon.ShowBalloonTip(2000, title, text, icon);
                }
            }
            catch
            {
            }
        }
    }
}
