# ShipDebugUI Fix - Networked Gameplay Support

**Issue**: ShipDebugUI stopped showing ship information after switching to multiplayer.

**Root Causes**:
1. ShipDebugUI was designed for `SimpleNavalController` (single-player), but multiplayer uses `NetworkedNavalController`.
2. Race condition: ShipDebugUI.Start() ran before player ship spawned, causing it to disable itself prematurely.

---

## 🔍 Root Cause Analysis

### The Problem

**ShipDebugUI.cs was looking for the wrong controller type**:

```csharp
// OLD CODE (line 54):
[SerializeField] private SimpleNavalController shipController;

// OLD CODE (line 82):
shipController = FindFirstObjectByType<SimpleNavalController>();
```

**Why this broke**:
1. Single-player testing uses `SimpleNavalController`
2. Multiplayer uses `NetworkedNavalController`
3. ShipDebugUI only looked for `SimpleNavalController`
4. **Result**: No ship controller found → Debug UI disabled → No data displayed

### Missing Methods

**NetworkedNavalController was missing UI support methods**:
- `GetShipStatus()` - Returns ship telemetry for UI
- `GetShipConfiguration()` - Returns ship config for specs display

These existed in `SimpleNavalController` but not in `NetworkedNavalController`.

### Race Condition Problem

