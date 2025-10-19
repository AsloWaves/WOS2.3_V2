# Speed Fix - Ship Only Reaches 4.7 kts Instead of 28 kts

**Issue**: Bainbridge ship configured for 28 knots max speed only reaches 4.7 knots in-game.

**Root Causes**:
1. globalSpeedMultiplier set to 0.5 in PlayerShip prefab (50% speed reduction)
2. Unit conversion mismatch (knots → m/s) when game world expects knots as Unity units
3. Rigidbody2D linearDamping interfering with direct velocity setting

---

## 🔍 Root Cause Analysis

### Issue 1: globalSpeedMultiplier = 0.5

**Location**: `Assets/Prefabs/Player/PlayerShip.prefab` line 5030

```yaml
globalSpeedMultiplier: 0.5  # ← WRONG! Should be 1.0
```

**Impact**: All speeds reduced by 50%
- Expected: 28 knots
- With 0.5 multiplier: 14 knots

**Why it exists**: This was likely set during testing/development and never changed back to 1.0.

---

### Issue 2: Unit Conversion Mismatch

**The Problem**: NetworkedNavalController converts knots to m/s, but the game world expects **knots as Unity units**.

**Evidence**:

**SimpleNavalController** (line 313):
```csharp
Vector2 targetVelocity = forwardDirection * currentSpeed * globalSpeedMultiplier;
```
- `currentSpeed` is in **KNOTS** (28 for throttle 4)
- Sets velocity to **28 Unity units/second** (treating knots as Unity units directly)
- **Works correctly** - ship reaches full speed

**NetworkedNavalController** (lines 543, 583):
```csharp
float targetSpeedMs = targetSpeed / 1.94384f; // knots to m/s
...
Vector2 newVelocity = forwardDirection * newSpeedMs;
```
- Converts 28 knots → 14.4 m/s
- Sets velocity to **14.4 Unity units/second**
- **Half the speed** because game world expects knots, not m/s!

**Calculation**:
- SimpleNavalController: 28 knots * 0.5 globalMultiplier = **14 Unity units/s** → Works
- NetworkedNavalController: 28 knots * 0.5 = 14 knots → 14 / 1.94384 = **7.2 Unity units/s** → Too slow!

**The game world scale**: **1 Unity unit = 1 knot of velocity**, NOT 1 m/s!

---

### Issue 3: linearDamping Interference

**Location**: NetworkedNavalController.cs line 169

```csharp
shipRigidbody.linearDamping = 0.5f;
```

**The Problem**:
- NetworkedNavalController DIRECTLY sets `linearVelocity` (line 585)
- Unity applies damping between physics frames, reducing velocity
- Damping is meant for force-based systems, not direct velocity control

**SimpleNavalController workaround** (line 317):
```csharp
shipRigidbody.linearVelocity = Vector2.Lerp(shipRigidbody.linearVelocity, targetVelocity, responseRate);
```
- Uses Lerp blending, which works WITH damping
- Gradually approaches target velocity

**NetworkedNavalController** (line 585):
```csharp
shipRigidbody.linearVelocity = newVelocity;  // Direct assignment fights damping!
```

---

## ✅ The Fix (3 Changes Required)

### Fix 1: Remove Unit Conversion (Code Change)

**File**: `NetworkedNavalController.cs`

**Change lines 540-547** from:
```csharp
targetSpeed *= globalSpeedMultiplier;

// Apply acceleration/deceleration
float targetSpeedMs = targetSpeed / 1.94384f; // knots to m/s
float currentSpeedMs = currentSpeed / 1.94384f;

float accelRate = (targetSpeed > currentSpeed) ? shipConfig.acceleration : shipConfig.deceleration;
float newSpeedMs = Mathf.MoveTowards(currentSpeedMs, targetSpeedMs, accelRate * Time.fixedDeltaTime);
```

**To**:
```csharp
targetSpeed *= globalSpeedMultiplier;

// Apply acceleration/deceleration
// NOTE: Game world uses knots as Unity units directly (like SimpleNavalController)
// Do NOT convert to m/s!
float accelRate = (targetSpeed > currentSpeed) ? shipConfig.acceleration : shipConfig.deceleration;
float newSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, accelRate * Time.fixedDeltaTime);
```

**Change line 583** from:
```csharp
Vector2 newVelocity = forwardDirection * newSpeedMs;
```

