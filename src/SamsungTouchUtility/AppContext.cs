using System;
using System.Windows.Forms;
using SamsungTouchUtility.Configuration;
using SamsungTouchUtility.Core;
using SamsungTouchUtility.Models;
using SamsungTouchUtility.Services;
using SamsungTouchUtility.Tray;
using SamsungTouchUtility.Windows;

namespace SamsungTouchUtility
{
    public sealed class TouchApplicationContext : ApplicationContext
    {
        private readonly ILoggingService _logger;
        private readonly AppConfig _config;
        private readonly IDeviceDetector _detector;
        private readonly ITouchController _controller;
        private readonly TrayIconManager _tray;
        private readonly HotkeyWindow _hotkeyWindow;
        private readonly OperationCoordinator _coordinator;

        public TouchApplicationContext()
        {
            _logger = new LoggingService();
            _logger.LogInfo("=== Samsung Touch Utility Starting ===");

            _config = AppConfig.Load();
            _detector = new DeviceDetector();
            _controller = new TouchController(_detector);
            _tray = new TrayIconManager();
            _coordinator = new OperationCoordinator(_detector, _controller, _tray, _logger);

            // Create message-only native window for Hotkey and PnP broadcast notifications
            _hotkeyWindow = new HotkeyWindow();

            // Wire up user interaction events
            _tray.Clicked += OnUserToggleRequested;
            _hotkeyWindow.HotkeyTriggered += OnUserToggleRequested;
            _hotkeyWindow.DeviceChanged += OnHardwareStateChanged;

            // Initialize hardware detection
            _coordinator.Initialize();

            // Display startup notification so user knows application is active in tray
            var status = _coordinator.ActiveDevice?.CurrentStatus ?? DeviceStatus.Unknown;
            string statusText = status == DeviceStatus.Enabled ? "ENABLED (TOUCH ON)" : "DISABLED (TOUCH OFF)";
            _tray.ShowNotification(
                "Samsung Touch Utility",
                $"Touchscreen is {statusText}.\nPress Ctrl+Alt+T or click tray icon to toggle.",
                ToolTipIcon.Info);
        }

        private async void OnUserToggleRequested()
        {
            await _coordinator.ToggleAsync();
        }

        private void OnHardwareStateChanged()
        {
            _logger.LogInfo("PnP hardware broadcast received. Refreshing device state...");
            _coordinator.RefreshDeviceState();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _logger.LogInfo("Samsung Touch Utility shutting down cleanly...");
                _hotkeyWindow.Dispose();
                _tray.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
