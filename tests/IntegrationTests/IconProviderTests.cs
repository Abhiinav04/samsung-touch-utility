using System;
using System.Drawing;
using Xunit;
using SamsungTouchUtility.Models;
using SamsungTouchUtility.Tray;

namespace IntegrationTests
{
    public class IconProviderTests
    {
        [Theory]
        [InlineData(DeviceStatus.Enabled, 16)]
        [InlineData(DeviceStatus.Enabled, 32)]
        [InlineData(DeviceStatus.Enabled, 48)]
        [InlineData(DeviceStatus.Disabled, 16)]
        [InlineData(DeviceStatus.Disabled, 32)]
        [InlineData(DeviceStatus.Disabled, 48)]
        [InlineData(DeviceStatus.Unknown, 16)]
        [InlineData(DeviceStatus.Unknown, 32)]
        [InlineData(DeviceStatus.Unknown, 48)]
        public void CreateStatusIcon_GeneratesValidHighDpiIcon(DeviceStatus status, int size)
        {
            using Icon icon = IconProvider.CreateStatusIcon(status, size);

            Assert.NotNull(icon);
            Assert.NotEqual(IntPtr.Zero, icon.Handle);
            Assert.True(icon.Width > 0);
            Assert.True(icon.Height > 0);
        }
    }
}
