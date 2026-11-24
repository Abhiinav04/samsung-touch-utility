using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using SamsungTouchUtility.Models;

namespace SamsungTouchUtility.Tray
{
    public static class IconProvider
    {
        private static readonly Color GreenActive = Color.FromArgb(16, 185, 129); // Modern Emerald 500
        private static readonly Color RedDisabled = Color.FromArgb(239, 68, 68);  // Modern Rose/Red 500
        private static readonly Color GrayUnknown = Color.FromArgb(107, 114, 128); // Modern Slate/Gray 500

        public static Icon CreateStatusIcon(DeviceStatus status, int size = 32)
        {
            using var bitmap = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.Clear(Color.Transparent);

                Color baseColor = status switch
                {
                    DeviceStatus.Enabled => GreenActive,
                    DeviceStatus.Disabled => RedDisabled,
                    _ => GrayUnknown
                };

                // Draw Outer Circle
                int margin = 3;
                int diameter = size - (margin * 2);
                using (var brush = new SolidBrush(baseColor))
                {
                    g.FillEllipse(brush, margin, margin, diameter, diameter);
                }

                // Inner glyph design
                using (var whitePen = new Pen(Color.White, 2.5f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                using (var whiteBrush = new SolidBrush(Color.White))
                {
                    if (status == DeviceStatus.Enabled)
                    {
                        // Clean Touch Point Glyph (Center Dot and Subtle Ripple)
                        float center = size / 2.0f;
                        g.FillEllipse(whiteBrush, center - 3f, center - 3f, 6f, 6f);
                        g.DrawArc(whitePen, center - 8f, center - 8f, 16f, 16f, 200, 140);
                    }
                    else if (status == DeviceStatus.Disabled)
                    {
                        // Clean Diagonal Strike Line
                        float offset = 9f;
                        g.DrawLine(whitePen, offset, offset, size - offset, size - offset);
                    }
                    else
                    {
                        // Neutral Dot
                        float center = size / 2.0f;
                        g.FillEllipse(whiteBrush, center - 2.5f, center - 2.5f, 5f, 5f);
                    }
                }
            }

            IntPtr hIcon = bitmap.GetHicon();
            Icon icon = (Icon)Icon.FromHandle(hIcon).Clone();
            DestroyIcon(hIcon);
            return icon;
        }

        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);
    }
}
