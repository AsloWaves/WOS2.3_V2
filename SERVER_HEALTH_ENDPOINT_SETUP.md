# Server Health Endpoint Setup Guide

HTTP health endpoint implementation for WOS2.3 server monitoring and client connectivity checking.

---

## Overview

Implements industry-standard HTTP health endpoint for:
- ✅ Client connectivity validation (shows green "Server Up" only when server responds)
- ✅ Server monitoring and status dashboards
- ✅ Load balancer health checks
- ✅ Deployment validation
- ✅ Player count display

**Endpoints**:
- `GET /health` - Simple health check with player count
- `GET /info` - Detailed server information

---

## Server-Side Setup (Unity)

### Step 1: Add ServerHealthEndpoint Component

```
1. Hierarchy → Find your NetworkManager GameObject (or create "ServerHealthManager")
2. Add Component → ServerHealthEndpoint
3. Inspector settings:
   - Health Port: 8080 (default, must expose in Edgegap)
   - Enable Health Endpoint: ✅ Checked
```

**Component automatically**:
- ✅ Only runs in headless server builds
- ✅ Starts HTTP listener on port 8080
- ✅ Updates player count in real-time
- ✅ Responds to health check requests
- ✅ Handles CORS for web dashboards
- ✅ Graceful shutdown on server stop

### Step 2: Test Locally (Optional)

If testing locally before deploying:

```bash
# Start Unity server build
./ServerBuild.exe

# In another terminal/browser, test health endpoint:
curl http://localhost:8080/health

# Expected response:
{
    "status": "ok",
    "server": "running",
    "players": 0,
    "maxPlayers": 300,
    "uptime": 42,
    "timestamp": 1234567890
}
```

---

## Edgegap Deployment Configuration

### Step 1: Update Port Mappings

**Current Port Mapping**:
```json
{
  "gameport": {
    "internal": 7777,
    "external": 32479,
    "protocol": "UDP"
  }
}
```

**NEW Port Mapping** (add HTTP health port):
```json
{
  "gameport": {
    "internal": 7777,
    "external": 32479,
    "protocol": "UDP"
  },
  "healthport": {
    "internal": 8080,
    "external": 8080,
    "protocol": "TCP"
  }
}
```

### Step 2: Edgegap Configuration Methods

**Option A: Edgegap Dashboard** (Easiest):
```
1. Login to Edgegap dashboard
2. Navigate to your application/deployment
3. Edit Port Mappings:
   - Add new port mapping
   - Name: healthport
   - Internal Port: 8080
   - External Port: 8080 (or auto-assign)
   - Protocol: TCP
4. Redeploy server
```

**Option B: Edgegap API** (Programmatic):
```bash
# Update application port configuration
curl -X PATCH https://api.edgegap.com/v1/app/{app_name}/version/{version_name} \
  -H "Authorization: token YOUR_API_KEY" \
  -H "Content-Type: application/json" \
  -d '{
    "ports": [
      {
        "port": 7777,
        "protocol": "UDP",
        "name": "gameport"
      },
      {
        "port": 8080,
        "protocol": "TCP",
        "name": "healthport"
      }
    ]
  }'
```

**Option C: Edgegap Unity Plugin** (If installed):
```
1. Unity → Tools → Edgegap Hosting
2. Port Configuration:
   - Add Port:
     - Port: 8080
     - Protocol: TCP
     - Name: healthport
3. Deploy
```

### Step 3: Verify Deployment

After redeploying with new port mapping:

```bash
# Get deployment info
# Example public IP: 172.234.217.194
# Example health port: 8080 (or auto-assigned external port)

# Test health endpoint
curl http://172.234.217.194:8080/health

# Expected response:
{
    "status": "ok",
    "server": "running",
    "players": 3,
    "maxPlayers": 300,
    "uptime": 1234,
    "timestamp": 1234567890
}
```

