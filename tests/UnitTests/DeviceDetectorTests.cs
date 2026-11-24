using System;
using Xunit;
using SamsungTouchUtility.Core;
using SamsungTouchUtility.Models;
using SamsungTouchUtility.Windows;

namespace UnitTests
{
    public class DeviceDetectorTests
    {
        private readonly DeviceDetector _detector = new DeviceDetector();

        [Fact]
        public void IsSupportedTouchScreen_IdentifiesGoodixTouchScreenCol01()
        {
            string instanceId = @"HID\GXTP7936&COL01\5&1FFF58BB&0&0000";
            string friendlyName = "HID-compliant touch screen";
            string hardwareId = @"HID\VEN_GXTP&DEV_7936&Col01";

            bool result = _detector.IsSupportedTouchScreen(instanceId, friendlyName, hardwareId);
            Assert.True(result);
        }

        [Fact]
        public void IsSupportedTouchScreen_RejectsGoodixCol02AndCol03VendorDefined()
        {
            string instanceId2 = @"HID\GXTP7936&COL02\5&1FFF58BB&0&0001";
            string friendlyName2 = "HID-compliant vendor-defined device";
            string hardwareId2 = @"HID\VEN_GXTP&DEV_7936&Col02";

            Assert.False(_detector.IsSupportedTouchScreen(instanceId2, friendlyName2, hardwareId2));

            string instanceId3 = @"HID\GXTP7936&COL03\5&1FFF58BB&0&0002";
            string friendlyName3 = "HID-compliant vendor-defined device";
            string hardwareId3 = @"HID\VEN_GXTP&DEV_7936&Col03";

            Assert.False(_detector.IsSupportedTouchScreen(instanceId3, friendlyName3, hardwareId3));
        }

        [Fact]
        public void IsSupportedTouchScreen_StrictlyExcludesTouchpad()
        {
            string instanceId = @"HID\ELAN0B00&COL03\5&42DB6EA&0&0002";
            string friendlyName = "HID-compliant touch pad";
            string hardwareId = @"HID\VID_04F3&PID_31C0&Col03";

            bool result = _detector.IsSupportedTouchScreen(instanceId, friendlyName, hardwareId);
            Assert.False(result);
        }

        [Fact]
        public void IsSupportedTouchScreen_StrictlyExcludesWacomPenStylus()
        {
            string instanceId = @"HID\WCOM016B&COL04\5&2D6CF7A0&0&0003";
            string friendlyName = "HID-compliant pen";
            string hardwareId = @"HID\WCOM016B&Col04";

            bool result = _detector.IsSupportedTouchScreen(instanceId, friendlyName, hardwareId);
            Assert.False(result);
        }

        [Fact]
        public void IsSupportedTouchScreen_StrictlyExcludesKeyboardAndButtonConverter()
        {
            string instanceId = @"BUTTONCONVERTER\CONVERTEDDEVICE\4&1243AD85&0&0";
            string friendlyName = "Converted Portable Device Control device";
            string hardwareId = @"BUTTONCONVERTER\CONVERTEDDEVICE";

            bool result = _detector.IsSupportedTouchScreen(instanceId, friendlyName, hardwareId);
            Assert.False(result);
        }

        [Theory]
        [InlineData(NativeMethods.DN_STARTED, NativeMethods.CM_PROB_NONE, DeviceStatus.Enabled)]
        [InlineData(0u, NativeMethods.CM_PROB_DISABLED, DeviceStatus.Disabled)]
        [InlineData(0u, NativeMethods.CM_PROB_NEED_RESTART, DeviceStatus.RebootPending)]
        [InlineData(0u, NativeMethods.CM_PROB_REINSTALL, DeviceStatus.RebootPending)]
        [InlineData(NativeMethods.DN_HAS_PROBLEM, 999u, DeviceStatus.Unknown)]
        public void EvaluateStatus_CorrectlyMapsDevNodeStatusAndProblemCodes(
            uint devnodeStatus, uint problemCode, DeviceStatus expected)
        {
            var status = DeviceDetector.EvaluateStatus(devnodeStatus, problemCode);
            Assert.Equal(expected, status);
        }
    }
}
