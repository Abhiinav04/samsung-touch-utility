using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Xunit;
using SamsungTouchUtility.Core;
using SamsungTouchUtility.Models;
using SamsungTouchUtility.Services;
using SamsungTouchUtility.Tray;

namespace UnitTests
{
    public class OperationCoordinatorTests
    {
        private sealed class MockDeviceDetector : IDeviceDetector
        {
            public TouchDeviceInfo? MockDevice { get; set; }

            public TouchDeviceInfo? FindTouchDevice() => MockDevice;
            public bool IsSupportedTouchScreen(string instanceId, string friendlyName, string hardwareId) => true;
        }

        private sealed class MockTouchController : ITouchController
        {
            public DeviceStatus StatusToReturn { get; set; } = DeviceStatus.Enabled;
            public uint ProblemCodeToReturn { get; set; } = 0;
            public bool SetDeviceStateCalled { get; private set; }
            public bool LastRequestedEnable { get; private set; }

            public DeviceStatus QueryDeviceState(string instanceId, out uint devnodeStatus, out uint problemCode)
            {
                devnodeStatus = StatusToReturn == DeviceStatus.Enabled ? 0x00000008u : 0u;
                problemCode = ProblemCodeToReturn;
                return StatusToReturn;
            }

            public ToggleResult SetDeviceState(TouchDeviceInfo device, bool enable)
            {
                SetDeviceStateCalled = true;
                LastRequestedEnable = enable;
                var prev = StatusToReturn;
                StatusToReturn = enable ? DeviceStatus.Enabled : DeviceStatus.Disabled;
                return ToggleResult.CreateSuccess(prev, StatusToReturn);
            }
        }

        private sealed class MockTrayIconManager : ITrayIconManager
        {
            public DeviceStatus LastUpdatedStatus { get; private set; } = DeviceStatus.Unknown;
            public int NotificationCount { get; private set; }

            public event Action? Clicked;

            public void UpdateState(DeviceStatus status)
            {
                LastUpdatedStatus = status;
            }

            public void ShowNotification(string title, string message, ToolTipIcon icon)
            {
                NotificationCount++;
            }

            public void Dispose() { }
        }

        private sealed class MockLoggingService : ILoggingService
        {
            public void LogInfo(string message) { }
            public void LogWarning(string message) { }
            public void LogError(string message, Exception? ex = null) { }
        }

        [Fact]
        public async Task ToggleAsync_WhenEnabled_RequestsDisabled()
        {
            var detector = new MockDeviceDetector
            {
                MockDevice = new TouchDeviceInfo
                {
                    InstanceId = @"HID\GXTP7936&COL01\0",
                    FriendlyName = "HID-compliant touch screen",
                    HardwareId = "GXTP7936",
                    CurrentStatus = DeviceStatus.Enabled
                }
            };

            var controller = new MockTouchController { StatusToReturn = DeviceStatus.Enabled };
            var tray = new MockTrayIconManager();
            var logger = new MockLoggingService();

            var coordinator = new OperationCoordinator(detector, controller, tray, logger);
            coordinator.Initialize();

            var result = await coordinator.ToggleAsync();

            Assert.True(result.Success);
            Assert.True(controller.SetDeviceStateCalled);
            Assert.False(controller.LastRequestedEnable); // Should request disable
            Assert.Equal(DeviceStatus.Disabled, tray.LastUpdatedStatus);
        }

        [Fact]
        public async Task ToggleAsync_WhenDisabled_RequestsEnabled()
        {
            var detector = new MockDeviceDetector
            {
                MockDevice = new TouchDeviceInfo
                {
                    InstanceId = @"HID\GXTP7936&COL01\0",
                    FriendlyName = "HID-compliant touch screen",
                    HardwareId = "GXTP7936",
                    CurrentStatus = DeviceStatus.Disabled
                }
            };

            var controller = new MockTouchController { StatusToReturn = DeviceStatus.Disabled };
            var tray = new MockTrayIconManager();
            var logger = new MockLoggingService();

            var coordinator = new OperationCoordinator(detector, controller, tray, logger);
            coordinator.Initialize();

            var result = await coordinator.ToggleAsync();

            Assert.True(result.Success);
            Assert.True(controller.SetDeviceStateCalled);
            Assert.True(controller.LastRequestedEnable); // Should request enable
            Assert.Equal(DeviceStatus.Enabled, tray.LastUpdatedStatus);
        }

        [Fact]
        public async Task ToggleAsync_WhenStateIsUnknown_AbortsSafely()
        {
            var detector = new MockDeviceDetector
            {
                MockDevice = new TouchDeviceInfo
                {
                    InstanceId = @"HID\GXTP7936&COL01\0",
                    FriendlyName = "HID-compliant touch screen",
                    HardwareId = "GXTP7936",
                    CurrentStatus = DeviceStatus.Unknown
                }
            };

            var controller = new MockTouchController
            {
                StatusToReturn = DeviceStatus.Unknown,
                ProblemCodeToReturn = 999
            };
            var tray = new MockTrayIconManager();
            var logger = new MockLoggingService();

            var coordinator = new OperationCoordinator(detector, controller, tray, logger);
            coordinator.Initialize();

            var result = await coordinator.ToggleAsync();

            Assert.False(result.Success);
            Assert.False(controller.SetDeviceStateCalled); // Must NOT call hardware modification
            Assert.Equal(DeviceStatus.Unknown, tray.LastUpdatedStatus);
        }

        [Fact]
        public async Task ToggleAsync_WhenRebootPending_AbortsAndFlagsReboot()
        {
            var detector = new MockDeviceDetector
            {
                MockDevice = new TouchDeviceInfo
                {
                    InstanceId = @"HID\GXTP7936&COL01\0",
                    FriendlyName = "HID-compliant touch screen",
                    HardwareId = "GXTP7936",
                    CurrentStatus = DeviceStatus.RebootPending
                }
            };

            var controller = new MockTouchController
            {
                StatusToReturn = DeviceStatus.RebootPending,
                ProblemCodeToReturn = 14 // CM_PROB_NEED_RESTART
            };
            var tray = new MockTrayIconManager();
            var logger = new MockLoggingService();

            var coordinator = new OperationCoordinator(detector, controller, tray, logger);
            coordinator.Initialize();

            var result = await coordinator.ToggleAsync();

            Assert.False(result.Success);
            Assert.True(result.RebootRequired);
            Assert.False(controller.SetDeviceStateCalled);
        }
    }
}
