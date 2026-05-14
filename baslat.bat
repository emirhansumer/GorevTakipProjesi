@echo off
title Gorev Takip Sistemi
cd /d "%~dp0"
echo ============================================
echo   Gorev Takip Sistemi baslatiliyor...
echo ============================================
echo.
echo Site adresi: http://localhost:5009
echo.
echo Durdurmak icin bu pencerede Ctrl+C bas
echo ============================================
echo.
start "" "http://localhost:5009"
dotnet run --launch-profile http
pause
