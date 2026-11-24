# Product Requirements Document (PRD)
## Samsung Touchscreen Utility (SamsungTouchUtility)

### 1. Overview
SamsungTouchUtility is a minimalist, single-purpose Windows 11 utility engineered to toggle the integrated touchscreen on Samsung laptops (specifically Samsung Galaxy Book series such as Galaxy Book3 360, while dynamically supporting other Samsung touchscreen hardware) via a single system tray click or a global keyboard shortcut.

### 2. Product Principles & Non-Negotiables
- **Single Functionality**: Toggle touchscreen ON / OFF.
- **Strictly One User Control**: No separate ON/OFF buttons, no context menu, no dashboard, no configuration window, no web interface, no telemetry, no cloud dependencies.
- **Hardware Verified State**: The UI state (icon and tooltip) must always represent the verified hardware state queried from the Windows Plug and Play (PnP) subsystem, never an assumed or optimistic state.
- **Safety First**: Never disable parent ACPI buses, keyboards, touchpads, or S Pen/stylus interfaces. Never force reboots or make unverified hardware changes.
- **Complete Offline Operation**: Zero network calls, zero telemetry, zero background updates.

### 3. User Experience & Interactions
- **System Tray Icon**:
  - **Left Click**: Requests state toggle (ON -> OFF or OFF -> ON).
  - **Right Click / Double Click**: No context menu; performs same toggle or remains passive.
- **Visual States**:
  - **Green Icon**: Touchscreen is ACTIVE (Tooltip: `TOUCH ON`).
  - **Red Icon**: Touchscreen is DISABLED (Tooltip: `TOUCH OFF`).
  - **Grey Icon**: Touchscreen state cannot be determined or requires action (Tooltip: `TOUCH STATUS UNKNOWN`).
- **Global Keyboard Shortcut**:
  - `Ctrl + Alt + T` registered via Windows `RegisterHotKey`.
- **System Notifications**:
  - Transient native Windows balloon/toast notifications upon state change or failure.
  - No notifications when state is unchanged or on passive background monitoring.

### 4. Technical Constraints
- **Target OS**: Windows 11 x64 (Build 22000+).
- **Target Framework**: .NET 10.0 (Windows desktop runtime, self-contained single-file publishable).
- **Footprint**: Idle RAM < 50 MB, CPU near 0%, startup time < 2 seconds.
- **Privilege Model**: Least privilege for discovery and monitoring; standard Windows UAC elevation requested only when invoking hardware state change APIs if required by the driver.
