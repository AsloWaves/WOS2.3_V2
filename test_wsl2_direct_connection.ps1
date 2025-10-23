# Test Direct WSL2 Connection (Bypass Docker Port Mapping)
# This bypasses Docker Desktop's problematic port mapping layer

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "WSL2 Direct Connection Test" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Get WSL2 IP address
Write-Host "Finding WSL2 IP address..." -ForegroundColor Yellow
$wslIP = wsl ip addr show eth0 | Select-String "inet " | ForEach-Object { ($_ -split "\s+")[2] -replace "/.*","" }

if ([string]::IsNullOrEmpty($wslIP)) {
    Write-Host "ERROR: Could not find WSL2 IP address" -ForegroundColor Red
    Write-Host "Make sure WSL2 is running and Docker Desktop is started" -ForegroundColor Yellow
    exit 1
}

Write-Host "WSL2 IP Address: $wslIP" -ForegroundColor Green
Write-Host ""

# Check if container is running
Write-Host "Checking for Docker container..." -ForegroundColor Yellow
$containerName = docker ps --filter "name=edgegap" --format "{{.Names}}" | Select-Object -First 1

if ([string]::IsNullOrEmpty($containerName)) {
    Write-Host "WARNING: No Edgegap container running" -ForegroundColor Red
    Write-Host "Please deploy container via Unity → Tools → Edgegap → Deploy Local Container" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "When container is running, connect to: ${wslIP}:7777" -ForegroundColor Cyan
    exit 1
}

Write-Host "Found container: $containerName" -ForegroundColor Green
Write-Host ""

# Get container's internal IP
$containerIP = docker inspect $containerName --format '{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}'
$containerPort = docker inspect $containerName --format '{{range $p, $conf := .NetworkSettings.Ports}}{{if eq $p "7777/udp"}}{{(index $conf 0).HostPort}}{{end}}{{end}}'

Write-Host "Container Details:" -ForegroundColor Yellow
Write-Host "  Container IP: $containerIP" -ForegroundColor White
Write-Host "  Container Name: $containerName" -ForegroundColor White
Write-Host "  Mapped Port: $containerPort" -ForegroundColor White
Write-Host ""

# Test UDP connectivity
Write-Host "Testing UDP connectivity to WSL2..." -ForegroundColor Yellow
Write-Host ""
Write-Host "Method 1: Connect to WSL2 IP (Recommended)" -ForegroundColor Cyan
Write-Host "  Connection Address: ${wslIP}:7777" -ForegroundColor Green
Write-Host "  This bypasses Docker Desktop's port mapping!" -ForegroundColor Gray
Write-Host ""

# Update ServerConfig with WSL2 IP
Write-Host "To use this, update ServerConfig.asset:" -ForegroundColor Yellow
Write-Host "  localServerAddress: ${wslIP}:7777" -ForegroundColor Green
Write-Host ""

Write-Host "Method 2: Connect to Docker Container directly (Advanced)" -ForegroundColor Cyan
Write-Host "  Connection Address: ${containerIP}:7777" -ForegroundColor Green
Write-Host "  This requires adding route to Docker network!" -ForegroundColor Gray
Write-Host ""

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Next Steps:" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "1. Copy this IP: $wslIP" -ForegroundColor White
Write-Host "2. Unity → Assets/Resources/ServerConfigs/ServerConfig.asset" -ForegroundColor White
Write-Host "3. Set localServerAddress = ${wslIP}:7777" -ForegroundColor White
Write-Host "4. Unity → Play → Join Game → Connect" -ForegroundColor White
Write-Host ""
Write-Host "This should enable bidirectional UDP!" -ForegroundColor Green
