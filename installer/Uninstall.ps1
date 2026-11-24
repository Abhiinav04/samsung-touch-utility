# PowerShell Uninstaller for Samsung Touch Utility
[CmdletBinding()]
param (
    [string]$InstallPath = "$env:LOCALAPPDATA\Programs\SamsungTouchUtility"
)

$ErrorActionPreference = 'SilentlyContinue'

Write-Host "=== Uninstalling Samsung Touchscreen Utility ===" -ForegroundColor Yellow

# 1. Terminate running process
Get-Process -Name "SamsungTouchUtility" | Stop-Process -Force

# 2. Remove startup registry entry
$runKey = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run"
Remove-ItemProperty -Path $runKey -Name "SamsungTouchUtility" -ErrorAction SilentlyContinue

# 3. Remove Start Menu shortcut
$startMenuDir = "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\Samsung Touchscreen Utility"
if (Test-Path $startMenuDir) {
    Remove-Item -Path $startMenuDir -Recurse -Force
}

# 4. Remove installation files
if (Test-Path $InstallPath) {
    Remove-Item -Path $InstallPath -Recurse -Force
}

Write-Host "Samsung Touchscreen Utility uninstalled cleanly." -ForegroundColor Green
Write-Host "Touchscreen device drivers remain 100% untouched." -ForegroundColor Cyan
