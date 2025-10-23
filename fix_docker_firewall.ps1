# Fix Windows Firewall for Docker Desktop UDP 7777
# Allows Unity client to connect to Docker server on localhost

Write-Host "Adding Windows Firewall rule for Docker UDP 7777..." -ForegroundColor Cyan

# Add inbound rule for UDP 7777
New-NetFirewallRule `
    -DisplayName "WOS Docker Server UDP 7777" `
    -Description "Allow UDP traffic for WOS Naval MMO Docker Desktop testing" `
    -Direction Inbound `
    -Protocol UDP `
    -LocalPort 7777 `
    -Action Allow `
    -Profile Any `
    -Enabled True

Write-Host "Firewall rule added successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "You can now connect to Docker server at localhost:7777" -ForegroundColor Yellow
Write-Host ""
Write-Host "To remove this rule later, run:" -ForegroundColor Gray
Write-Host "  Remove-NetFirewallRule -DisplayName 'WOS Docker Server UDP 7777'" -ForegroundColor Gray
