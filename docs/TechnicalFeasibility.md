# Technical Feasibility & Device Research Report
## Target: Samsung Galaxy Book3 360 & Samsung Laptop Ecosystem

### 1. Verified Hardware Facts (Inspected on Target Machine)
- **Host Device**: Samsung Galaxy Book3 360 (Host: ABHINAV-GALAXYB, Windows 11 x64 Build 26100).
- **Touch Controller Device**:
  - **Parent ACPI Node**: `ACPI\GXTP7936\10`
    - Service: `hidi2c` (Microsoft I2C HID Minidriver)
    - UpperFilters: `mshidkmdf`
    - BIOS Name: `\_SB.PC00.I2C1.STSP`
    - Hardware IDs: `ACPI\VEN_GXTP&DEV_7936`, `ACPI\GXTP7936`, `*GXTP7936`
    - Status: `OK`, Problem Code: `CM_PROB_NONE`
    - Reboot Required: `False`
  - **Child HID Collections**:
    - `HID\GXTP7936&COL01\5&1FFF58BB&0&0000` (Friendly Name: `HID-compliant touch screen`)
      - MatchingDeviceId: `HID_DEVICE_UP:000D_U:0004` (Digitizer Usage Page 0x0D, Touch Screen Usage 0x04)
      - Class: `HIDClass` (`{745A17A0-74D3-11D0-B6FE-00A0C90F57DA}`)
      - Driver: `input.inf` (`HID_Raw_Inst.NT`)
      - Current Problem Code: `CM_PROB_NONE`
    - `HID\GXTP7936&COL02\...` (Vendor-defined collection)
    - `HID\GXTP7936&COL03\...` (Vendor-defined collection)
  - **Isolated Peripheral Devices (Must NEVER Be Affected)**:
    - **Touchpad**: `ACPI\ELAN0B00\10` -> `HID\ELAN0B00&COL03\...` (ELAN I2C HID)
    - **Stylus / S Pen**: `ACPI\WCOM016B\1` -> `HID\WCOM016B&COL04\...` (Wacom Digitizer / Pen)
    - **Keyboard & System Controls**: Standard PS/2 / ACPI keyboard and converted button devices.

### 2. Documented Windows API Behavior
Windows PnP provides multiple layers for querying and altering device state:
1. **Configuration Manager APIs (`cfgmgr32.dll`)**:
   - `CM_Get_DevNode_Status(&status, &problem, dnDevInst, 0)`: Instantaneous, non-blocking hardware state evaluation.
   - `CM_Locate_DevNode`, `CM_Get_Parent`, `CM_Get_Child`, `CM_Get_Sibling`: Traversal of the PnP device hierarchy without WMI overhead.
   - `CM_Disable_DevNode` / `CM_Enable_DevNode`: Request hardware enable/disable.
2. **SetupAPI (`setupapi.dll`)**:
   - `SetupDiGetClassDevs`, `SetupDiEnumDeviceInfo`: Enumerates installed devices matching GUIDs or enumerators.
   - `SetupDiSetClassInstallParams` with `SP_PROPCHANGE_PARAMS` (`DIF_PROPERTYCHANGE`):
     - `DICS_DISABLE` / `DICS_ENABLE` with scopes `DICS_FLAG_GLOBAL` and `DICS_FLAG_CONFIGSPECIFIC`.
   - `SetupDiCallClassInstaller(DIF_PROPERTYCHANGE, ...)`: Standard mechanism used by Windows Device Manager.
   - `SetupDiGetDeviceInstallParams`: Returns `DI_NEEDREBOOT` or `DI_NEEDRESTART` when driver requires system reboot.

### 3. Investigation of Previous Failures & Controller Nuances
- **Why PowerShell `Disable-PnpDevice` Failed**:
  - `Disable-PnpDevice` invokes WMI (`root\cimv2:Win32_PnPEntity`). WMI executes an asynchronous method that calls generic removal routines. On KMDF top-level HID collection PDOs (`mshidkmdf.sys`), raw collection children reject dynamic subtree teardown unless handled through SetupAPI property change.
  - Previous attempts left `DEVPKEY_Device_IsRebootRequired = True` on the child device node.
- **Safety Boundary Rules**:
  - Never target `ACPI\GXTP7936\10` directly if doing so risks unbinding power or interrupting child devices during live runtime.
  - Never target the generic HID class GUID without strict instance ID allowlisting matching Usage Page `0x0D` and Usage `0x04`.
  - Distinguish live switching capability from reboot-required states. If Windows flags `DI_NEEDREBOOT` or `DEVPKEY_Device_IsRebootRequired`, report this accurately to the user rather than claiming instant success.

### 4. Assumptions & Resolution Strategy
- **Dynamic Detection**: Search for devices with class `HIDClass`, matching digitizer usage `HID_DEVICE_UP:000D_U:0004`, and parent hardware IDs containing known Samsung touchscreen vendors (Goodix `GXTP`, Atmel `ATML`, Elan `ELAN`, etc.).
- **Atomic Concurrency**: Use asynchronous locking (`SemaphoreSlim(1, 1)`) so rapid clicks or hotkeys never trigger overlapping PnP operations.