**From Edgegap logs**, you should see:
```
[HealthEndpoint] 🏥 Starting HTTP health endpoint on port 8080...
[HealthEndpoint] ✅ Health endpoint started successfully
[HealthEndpoint] 🌐 Accessible at: http://SERVER_IP:8080/health
[HealthEndpoint] 💡 Configure Edgegap to expose port 8080 (TCP/HTTP)
[HealthEndpoint] 👂 Listener thread started
```

---

## Client-Side Setup (Unity Editor)

### Update JoinMenuController

**Already configured automatically!** JoinMenuController now:
- ✅ Uses HTTP health check instead of UDP probes
- ✅ Checks `http://SERVER_IP:8080/health`
- ✅ Shows green "Server Up" only when server responds
- ✅ Shows red "Server Down" if no response within 5 seconds
- ✅ Extracts player count from response (displayed in console)

### Inspector Settings

```
JoinMenuController component:
- Health Check Port: 8080 (must match server configuration)
- Status Check Timeout: 5f (seconds to wait for response)
- Status Check Interval: 30f (seconds between checks)
```

---

## Testing Workflow

### Test 1: Server Down Detection
```
1. Ensure Edgegap server is STOPPED
2. Unity Editor → MainMenu → Play
3. Navigate to Join Game panel
4. Observe status text:
   Expected: "Server Down - 172.234.217.194:32479 (Chicago, Illinois)" (RED)
5. Console shows:
   "[JoinMenu] ❌ Health check FAILED - Error: Cannot connect to destination host"
```

### Test 2: Server Up Detection
```
1. Start Edgegap server
2. Wait 30 seconds (or restart Unity)
3. Navigate to Join Game panel
4. Observe status text:
   Expected: "Server Up - 172.234.217.194:32479 (Chicago, Illinois)" (GREEN)
5. Console shows:
   "[JoinMenu] ✅ Health check SUCCESS - Response: {json}"
   "[JoinMenu] 👥 Server has 0 players"
```

### Test 3: Health Endpoint Direct Access
```
# Browser or curl:
http://172.234.217.194:8080/health

# Expected JSON response:
{
    "status": "ok",
    "server": "running",
    "players": 0,
    "maxPlayers": 300,
    "uptime": 3600,
    "timestamp": 1234567890
}
```

### Test 4: Detailed Server Info
```
# Browser or curl:
http://172.234.217.194:8080/info

# Expected JSON response:
{
    "status": "Running",
    "players": {
        "current": 5,
        "max": 300
    },
    "uptime": 7200,
    "scene": "PortHarbor",
    "version": "1.0.0",
    "platform": "Linux Server",
    "timestamp": 1234567890
}
```

---

## Troubleshooting

### Issue: Health Check Always Fails (Server Down)

**Possible Causes**:
1. ❌ Port 8080 not exposed in Edgegap
2. ❌ Wrong health check port in JoinMenuController
3. ❌ ServerHealthEndpoint component not added to server
4. ❌ Firewall blocking port 8080

**Solutions**:
- Check Edgegap port mappings (must have healthport: 8080/TCP)
- Verify JoinMenuController.healthCheckPort = 8080
- Verify ServerHealthEndpoint exists in server build scene
- Check server logs for "[HealthEndpoint] ✅ Health endpoint started"

### Issue: "Access Denied" Error in Server Logs

**Error**: `[HealthEndpoint] ❌ Failed to start HTTP listener: Access is denied`

**Cause**: Port 8080 requires admin privileges on some systems

**Solution**:
- Linux/Docker: No issue (default behavior)
- Windows local testing: Run server as Administrator
- Edgegap: No issue (containers have necessary permissions)

### Issue: Wrong External Port

**Problem**: Edgegap assigns different external port (e.g., 35123 instead of 8080)

**Solution**:
```
1. Check Edgegap deployment info for actual external port
2. Update JoinMenuController.healthCheckPort to match
   OR
3. Set healthport to "link_to: gameport" in Edgegap config to use same external port
```

### Issue: CORS Errors in Web Dashboards

