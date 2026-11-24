using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SamsungTouchUtility.Models;

namespace SamsungTouchUtility.Tray
{
    public sealed class OnScreenDisplay : Form
    {
        private static OnScreenDisplay? _activeInstance;
        private static readonly object _sync = new object();

        private readonly System.Windows.Forms.Timer _fadeTimer;
        private readonly System.Windows.Forms.Timer _displayTimer;
        private string _message;
        private DeviceStatus _status;

        // Ensure window never activates or steals focus from active applications
        protected override bool ShowWithoutActivation => true;

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                // WS_EX_TOPMOST (0x08) | WS_EX_TOOLWINDOW (0x80) | WS_EX_NOACTIVATE (0x08000000)
                cp.ExStyle |= 0x08000088;
                return cp;
            }
        }

        public OnScreenDisplay(string message, DeviceStatus status)
        {
            _message = message;
            _status = status;

            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            TopMost = true;
            DoubleBuffered = true;
            Size = new Size(260, 56);
            BackColor = Color.FromArgb(28, 28, 30);
            Opacity = 0.96;

            // Position centered at bottom of primary screen, above taskbar
            var screen = Screen.PrimaryScreen?.WorkingArea ?? Screen.GetWorkingArea(this);
            int x = screen.Left + (screen.Width - Width) / 2;
            int y = screen.Bottom - Height - 50;
            Location = new Point(x, y);

            _displayTimer = new System.Windows.Forms.Timer { Interval = 1400 };
            _displayTimer.Tick += (s, e) =>
            {
                _displayTimer.Stop();
                _fadeTimer.Start();
            };

            _fadeTimer = new System.Windows.Forms.Timer { Interval = 25 };
            _fadeTimer.Tick += (s, e) =>
            {
                if (Opacity > 0.08)
                {
                    Opacity -= 0.08;
                }
                else
                {
                    _fadeTimer.Stop();
                    Close();
                }
            };

            _displayTimer.Start();
        }

        public static void ShowToast(string message, DeviceStatus status)
        {
            try
            {
                lock (_sync)
                {
                    if (_activeInstance != null && !_activeInstance.IsDisposed)
                    {
                        _activeInstance.UpdateContent(message, status);
                        return;
                    }

                    _activeInstance = new OnScreenDisplay(message, status);
                    _activeInstance.Show();
                }
            }
            catch
            {
            }
        }

        public void UpdateContent(string message, DeviceStatus status)
        {
            _message = message;
            _status = status;
            Opacity = 0.96;
            _fadeTimer.Stop();
            _displayTimer.Stop();
            _displayTimer.Start();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Draw rounded pill border and background
            using (var path = GetRoundedRectanglePath(new Rectangle(0, 0, Width - 1, Height - 1), 22))
            {
                using (var bgBrush = new SolidBrush(Color.FromArgb(28, 28, 30)))
                {
                    g.FillPath(bgBrush, path);
                }
                using (var borderPen = new Pen(Color.FromArgb(60, 60, 65), 1.5f))
                {
                    g.DrawPath(borderPen, path);
                }
            }

            // Draw status indicator dot
            Color dotColor = _status switch
            {
                DeviceStatus.Enabled => Color.FromArgb(16, 185, 129),   // Modern Emerald 500
                DeviceStatus.Disabled => Color.FromArgb(239, 68, 68),  // Modern Red 500
                DeviceStatus.RebootPending => Color.FromArgb(245, 158, 11), // Amber 500
                _ => Color.FromArgb(156, 163, 175)                     // Gray 400
            };

            int dotSize = 14;
            int dotX = 22;
            int dotY = (Height - dotSize) / 2;
            using (var dotBrush = new SolidBrush(dotColor))
            {
                g.FillEllipse(dotBrush, dotX, dotY, dotSize, dotSize);
            }

            // Draw message text
            using (var font = new Font("Segoe UI", 12f, FontStyle.Bold))
            using (var textBrush = new SolidBrush(Color.White))
            {
                var stringFormat = new StringFormat
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Center
                };
                var textRect = new Rectangle(48, 0, Width - 56, Height);
                g.DrawString(_message, font, textBrush, textRect, stringFormat);
            }
        }

        private static GraphicsPath GetRoundedRectanglePath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _displayTimer.Dispose();
                _fadeTimer.Dispose();
                lock (_sync)
                {
                    if (_activeInstance == this)
                        _activeInstance = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}
