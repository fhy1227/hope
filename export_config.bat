@echo off
chcp 65001 >nul
cd /d "%~dp0"
C:\Users\admin\.workbuddy\binaries\python\versions\3.13.12\python.exe tools/export_config.py
pause