**Solution**: Already handled! ServerHealthEndpoint includes CORS headers:
```csharp
response.AddHeader("Access-Control-Allow-Origin", "*");
response.AddHeader("Access-Control-Allow-Methods", "GET, OPTIONS");
```

---

## Production Considerations

### Security
- Health endpoint is **read-only** (only GET requests allowed)
- No sensitive data exposed (only public server stats)
- Rate limiting recommended for production (add to ServerHealthEndpoint if needed)

### Monitoring Integration
Health endpoint is compatible with:
- ✅ Prometheus (scrape /health)
- ✅ Kubernetes health checks
- ✅ Load balancer health probes (AWS ELB, GCP LB, etc.)
- ✅ Custom monitoring dashboards
- ✅ Uptime monitoring services (UptimeRobot, Pingdom, etc.)

### Performance
- Minimal overhead: ~1ms per health check request
- Runs on separate thread (no game loop impact)
- No database or file I/O
- Handles concurrent requests via ThreadPool

---

## API Reference

### Endpoint: GET /health

**URL**: `http://SERVER_IP:8080/health`

**Response** (200 OK):
```json
{
    "status": "ok",
    "server": "running",
    "players": 12,
    "maxPlayers": 300,
    "uptime": 3661,
    "timestamp": 1734567890
}
```

**Response Codes**:
- `200 OK` - Server healthy and responding
- `405 Method Not Allowed` - Only GET requests accepted
- `500 Internal Server Error` - Server error (should not happen)

### Endpoint: GET /info

**URL**: `http://SERVER_IP:8080/info`

**Response** (200 OK):
```json
{
    "status": "Running",
    "players": {
        "current": 12,
        "max": 300
    },
    "uptime": 3661,
    "scene": "Main",
    "version": "1.0.0",
    "platform": "Linux Server",
    "timestamp": 1734567890
}
```

---

## Console Log Reference

### Server Logs (Success)
```
[HealthEndpoint] 🏥 Starting HTTP health endpoint on port 8080...
[HealthEndpoint] ✅ Health endpoint started successfully
[HealthEndpoint] 🌐 Accessible at: http://SERVER_IP:8080/health
[HealthEndpoint] 👂 Listener thread started
[HealthEndpoint] 📥 GET /health from 1.2.3.4:5678
[HealthEndpoint] ✅ Health check response sent: 0 players, 42s uptime
```

### Client Logs (Success)
```
[JoinMenu] ========== SERVER CHECK START ==========
[JoinMenu] Performing HTTP health check to 172.234.217.194:8080/health...
[JoinMenu] Health check URL: http://172.234.217.194:8080/health
[JoinMenu] ========== SERVER CHECK END ==========
[JoinMenu] ✅ Health check SUCCESS - Response: {"status":"ok",...}
[JoinMenu] 👥 Server has 0 players
[JoinMenu] UpdateServerStatus called with: Up
```

### Client Logs (Failure)
```
[JoinMenu] ========== SERVER CHECK START ==========
[JoinMenu] Performing HTTP health check to 172.234.217.194:8080/health...
[JoinMenu] ========== SERVER CHECK END ==========
[JoinMenu] ❌ Health check FAILED - Error: Cannot connect to destination host
[JoinMenu] Result: ConnectionError
[JoinMenu] Response Code: 0
[JoinMenu] UpdateServerStatus called with: Down
```

---

## Summary

**Server Setup**:
1. Add ServerHealthEndpoint component to NetworkManager
2. Configure health port (8080 default)
3. Redeploy to Edgegap with HTTP port exposed

**Edgegap Setup**:
1. Add port mapping: healthport (8080/TCP)
2. Redeploy server
3. Verify health endpoint responds

**Client Setup**:
1. JoinMenuController auto-configured (no changes needed)
2. Verify healthCheckPort = 8080
3. Test server status shows correct Up/Down state

**Result**: ✅ Accurate server status, ✅ Real-time player count, ✅ Professional monitoring!
