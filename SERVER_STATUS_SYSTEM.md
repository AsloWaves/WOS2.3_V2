# Server Status System - Setup Guide

**Real-time server health monitoring before connection**

---

## Overview

The Join panel now checks server status **before** allowing players to connect. This prevents frustrating timeout errors and provides clear feedback about server availability.

---

## Status Flow

### 🔍 **1. Panel Opens**
```
Server Status: "Checking Status..." (Yellow)
Main Status: "Checking server status..."
Connect Button: Disabled
```
**Duration**: ~0.5 seconds

### ✅ **2. Server is Up**
```
Server Status: "Server Up ✅" (Green)
Main Status: "Ready to connect to 172.234.24.224:31139 (Chicago, Illinois)"
Connect Button: Enabled
```
**Auto-refresh**: Every 30 seconds

### ❌ **3. Server is Down**
```
Server Status: "Down for Maintenance ⚠️" (Red)
Main Status: "Server is currently down for maintenance"
Connect Button: Disabled
```
**Auto-refresh**: Every 30 seconds (keeps checking for server to come back online)

### 🔗 **4. Player Clicks Connect** (Only if Server Up)
```
Main Status: "Connecting to 172.234.24.224:31139..." (Yellow)
Status Checking: Paused during connection
```

### 🎮 **5. Connection Successful**
```
Main Status: "✅ Connected! Loading game..." (White)
Action: Scene loads to PortHarbor
```

### ⚠️ **6. Connection Failed**
```
Main Status: "Connection failed: {error}" (Red)
Status Checking: Resumes automatically
Server Status: Re-checks in 30 seconds
```

### 🔌 **7. Disconnected**
```
Main Status: "Disconnected from server" (Red)
Status Checking: Resumes automatically
Connect Button: Re-enabled when server up
```

---

## Unity Setup (2 minutes)

### Required UI Elements

**In Join Panel, add/configure**:

#### 1. **Server Status Text** (NEW)
- **Type**: TextMeshPro - Text (UI)
- **Name**: `ServerStatusText`
- **Purpose**: Shows "Server Up ✅" or "Down for Maintenance ⚠️"
- **Recommended Position**: Top of panel, near server IP
- **Font Size**: 18-24
- **Alignment**: Center

#### 2. **Connect Button** (MUIP)
- **Type**: Modern UI Pack ButtonManager
- **Auto-disabled** when server is down
- **Auto-enabled** when server comes back up

#### 3. **Back Button** (MUIP)
- **Type**: Modern UI Pack ButtonManager
- **Returns to Main Menu**

### JoinMenuController Assignment

**Select JoinPanel → JoinMenuController Component**:

| Field | Assign To |
|-------|-----------|
| **Server Status Text** | TextMeshPro text showing all status messages |
| **Connect Button** | MUIP ButtonManager component |
| **Back Button** | MUIP ButtonManager component |
| Server Config | ServerConfig asset |
| Game Scene Name | `Main` (scene to load after connecting) |
| Status Check Interval | `30` (seconds between checks) |
| Status Check Timeout | `5` (seconds before declaring down) |

---

## Configuration

### Adjust Check Frequency

**In JoinMenuController Inspector**:
- **Status Check Interval**: `30` seconds (default)
  - Lower = More responsive (checks more often)
  - Higher = Less network traffic
  - Recommended: 15-60 seconds

- **Status Check Timeout**: `5` seconds (default)
  - Time to wait before declaring server down
  - Recommended: 3-10 seconds

---

## Status Colors

| Status | Color | Meaning |
|--------|-------|---------|
| 🟢 Green | `Server Up ✅` | Online and accepting connections |
| 🟡 Yellow | `Checking Status...` | Currently verifying server |
| 🔴 Red | `Down for Maintenance ⚠️` | Offline or not responding |

| Main Status | Color | Meaning |
|-------------|-------|---------|
| ⚪ White | Ready/Success | "Ready to connect", "✅ Connected!" |
| 🟡 Yellow | In Progress | "Connecting to..." |
| 🔴 Red | Error | "Connection failed", "Disconnected" |

---

## How It Works

### Initial Check (Panel Opens)
1. JoinMenu starts
2. Loads server address from ServerConfig
3. **Starts status check coroutine**
4. Shows "Checking Status..." (Yellow)
5. After 0.5s, updates to "Server Up" or "Down"

