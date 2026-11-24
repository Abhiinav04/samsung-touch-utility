#!/usr/bin/env python3
"""
Samsung Touchscreen System Tray Indicator for Linux
Lightweight, single-click indicator for Samsung laptops.
Works with AyatanaAppIndicator3 or AppIndicator3 on GNOME, KDE, XFCE.
"""

import os
import subprocess
import sys
import gi

try:
    gi.require_version('Gtk', '3.0')
    from gi.repository import Gtk, GLib
except Exception as e:
    sys.exit(f"GTK3 not available: {e}")

# Try AyatanaAppIndicator3 first (modern), fallback to AppIndicator3
indicator_pkg = None
for pkg in ('AyatanaAppIndicator3', 'AppIndicator3'):
    try:
        gi.require_version(pkg, '0.1')
        from gi.repository import AyatanaAppIndicator3 as appindicator
        indicator_pkg = pkg
        break
    except Exception:
        continue

if not indicator_pkg:
    try:
        gi.require_version('AppIndicator3', '0.1')
        from gi.repository import AppIndicator3 as appindicator
        indicator_pkg = 'AppIndicator3'
    except Exception:
        indicator_pkg = None

APPINDICATOR_ID = 'samsung-touch-utility'

def get_cli_path():
    script_dir = os.path.dirname(os.path.realpath(__file__))
    local_cli = os.path.join(script_dir, "samsung-touch-toggle")
    if os.path.exists(local_cli):
        return local_cli
    return "samsung-touch-toggle"

class SamsungTouchTray:
    def __init__(self):
        self.cli = get_cli_path()
        self.current_state = "UNKNOWN"

        if indicator_pkg:
            self.indicator = appindicator.Indicator.new(
                APPINDICATOR_ID,
                "input-touchscreen",
                appindicator.IndicatorCategory.HARDWARE
            )
            self.indicator.set_status(appindicator.IndicatorStatus.ACTIVE)
            self.menu = Gtk.Menu()

            # Single toggle action menu item
            self.item_toggle = Gtk.MenuItem(label="Toggle Touchscreen")
            self.item_toggle.connect("activate", self.on_toggle_clicked)
            self.menu.append(self.item_toggle)

            self.menu.append(Gtk.SeparatorMenuItem())
            item_quit = Gtk.MenuItem(label="Quit")
            item_quit.connect("activate", Gtk.main_quit)
            self.menu.append(item_quit)

            self.menu.show_all()
            self.indicator.set_menu(self.menu)
        else:
            # Fallback to standard Gtk.StatusIcon
            self.status_icon = Gtk.StatusIcon()
            self.status_icon.set_from_icon_name("input-touchscreen")
            self.status_icon.connect("activate", self.on_toggle_clicked)

        self.update_state()
        GLib.timeout_add_seconds(3, self.periodic_check)

    def query_status(self):
        try:
            res = subprocess.run([self.cli, "status"], capture_output=True, text=True, timeout=2)
            return res.stdout.strip()
        except Exception:
            return "UNKNOWN"

    def update_state(self):
        status = self.query_status()
        self.current_state = status

        if status == "ENABLED":
            title = "TOUCH ON"
            icon_name = "touch-screen-active" if Gtk.IconTheme.get_default().has_icon("touch-screen-active") else "input-touchscreen"
        elif status == "DISABLED":
            title = "TOUCH OFF"
            icon_name = "touch-screen-disabled" if Gtk.IconTheme.get_default().has_icon("touch-screen-disabled") else "action-unavailable"
        else:
            title = "TOUCH STATUS UNKNOWN"
            icon_name = "dialog-warning"

        if indicator_pkg:
            self.indicator.set_title(title)
            self.indicator.set_icon_full(icon_name, title)
            self.item_toggle.set_label(f"{title} (Click to toggle)")
        else:
            self.status_icon.set_tooltip_text(title)
            self.status_icon.set_from_icon_name(icon_name)

    def on_toggle_clicked(self, widget):
        try:
            subprocess.run([self.cli, "toggle"], timeout=3)
            self.update_state()
            self.show_notification(f"Touchscreen: {self.current_state}")
        except Exception as ex:
            self.show_notification(f"Toggle error: {ex}")

    def periodic_check(self):
        self.update_state()
        return True

    def show_notification(self, message):
        try:
            subprocess.run(["notify-send", "-a", "Samsung Touchscreen Utility", "-i", "input-touchscreen", "Touchscreen", message])
        except Exception:
            pass

def main():
    SamsungTouchTray()
    Gtk.main()

if __name__ == "__main__":
    main()
