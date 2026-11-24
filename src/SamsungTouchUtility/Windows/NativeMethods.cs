using System;
using System.Runtime.InteropServices;

namespace SamsungTouchUtility.Windows
{
    internal static class NativeMethods
    {
        // SetupAPI constants
        public const uint DIGCF_PRESENT = 0x00000002;
        public const uint DIGCF_ALLCLASSES = 0x00000004;
        public const uint DIGCF_DEVICEINTERFACE = 0x00000010;

        public const uint SPDRP_DEVICEDESC = 0x00000000;
        public const uint SPDRP_HARDWAREID = 0x00000001;
        public const uint SPDRP_COMPATIBLEIDS = 0x00000002;
        public const uint SPDRP_CLASS = 0x00000007;
        public const uint SPDRP_CLASSGUID = 0x00000008;
        public const uint SPDRP_FRIENDLYNAME = 0x0000000C;

        public const uint DIF_PROPERTYCHANGE = 0x00000012;
        public const uint DICS_ENABLE = 0x00000001;
        public const uint DICS_DISABLE = 0x00000002;
        public const uint DICS_PROPCHANGE = 0x00000003;
        public const uint DICS_START = 0x00000004;
        public const uint DICS_STOP = 0x00000005;

        public const uint DICS_FLAG_GLOBAL = 0x00000001;
        public const uint DICS_FLAG_CONFIGSPECIFIC = 0x00000002;

        public const uint DI_NEEDREBOOT = 0x00000100;
        public const uint DI_NEEDRESTART = 0x00000080;

        // Configuration Manager (cfgmgr32) return codes
        public const uint CR_SUCCESS = 0x00000000;
        public const uint CR_NO_SUCH_DEVNODE = 0x0000000D;

        // DevNode status flags (DN_*)
        public const uint DN_ROOT_ENUMERATED = 0x00000001;
        public const uint DN_DRIVER_LOADED = 0x00000002;
        public const uint DN_ENUM_LOADED = 0x00000004;
        public const uint DN_STARTED = 0x00000008;
        public const uint DN_HAS_PROBLEM = 0x00000400;
        public const uint DN_DISABLEABLE = 0x00002000;
        public const uint DN_REMOVABLE = 0x00004000;

        // Problem codes (CM_PROB_*)
        public const uint CM_PROB_NONE = 0x00000000;
        public const uint CM_PROB_NOT_CONFIGURED = 0x00000001;
        public const uint CM_PROB_DEVLOADER_FAILED = 0x00000002;
        public const uint CM_PROB_OUT_OF_MEMORY = 0x00000003;
        public const uint CM_PROB_WRONG_TYPE = 0x00000004;
        public const uint CM_PROB_LACKED_ARBITRATOR = 0x00000005;
        public const uint CM_PROB_BOOT_CONFIG_CONFLICT = 0x00000006;
        public const uint CM_PROB_FAILED_FILTER = 0x00000007;
        public const uint CM_PROB_DEVLOADER_NOT_FOUND = 0x00000008;
        public const uint CM_PROB_INVALID_DATA = 0x00000009;
        public const uint CM_PROB_FAILED_START = 0x0000000A;
        public const uint CM_PROB_LIAR = 0x0000000B;
        public const uint CM_PROB_NORMAL_CONFLICT = 0x0000000C;
        public const uint CM_PROB_NOT_VERIFIED = 0x0000000D;
        public const uint CM_PROB_NEED_RESTART = 0x0000000E;
        public const uint CM_PROB_REENUMERATION = 0x0000000F;
        public const uint CM_PROB_PARTIAL_LOG_CONF = 0x00000010;
        public const uint CM_PROB_UNKNOWN_RESOURCE = 0x00000011;
        public const uint CM_PROB_REINSTALL = 0x00000012;
        public const uint CM_PROB_REGISTRY = 0x00000013;
        public const uint CM_PROB_VXDLDR = 0x00000014;
        public const uint CM_PROB_DISABLED = 0x00000016; // 22

        // Windows Messages & Hotkeys
        public const int WM_HOTKEY = 0x0312;
        public const int WM_DEVICECHANGE = 0x0219;
        public const int DBT_DEVNODES_CHANGED = 0x0007;
        public const int DBT_DEVICEARRIVAL = 0x8000;
        public const int DBT_DEVICEREMOVECOMPLETE = 0x8004;

        public const uint MOD_ALT = 0x0001;
        public const uint MOD_CONTROL = 0x0002;
        public const uint MOD_SHIFT = 0x0004;
        public const uint MOD_WIN = 0x0008;
        public const uint MOD_NOREPEAT = 0x4000;

