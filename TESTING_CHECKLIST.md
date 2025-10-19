# WOS2.3 Testing Checklist - Multiplayer Networking

**Quick Reference**: 5 Unity Editor tasks required before testing | Code fixes already complete ✅

**Status**: Input System fixed | Ship movement fixed | ShipDebugUI fixed | Spawn point auto-discovery added

---

## ✅ Code Fixes Already Complete

All code fixes are complete - no changes needed to scripts:

1. ✅ **Ship Movement Direction**: Fixed transform.up → transform.right (NetworkedNavalController.cs:563)
2. ✅ **Input System**: Direct InputActionAsset loading (no PlayerInput component)
3. ✅ **Spawn Point Auto-Discovery**: WOSNetworkManager finds spawn points at runtime
4. ✅ **ShipDebugUI Multiplayer Support**: Supports both controller types + retry logic for race condition

**See Documentation**:
- ISSUE_DIAGNOSIS.md - Ship movement fix details
- SPAWN_POINTS_SETUP.md - Automatic spawn point discovery
- SHIPDEBUGUI_FIX.md - Complete debug UI fixes (4 changes)
- SCENE_ARCHITECTURE_FIX.md - NetworkManager architecture

---

## ⚠️ CRITICAL: 5 Unity Editor Tasks Required Before Testing

### Fix 1: Remove Duplicate NetworkManager (Main Scene)

**Problem**: "Multiple NetworkManagers detected in the scene" warning

**Steps**:
1. Open **Main.unity** scene
2. Find "NetworkManager" GameObject in Hierarchy
3. **Delete it** (press Delete key)
4. Save scene (Ctrl+S)

**Why**: NetworkManager in MainMenu has DontDestroyOnLoad enabled, so it persists when loading Main scene.

**Validation**: Main scene should have NO NetworkManager GameObject

---

### Fix 2: Create SpawnPoint Tag and Player Layers

**Problem**: Missing tag for spawn point auto-discovery + layer errors

**Steps**:
1. **Edit → Project Settings → Tags and Layers**

2. **Create SpawnPoint Tag**:
   - Under "Tags" section, click "+" button
   - Type: `SpawnPoint` (exact spelling, capital S and P, no space!)

3. **Create Layers**:
   - Expand "Layers" section
   - **Layer 6**: type `Player`
   - **Layer 7**: type `RemotePlayer`

4. Close Project Settings

**Why**:
- SpawnPoint tag: WOSNetworkManager auto-discovers spawn points by tag when scene loads
- Player layers: NetworkedNavalController sets layer based on ownership

**Validation**:
- Tag "SpawnPoint" exists
- Layer 6 = "Player"
- Layer 7 = "RemotePlayer"

---

### Fix 3: Setup Spawn Points with Auto-Discovery (Main Scene)

**Problem**: Player spawns at origin (0, 0, 0)

**NEW SYSTEM**: Spawn points are automatically discovered at runtime - NO Inspector assignment needed!

**Steps**:
1. Open **Main.unity** scene

2. **Create 5 spawn point GameObjects**:
   - Right-click Hierarchy → Create Empty
   - Name: `SpawnPoint_01`
   - Tag: **SpawnPoint** ← CRITICAL!
   - Position: `(-50, 0, 0)`

   Repeat for:
   - `SpawnPoint_02` → Tag: SpawnPoint → Position: `(50, 0, 0)`
   - `SpawnPoint_03` → Tag: SpawnPoint → Position: `(0, 50, 0)`
   - `SpawnPoint_04` → Tag: SpawnPoint → Position: `(0, -50, 0)`
   - `SpawnPoint_05` → Tag: SpawnPoint → Position: `(0, 0, 0)`

3. **Save Main scene** (Ctrl+S)

4. **DO NOT try to assign spawn points to NetworkManager!**
   - Unity doesn't allow cross-scene references
   - WOSNetworkManager finds them automatically when Main scene loads

**Why**:
- NetworkManager is in MainMenu scene, spawn points are in Main scene
- Unity doesn't allow cross-scene GameObject references
- WOSNetworkManager.OnServerSceneChanged() auto-discovers spawn points by tag

