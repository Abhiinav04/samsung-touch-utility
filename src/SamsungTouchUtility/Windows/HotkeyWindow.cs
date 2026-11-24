using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SamsungTouchUtility.Windows;

namespace SamsungTouchUtility.Windows
{
    public sealed class HotkeyWindow : NativeWindow, IDisposable
    {
        private const int HOTKEY_ID = 9001;
        private const uint VK_T = 0x54;

        public event Action? HotkeyTriggered;
        public event Action? DeviceChanged;

        private bool _isHotkeyRegistered = false;
        private IntPtr _hDeviceNotify = IntPtr.Zero;

        public HotkeyWindow()
        {
            // Create a message-only window
            var cp = new CreateParams
            {
                Parent = (IntPtr)(-3) // HWND_MESSAGE
            };
            CreateHandle(cp);

            // Allow WM_HOTKEY and WM_DEVICECHANGE across UIPI integrity levels when elevated
            NativeMethods.ChangeWindowMessageFilterEx(Handle, NativeMethods.WM_HOTKEY, NativeMethods.MSGFLT_ALLOW, IntPtr.Zero);
            NativeMethods.ChangeWindowMessageFilterEx(Handle, (uint)NativeMethods.WM_DEVICECHANGE, NativeMethods.MSGFLT_ALLOW, IntPtr.Zero);

            RegisterHotkey();
            RegisterDeviceNotification();
        }

        private void RegisterHotkey()
        {
            try
            {
                // Register Ctrl + Alt + T (with MOD_NOREPEAT to avoid rapid key-repeat storm)
                uint modifiers = NativeMethods.MOD_CONTROL | NativeMethods.MOD_ALT | NativeMethods.MOD_NOREPEAT;
                _isHotkeyRegistered = NativeMethods.RegisterHotKey(Handle, HOTKEY_ID, modifiers, VK_T);
            }
            catch
            {
                _isHotkeyRegistered = false;
            }
        }

        private void RegisterDeviceNotification()
        {
            try
            {
                var dbi = new NativeMethods.DEV_BROADCAST_DEVICEINTERFACE
                {
                    dbcc_size = (uint)Marshal.SizeOf<NativeMethods.DEV_BROADCAST_DEVICEINTERFACE>(),
                    dbcc_devicetype = NativeMethods.DBT_DEVTYP_DEVICEINTERFACE,
                    dbcc_reserved = 0,
                    dbcc_classguid = Guid.Empty
                };

                IntPtr pDbi = Marshal.AllocHGlobal(Marshal.SizeOf(dbi));
                try
                {
                    Marshal.StructureToPtr(dbi, pDbi, false);
                    _hDeviceNotify = NativeMethods.RegisterDeviceNotificationW(
                        Handle,
                        pDbi,
                        0x00000004); // DEVICE_NOTIFY_ALL_INTERFACE_CLASSES
                }
                finally
                {
                    Marshal.FreeHGlobal(pDbi);
                }
            }
            catch
            {
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == NativeMethods.WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
            {
                HotkeyTriggered?.Invoke();
                return;
            }

            if (m.Msg == NativeMethods.WM_DEVICECHANGE)
            {
                int eventType = m.WParam.ToInt32();
                if (eventType == NativeMethods.DBT_DEVNODES_CHANGED ||
                    eventType == NativeMethods.DBT_DEVICEARRIVAL ||
                    eventType == NativeMethods.DBT_DEVICEREMOVECOMPLETE)
                {
                    DeviceChanged?.Invoke();
                }
            }

            base.WndProc(ref m);
        }

        public void Dispose()
        {
            if (_isHotkeyRegistered && Handle != IntPtr.Zero)
            {
                NativeMethods.UnregisterHotKey(Handle, HOTKEY_ID);
                _isHotkeyRegistered = false;
            }

            if (_hDeviceNotify != IntPtr.Zero)
            {
                NativeMethods.UnregisterDeviceNotification(_hDeviceNotify);
                _hDeviceNotify = IntPtr.Zero;
            }

            if (Handle != IntPtr.Zero)
            {
                DestroyHandle();
            }
        }
    }
}
