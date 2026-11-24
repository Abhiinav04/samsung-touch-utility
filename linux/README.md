# Samsung Touchscreen Utility for Linux

A lightweight, seamless touchscreen toggle utility engineered for Samsung laptops (Galaxy Book series, Galaxy Book3 360, etc.) running Linux (Ubuntu, Fedora, Arch Linux, Debian, openSUSE).

## Features

- **Instant Switching**: Disables and enables the Goodix / Samsung touchscreen dynamically via Linux kernel driver `sysfs` unbind/bind or `xinput`.
- **Zero Reboot Required**: Unlike Windows KMDF restrictions, Linux kernel driver unbinding happens instantly in real-time.
- **Peripheral Protection**: Strictly protects touchpad (`i2c-ELAN0B00`), S Pen / Stylus digitizer (`i2c-WCOM016B`), and keyboard.
- **System Tray Icon**: Python 3 GTK indicator showing `TOUCH ON` / `TOUCH OFF`.
- **Keyboard Shortcut**: Seamlessly binds to `Ctrl + Alt + T` in GNOME, KDE Plasma, or Sway/Hyprland.
- **Passwordless Execution**: Bundles a `udev` rule so toggling requires zero `sudo` password prompts.

## Quick Install

```bash
git clone https://github.com/Abhiinav04/samsung-touch-utility.git
cd samsung-touch-utility/linux
chmod +x install.sh
./install.sh
```

## Usage

### CLI
```bash
samsung-touch-toggle          # Toggles touch state
samsung-touch-toggle status   # Prints ENABLED, DISABLED, or UNKNOWN
samsung-touch-toggle enable   # Enables touchscreen
samsung-touch-toggle disable  # Disables touchscreen
```

### GUI / System Tray
Run:
```bash
samsung-touch-utility &
```

### Global Shortcut (GNOME / KDE)
In your Desktop Environment:
- Go to **Settings** > **Keyboard** > **Keyboard Shortcuts** > **Custom Shortcuts**.
- Name: `Toggle Touchscreen`
- Command: `/usr/local/bin/samsung-touch-toggle`
- Shortcut: `Ctrl + Alt + T`