**Validation**:
- Main scene has 5 GameObjects named SpawnPoint_01 through SpawnPoint_05
- All have Tag = "SpawnPoint"
- Positions are spread out
- Console shows "🎯 Found 5 spawn points in scene" when hosting

**See**: SPAWN_POINTS_SETUP.md for detailed guide

---

### Fix 4: Rename Managers GameObject (Main Scene)

**Problem**: Main scene's managers disappear when scene loads

**Steps**:
1. Open **Main.unity** scene
2. Find "Managers" GameObject in Hierarchy
3. Select it and press F2 (or right-click → Rename)
4. Rename to: `MainSceneManagers`
5. Save scene (Ctrl+S)

**Why**:
- MainMenu scene has "Managers" GameObject with DontDestroyOnLoad
- When Main scene loads, persisted "Managers" overwrites Main's "Managers"
- This destroys all managers in Main scene (OceanChunkManager, etc.)
- Different names prevent collision

**Validation**:
- Main scene has "MainSceneManagers" GameObject
- MainMenu scene still has "Managers" GameObject (don't rename that one!)

**See**: SCENE_ARCHITECTURE_FIX.md for details

---

### Fix 5: Verify Main Scene Has NO NetworkManager

**Problem**: "Multiple NetworkManagers detected" warning

**Steps**:
1. Open **Main.unity** scene
2. Search Hierarchy for "NetworkManager" or "WOSNetworkManager"
3. **If found → DELETE IT** (press Delete key)
4. Save scene (Ctrl+S)

**Why**:
- NetworkManager in MainMenu has DontDestroyOnLoad
- It persists when transitioning to Main scene
- Having one in Main scene creates duplicates

**Validation**:
- Main scene has NO NetworkManager GameObject
- MainMenu scene still has NetworkManager (don't delete that one!)
- Console shows NO "Multiple NetworkManagers" warning when hosting

**See**: SCENE_ARCHITECTURE_FIX.md for architecture details

---

## ✅ Already Fixed

- ✅ **Input System**: NetworkedNavalController fixed (no PlayerInput component, direct InputActionAsset)
- ✅ **5-Panel Menu**: MenuManager + ConnectionMenuController + HostMenuController + JoinMenuController + OptionsMenuController
- ✅ **Menu Navigation**: All panels show/hide correctly via MenuManager singleton

---

## 🧪 Test 1: First Host Test (After All 5 Fixes)

**After completing all 5 Unity Editor tasks above**:

1. Press **Play** in Unity Editor
2. Navigate: Main Menu → Start → Connection → Host
3. Click **Start Host**

**Expected Console Output**:
```
🏝️ Server changing scene to: Main
✅ Server scene loaded: Main
🎯 Found 5 spawn points in scene
[UI] Waiting for player ship to spawn...
✅ Spawned player ship for connection 0 at (-50.00, 0.00, 0.00)
[UI] Found NetworkedNavalController (local player)
[UI] ✅ ShipDebugUI initialization complete - displaying telemetry
```

**Expected Behavior**:
- ✅ Main scene loads
- ✅ NO "Multiple NetworkManagers detected" warning
- ✅ NO "layer needs to be in range" error
- ✅ NO "MissingMethodException" error
- ✅ Player spawns at spawn point (NOT origin)
- ✅ Ship moves in direction it's facing (NOT sliding up)
- ✅ Ship responds to WASD input
- ✅ **Debug UI appears in top-right** within ~1 second showing ship telemetry
- ✅ **Debug UI updates in real-time** as ship moves

**Expected Debug UI Display**:
```
=== SHIP TELEMETRY ===
VESSEL: [Your Ship Name]
CLASS: [Ship Class]

PROPULSION
Current Speed: 0.0 kts
Target Speed: 0.0 kts
Throttle: Full Stop (0)

NAVIGATION
Bearing: 0.0°
Rate of Turn: 0.0°/s
Rudder Angle: 0.0°
Mode: MANUAL

OCEAN
Depth: [depth]m
Tile: [tile type]
Zone: [zone]
```

**Debug UI Controls**:
- Press **F3** to toggle visibility on/off

---

## 🎮 Input Testing

| Input | Expected Result |
|-------|----------------|
| **W** | Throttle +1 (accelerate) |
| **S** | Throttle -1 (decelerate) |
| **A** | Turn left |
| **D** | Turn right |
| **Space** | Emergency stop |
| **Right-click** | Place waypoint |
| **Z** | Toggle autopilot |
| **X** | Clear waypoints |

---

## 🧪 Test 1: Script Compilation & Inspector

### Test 1.1: Scripts Compile
- [ ] No compile errors in Console
- [ ] No warnings about missing namespaces
- [ ] All custom scripts visible in Add Component menu

### Test 1.2: Inspector Configuration
- [ ] MainMenuController shows in Inspector
- [ ] All public fields visible and assignable
- [ ] No "Missing Script" warnings
- [ ] Tooltips display when hovering over fields

### Test 1.3: Button Manager Integration
- [ ] Modern UI Pack buttons display correctly
- [ ] Button text shows properly
- [ ] OnClick events show in Inspector
- [ ] Can assign MainMenuController functions

**Expected Result**: ✅ All scripts compile, Inspector shows all components

---

## 🖥️ Test 2: Main Menu UI

### Test 2.1: Scene Load
1. [ ] Open MainMenu scene
2. [ ] Press Play button
3. [ ] Main menu displays correctly
4. [ ] No errors in Console

### Test 2.2: UI Elements Display
- [ ] Title text visible: "WOS 2.3 - World of Ships"
- [ ] Version text visible: "v0.3.0-alpha"
- [ ] Server IP input field visible
- [ ] Connect button visible
- [ ] Settings button visible
- [ ] Exit button visible
- [ ] Status text visible: "Ready to connect"

### Test 2.3: Input Field
1. [ ] Click server IP input field
2. [ ] Can type text
3. [ ] Default shows "127.0.0.1" or last used IP
4. [ ] Can clear and enter new IP
5. [ ] Can paste IP address

### Test 2.4: Button Interactions
1. [ ] Hover over Connect button → visual feedback
2. [ ] Click Connect button → status text updates
3. [ ] Click Settings button → status shows "coming soon"
4. [ ] Click Exit button → game exits (or Editor stops)

**Expected Result**: ✅ UI fully functional, no errors

---

## 🔌 Test 3: Local Host Mode

### Test 3.1: Start Local Server
1. [ ] Play MainMenu scene
2. [ ] Leave IP as "127.0.0.1"
3. [ ] Click "Connect to Server" (or "Host Local Server" if added)
4. [ ] Console shows: "🌊 WOS Server started!"
5. [ ] Scene transitions to PortHarbor
6. [ ] Player ship spawns at spawn point

### Test 3.2: Player Ship Spawn
- [ ] Ship appears in PortHarbor scene
- [ ] Ship positioned at PortSpawn_01 location
- [ ] Ship facing correct direction
- [ ] Camera follows ship (if camera controller active)
- [ ] No "Failed to spawn player" errors

### Test 3.3: Ship Controls
- [ ] W key increases throttle
- [ ] S key decreases throttle
- [ ] A/D keys turn ship
- [ ] Space key emergency stop works
- [ ] Ship moves smoothly
- [ ] Ship rotation smooth

### Test 3.4: Network Manager Status
- [ ] Console shows: "Local player ship initialized"
- [ ] NetworkManager active
- [ ] NetworkServer.active = true
- [ ] 1 connection shown in Network Manager HUD

**Expected Result**: ✅ Host mode works, ship spawns and moves

---

## 🌐 Test 4: Dedicated Server + Client

### Test 4.1: Build Server
1. [ ] File → Build Settings
2. [ ] Scenes in correct order (MainMenu, PortHarbor, Main)
3. [ ] Build (not Build and Run)
4. [ ] Save as "WOS2.3_Server.exe" in Builds/Server/
5. [ ] Build completes without errors

### Test 4.2: Launch Server
1. [ ] Run WOS2.3_Server.exe
2. [ ] Console window appears
3. [ ] Shows: "🌊 Starting WOS2.3 Dedicated Server..."
4. [ ] Shows: "✅ Server started successfully!"
5. [ ] Shows: Server IP and port information
6. [ ] No crash or errors

### Test 4.3: Connect Client from Editor
1. [ ] Unity Editor → Play MainMenu
2. [ ] Enter "127.0.0.1" in IP field
3. [ ] Click "Connect to Server"
4. [ ] Status shows: "🔌 Connecting to 127.0.0.1..."
5. [ ] Scene transitions to PortHarbor
6. [ ] Player ship spawns

### Test 4.4: Verify Connection
- [ ] Server console shows: "Player connected"
- [ ] Client shows: "Connected to WOS server!"
- [ ] Ship spawns at spawn point
- [ ] Ship controls work
- [ ] No disconnection errors

**Expected Result**: ✅ Dedicated server works, client connects successfully

---

## 👥 Test 5: Multi-Client (ParrelSync)

### Test 5.1: Create Clone
1. [ ] Window → ParrelSync → Clones Manager
2. [ ] Click "Create new clone"
3. [ ] Wait for clone creation
4. [ ] Clone appears in list

### Test 5.2: Launch Server
- [ ] Run WOS2.3_Server.exe (from Test 4)
- [ ] Verify server running

### Test 5.3: Connect Client 1 (Main Editor)
1. [ ] Play MainMenu in main Unity Editor
2. [ ] Enter "127.0.0.1"
3. [ ] Click "Connect to Server"
4. [ ] Ship spawns in PortHarbor
5. [ ] Note ship position

### Test 5.4: Connect Client 2 (Clone)
1. [ ] Open ParrelSync clone project
2. [ ] Play MainMenu in clone
3. [ ] Enter "127.0.0.1"
4. [ ] Click "Connect to Server"
5. [ ] Second ship spawns

### Test 5.5: Verify Multi-Player
- [ ] Both clients see 2 ships total
- [ ] Moving Client 1 ship → Client 2 sees movement
- [ ] Moving Client 2 ship → Client 1 sees movement
- [ ] Ships don't overlap at spawn
- [ ] Network position sync smooth (< 100ms lag)
- [ ] No "Lost connection" errors

### Test 5.6: Ship Interaction
1. [ ] Client 1 moves ship forward
2. [ ] Client 2 observes smooth movement
3. [ ] Client 2 moves ship
4. [ ] Client 1 observes smooth movement
5. [ ] Both ships can move independently

**Expected Result**: ✅ Multi-client works, ships sync correctly

---

## ☁️ Test 6: Edgegap Cloud Deployment (Optional)

### Test 6.1: Prerequisites
- [ ] Edgegap account created
- [ ] Docker Desktop installed and running
- [ ] Linux Build Support installed in Unity Hub
- [ ] Edgegap plugin configured (if available)

### Test 6.2: Deploy from Editor
1. [ ] Play MainMenu scene
2. [ ] Click "Quick Deploy to Cloud" (if button exists)
3. [ ] Status shows: "☁️ Deploying to Edgegap cloud..."
4. [ ] Wait 2-5 minutes
5. [ ] Status shows: "✅ Server deployed! IP: [server_ip]"
6. [ ] Server IP auto-fills input field

### Test 6.3: Connect to Cloud Server
1. [ ] Server IP should be auto-filled
2. [ ] Click "Connect to Server"
3. [ ] Status: "🔌 Connecting to [edgegap_ip]..."
4. [ ] Scene loads PortHarbor
5. [ ] Ship spawns

### Test 6.4: Multi-Client Cloud Test
1. [ ] One client connected to cloud server
2. [ ] Open second client (ParrelSync or separate PC)
3. [ ] Enter cloud server IP
4. [ ] Connect
5. [ ] Verify both players see each other

**Expected Result**: ✅ Cloud deployment works, public IP accessible

---

## 🔧 Test 7: Server Build Arguments

### Test 7.1: Command-Line Server Start
1. [ ] Open Command Prompt
2. [ ] Navigate to server build folder
3. [ ] Run: `WOS2.3_Server.exe -server -port 7777`
4. [ ] Server starts automatically
5. [ ] Uses port 7777

### Test 7.2: Custom Port
1. [ ] Run: `WOS2.3_Server.exe -server -port 8888`
2. [ ] Server starts on port 8888
3. [ ] Client can connect on port 8888
4. [ ] Verify in server console

### Test 7.3: Max Players Argument
1. [ ] Run: `WOS2.3_Server.exe -server -maxplayers 50`
2. [ ] Server limits to 50 connections
3. [ ] Verify in console: "Max connections set to 50"

**Expected Result**: ✅ Command-line args work correctly

---

## 📊 Test 8: NetworkAddressManager

### Test 8.1: Save Last IP
1. [ ] Play MainMenu
2. [ ] Enter "192.168.1.100" in IP field
3. [ ] Click Connect (even if fails)
4. [ ] Stop Play mode
5. [ ] Play again
6. [ ] IP field shows "192.168.1.100"

### Test 8.2: Recent Servers List
1. [ ] Connect to 3 different IPs
2. [ ] Stop and restart
3. [ ] Check NetworkAddressManager → Recent Servers
4. [ ] Should show 3 entries

### Test 8.3: Clear Saved Data
1. [ ] Select NetworkAddressManager
2. [ ] Right-click → "Clear All Servers (Debug)"
3. [ ] Restart
4. [ ] IP field shows default: "127.0.0.1"

**Expected Result**: ✅ IP persistence works correctly

---

## ⚠️ Error Testing

### Test 9.1: Invalid IP Handling
1. [ ] Enter "999.999.999.999"
2. [ ] Click Connect
3. [ ] Status shows: "❌ Invalid IP address"
4. [ ] No crash or freeze

### Test 9.2: Server Not Running
1. [ ] Ensure no server running
2. [ ] Enter "127.0.0.1"
3. [ ] Click Connect
4. [ ] Status shows: "❌ Connection failed" or timeout
5. [ ] Can retry connection

### Test 9.3: Disconnect During Gameplay
1. [ ] Connect to server
2. [ ] Close server.exe forcefully
3. [ ] Client shows disconnect message
4. [ ] Can return to main menu gracefully

### Test 9.4: Network Manager Missing
1. [ ] Remove NetworkManager from PortHarbor scene
2. [ ] Try to connect
3. [ ] Shows warning: "NetworkManager not found"
4. [ ] Doesn't crash Unity

**Expected Result**: ✅ Errors handled gracefully, no crashes

---

## 📋 Performance Testing

### Test 10.1: Connection Time
- [ ] Connection time < 3 seconds for localhost
- [ ] Connection time < 5 seconds for LAN
- [ ] Connection time < 10 seconds for Edgegap

### Test 10.2: Network Performance
- [ ] Ship movement latency < 100ms
- [ ] Position sync updates 10 times/second (100ms intervals)
- [ ] No rubber-banding with good connection
- [ ] CPU usage < 30% on server

### Test 10.3: Memory Usage
- [ ] Server RAM usage < 500MB with 10 players
- [ ] Client RAM usage < 800MB
- [ ] No memory leaks over 30 minutes

**Expected Result**: ✅ Performance within acceptable ranges

---

## ✅ Final Acceptance Criteria

### Must Have (Critical)
- [x] Scripts compile without errors
- [ ] Main Menu displays and is functional
- [ ] Can host local server
- [ ] Can connect to dedicated server
- [ ] Player ship spawns correctly
- [ ] Ship controls work
- [ ] Multi-client works with 2+ players

### Should Have (Important)
- [ ] Server IP persistence works
- [ ] Button visual feedback works
- [ ] Error messages display correctly
- [ ] Command-line arguments functional
- [ ] Graceful disconnect handling

### Could Have (Nice to Have)
- [ ] Edgegap deployment works
- [ ] Settings menu implemented
- [ ] Recent servers list working
- [ ] Predefined servers feature

---

## 🐛 Known Issues Log

**Template for logging issues**:
```
Issue #1: [Brief description]
- Priority: [Critical/High/Medium/Low]
- Repro Steps: [How to reproduce]
- Expected: [What should happen]
- Actual: [What actually happens]
- Workaround: [Temporary fix if available]
- Status: [Open/In Progress/Fixed]
```

---

## 📝 Testing Notes

**Date**: _________________
**Tester**: _________________
**Unity Version**: 6000.0.55f1
**Mirror Version**: _________________

**Overall Status**:
- [ ] All critical tests passed
- [ ] Ready for next phase (Port → Ocean transition)
- [ ] Known issues documented
- [ ] Performance acceptable

**Additional Notes**:
_________________________________________________
_________________________________________________
_________________________________________________

---

**Last Updated**: January 2025