**Timing Issue**:
1. Main scene loads
2. ShipDebugUI.Start() runs immediately
3. Looks for player ship... **NOT FOUND** (hasn't spawned yet)
4. Script disables itself
5. Player ship spawns later via NetworkManager
6. **Too late** - ShipDebugUI already disabled!

**Why this happened**:
- NetworkManager spawns player AFTER `OnClientConnect()` callback
- Scene's Start() methods run BEFORE player spawning
- ShipDebugUI gave up immediately instead of waiting

---

## ✅ The Fix (4 Changes)

### Fix 1: Added Missing Methods to NetworkedNavalController

**File**: `NetworkedNavalController.cs`

**Added** (lines 627-650):
```csharp
/// <summary>
/// Get current ship status for UI display (ShipDebugUI compatibility)
/// </summary>
public ShipStatus GetShipStatus()
{
    return new ShipStatus
    {
        speed = currentSpeed,
        throttle = currentThrottle,
        heading = transform.eulerAngles.z,
        rudderAngle = rudderAngle,
        isAutoNavigating = autoNavigationEnabled,
        waypointCount = waypoints != null ? waypoints.Count : 0,
        currentWaypoint = currentWaypointIndex
    };
}

/// <summary>
/// Get the current ship configuration (ShipDebugUI compatibility)
/// </summary>
public ShipConfigurationSO GetShipConfiguration()
{
    return shipConfig;
}
```

---

### Fix 2: Updated ShipDebugUI to Support Both Controller Types

**File**: `ShipDebugUI.cs`

**Changed field type** (line 54):
```csharp
// BEFORE:
[SerializeField] private SimpleNavalController shipController;

// AFTER:
[SerializeField] private MonoBehaviour shipController; // Can be either type
```

**Updated controller discovery** (lines 79-116):
```csharp
// Auto-find ship controller if not assigned
if (shipController == null)
{
    // First try to find NetworkedNavalController (for multiplayer)
    var networkedController = FindFirstObjectByType<NetworkedNavalController>();
    if (networkedController != null)
    {
        // Only show debug UI for LOCAL player in networked mode
        if (networkedController.isLocalPlayer)
        {
            shipController = networkedController;
            DebugManager.Log(DebugCategory.UI, "Found NetworkedNavalController (local player)", this);
        }
        else
        {
            DebugManager.Log(DebugCategory.UI, "NetworkedNavalController found but not local player - disabling debug UI", this);
            enabled = false;
            return;
        }
    }
    else
    {
        // Fallback to SimpleNavalController (for single-player testing)
        shipController = FindFirstObjectByType<SimpleNavalController>();
        if (shipController != null)
        {
            DebugManager.Log(DebugCategory.UI, "Found SimpleNavalController", this);
        }
    }

    // If still no controller found, disable
    if (shipController == null)
    {
        DebugManager.LogWarning(DebugCategory.UI, "No ship controller found in scene!", this);
        enabled = false;
        return;
    }
}
```

**Key Features**:
- ✅ Tries `NetworkedNavalController` first (multiplayer)
- ✅ Falls back to `SimpleNavalController` (single-player)
- ✅ **Only shows debug UI for local player** (not remote players)
- ✅ Logs which controller type was found

---

### Fix 3: Added Retry Logic for Player Spawning (Race Condition Fix)

**File**: `ShipDebugUI.cs`

**Added Initialization State Tracking** (lines 72-75):
```csharp
// Initialization state
private bool isInitialized = false;
private float initializationStartTime;
private const float INITIALIZATION_TIMEOUT = 30f; // Wait up to 30 seconds for player to spawn
```

**Modified Start() to Use Retry Approach** (lines 77-87):
```csharp
private void Start()
{
    // Calculate update interval
    updateInterval = 1f / updateRate;
    lastUpdateTime = Time.time;
    lastUpdateTimeForRates = Time.time;
    initializationStartTime = Time.time;

    // Try initial setup (might not find player yet if they haven't spawned)
    TryInitialize();  // Changed from immediate failure to retry approach
}
```

**Created TryInitialize() with Timeout Logic** (lines 89-189):
```csharp
private void TryInitialize()
{
    // Auto-find ship controller if not assigned
    if (shipController == null)
    {
        // First try NetworkedNavalController, fallback to SimpleNavalController
        // ... controller discovery logic

        // If still no controller found, keep waiting (don't disable yet!)
        if (shipController == null)
        {
            // Check if we've exceeded timeout
            if (Time.time - initializationStartTime > INITIALIZATION_TIMEOUT)
            {
                DebugManager.LogWarning("No ship controller found after 30 seconds - disabling");
                enabled = false;
                return;
            }
            // Otherwise, keep waiting - will retry in Update()
            DebugManager.Log("Waiting for player ship to spawn...");
            return;  // Exit early, will retry next frame
        }
    }
    // ... rest of initialization (ocean manager, text components)
}
```

**Updated Update() with Retry Loop** (lines 191-225):
```csharp
private void Update()
{
    // If not initialized yet, keep trying to find player ship
    if (!isInitialized && shipController == null)
    {
        TryInitialize();
        if (shipController == null)
        {
            return; // Don't try to update display yet - player hasn't spawned
        }
    }

    // Mark as initialized once controller and text component are found
    if (shipController != null && activeTextComponent != null && !isInitialized)
    {
        isInitialized = true;
        DebugManager.Log("✅ ShipDebugUI initialization complete - displaying telemetry");
    }

    // Handle F3 toggle key
    if (enableToggleKey && Input.GetKeyDown(KeyCode.F3))
    {
        ToggleVisibility();
    }

    // Update at specified rate
    if (Time.time - lastUpdateTime >= updateInterval)
    {
        UpdateDisplay();
        lastUpdateTime = Time.time;
    }

    // Calculate rate of turn
    CalculateRateOfTurn();
}
```

**Key Features**:
- ✅ Keeps retrying every frame until player spawns
- ✅ 30-second timeout prevents infinite waiting
- ✅ Logs status: "Waiting for player ship to spawn..." then "✅ ShipDebugUI initialization complete"
- ✅ Only enables normal update logic after successful initialization

---

### Fix 4: Updated Display Methods to Handle Both Types

**Updated `UpdateDisplay()` method** (lines 191-231):
```csharp
private void UpdateDisplay()
{
    if (shipController == null || activeTextComponent == null) return;

    // Get ship status from either controller type
    ShipStatus shipStatus;
    ShipConfigurationSO shipConfig;

    if (shipController is NetworkedNavalController networkedController)
    {
        shipStatus = networkedController.GetShipStatus();
        shipConfig = networkedController.GetShipConfiguration();
    }
    else if (shipController is SimpleNavalController simpleController)
    {
        shipStatus = simpleController.GetShipStatus();
        shipConfig = simpleController.GetShipConfiguration();
    }
    else
    {
        DebugManager.LogError(DebugCategory.UI, $"Unknown ship controller type: {shipController.GetType().Name}", this);
        return;
    }

    // Build information string
    string infoText = BuildShipInfoText(shipStatus, shipConfig);

    // Update the active text component
    // ... rest of method
}
```

**Updated `CalculateRateOfTurn()` method** (lines 407-440):
```csharp
private void CalculateRateOfTurn()
{
    if (shipController == null) return;

    float currentTime = Time.time;
    float deltaTime = currentTime - lastUpdateTimeForRates;

    if (deltaTime >= 0.1f) // Update rate of turn every 100ms
    {
        // Get ship status from either controller type
        ShipStatus shipStatus;
        if (shipController is NetworkedNavalController networkedController)
        {
            shipStatus = networkedController.GetShipStatus();
        }
        else if (shipController is SimpleNavalController simpleController)
        {
            shipStatus = simpleController.GetShipStatus();
        }
        else
        {
            return;
        }

        float currentBearing = shipStatus.heading;
        // ... rest of method
    }
}
```

---

## 🧪 Testing The Fix

### Expected Behavior

**Single-Player Mode** (SimpleNavalController):
```
Console:
[UI] Found SimpleNavalController

Debug UI:
Shows ship telemetry ✅
```

**Multiplayer Mode** (NetworkedNavalController):
```
Console (Local Player):
[UI] Found NetworkedNavalController (local player)

Debug UI:
Shows ship telemetry ✅
```

```
Console (Remote Player):
[UI] NetworkedNavalController found but not local player - disabling debug UI

Debug UI:
Hidden (correct - we don't want to see other players' debug info) ✅
```

### Test Steps

1. **Test Single-Player**:
   - Open Main scene
   - Add SimpleNavalController to a ship
   - Press Play
   - **Expected**: Debug UI shows ship stats

2. **Test Multiplayer (Local Player)**:
   - Press Play
   - Host server
   - **Expected**: Debug UI shows YOUR ship stats
   - Console shows: "Found NetworkedNavalController (local player)"

3. **Test Multiplayer (Remote Player)**:
   - Connect second client (ParrelSync or build)
   - Second client's ship spawns
   - **Expected**: Second client sees their own debug UI, not yours
   - First client doesn't see remote player's debug UI

4. **Test F3 Toggle**:
   - Press **F3** key
   - **Expected**: Debug UI toggles on/off

---

## 📊 What The Debug UI Shows

**Now displays correctly for networked ships**:

```
=== SHIP TELEMETRY ===
VESSEL: USS Example
CLASS: Destroyer

PROPULSION
Current Speed: 15.5 kts
Target Speed: 20.0 kts
Throttle: Half Ahead (2)

NAVIGATION
Bearing: 90.0°
Rate of Turn: 2.5°/s
Rudder Angle: 10.0°
Mode: MANUAL

OCEAN
Depth: 50.5m
Tile: Deep Water
Zone: Abyssal

SPECIFICATIONS
Max Speed: 30 knots
Length: 120m
Displacement: 2500 tons
Max Rudder: ±35°
```

---

## 🔧 Technical Details

### Network Synchronization

**The debug UI reads SyncVars from NetworkedNavalController**:
- `currentSpeed` - Synced from server
- `currentThrottle` - Synced from server
- `rudderAngle` - Synced from server
- `autoNavigationEnabled` - Synced from server

**Local values**:
- `transform.eulerAngles.z` - Local transform (already synced by NetworkTransform)
- `waypoints` - Local list (not synced, but only matters for local player)

**Why this works**:
- Mirror automatically syncs SyncVars
- NetworkTransform syncs position/rotation
- Debug UI reads current synced values
- **No additional network messages needed!**

---

## 🎯 Benefits of The Fix

1. ✅ **Works with both controller types** (single-player and multiplayer)
2. ✅ **Automatic detection** (finds correct controller at runtime)
3. ✅ **Local player only** (doesn't show debug UI for remote players)
4. ✅ **No performance impact** (same update rate as before)
5. ✅ **Backward compatible** (still works with SimpleNavalController for testing)

---

## 🐛 Common Issues

### Issue: Debug UI still not showing

**Check**:
1. Is the Debug UI GameObject active in the scene?
2. Is ShipDebugUI component enabled?
3. Is MUIP InputField or TextMeshPro component assigned?
4. Check console for: "Found NetworkedNavalController (local player)"

### Issue: "No ship controller found"

**Cause**: Player ship hasn't spawned yet or doesn't have NetworkedNavalController component

**Fix**:
- Make sure player prefab has NetworkedNavalController component
- Check that hosting/connecting succeeded
- Verify player spawned (check console for "Spawned player ship")

### Issue: Debug UI showing for wrong player

**Cause**: ShipDebugUI is on a non-local player ship

**Fix**:
- Debug UI should be in the scene (NOT on player prefab)
- It automatically finds the local player's controller
- Don't attach ShipDebugUI to player prefab

### Issue: Data not updating

**Cause**: Update rate too slow or controller reference lost

**Fix**:
- Check Update Rate in ShipDebugUI Inspector (default: 10Hz)
- Verify shipController reference isn't null (check console for errors)
- Press F3 to toggle visibility (ensure it's not hidden)

---

## 📝 Summary

**What Was Broken**:
- ShipDebugUI only looked for `SimpleNavalController`
- `NetworkedNavalController` didn't have UI support methods
- Race condition: Debug UI disabled before player spawned
- Debug UI disabled in multiplayer

**What Was Fixed**:
1. Added `GetShipStatus()` and `GetShipConfiguration()` to `NetworkedNavalController`
2. Updated `ShipDebugUI` to support both controller types
3. Added retry logic with 30-second timeout for player spawning
4. Added local player check (only shows debug UI for YOUR ship)
5. Type-safe handling with proper fallback

**Result**:
- ✅ Debug UI works in single-player mode
- ✅ Debug UI works in multiplayer mode
- ✅ Only shows local player's data
- ✅ Automatic controller detection
- ✅ Waits for player to spawn (no more premature shutdown)
- ✅ No manual setup required

---

**Last Updated**: 2025-10-18
**Status**: Fix complete - ready for testing
