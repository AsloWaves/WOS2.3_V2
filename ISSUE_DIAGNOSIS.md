# Issue Diagnosis & Fixes

Three major issues identified and fixed.

---

## ✅ Issue 1: FIXED - Ship Sliding Up Instead of Forward

### What You Reported
- Ship is "facing right" but "sliding up the screen"
- Controls seem wrong after Input System changes

### Root Cause Analysis

**The Bug** (NetworkedNavalController.cs:561):
```csharp
Vector2 forwardDirection = transform.up;  // ← WRONG for right-facing ships!
```

**Why It's Wrong**:
- Your ship sprite faces RIGHT (rotation = 0° in Unity)
- `transform.up` = local Y-axis = points UP in world space at 0° rotation
- `transform.right` = local X-axis = points RIGHT in world space at 0° rotation
- **Result**: Ship always moves up regardless of orientation

**What I Changed in Input System** (NOT the cause, but revealed the bug):
- **Before**: PlayerInput component (high-level Unity wrapper)
- **After**: Direct InputActionAsset loading (more control, better for networking)

**Why The Bug Appeared Now**:
1. Before my changes: MissingMethodException prevented ANY movement (ship never moved)
2. After my fix: Input System works correctly, ship CAN move... but physics bug is revealed
3. The physics bug was ALWAYS there, you just couldn't see it because inputs weren't working

### The Fix ✅ APPLIED

**Changed Line 561**:
```csharp
// OLD (WRONG):
Vector2 forwardDirection = transform.up;

// NEW (CORRECT):
Vector2 forwardDirection = transform.right;  // For right-facing ships
```

### Verification Steps