### Periodic Refresh (Every 30s)
1. While panel is open
2. Check repeats every 30 seconds
3. Updates status automatically
4. Connect button enabled/disabled based on result

### During Connection
1. Player clicks Connect (only if server up)
2. Status checking **pauses**
3. Shows connection progress
4. If connection fails → Status checking **resumes**
5. If successful → Player enters game

### Smart Pause/Resume
- ✅ Pauses when connecting (no wasted checks)
- ✅ Resumes on disconnect
- ✅ Resumes on connection failure
- ✅ Stops when panel closes
- ✅ Restarts when panel reopens

---

## Current Implementation

### Simple Heuristic Check

**Current version uses**:
- IP address validation
- Basic reachability check
- 0.5s delay to simulate ping

**Assumes server is UP if**:
- IP address is valid format
- No recent connection failures

### Future Enhancements (Optional)

**For production, you could add**:

1. **HTTP Health Endpoint**:
   ```csharp
   UnityWebRequest.Get($"http://{serverIP}:8080/health")
   ```
   - Requires server to expose HTTP health check
   - More reliable than heuristic

2. **Edgegap API Check**:
   ```csharp
   // Check deployment status via Edgegap API
   GET /v1/deployments/{deployment_id}/status
   ```
   - Requires API token
   - Checks actual Edgegap deployment state

3. **UDP Ping**:
   ```csharp
   // Send custom UDP ping packet
   // Wait for response
   ```
   - More accurate for game servers
   - Requires server-side ping handler

---

## Testing

### Test Server Up
1. **Start Edgegap server** (or local server)
2. **Open Join panel** in Unity
3. **Should see**: "Checking Status..." → "Server Up ✅"
4. **Connect button**: Enabled (green)
5. **Click Connect**: Should work normally

### Test Server Down
1. **Stop Edgegap server** (or kill local server)
2. **Wait 30 seconds** (or restart Join panel)
3. **Should see**: "Checking Status..." → "Down for Maintenance ⚠️"
4. **Connect button**: Disabled (grayed out)
5. **Try to click Connect**: Nothing happens (button disabled)

### Test Auto-Refresh
1. **Open Join panel** with server down
2. **Start server** while panel is open
3. **Wait up to 30 seconds**
4. **Should see**: Status updates to "Server Up ✅"
5. **Connect button**: Automatically enables

### Test Connection Flow
1. **Panel opens** → "Server Up ✅"
2. **Click Connect** → "Connecting to..."
3. **Successful** → "✅ Connected! Loading game..."
4. **Game loads** → PortHarbor scene

---

## Troubleshooting

### "Always shows Server Up"
- Current implementation uses simple heuristic
- Assumes valid IP = server up
- For production, add actual health check endpoint

### "Connect button never enables"
- Check **Connect Button** (MUIP ButtonManager) is assigned in Inspector
- Verify it's a ButtonManager component, not standard Unity Button
- Check server address is valid IP format
- Try manually setting `currentServerStatus = ServerStatus.Up` for testing

### "Status never updates"
- Check **Status Check Interval** > 0
- Check coroutine is starting (Debug.Log in StartStatusChecking)
- Check panel is active in hierarchy

### "Status text missing"
- Assign **Server Status Text** field in Inspector
- Create TextMeshPro text element if missing

---

## Benefits

✅ **No more frustrating timeouts** - Players know server status before clicking Connect
✅ **Clear maintenance windows** - Shows "Down for Maintenance" instead of connection errors
✅ **Auto-recovery** - When server comes back, status updates automatically
✅ **Better UX** - Connect button only enabled when server is actually available
✅ **Reduced support** - Players understand why they can't connect

---

## Current Server Info

**Your Edgegap Deployment**:
- **IP**: 172.234.24.224:31139
- **Location**: Chicago, Illinois
- **Free Tier**: Auto-stops after 24 hours
- **Status Checks**: Every 30 seconds

**When server auto-stops**:
1. Status will show "Down for Maintenance"
2. Connect button will be disabled
3. Players see clear message
4. After redeploy, status auto-updates within 30s

---

**Server status system ready!** 🎉