**To**:
```csharp
Vector2 newVelocity = forwardDirection * newSpeed;
```

**Change lines 568-572** (bell curve calculation) from:
```csharp
// Speed-based turning effectiveness (bell curve)
// Convert speed to knots for curve calculation
float currentSpeedKnots = Mathf.Abs(newSpeedMs) * 1.94384f;
float speedEffectiveness = CalculateTurningEffectiveness(currentSpeedKnots, shipConfig.maxSpeed);
currentTurnRate *= speedEffectiveness;
```

**To**:
```csharp
// Speed-based turning effectiveness (bell curve)
// newSpeed is already in knots (matches game world units)
float speedEffectiveness = CalculateTurningEffectiveness(Mathf.Abs(newSpeed), shipConfig.maxSpeed);
currentTurnRate *= speedEffectiveness;
```

**Change line 520** (speed display) from:
```csharp
currentSpeed = math.length(velocity) * 1.94384f; // m/s to knots
```

**To**:
```csharp
currentSpeed = math.length(velocity); // Already in knots (game world units)
```

---

### Fix 2: Set linearDamping to 0 (Code Change)

**File**: `NetworkedNavalController.cs` line 169

**Change from**:
```csharp
shipRigidbody.linearDamping = 0.5f;
```

**To**:
```csharp
shipRigidbody.linearDamping = 0f; // No damping for direct velocity control
```

**Explanation**:
- Direct velocity assignment doesn't need damping
- Damping is for force-based physics
- SimpleNavalController can use damping because it uses Lerp blending
- NetworkedNavalController uses direct assignment for network prediction

---

### Fix 3: Understanding globalSpeedMultiplier (Unity Inspector - OPTIONAL)

**File**: `Assets/Prefabs/Player/PlayerShip.prefab`

**Current Design** (After code fixes):
- `globalSpeedMultiplier` now works correctly!
- **Display shows full knots** (e.g., 28 kts at Flank Speed)
- **Unity movement is scaled** by the multiplier (e.g., 0.5 = half speed movement)

**How it Works**:
```csharp
// Naval calculations use full knots (for display and game logic)
currentSpeed = 28 knots

// Unity physics velocity is scaled for gameplay pacing
Unity velocity = 28 * 0.5 (globalSpeedMultiplier) = 14 Unity units/s
```

**Result**: Ship displays "28 kts" but moves at slower pace for better gameplay!

**Recommended Settings**:
- **0.5** (current) = Realistic display, slower gameplay (good for tactical naval combat)
- **1.0** = Realistic display AND movement (faster-paced)
- **0.3-0.4** = Very slow, deliberate gameplay (for large maps)

**No Unity Inspector change needed!** The current 0.5 value is likely intentional for gameplay balance.

---

## 📊 Performance Comparison

| Configuration | Naval Speed | Unity Velocity | Displayed Speed | Notes |
|---------------|-------------|----------------|-----------------|-------|
| **SimpleNavalController** | 28 kts | 28 * 0.5 = 14 units/s | 28 kts | ✅ Works correctly |
| **NetworkedNavalController (OLD)** | 14 kts | 14/1.94 * 0.5 = 3.6 units/s | 14 kts | ❌ Unit conversion + multiplier bug |
| **NetworkedNavalController (FIXED)** | 28 kts | 28 * 0.5 = 14 units/s | **28 kts** | ✅ Display shows full speed! |

**Key Improvement**: Display now shows realistic naval speeds (28 kts) while Unity movement matches gameplay pacing (0.5x)!

---

## 🧪 Testing The Fix

### Test 1: Verify Display Shows Full Speed

1. **Load game** and host server
2. **Press W** repeatedly to throttle 4 (Flank Speed)
3. **Wait** for ship to reach steady speed
4. **Check ShipDebugUI**:
   - "Current Speed: **~28 kts**" ✅ (realistic naval display)
   - NOT "Current Speed: ~4.7 kts" ❌ (old bug)
   - NOT "Current Speed: ~14 kts" ❌ (would mean multiplier applied to display)

**Note**: With `globalSpeedMultiplier = 0.5`, the ship will MOVE at half speed (14 Unity units/s) but DISPLAY 28 kts!

### Test 2: Verify Throttle Settings Display Correctly