        [StructLayout(LayoutKind.Sequential)]
        public struct SP_DEVINFO_DATA
        {
            public uint cbSize;
            public Guid ClassGuid;
            public uint DevInst;
            public IntPtr Reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SP_CLASSINSTALL_HEADER
        {
            public uint cbSize;
            public uint InstallFunction;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SP_PROPCHANGE_PARAMS
        {
            public SP_CLASSINSTALL_HEADER ClassInstallHeader;
            public uint StateChange;
            public uint Scope;
            public uint HwProfile;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SP_DEVINSTALL_PARAMS
        {
            public uint cbSize;
            public uint Flags;
            public uint FlagsEx;
            public IntPtr hwndParent;
            public IntPtr InstallMsgHandler;
            public IntPtr InstallMsgHandlerContext;
            public IntPtr FileQueue;
            public UIntPtr ClassInstallReserved;
            public uint Reserved;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string DriverPath;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DEV_BROADCAST_HDR
        {
            public uint dbch_size;
            public uint dbch_devicetype;
            public uint dbch_reserved;
        }

        public const uint DBT_DEVTYP_DEVICEINTERFACE = 0x00000005;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct DEV_BROADCAST_DEVICEINTERFACE
        {
            public uint dbcc_size;
            public uint dbcc_devicetype;
            public uint dbcc_reserved;
            public Guid dbcc_classguid;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string dbcc_name;
        }

        // SetupAPI P/Invokes
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr SetupDiGetClassDevsW(
            ref Guid ClassGuid,
            IntPtr Enumerator,
            IntPtr hwndParent,
            uint Flags);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr SetupDiGetClassDevsW(
            IntPtr ClassGuid,
            string? Enumerator,
            IntPtr hwndParent,
            uint Flags);

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiEnumDeviceInfo(
            IntPtr DeviceInfoSet,
            uint MemberIndex,
            ref SP_DEVINFO_DATA DeviceInfoData);

        [StructLayout(LayoutKind.Sequential)]
        public struct DEVPROPKEY
        {
            public Guid fmtid;
            public uint pid;
        }

        public static readonly DEVPROPKEY DEVPKEY_Device_IsRebootRequired = new DEVPROPKEY
        {
            fmtid = new Guid("{540B947E-8B40-45BC-A8A2-6A0B894CBDA2}"),
            pid = 16
        };

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool SetupDiGetDevicePropertyW(
            IntPtr DeviceInfoSet,
            ref SP_DEVINFO_DATA DeviceInfoData,
            ref DEVPROPKEY PropertyKey,
            out uint PropertyType,
            byte[]? PropertyBuffer,
            uint PropertyBufferSize,
            out uint RequiredSize,
            uint Flags);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool SetupDiGetDeviceRegistryPropertyW(
            IntPtr DeviceInfoSet,
            ref SP_DEVINFO_DATA DeviceInfoData,
            uint Property,
            out uint PropertyRegDataType,
            byte[]? PropertyBuffer,
            uint PropertyBufferSize,
            out uint RequiredSize);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool SetupDiGetDeviceInstanceIdW(
            IntPtr DeviceInfoSet,
            ref SP_DEVINFO_DATA DeviceInfoData,
            IntPtr DeviceInstanceId,
            uint DeviceInstanceIdSize,
            out uint RequiredSize);

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiSetClassInstallParamsW(
            IntPtr DeviceInfoSet,
            ref SP_DEVINFO_DATA DeviceInfoData,
            ref SP_PROPCHANGE_PARAMS ClassInstallParams,
            uint ClassInstallParamsSize);

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiCallClassInstaller(
            uint InstallFunction,
            IntPtr DeviceInfoSet,
            ref SP_DEVINFO_DATA DeviceInfoData);

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiGetDeviceInstallParamsW(
            IntPtr DeviceInfoSet,
            ref SP_DEVINFO_DATA DeviceInfoData,
            ref SP_DEVINSTALL_PARAMS DeviceInstallParams);

        // Configuration Manager (cfgmgr32) P/Invokes
        [DllImport("cfgmgr32.dll", CharSet = CharSet.Unicode)]
        public static extern uint CM_Locate_DevNodeW(
            out uint pdnDevInst,
            string pDeviceID,
            uint ulFlags);

        [DllImport("cfgmgr32.dll")]
        public static extern uint CM_Get_DevNode_Status(
            out uint pulStatus,
            out uint pulProblemNumber,
            uint dnDevInst,
            uint ulFlags);

        [DllImport("cfgmgr32.dll")]
        public static extern uint CM_Disable_DevNode(
            uint dnDevInst,
            uint ulFlags);

        [DllImport("cfgmgr32.dll")]
        public static extern uint CM_Enable_DevNode(
            uint dnDevInst,
            uint ulFlags);

        [DllImport("cfgmgr32.dll", CharSet = CharSet.Unicode)]
        public static extern uint CM_Get_Parent(
            out uint pdnDevInst,
            uint dnDevInst,
            uint ulFlags);

        [DllImport("cfgmgr32.dll", CharSet = CharSet.Unicode)]
        public static extern uint CM_Get_Device_IDW(
            uint dnDevInst,
            IntPtr Buffer,
            uint BufferLen,
            uint ulFlags);

        // User32 P/Invokes
        public const uint MSGFLT_ALLOW = 1;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool ChangeWindowMessageFilterEx(
            IntPtr hWnd,
            uint msg,
            uint action,
            IntPtr pChangeFilterStruct);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr RegisterDeviceNotificationW(
            IntPtr hRecipient,
            IntPtr NotificationFilter,
            uint Flags);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnregisterDeviceNotification(IntPtr Handle);
    }
}
