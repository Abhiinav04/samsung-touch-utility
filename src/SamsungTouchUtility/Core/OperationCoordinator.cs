using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SamsungTouchUtility.Models;
using SamsungTouchUtility.Services;
using SamsungTouchUtility.Tray;

namespace SamsungTouchUtility.Core
{
    public sealed class OperationCoordinator
    {
        private readonly IDeviceDetector _detector;
        private readonly ITouchController _controller;
        private readonly ITrayIconManager _tray;
        private readonly ILoggingService _logger;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        private TouchDeviceInfo? _activeDevice;

        public TouchDeviceInfo? ActiveDevice => _activeDevice;

        public OperationCoordinator(
            IDeviceDetector detector,
            ITouchController controller,
            ITrayIconManager tray,
            ILoggingService logger)
        {
            _detector = detector ?? throw new ArgumentNullException(nameof(detector));
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
            _tray = tray ?? throw new ArgumentNullException(nameof(tray));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Initialize()
        {
            _logger.LogInfo("Initializing touchscreen device discovery...");
            RefreshDeviceState();
        }

        public void RefreshDeviceState()
        {
            try
            {
                _activeDevice = _detector.FindTouchDevice();
                if (_activeDevice == null)
                {
                    _logger.LogWarning("No supported touchscreen device detected.");
                    _tray.UpdateState(DeviceStatus.DeviceNotFound);
                    return;
                }

                _logger.LogInfo($"Detected Touchscreen: {_activeDevice.FriendlyName} [{_activeDevice.InstanceId}], Initial Status: {_activeDevice.CurrentStatus}");
                _tray.UpdateState(_activeDevice.CurrentStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception during device discovery/refresh", ex);
                _tray.UpdateState(DeviceStatus.Unknown);
            }
        }

        public async Task<ToggleResult> ToggleAsync()
        {
            // Enforce single active toggle operation lock
            if (!await _lock.WaitAsync(0))
            {
                _logger.LogWarning("Toggle operation already in flight. Rejecting concurrent request.");
                return ToggleResult.CreateFailure(
                    DeviceStatus.Unknown,
                    DeviceStatus.Unknown,
                    "An operation is already in progress.");
            }

            try
            {
                // Ensure device is detected
                if (_activeDevice == null)
                {
                    _activeDevice = _detector.FindTouchDevice();
                    if (_activeDevice == null)
                    {
                        _logger.LogError("Cannot toggle: No supported touchscreen detected.");
                        _tray.UpdateState(DeviceStatus.DeviceNotFound);
                        _tray.ShowNotification("Touchscreen Error", "No compatible touchscreen device found.", ToolTipIcon.Error);
                        return ToggleResult.CreateFailure(
                            DeviceStatus.Unknown,
                            DeviceStatus.DeviceNotFound,
                            "Touchscreen device not found.");
                    }
                }

                // Verify current hardware state directly
                DeviceStatus currentStatus = _controller.QueryDeviceState(
                    _activeDevice.InstanceId,
                    out uint devnodeStatus,
                    out uint problemCode);

                _logger.LogInfo($"Verified current hardware state before toggle: {currentStatus} (Problem code: {problemCode})");

                if (currentStatus == DeviceStatus.Unknown || currentStatus == DeviceStatus.DeviceNotFound)
                {
                    _logger.LogError($"Cannot toggle: Current device state is unknown or not ready. Status={currentStatus}, ProblemCode={problemCode}");
                    _tray.UpdateState(DeviceStatus.Unknown);
                    _tray.ShowNotification("Touchscreen Warning", "Current hardware state is unknown. Toggle aborted for safety.", ToolTipIcon.Warning);
                    return ToggleResult.CreateFailure(currentStatus, currentStatus, "Hardware state cannot be verified.");
                }

                if (currentStatus == DeviceStatus.RebootPending)
                {
                    _logger.LogWarning("Touchscreen is pending a system reboot. State change cannot proceed.");
                    _tray.UpdateState(DeviceStatus.RebootPending);
                    _tray.ShowNotification("Reboot Required", "Windows requires a system restart before the touchscreen can be toggled.", ToolTipIcon.Warning);
                    return ToggleResult.CreateFailure(currentStatus, currentStatus, "Reboot pending on device.", reboot: true);
                }

                // Target state is the exact opposite of verified current state
                bool targetEnable = (currentStatus == DeviceStatus.Disabled);
                _logger.LogInfo($"Requesting state change: {currentStatus} -> {(targetEnable ? "Enabled" : "Disabled")}");

                ToggleResult result = await Task.Run(() => _controller.SetDeviceState(_activeDevice, targetEnable));

                if (result.Success)
                {
                    _logger.LogInfo($"Toggle SUCCESS: New verified state is {result.NewStatus}");
                    _tray.UpdateState(result.NewStatus);
                    _tray.ShowNotification(
                        "Touchscreen",
                        result.NewStatus == DeviceStatus.Enabled ? "Touchscreen ENABLED" : "Touchscreen DISABLED",
                        ToolTipIcon.Info);
                }
                else
                {
                    _logger.LogError($"Toggle FAILED: {result.Message} (Error: {result.ErrorCode}, RebootRequired: {result.RebootRequired})");
                    _tray.UpdateState(result.NewStatus);
                    _tray.ShowNotification(
                        "Touchscreen Toggle Failed",
                        result.Message,
                        result.RebootRequired ? ToolTipIcon.Warning : ToolTipIcon.Error);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Unexpected error during toggle operation", ex);
                _tray.UpdateState(DeviceStatus.Unknown);
                _tray.ShowNotification("Touchscreen Error", "An unexpected error occurred during toggle.", ToolTipIcon.Error);
                return ToggleResult.CreateFailure(DeviceStatus.Unknown, DeviceStatus.Unknown, ex.Message);
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}
