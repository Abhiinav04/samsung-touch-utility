using System;
using System.Collections.Generic;

namespace SamsungTouchUtility.Models
{
    public enum DeviceStatus
    {
        Unknown = 0,
        Enabled = 1,
        Disabled = 2,
        RebootPending = 3,
        DeviceNotFound = 4
    }

    public sealed class TouchDeviceInfo
    {
        public required string InstanceId { get; init; }
        public required string FriendlyName { get; init; }
        public required string HardwareId { get; init; }
        public string? ParentInstanceId { get; init; }
        public uint ProblemCode { get; set; }
        public uint DevNodeStatus { get; set; }
        public bool IsRebootRequired { get; set; }
        public DeviceStatus CurrentStatus { get; set; } = DeviceStatus.Unknown;

        public override string ToString() => $"{FriendlyName} [{InstanceId}] (Status: {CurrentStatus}, Prob: {ProblemCode})";
    }

    public sealed class ToggleResult
    {
        public bool Success { get; init; }
        public DeviceStatus PreviousStatus { get; init; }
        public DeviceStatus NewStatus { get; init; }
        public string Message { get; init; } = string.Empty;
        public bool RebootRequired { get; init; }
        public int ErrorCode { get; init; }

        public static ToggleResult CreateSuccess(DeviceStatus prev, DeviceStatus current, string msg = "") =>
            new ToggleResult
            {
                Success = true,
                PreviousStatus = prev,
                NewStatus = current,
                Message = msg
            };

        public static ToggleResult CreateFailure(DeviceStatus prev, DeviceStatus current, string msg, int errCode = 0, bool reboot = false) =>
            new ToggleResult
            {
                Success = false,
                PreviousStatus = prev,
                NewStatus = current,
                Message = msg,
                ErrorCode = errCode,
                RebootRequired = reboot
            };
    }
}
