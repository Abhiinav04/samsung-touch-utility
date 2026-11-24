using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using SamsungTouchUtility.Models;
using SamsungTouchUtility.Windows;

namespace SamsungTouchUtility.Core
{
    public interface IDeviceDetector
    {
        TouchDeviceInfo? FindTouchDevice();
        bool IsSupportedTouchScreen(string instanceId, string friendlyName, string hardwareId);
    }

    public sealed class DeviceDetector : IDeviceDetector
    {
        // HID Class GUID
        private static readonly Guid GUID_DEVCLASS_HIDCLASS = new Guid("{745A17A0-74D3-11D0-B6FE-00A0C90F57DA}");

        // Strict Exclusion Filters to protect Touchpad, Stylus/Pen, and Keyboards
        private static readonly string[] ExcludedKeywords = new[]
        {
            "touch pad",
            "touchpad",
            "pen",
            "stylus",
            "keyboard",
            "mouse",
            "elan0b00&col03", // Samsung touchpad collection
            "wcom016b&col04", // Samsung S Pen / digitizer pen collection
            "converteddevice"
        };

        // Known Touchscreen Digitizer Indicators
        private static readonly string[] TouchScreenIndicators = new[]
        {
            "touch screen",
            "touchscreen",
            "tela touch",
            "gxtp7936",
            "hid_device_up:000d_u:0004" // HID Digitizer Touch Screen Usage
        };

        public TouchDeviceInfo? FindTouchDevice()
        {
            IntPtr hDevInfo = NativeMethods.SetupDiGetClassDevsW(
                IntPtr.Zero,
                "HID",
                IntPtr.Zero,
                NativeMethods.DIGCF_ALLCLASSES);

            if (hDevInfo == IntPtr.Zero || hDevInfo.ToInt64() == -1)
            {
                return null;
            }

            try
            {
                var devInfoData = new NativeMethods.SP_DEVINFO_DATA();
                devInfoData.cbSize = (uint)Marshal.SizeOf<NativeMethods.SP_DEVINFO_DATA>();

                uint memberIndex = 0;
                while (NativeMethods.SetupDiEnumDeviceInfo(hDevInfo, memberIndex, ref devInfoData))
                {
                    memberIndex++;

                    string instanceId = GetDeviceInstanceId(hDevInfo, ref devInfoData);
                    if (string.IsNullOrWhiteSpace(instanceId))
                        continue;

                    string friendlyName = GetDeviceRegistryProperty(hDevInfo, ref devInfoData, NativeMethods.SPDRP_FRIENDLYNAME)
                        ?? GetDeviceRegistryProperty(hDevInfo, ref devInfoData, NativeMethods.SPDRP_DEVICEDESC)
                        ?? string.Empty;

                    string hardwareId = GetDeviceRegistryProperty(hDevInfo, ref devInfoData, NativeMethods.SPDRP_HARDWAREID)
                        ?? string.Empty;

                    if (IsSupportedTouchScreen(instanceId, friendlyName, hardwareId))
                    {
                        // Get Parent DevNode instance ID
                        string? parentId = GetParentDeviceInstanceId(instanceId);

                        // Read DevNode status & problem code via cfgmgr32
                        uint devnodeStatus = 0;
                        uint problemCode = 0;
                        uint cr = NativeMethods.CM_Locate_DevNodeW(out uint dnDevInst, instanceId, 0);
                        if (cr == NativeMethods.CR_SUCCESS)
                        {
                            NativeMethods.CM_Get_DevNode_Status(out devnodeStatus, out problemCode, dnDevInst, 0);
                        }

                        bool isRebootRequired = (problemCode == NativeMethods.CM_PROB_NEED_RESTART || problemCode == NativeMethods.CM_PROB_REINSTALL);

                        // Interrogate DEVPKEY_Device_IsRebootRequired directly from PnP subsystem
                        var rebootKey = NativeMethods.DEVPKEY_Device_IsRebootRequired;
                        byte[] propBuf = new byte[4];
                        if (NativeMethods.SetupDiGetDevicePropertyW(hDevInfo, ref devInfoData, ref rebootKey, out _, propBuf, (uint)propBuf.Length, out _, 0))
                        {
                            if (propBuf[0] != 0)
                            {
                                isRebootRequired = true;
                            }
                        }

                        var status = isRebootRequired ? DeviceStatus.RebootPending : EvaluateStatus(devnodeStatus, problemCode);

                        return new TouchDeviceInfo
                        {
                            InstanceId = instanceId,
                            FriendlyName = friendlyName,
                            HardwareId = hardwareId,
                            ParentInstanceId = parentId,
                            DevNodeStatus = devnodeStatus,
                            ProblemCode = problemCode,
                            CurrentStatus = status,
                            IsRebootRequired = isRebootRequired
                        };
                    }
                }
            }
            finally
            {
                NativeMethods.SetupDiDestroyDeviceInfoList(hDevInfo);
            }

            return null;
        }