1. **Test the fix**:
   - Press Play
   - Host server
   - Press **W** key
   - **Expected**: Ship moves RIGHT (in the direction it's facing)
   - **Before fix**: Ship would slide UP

2. **If ship still moves wrong direction**:
   - Check your ship sprite orientation in Unity
   - Is it ACTUALLY facing right at rotation = 0°?
   - If facing UP, change back to `transform.up`
   - If facing DOWN, use `-transform.up`
   - If facing LEFT, use `-transform.right`

---

## ⚠️ Issue 2: Multiple NetworkManagers Detected

### What You Reported
- "Multiple NetworkManagers detected" warning
- You deleted one but still see the warning

### Diagnosis Steps

**Step 1: Clear Console and Test Fresh**
1. Clear Unity Console (right-click → Clear)
2. Press Play
3. Check if warning appears AGAIN (old warnings linger)

**Step 2: Verify Scene Setup**

**CORRECT Setup**:
```
MainMenu.unity:  ✅ HAS NetworkManager (with DontDestroyOnLoad)
Main.unity:      ❌ NO NetworkManager (it persists from MainMenu)
PortHarbor.unity: ❌ NO NetworkManager (optional for now)
```

**How to Verify**:

**Check MainMenu Scene**:
1. Open **MainMenu.unity**
2. Hierarchy → Find "NetworkManager" GameObject
3. Select it → Inspector → WOSNetworkManager component
4. Verify "Don't Destroy On Load" is ✅ **CHECKED**
5. **Expected**: Should exist with WOSNetworkManager + TelepathyTransport

**Check Main Scene**:
1. Open **Main.unity**
2. Hierarchy → Search for "NetworkManager"
3. **Expected**: Should find NOTHING
4. If you find NetworkManager: **DELETE IT**

**Check PortHarbor Scene**:
1. Open **PortHarbor.unity**
2. Hierarchy → Search for "NetworkManager"
3. **Expected**: Should find NOTHING
4. If you find NetworkManager: **DELETE IT**

### Common Mistakes

**Mistake 1**: Deleted from WRONG scene
- ❌ Deleted from MainMenu (WRONG!)
- ✅ Should delete from Main scene only

**Mistake 2**: NetworkManager still in Prefabs
- Check **Assets/Prefabs/** folder
- If NetworkManager.prefab exists, delete it
- NetworkManager should ONLY be in MainMenu scene

**Mistake 3**: Script is instantiating NetworkManager
- Check if any script has `new GameObject("NetworkManager")`
- Check ServerLauncher.cs (shouldn't create new NetworkManager)

### Advanced Diagnostics

**If warning persists after cleanup**:

1. **Search all scenes**:
   - File → Build Settings
   - Check each scene in the list
   - Open each one and verify NO NetworkManager (except MainMenu)

2. **Check for duplicate scripts**:
   - Search Project for "WOSNetworkManager"
   - Should be exactly ONE script file
   - Delete any duplicates

3. **Unity cache issue**:
   - Close Unity
   - Delete `Library/` folder (Unity will regenerate)
   - Reopen project
   - Test again

4. **Runtime instantiation check**:
   - Add breakpoint in WOSNetworkManager.Awake()
   - Count how many times it's called
   - Should be called ONCE

### Expected Console Output After Fixes

**After clearing console and testing**:
```
[HostMenu] Starting local host...
[HostMenu] ✅ Local host started
[NetworkedNavalController] ✅ Input System initialized
[NetworkedNavalController] Player ship position: (X, Y, 0)
```

**NO warnings about**:
- ❌ "Multiple NetworkManagers detected"
- ❌ "layer needs to be in range"
- ❌ "MissingMethodException"

---

## ✅ Issue 3: FIXED - Reversed Steering & Slow Turn Rate

### What You Reported
- Pressing A turns ship RIGHT (should turn left)
- Pressing D turns ship LEFT (should turn right)
- Turn rate much slower than single-player version

### Root Cause Analysis

**Problem 1: Reversed Steering**

Unity's 2D rotation system:
- Positive angularVelocity = Counterclockwise rotation
- Negative angularVelocity = Clockwise rotation

Desired behavior:
- D key (positive rudder) = Turn right (clockwise) → Needs NEGATIVE angularVelocity
- A key (negative rudder) = Turn left (counterclockwise) → Needs POSITIVE angularVelocity

**SimpleNavalController** (CORRECT):
```csharp
float turnAmount = -currentTurnRate * Time.fixedDeltaTime;  // ← NEGATIVE SIGN!
transform.Rotate(0, 0, turnAmount);
```

**NetworkedNavalController** (BROKEN):
```csharp
shipRigidbody.angularVelocity = angularVelocity;  // ← NO NEGATIVE SIGN!
```

Missing negative sign when converting from `transform.Rotate()` to `angularVelocity`.

**Problem 2: Slow Turn Rate**

**SimpleNavalController** (CORRECT):
```csharp
float maxTurnRate = shipConfig.rudderRate; // degrees per second
float currentTurnRate = (effectiveRudderAngle / shipConfig.maxRudderAngle) * maxTurnRate;
```

**NetworkedNavalController** (BROKEN):
```csharp
float turningFactor = (shipConfig.rudderRate / shipConfig.length) * 10f;  // ← DIVIDING BY LENGTH!
```

For a 120m ship with rudderRate=30:
- SimpleNavalController: 30°/s turn rate
- NetworkedNavalController: 2.5°/s turn rate (12x slower!)

### The Fix ✅ APPLIED

**Changed Lines 549-565** (NetworkedNavalController.cs):
```csharp
// OLD (BROKEN):
float turningFactor = (shipConfig.rudderRate / shipConfig.length) * 10f;
float turnRate = turningFactor * (effectiveRudderAngle / shipConfig.maxRudderAngle);
float speedFactor = Mathf.Clamp01(Mathf.Abs(newSpeedMs) / (shipConfig.maxSpeed / 1.94384f));
angularVelocity = turnRate * speedFactor;  // ❌ NO NEGATIVE SIGN, WRONG CALC

// NEW (FIXED):
float maxTurnRate = shipConfig.rudderRate;
float currentTurnRate = (effectiveRudderAngle / shipConfig.maxRudderAngle) * maxTurnRate;
float speedFactor = Mathf.Clamp01(Mathf.Abs(newSpeedMs) / (shipConfig.maxSpeed / 1.94384f));
currentTurnRate *= speedFactor;
angularVelocity = -currentTurnRate;  // ✅ NEGATIVE SIGN ADDED, CORRECT CALC
```

### Verification Steps

1. **Test steering direction**:
   - Press Play → Host server
   - Press W (throttle up)
   - **Press D** → Ship should turn RIGHT (clockwise)
   - **Press A** → Ship should turn LEFT (counterclockwise)

2. **Test turn rate**:
   - Full speed (throttle = 4)
   - Full rudder deflection (hold A or D)
   - ShipDebugUI should show "Rate of Turn: ~30°/s" (depends on shipConfig.rudderRate)
   - Turn rate should match single-player version

**See**: STEERING_FIX.md for complete technical details

---

## 🧪 Full Test After All Three Fixes

1. **Clear Unity Console** (right-click → Clear)
2. **Press Play**
3. **Navigate**: Main Menu → Start → Connection → Host
4. **Click "Start Host"**

**Expected Results**:
- ✅ Main scene loads
- ✅ Player spawns (may still be at 0,0,0 until spawn points fixed)
- ✅ NO "Multiple NetworkManagers" warning
- ✅ NO "MissingMethodException" error
- ✅ Input System initializes successfully

5. **Press W key**

**Expected Results**:
- ✅ Ship moves RIGHT (or in the direction it's facing)
- ✅ NOT sliding up the screen
- ✅ Console shows: "[NavalController] Throttle: 1"

6. **Press A/D keys**

**Expected Results**:
- ✅ A key turns ship LEFT (counterclockwise)
- ✅ D key turns ship RIGHT (clockwise)
- ✅ Turn rate matches single-player version (~30°/s)
- ✅ Movement follows ship orientation

---

## Summary

### Fixed Issues ✅
1. ✅ Ship movement direction (changed `transform.up` → `transform.right` - NetworkedNavalController.cs:563)
2. ✅ Reversed steering controls (added negative sign to angularVelocity - NetworkedNavalController.cs:564)
3. ✅ Slow turn rate (fixed calculation to match SimpleNavalController - NetworkedNavalController.cs:549-565)
4. ⚠️ NetworkManager warning (needs manual verification in Unity)

### Remaining Fixes (from TESTING_CHECKLIST.md)
1. Create Player/RemotePlayer layers
2. Setup spawn points in Main scene
3. Assign spawn points to NetworkManager

---

**Next Steps**:
1. Test both fixes
2. Report back if warnings persist
3. Continue with remaining setup tasks

---

**Last Updated**: 2025-10-18
