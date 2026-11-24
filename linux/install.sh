#!/usr/bin/env bash
# ==============================================================================
# Installer for Samsung Touchscreen Utility on Linux
# ==============================================================================

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BIN_DIR="/usr/local/bin"
UDEV_DIR="/etc/udev/rules.d"
AUTOSTART_DIR="$HOME/.config/autostart"
DESKTOP_DIR="$HOME/.local/share/applications"

echo "=== Installing Samsung Touchscreen Utility for Linux ==="

# 1. Install CLI and GUI scripts to /usr/local/bin
echo "Installing binaries to $BIN_DIR..."
sudo cp "$SCRIPT_DIR/samsung-touch-toggle" "$BIN_DIR/samsung-touch-toggle"
sudo chmod +x "$BIN_DIR/samsung-touch-toggle"

sudo cp "$SCRIPT_DIR/samsung_touch_utility.py" "$BIN_DIR/samsung-touch-utility"
sudo chmod +x "$BIN_DIR/samsung-touch-utility"

# 2. Install udev rule for passwordless toggle
echo "Installing udev rules to $UDEV_DIR..."
sudo cp "$SCRIPT_DIR/99-samsung-touch.rules" "$UDEV_DIR/99-samsung-touch.rules"
sudo udevadm control --reload-rules && sudo udevadm trigger

# 3. Install Desktop shortcut & autostart
echo "Configuring desktop integration..."
mkdir -p "$DESKTOP_DIR" "$AUTOSTART_DIR"
cp "$SCRIPT_DIR/samsung-touch-utility.desktop" "$DESKTOP_DIR/"
cp "$SCRIPT_DIR/samsung-touch-utility.desktop" "$AUTOSTART_DIR/"

echo "=========================================================="
echo "Installation complete!"
echo "You can now run: 'samsung-touch-toggle' from your terminal"
echo "Or launch: 'samsung-touch-utility' to start the tray icon."
echo "Global shortcut tip: Set Ctrl+Alt+T in your Desktop Keyboard"
echo "shortcuts to run: /usr/local/bin/samsung-touch-toggle"
echo "=========================================================="
