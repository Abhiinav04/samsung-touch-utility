#!/usr/bin/env bash
# ==============================================================================
# Uninstaller for Samsung Touchscreen Utility on Linux
# ==============================================================================

set -euo pipefail

BIN_DIR="/usr/local/bin"
UDEV_DIR="/etc/udev/rules.d"
AUTOSTART_DIR="$HOME/.config/autostart"
DESKTOP_DIR="$HOME/.local/share/applications"

echo "=== Uninstalling Samsung Touchscreen Utility ==="

# Kill running tray instances
killall -q samsung-touch-utility || true

# Remove binaries
sudo rm -f "$BIN_DIR/samsung-touch-toggle" "$BIN_DIR/samsung-touch-utility"

# Remove udev rule
sudo rm -f "$UDEV_DIR/99-samsung-touch.rules"
sudo udevadm control --reload-rules && sudo udevadm trigger

# Remove autostart and desktop entries
rm -f "$DESKTOP_DIR/samsung-touch-utility.desktop"
rm -f "$AUTOSTART_DIR/samsung-touch-utility.desktop"

echo "Samsung Touchscreen Utility uninstalled cleanly."
