using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using SamsungTouchUtility.Models;
using SamsungTouchUtility.Windows;

namespace SamsungTouchUtility.Core
{
    public interface ITouchController
    {
        DeviceStatus QueryDeviceState(string instanceId, out uint devnodeStatus, out uint problemCode);
        ToggleResult SetDeviceState(TouchDeviceInfo device, bool enable);
    }

    public sealed class TouchController : ITouchController
    {
        private readonly IDeviceDetector _detector;

        public TouchController(IDeviceDetector detector)
        {
            _detector = detector ?? throw new ArgumentNullException(nameof(detector));
        }

        public DeviceStatus QueryDeviceState(string instanceId, out uint devnodeStatus, out uint problemCode)
        {
            devnodeStatus = 0;
            problemCode = 0;

            if (string.IsNullOrWhiteSpace(instanceId))
                return DeviceStatus.DeviceNotFound;

            uint cr = NativeMethods.CM_Locate_DevNodeW(out uint dnDevInst, instanceId, 0);
            if (cr != NativeMethods.CR_SUCCESS)
            {
                return DeviceStatus.DeviceNotFound;
            }

            cr = NativeMethods.CM_Get_DevNode_Status(out devnodeStatus, out problemCode, dnDevInst, 0);
            if (cr != NativeMethods.CR_SUCCESS)
            {
                return DeviceStatus.Unknown;
            }

            return DeviceDetector.EvaluateStatus(devnodeStatus, problemCode);
        }

