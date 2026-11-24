# PowerShell Installer for Samsung Touch Utility
[CmdletBinding()]
param (
    [switch]$EnableStartup = $true,
    [string]$InstallPath = "$env:LOCALAPPDATA\Programs\SamsungTouchUtility"
)

$ErrorActionPreference = 'Stop'

Write-Host "=== Installing Samsung Touchscreen Utility 1.0.0 ===" -ForegroundColor Cyan

# 1. Stop any running instances
$existing = Get-Process -Name "SamsungTouchUtility" -ErrorAction SilentlyContinue
if ($existing) {
    Write-Host "Stopping existing instance..." -ForegroundColor Yellow
    $existing | Stop-Process -Force
    Start-Sleep -Seconds 1
}

# 2. Create target directory
if (-not (Test-Path $InstallPath)) {
    New-Item -ItemType Directory -Path $InstallPath -Force | Out-Null
}

# 3. Copy application binary
$sourceExe = Join-Path $PSScriptRoot "..\release\publish\SamsungTouchUtility.exe"
if (-not (Test-Path $sourceExe)) {
    throw "Published executable not found at $sourceExe"
}

Copy-Item -Path $sourceExe -Destination (Join-Path $InstallPath "SamsungTouchUtility.exe") -Force
Write-Host "Copied application binary to $InstallPath" -ForegroundColor Green

# 4. Optional startup configuration
$runKey = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run"
if ($EnableStartup) {
    Set-ItemProperty -Path $runKey -Name "SamsungTouchUtility" -Value "`"$InstallPath\SamsungTouchUtility.exe`""
    Write-Host "Configured Windows startup entry." -ForegroundColor Green
}

# 5. Create Start Menu shortcut
$startMenuDir = "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\Samsung Touchscreen Utility"
if (-not (Test-Path $startMenuDir)) {
    New-Item -ItemType Directory -Path $startMenuDir -Force | Out-Null
}

$wshShell = New-Object -ComObject WScript.Shell
$shortcut = $wshShell.CreateShortcut("$startMenuDir\Samsung Touchscreen Utility.lnk")
$shortcut.TargetPath = "$InstallPath\SamsungTouchUtility.exe"
$shortcut.Description = "Single-click system tray touchscreen toggle for Samsung laptops"
$shortcut.Save()

Write-Host "Created Start Menu shortcut." -ForegroundColor Green

# 6. Launch application with elevation
Write-Host "Launching Samsung Touchscreen Utility..." -ForegroundColor Cyan
Start-Process "$InstallPath\SamsungTouchUtility.exe" -Verb RunAs

Write-Host "Installation completed successfully! Utility is active in the system tray." -ForegroundColor Green