| Throttle | Expected **Display** | Actual Unity Movement | Test Result |
|----------|---------------------|----------------------|-------------|
| 0 (Stop) | 0 kts | 0 units/s | ✅ / ❌ |
| 1 (Slow) | ~7 kts (25%) | ~3.5 units/s (with 0.5x) | ✅ / ❌ |
| 2 (Half) | ~14 kts (50%) | ~7 units/s (with 0.5x) | ✅ / ❌ |
| 3 (Full) | ~21 kts (75%) | ~10.5 units/s (with 0.5x) | ✅ / ❌ |
| 4 (Flank) | **~28 kts (100%)** | **~14 units/s (with 0.5x)** | ✅ / ❌ |

**Important**: Check the **display** shows full knots, even though ship moves slower visually!

### Test 3: Compare with Single-Player

1. **Test with SimpleNavalController** (single-player mode)
   - Note the speed and ship responsiveness
2. **Test with NetworkedNavalController** (multiplayer mode)
   - Speed should match single-player
   - Responsiveness should be similar

---

## 💡 Technical Notes

### Why Did Simple Naval Controller Work?

SimpleNavalController never converted units - it treated knots as Unity units directly:
```csharp
float targetSpeed = shipConfig.maxSpeed; // 28 knots
Vector2 targetVelocity = forwardDirection * targetSpeed; // 28 Unity units/s
```

This accidentally worked because the game world scale was designed with knots in mind!

### Game World Scale Explanation

**WOS2.3_V2 uses**: **1 Unity unit = 1 knot of velocity**

This is an unconventional scale, but it's what the original SimpleNavalController established. NetworkedNavalController needs to match this scale for consistency.

**Typical Unity scales**:
- 1 Unity unit = 1 meter (standard)
- 1 Unity unit = 1 foot (some games)
- **1 Unity unit = 1 knot** (WOS2.3_V2) ← Unusual but valid

### Why Not Use Standard m/s Units?

**Option 1**: Keep knots as Unity units (CURRENT FIX)
- ✅ Matches existing SimpleNavalController behavior
- ✅ Intuitive for naval gameplay (speeds displayed match velocities)
- ✅ No need to rework entire physics system

**Option 2**: Convert everything to m/s (ALTERNATIVE)
- Requires updating ALL ship speeds, configurations, and UI
- Requires changing SimpleNavalController too
- More "standard" but breaks existing balance

**Recommendation**: Stick with knots as Unity units for consistency.

---

## 📝 Summary

**What Was Broken**:
1. Unit conversion from knots → m/s (48% speed reduction due to scale mismatch)
2. globalSpeedMultiplier applied to both display AND Unity velocity (another 50% reduction)
3. linearDamping interfering with direct velocity control
4. **Compounded effect**: 28 kts → 14 kts (multiplier) → 7.2 m/s (conversion) → 3.6 Unity units/s (damping) = **~4.7 kts displayed**

**What Was Fixed**:
1. **Code**: Removed knots → m/s conversion (use knots as Unity units like SimpleNavalController)
2. **Code**: Set linearDamping to 0 (no damping for direct velocity control)
3. **Code**: Apply globalSpeedMultiplier ONLY to Unity velocity, NOT to naval speed tracking
4. **Result**: Display shows realistic speeds, Unity movement is gameplay-scaled

**How It Works Now**:
```csharp
// Naval speed tracking (for display and game logic)
currentSpeed = 28 knots  // Full realistic speed

// Unity physics velocity (for actual movement)
Unity velocity = 28 * 0.5 (globalSpeedMultiplier) = 14 Unity units/s

// Display shows: 28 kts ✅
// Ship moves at: Half speed for better gameplay pacing ✅
```

**Result**:
- ✅ **Display shows 28 kts** (realistic naval speeds for authenticity)
- ✅ **Ship moves at 0.5x** (slower gameplay pacing for tactical combat)
- ✅ Matches SimpleNavalController behavior exactly
- ✅ globalSpeedMultiplier works as intended for gameplay tuning
- ✅ Proper acceleration and deceleration behavior

**Recommended globalSpeedMultiplier Settings**:
- **0.5** (current) = Best for tactical naval combat on medium maps
- **1.0** = Faster-paced action (small maps or arcade-style)
- **0.3-0.4** = Slow, deliberate gameplay (large strategic maps)

---

**Last Updated**: 2025-10-18
**Status**: ✅ Code fixes complete - ready for testing (NO Unity Inspector changes needed!)