        public ToggleResult SetDeviceState(TouchDeviceInfo device, bool enable)
        {
            if (device == null || string.IsNullOrWhiteSpace(device.InstanceId))
            {
                return ToggleResult.CreateFailure(
                    DeviceStatus.Unknown,
                    DeviceStatus.DeviceNotFound,
                    "No valid touchscreen device specified.");
            }

            // Security & Safety Validation
            if (!_detector.IsSupportedTouchScreen(device.InstanceId, device.FriendlyName, device.HardwareId))
            {
                return ToggleResult.CreateFailure(
                    device.CurrentStatus,
                    DeviceStatus.Unknown,
                    "Target device failed security validation check. Operation aborted.");
            }

            // 1. Query verified pre-operation state
            DeviceStatus preStatus = QueryDeviceState(device.InstanceId, out uint preDevStatus, out uint preProblem);
            if (enable && preStatus == DeviceStatus.Enabled)
            {
                return ToggleResult.CreateSuccess(preStatus, preStatus, "Touchscreen is already enabled.");
            }
            if (!enable && preStatus == DeviceStatus.Disabled)
            {
                return ToggleResult.CreateSuccess(preStatus, preStatus, "Touchscreen is already disabled.");
            }

            // 2. Setup SetupAPI enumeration to locate the device
            IntPtr hDevInfo = NativeMethods.SetupDiGetClassDevsW(
                IntPtr.Zero,
                "HID",
                IntPtr.Zero,
                NativeMethods.DIGCF_ALLCLASSES);

            if (hDevInfo == IntPtr.Zero || hDevInfo.ToInt64() == -1)
            {
                int err = Marshal.GetLastWin32Error();
                return ToggleResult.CreateFailure(preStatus, preStatus, $"SetupDiGetClassDevs failed: {err}", err);
            }

            int lastError = 0;
            bool rebootFlag = false;
            bool actionExecuted = false;

            try
            {
                var devInfoData = new NativeMethods.SP_DEVINFO_DATA();
                devInfoData.cbSize = (uint)Marshal.SizeOf<NativeMethods.SP_DEVINFO_DATA>();

                uint memberIndex = 0;
                bool found = false;

                while (NativeMethods.SetupDiEnumDeviceInfo(hDevInfo, memberIndex, ref devInfoData))
                {
                    memberIndex++;

                    IntPtr buffer = Marshal.AllocHGlobal(1024);
                    string currInstance = string.Empty;
                    try
                    {
                        if (NativeMethods.SetupDiGetDeviceInstanceIdW(hDevInfo, ref devInfoData, buffer, 512, out _))
                        {
                            currInstance = Marshal.PtrToStringUni(buffer) ?? string.Empty;
                        }
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(buffer);
                    }

                    if (string.Equals(currInstance, device.InstanceId, StringComparison.OrdinalIgnoreCase))
                    {
                        found = true;

                        // Strategy 1: SetupAPI with DICS_FLAG_GLOBAL
                        var propChangeParams = new NativeMethods.SP_PROPCHANGE_PARAMS
                        {
                            ClassInstallHeader = new NativeMethods.SP_CLASSINSTALL_HEADER
                            {
                                cbSize = (uint)Marshal.SizeOf<NativeMethods.SP_CLASSINSTALL_HEADER>(),
                                InstallFunction = NativeMethods.DIF_PROPERTYCHANGE
                            },
                            StateChange = enable ? NativeMethods.DICS_ENABLE : NativeMethods.DICS_DISABLE,
                            Scope = NativeMethods.DICS_FLAG_GLOBAL,
                            HwProfile = 0
                        };

                        uint paramsSize = (uint)Marshal.SizeOf<NativeMethods.SP_PROPCHANGE_PARAMS>();
                        bool paramSet = NativeMethods.SetupDiSetClassInstallParamsW(hDevInfo, ref devInfoData, ref propChangeParams, paramsSize);
                        bool callSuccess = paramSet && NativeMethods.SetupDiCallClassInstaller(NativeMethods.DIF_PROPERTYCHANGE, hDevInfo, ref devInfoData);

                        if (!callSuccess)
                        {
                            lastError = Marshal.GetLastWin32Error();

                            // Strategy 2: SetupAPI with DICS_FLAG_CONFIGSPECIFIC
                            propChangeParams.Scope = NativeMethods.DICS_FLAG_CONFIGSPECIFIC;
                            if (NativeMethods.SetupDiSetClassInstallParamsW(hDevInfo, ref devInfoData, ref propChangeParams, paramsSize))
                            {
                                callSuccess = NativeMethods.SetupDiCallClassInstaller(NativeMethods.DIF_PROPERTYCHANGE, hDevInfo, ref devInfoData);
                                if (callSuccess)
                                    lastError = 0;
                                else
                                    lastError = Marshal.GetLastWin32Error();
                            }
                        }

                        // Inspect install params for reboot flags
                        var installParams = new NativeMethods.SP_DEVINSTALL_PARAMS();
                        installParams.cbSize = (uint)Marshal.SizeOf<NativeMethods.SP_DEVINSTALL_PARAMS>();
                        if (NativeMethods.SetupDiGetDeviceInstallParamsW(hDevInfo, ref devInfoData, ref installParams))
                        {
                            if ((installParams.Flags & NativeMethods.DI_NEEDREBOOT) != 0 || (installParams.Flags & NativeMethods.DI_NEEDRESTART) != 0)
                            {
                                rebootFlag = true;
                            }
                        }

                        actionExecuted = callSuccess;
                        break;
                    }
                }

                if (!found)
                {
                    return ToggleResult.CreateFailure(preStatus, DeviceStatus.DeviceNotFound, "Target device not found in active SetupAPI enumeration.");
                }
            }
            finally
            {
                NativeMethods.SetupDiDestroyDeviceInfoList(hDevInfo);
            }

            // Strategy 3 fallback: Configuration Manager direct DevNode toggle if SetupAPI didn't succeed
            if (!actionExecuted)
            {
                uint crLocate = NativeMethods.CM_Locate_DevNodeW(out uint dnDevInst, device.InstanceId, 0);
                if (crLocate == NativeMethods.CR_SUCCESS)
                {
                    uint crToggle = enable
                        ? NativeMethods.CM_Enable_DevNode(dnDevInst, 0)
                        : NativeMethods.CM_Disable_DevNode(dnDevInst, 0);

                    if (crToggle == NativeMethods.CR_SUCCESS)
                    {
                        actionExecuted = true;
                        lastError = 0;
                    }
                }
            }

            // 3. Post-execution Independent Verification
            DeviceStatus postStatus = QueryDeviceState(device.InstanceId, out uint postDevStatus, out uint postProblem);

            if (rebootFlag || postStatus == DeviceStatus.RebootPending)
            {
                return ToggleResult.CreateFailure(
                    preStatus,
                    DeviceStatus.RebootPending,
                    "Windows requires a system restart before the touchscreen state change takes effect.",
                    lastError,
                    reboot: true);
            }

            DeviceStatus expectedStatus = enable ? DeviceStatus.Enabled : DeviceStatus.Disabled;
            if (postStatus == expectedStatus)
            {
                device.CurrentStatus = postStatus;
                device.DevNodeStatus = postDevStatus;
                device.ProblemCode = postProblem;
                return ToggleResult.CreateSuccess(
                    preStatus,
                    postStatus,
                    enable ? "Touchscreen enabled successfully." : "Touchscreen disabled successfully.");
            }

            // If access denied
            if (lastError == 5)
            {
                return ToggleResult.CreateFailure(
                    preStatus,
                    postStatus,
                    "Access Denied (Error 5). Administrator rights are required to toggle touchscreen hardware.",
                    5);
            }

            // Post-verification mismatch: Do not falsely report success
            return ToggleResult.CreateFailure(
                preStatus,
                postStatus,
                $"Device state verification mismatch. Expected: {expectedStatus}, Actual: {postStatus} (Problem code: {postProblem}, LastError: {lastError}).",
                lastError);
        }
    }
}
