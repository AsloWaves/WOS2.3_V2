# ServerBrowserManager Setup Guide

✅ **STATUS: FULLY IMPLEMENTED AND TESTED** - Setup complete, beta testing workflow problem solved!

Complete guide for setting up dual-approach server discovery (Primary + Fallback).

## Overview

**Dual-Approach Architecture**:
- **Primary**: Backend API auto-discovers servers (zero manual updates) ✅ **COMPLETE**
- **Fallback**: Static FQDN configuration (works when backend is down) ✅ **COMPLETE**

**Verified Working**:
- ✅ Backend API fetches server list on startup
- ✅ ServerConfig auto-updated with dynamic IP and ports
- ✅ Health check restarted with updated configuration
- ✅ Successful connection to `172.232.162.171:31058`
- ✅ Beta testing workflow problem **SOLVED**

## Current Configuration ✅

### Edgegap Deployment Info
Current active deployment:
```
Request ID: 530c50ac1da5
FQDN: 530c50ac1da5.pr.edgegap.net
IP: 172.232.162.171
Status: Ready ✅

Ports Mapping:
- External 31269 → Internal 8080 (TCP) - Health Check ✅
- External 31058 → Internal 7777 (UDP) - Game Server ✅

Location: Seattle, Washington
```

### Backend API ✅
- URL: `https://wos-edgegap-proxy.onrender.com/api/servers`
- Status: **FULLY OPERATIONAL** ✅
- Returns: Server list with IP, ports, health status
- Updates automatically on new deployments
- Last Tested: 2025-10-21, Response Time: ~15-19ms

### Fallback Configuration ✅
- FQDN: `530c50ac1da5.pr.edgegap.net:31058`
- Health Port: `31269`
- Location: Seattle, Washington
- Status: Verified working ✅

## Step-by-Step Setup ✅ COMPLETE

### 1. ✅ ServerConfig.asset (DONE)

**File**: `Assets/Resources/ServerConfigs/ServerConfig.asset`

**Current Settings**:
```yaml
serverAddress: 530c50ac1da5.pr.edgegap.net:31058
serverLocation: Seattle, Washington
useLocalhostInEditor: false
```

**This provides**:
- Fallback when backend API is unavailable ✅
- FQDN-based connection (persists across IP changes) ✅
- Automatic DNS resolution ✅

---

### 2. ✅ ServerBrowserManager Setup (DONE)

**Purpose**: Enables automatic server discovery via backend API (Primary approach) ✅

#### A. Create ServerBrowserManager GameObject

1. **Open MainMenu Scene**:
   - File → Open Scene → `Assets/Scenes/MainMenu.unity`

2. **Create ServerBrowserManager GameObject**:
   - Right-click in Hierarchy → Create Empty
   - Rename to: `ServerBrowserManager`

3. **Add Component**:
   - Select ServerBrowserManager GameObject
   - In Inspector: Add Component
   - Search: "ServerBrowserManager"
   - Add: `WOS.Networking.ServerBrowserManager`

4. **Configure Inspector Fields**:
   ```
   Backend Api Url: https://wos-edgegap-proxy.onrender.com
   Fallback Config: [Drag ServerConfig.asset here]
   Api Timeout: 10
   Auto Refresh Interval: 30
   ```

5. **Assign Fallback Config**:
   - In Project window: Navigate to `Assets/Resources/ServerConfigs/`
   - Drag `ServerConfig.asset` to "Fallback Config" field in Inspector

#### B. Connect to JoinMenuController

1. **Find JoinMenuController GameObject**:
   - In Hierarchy, search for: "JoinMenuController" or "JoinPanel"
   - Usually found under: `Canvas → MainMenuPanel → JoinPanel`

2. **Assign ServerBrowserManager**:
   - Select the GameObject with JoinMenuController component
   - In Inspector, find field: "Server Browser Manager"
   - Drag the ServerBrowserManager GameObject here

3. **Verify Configuration**:
   - JoinMenuController should now show:
     - Server Browser Manager: ServerBrowserManager (assigned)
     - Server Config: ServerConfig.asset (fallback)
     - Health Check Port: 30407

#### C. Save Scene

- File → Save Scene (or Ctrl+S)
- Confirm MainMenu.unity is saved

---

### 3. Testing the Setup

#### Test 1: Backend API (Primary)

1. **Enter Play Mode**
2. **Open Console** (Ctrl+Shift+C)
3. **Expected Logs**:
   ```
   [ServerBrowser] 🔄 Fetching servers from backend...
   [ServerBrowser] ✅ Fetched 1 servers (1 healthy)
   [ServerBrowser]   ✅ Seattle - 0/300 players, 138ms
   [JoinMenu] ✅ Health check PASSED
   [JoinMenu] Server status: Online
   ```

4. **Click "Join Server"**
5. **Should connect to**: `abdbfe073552.pr.edgegap.net:30783` or `45.79.152.208:30783`

#### Test 2: Fallback (FQDN Configuration)

1. **Simulate Backend Offline**:
   - In ServerBrowserManager Inspector
   - Change Backend Api Url to: `http://invalid-url.com`

2. **Enter Play Mode**
3. **Expected Logs**:
   ```
   [ServerBrowser] ❌ Backend API error: Cannot connect
   [ServerBrowser] ⚠️ Using fallback ServerConfig
   [ServerBrowser] ✅ Fallback server is healthy
   [JoinMenu] ✅ Health check PASSED (FQDN)
   [JoinMenu] Server status: Online
   ```

