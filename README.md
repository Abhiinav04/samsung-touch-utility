# Samsung Touchscreen Utility (SamsungTouchUtility)

A production-quality Windows 11 utility engineered specifically for Samsung laptops (primary target: Samsung Galaxy Book3 360, while dynamically detecting and supporting all compatible Samsung touchscreen hardware).

## Core Principles

- **ONE Tray Icon**: Always visible in the system notification area.
- **ONE Click**: Single left-click toggles the touchscreen state.
- **ONE Global Shortcut**: `Ctrl + Alt + T` toggles the touchscreen anytime.
- **ZERO Extra Windows**: No dashboard, no context menus, no settings dialogs, no web views.
- **100% Offline**: Zero telemetry, zero cloud calls, zero background network traffic.

## Visual States

| State | Tray Icon Appearance | Tooltip | Description |
|---|---|---|---|
| **Active / Enabled** | Modern Emerald Green Dot & Wave | `TOUCH ON` | Touchscreen hardware is operational. |
| **Disabled** | Crimson Red Dot with Strike | `TOUCH OFF` | Touchscreen hardware is disabled. |
| **Unknown / Attention** | Neutral Slate Gray Dot | `TOUCH STATUS UNKNOWN` | Hardware state is pending reboot or unverified. |

## Safety Invariants

- **Parent ACPI Protection**: Never touches or disables the parent ACPI bus controller (`ACPI\GXTP7936\10`).
- **Peripheral Isolation**: Touchpad (`ELAN0B00`), S Pen / Stylus digitizer (`WCOM016B`), and Keyboard devices are strictly excluded and remain 100% functional.
- **No Blind Inversion**: Always interrogates `CM_Get_DevNode_Status` before and after any state transition; never reports false success.

## Installation

### Option 1: Standalone Installer
Run `release/installer/SamsungTouchUtility-Setup-1.0.0.exe` and follow the prompt.

### Option 2: Portable Self-Contained Executable
Run `release/publish/SamsungTouchUtility.exe`. No external .NET runtime installation required.

## Logging & Diagnostics
Logs are maintained locally under:
`%LOCALAPPDATA%\SamsungTouchUtility\logs\touch-utility-YYYYMMDD.log`
