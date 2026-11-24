@echo off
setlocal
echo Uninstalling Samsung Touchscreen Utility...
powershell.exe -ExecutionPolicy Bypass -File "%~dp0Uninstall.ps1"
pause