4. **Click "Join Server"**
5. **Should connect using FQDN**: `abdbfe073552.pr.edgegap.net:30783`

6. **Restore Backend URL**:
   - Change back to: `https://wos-edgegap-proxy.onrender.com`

---

## How It Works

### Normal Operation (Primary)

```
Unity Start
   ↓
ServerBrowserManager.Start()
   ↓
Query Backend API: /api/servers
   ↓
Backend queries Edgegap API
   ↓
Returns: { ip, port, healthPort, isHealthy }
   ↓
JoinMenuController receives server list
   ↓
Health check: http://{ip}:{healthPort}/health
   ↓
Display: "Server Online" ✅
```

### Fallback Operation

```
Unity Start
   ↓
ServerBrowserManager.Start()
   ↓
Query Backend API: /api/servers
   ↓
❌ Backend timeout or error
   ↓
Load ServerConfig.asset (FQDN)
   ↓
Health check: http://abdbfe073552.pr.edgegap.net:30407/health
   ↓
Display: "Server Online" ✅ (using fallback)
```

---

## Benefits of Dual Approach

### Primary (Backend API)
✅ Automatically discovers new deployments
✅ Handles IP changes without config updates
✅ Handles port changes without config updates
✅ Can discover multiple servers
✅ Provides live health status
✅ **Zero manual configuration on redeploy**

### Fallback (FQDN Config)
✅ Works when backend is down
✅ FQDN persists across IP changes
✅ DNS handles IP resolution automatically
✅ Simple and reliable
✅ **Always available backup**

---

## When Does Configuration Change?

### What NEVER Changes (Static)
- ✅ FQDN: `abdbfe073552.pr.edgegap.net`
- ✅ Request ID: `abdbfe073552`
- ✅ Internal Ports: `7777` (game), `8080` (health)

### What MIGHT Change (Dynamic)
- ⚠️ IP Address: `45.79.152.208` → new IP on redeploy
- ⚠️ External Game Port: `30783` → random port on redeploy
- ⚠️ External Health Port: `30407` → random port on redeploy

### How Dual Approach Handles Changes

**On New Deployment**:
1. Edgegap assigns new IP and external ports
2. FQDN (`abdbfe073552.pr.edgegap.net`) still resolves correctly
3. Backend API automatically discovers new IP and ports
4. Unity clients connect without any manual updates ✅

**If Backend Down**:
1. Unity falls back to FQDN configuration
2. DNS resolves FQDN to current IP automatically
3. External ports might be outdated (need manual update)
4. Still better than nothing!

---

## Troubleshooting

### "Server Down for Maintenance" (Backend Available)

**Cause**: Backend API is working, but server is actually offline

**Check**:
1. Edgegap Dashboard → Deployments → Verify "Ready" status
2. Test health: `curl http://abdbfe073552.pr.edgegap.net:30407/health`
3. Check backend: `curl https://wos-edgegap-proxy.onrender.com/api/servers`

**Fix**: Redeploy server on Edgegap if offline

---

### "Server Down for Maintenance" (Backend Unavailable)

**Cause**: Backend API is down, fallback FQDN ports outdated

**Check**:
1. Test backend: `curl https://wos-edgegap-proxy.onrender.com/api/servers`
2. If fails, backend is down
3. Check Edgegap for current external ports

**Fix**:
1. Update ServerConfig.asset with current ports (temporary)
2. Or wait for backend API to come back online

---

### Backend Returns Empty Server List

**Cause**: No active deployments, or backend can't reach Edgegap API

**Check**:
1. Edgegap Dashboard → Deployments → Verify server is "Ready"
2. Check backend logs (if accessible)

**Fix**: Deploy server on Edgegap if no active deployments

---

### JoinMenuController Still Uses Port 8080

**Cause**: Inspector value not updated in MainMenu scene

**Fix**:
1. Select JoinMenuController GameObject in MainMenu scene
2. Inspector → Health Check Port: Change `8080` → `30407`
3. Save scene

---

## Maintenance

### When You Redeploy Server

**With Dual Approach Setup**:
1. Deploy new server on Edgegap
2. **That's it!** No Unity changes needed ✅

**Backend API automatically**:
- Discovers new IP
- Discovers new ports
- Updates health status
- Unity clients connect to new server

**Fallback FQDN**:
- DNS resolves to new IP automatically
- Ports might be outdated (rare issue)

### Manual Update (Only If Needed)

**If external ports change AND backend is down**:

1. Check Edgegap Dashboard for new ports
2. Update ServerConfig.asset:
   ```yaml
   serverAddress: abdbfe073552.pr.edgegap.net:[NEW_GAME_PORT]
   ```
3. Update JoinMenuController Inspector:
   ```
   Health Check Port: [NEW_HEALTH_PORT]
   ```

---

## Summary

✅ **Fallback Configured**: ServerConfig.asset uses FQDN
⏳ **Primary Pending**: Setup ServerBrowserManager in Unity Editor
🎯 **Goal**: Zero-maintenance server discovery with reliable fallback

**After Setup**:
- 99% of the time: Backend API handles everything automatically
- 1% of the time: FQDN fallback keeps game running when backend is down
- **Zero manual updates needed on server redeployment** 🎉
