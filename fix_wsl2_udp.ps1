# Fix WSL2 UDP Port Forwarding for Docker Desktop
# WSL2 sometimes fails to properly forward UDP ports
# This script manually configures Windows to forward UDP 7777 to WSL2

Write-Host "Configuring WSL2 UDP port forwarding for Docker..." -ForegroundColor Cyan

# Get WSL2 IP address
$wslIP = wsl hostname -I
$wslIP = $wslIP.Trim()

Write-Host "WSL2 IP Address: $wslIP" -ForegroundColor Yellow

# Remove existing port forward if it exists
netsh interface portproxy delete v4tov4 listenport=7777 listenaddress=0.0.0.0 protocol=udp 2>$null

# Add UDP port forward from Windows to WSL2
netsh interface portproxy add v4tov4 listenport=7777 listenaddress=0.0.0.0 connectport=7777 connectaddress=$wslIP protocol=udp

Write-Host ""
Write-Host "Port forwarding configured:" -ForegroundColor Green
Write-Host "  Windows 0.0.0.0:7777 (UDP) -> WSL2 ${wslIP}:7777" -ForegroundColor Green
Write-Host ""
Write-Host "Verifying configuration..." -ForegroundColor Cyan
netsh interface portproxy show v4tov4 | Select-String "7777"

Write-Host ""
Write-Host "Now try connecting from Unity Editor!" -ForegroundColor Yellow
Write-Host ""
Write-Host "To remove this forwarding later:" -ForegroundColor Gray
Write-Host "  netsh interface portproxy delete v4tov4 listenport=7777 listenaddress=0.0.0.0 protocol=udp" -ForegroundColor Gray
