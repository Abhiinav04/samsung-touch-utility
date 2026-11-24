@echo off
setlocal
:: Check for admin rights and self-elevate if not elevated
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo Requesting Administrator privileges to install Samsung Touch Utility...
    powershell.exe -Command "Start-Process '%~f0' -Verb RunAs"
    exit /b
)

echo ========================================================
echo   Samsung Touchscreen Utility - Installer
echo ========================================================
echo.
powershell.exe -ExecutionPolicy Bypass -File "%~dp0Install.ps1" -EnableStartup
echo.
pause
