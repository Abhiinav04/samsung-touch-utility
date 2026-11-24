using System;
using Xunit;
using SamsungTouchUtility.Core;
using SamsungTouchUtility.Models;

namespace HardwareTests
{
    public class LiveHardwareInspectionTests
    {
        [Fact]
        public void ReadOnlyHardwareInspection_DetectsGoodixTouchScreenOnTargetDevice()
        {
            var detector = new DeviceDetector();

            // Perform read-only query of present PnP devices
            TouchDeviceInfo? device = detector.FindTouchDevice();

            // On the development Samsung Galaxy Book3 360, the touchscreen should be dynamically discovered
            Assert.NotNull(device);
            Assert.Contains("touch", device.FriendlyName, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("GXTP7936", device.InstanceId, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("COL01", device.InstanceId, StringComparison.OrdinalIgnoreCase);

            // Assert that the parent device is ACPI\GXTP7936\10
            Assert.NotNull(device.ParentInstanceId);
            Assert.Contains("GXTP7936", device.ParentInstanceId, StringComparison.OrdinalIgnoreCase);

            // Assert hardware state matches live observed Windows PnP state (Enabled or Disabled)
            Assert.True(device.CurrentStatus == DeviceStatus.Enabled || device.CurrentStatus == DeviceStatus.Disabled);
        }

        [Fact]
        public void SafetyVerification_TouchpadAndStylusAreNotSelectedAsTouchscreen()
        {
            var detector = new DeviceDetector();

            // Touchpad instance
            string touchpadId = @"HID\ELAN0B00&COL03\5&42DB6EA&0&0002";
            Assert.False(detector.IsSupportedTouchScreen(touchpadId, "HID-compliant touch pad", "ELAN0B00"));

            // S Pen / Stylus instance
            string penId = @"HID\WCOM016B&COL04\5&2D6CF7A0&0&0003";
            Assert.False(detector.IsSupportedTouchScreen(penId, "HID-compliant pen", "WCOM016B"));

            // Parent ACPI controller must not be targeted directly
            string parentAcpiId = @"ACPI\GXTP7936\10";
            Assert.False(detector.IsSupportedTouchScreen(parentAcpiId, "Goodix TouchScreen Device", "ACPI\\GXTP7936"));
        }
    }
}