        public bool IsSupportedTouchScreen(string instanceId, string friendlyName, string hardwareId)
        {
            string idLower = instanceId.ToLowerInvariant();
            string nameLower = friendlyName.ToLowerInvariant();
            string hwLower = hardwareId.ToLowerInvariant();

            // 1. Check strict exclusions: Must NOT match touchpad, pen, stylus, keyboard
            foreach (var excluded in ExcludedKeywords)
            {
                if (idLower.Contains(excluded) || nameLower.Contains(excluded) || hwLower.Contains(excluded))
                {
                    return false;
                }
            }

            // 2. Check for positive Touch Screen match
            bool hasTouchIndicator = false;
            foreach (var indicator in TouchScreenIndicators)
            {
                if (idLower.Contains(indicator) || nameLower.Contains(indicator) || hwLower.Contains(indicator))
                {
                    hasTouchIndicator = true;
                    break;
                }
            }

            if (!hasTouchIndicator)
                return false;

            // 3. For Goodix controllers, ensure COL01 (the touch screen collection)
            if (idLower.Contains("gxtp") && !idLower.Contains("col01"))
            {
                return false;
            }

            return true;
        }

        public static DeviceStatus EvaluateStatus(uint devNodeStatus, uint problemCode)
        {
            if (problemCode == NativeMethods.CM_PROB_DISABLED)
            {
                return DeviceStatus.Disabled;
            }

            if (problemCode == NativeMethods.CM_PROB_NEED_RESTART || problemCode == NativeMethods.CM_PROB_REINSTALL)
            {
                return DeviceStatus.RebootPending;
            }

            if (problemCode == NativeMethods.CM_PROB_NONE && (devNodeStatus & NativeMethods.DN_STARTED) != 0)
            {
                return DeviceStatus.Enabled;
            }

            if ((devNodeStatus & NativeMethods.DN_HAS_PROBLEM) != 0)
            {
                return DeviceStatus.Unknown;
            }

            return DeviceStatus.Unknown;
        }

        private static string GetDeviceInstanceId(IntPtr hDevInfo, ref NativeMethods.SP_DEVINFO_DATA devInfoData)
        {
            IntPtr buffer = Marshal.AllocHGlobal(1024);
            try
            {
                if (NativeMethods.SetupDiGetDeviceInstanceIdW(hDevInfo, ref devInfoData, buffer, 512, out uint requiredSize))
                {
                    return Marshal.PtrToStringUni(buffer) ?? string.Empty;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
            return string.Empty;
        }

        private static string? GetDeviceRegistryProperty(IntPtr hDevInfo, ref NativeMethods.SP_DEVINFO_DATA devInfoData, uint property)
        {
            if (!NativeMethods.SetupDiGetDeviceRegistryPropertyW(hDevInfo, ref devInfoData, property, out _, null, 0, out uint requiredSize))
            {
                if (Marshal.GetLastWin32Error() != 122) // ERROR_INSUFFICIENT_BUFFER
                    return null;
            }

            byte[] buffer = new byte[requiredSize];
            if (NativeMethods.SetupDiGetDeviceRegistryPropertyW(hDevInfo, ref devInfoData, property, out uint regType, buffer, requiredSize, out _))
            {
                return Encoding.Unicode.GetString(buffer).TrimEnd('\0');
            }

            return null;
        }

        private static string? GetParentDeviceInstanceId(string childInstanceId)
        {
            uint cr = NativeMethods.CM_Locate_DevNodeW(out uint childDevInst, childInstanceId, 0);
            if (cr != NativeMethods.CR_SUCCESS)
                return null;

            cr = NativeMethods.CM_Get_Parent(out uint parentDevInst, childDevInst, 0);
            if (cr != NativeMethods.CR_SUCCESS)
                return null;

            IntPtr buffer = Marshal.AllocHGlobal(1024);
            try
            {
                cr = NativeMethods.CM_Get_Device_IDW(parentDevInst, buffer, 512, 0);
                if (cr == NativeMethods.CR_SUCCESS)
                {
                    return Marshal.PtrToStringUni(buffer);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }

            return null;
        }
    }
}
