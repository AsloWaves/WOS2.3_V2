# Fix WSL2 Networking for UDP (Windows 11 22H2+ only)
# Enables mirrored networking mode which properly forwards UDP

Write-Host "Checking Windows version..." -ForegroundColor Cyan

$winVersion = [System.Environment]::OSVersion.Version
Write-Host "Windows Version: $winVersion" -ForegroundColor Yellow

if ($winVersion.Major -lt 10 -or ($winVersion.Major -eq 10 -and $winVersion.Build -lt 22000)) {
    Write-Host ""
    Write-Host "ERROR: Mirrored networking requires Windows 11 22H2 or later" -ForegroundColor Red
    Write-Host "Your Windows version: $winVersion" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Alternative: Use Editor Host mode or deploy to Edgegap" -ForegroundColor Yellow
    exit 1
}

Write-Host "Windows 11 detected - proceeding with WSL2 configuration" -ForegroundColor Green
Write-Host ""

# Create .wslconfig file
$wslConfigPath = "$env:USERPROFILE\.wslconfig"
$wslConfig = @"
[wsl2]
networkingMode=mirrored
dnsTunneling=true
firewall=true
autoProxy=true
"@

Write-Host "Creating $wslConfigPath..." -ForegroundColor Cyan
$wslConfig | Out-File -FilePath $wslConfigPath -Encoding ASCII -Force

Write-Host "Configuration written:" -ForegroundColor Green
Get-Content $wslConfigPath

Write-Host ""
Write-Host "IMPORTANT: You must restart WSL2 for changes to take effect!" -ForegroundColor Yellow
Write-Host ""
Write-Host "Run these commands:" -ForegroundColor Cyan
Write-Host "  wsl --shutdown" -ForegroundColor White
Write-Host "  (Wait 10 seconds)" -ForegroundColor Gray
Write-Host "  Restart Docker Desktop" -ForegroundColor White
Write-Host ""
Write-Host "Then redeploy your container and try connecting again." -ForegroundColor Yellow
