Set-Location -Path $PSScriptRoot
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Gorev Takip Sistemi baslatiliyor..." -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Site adresi: http://localhost:5009" -ForegroundColor Yellow
Write-Host "Durdurmak icin Ctrl+C bas" -ForegroundColor DarkGray
Write-Host ""
Start-Process "http://localhost:5009"
dotnet run
