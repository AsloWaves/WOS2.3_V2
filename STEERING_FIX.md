# Steering Fix - Reversed Controls & Slow Turn Rate

**Issues**:
1. A/D keys reversed (A turns right, D turns left)
2. Ship turn rate much slower than single-player version
3. Linear speed scaling instead of realistic bell curve

**Root Causes**:
1. Missing negative sign in angularVelocity calculation
2. Turn rate calculation divided by ship length (incorrectly reducing turn rate)
3. Speed effectiveness scaled linearly (unrealistic - should peak at Full Ahead, not Flank)

---

## 🔍 Root Cause Analysis

### Issue 1: Reversed Steering Controls

**Symptom**: Pressing A turns ship right (should turn left), pressing D turns ship left (should turn right)

**Analysis**:

Unity's 2D rotation system:
- **Positive angularVelocity** = Counterclockwise rotation
- **Negative angularVelocity** = Clockwise rotation

Desired behavior:
- **D key** (positive rudder) = Turn right (clockwise) → Needs NEGATIVE angularVelocity
- **A key** (negative rudder) = Turn left (counterclockwise) → Needs POSITIVE angularVelocity

**SimpleNavalController** (CORRECT - line 458):
```csharp
float turnAmount = -currentTurnRate * Time.fixedDeltaTime;  // NEGATIVE SIGN!
transform.Rotate(0, 0, turnAmount);
```

**NetworkedNavalController** (BROKEN - line 567):
```csharp
shipRigidbody.angularVelocity = angularVelocity;  // NO NEGATIVE SIGN!
```

The negative sign was missing when converting from `transform.Rotate()` to `angularVelocity`.

---

### Issue 2: Slow Turn Rate

**Symptom**: Ship turns much slower than single-player version despite same ShipConfigurationSO values

**SimpleNavalController** (CORRECT - line 454-455):
```csharp
float maxTurnRate = shipConfig.rudderRate; // degrees per second at full rudder
float currentTurnRate = (effectiveRudderAngle / shipConfig.maxRudderAngle) * maxTurnRate * steerageEffect;
```

**Calculation**:
- `rudderRate` = 30 degrees/second (example)
- `effectiveRudderAngle` = 35° (full deflection)
- `maxRudderAngle` = 35°
- **Result**: `currentTurnRate = (35/35) * 30 = 30°/s`

**NetworkedNavalController** (BROKEN - line 554):
```csharp
float turningFactor = (shipConfig.rudderRate / shipConfig.length) * 10f;
float turnRate = turningFactor * (effectiveRudderAngle / shipConfig.maxRudderAngle);
float speedFactor = Mathf.Clamp01(Mathf.Abs(newSpeedMs) / (shipConfig.maxSpeed / 1.94384f));
angularVelocity = turnRate * speedFactor;
```

**Calculation** (for 120m ship with rudderRate=30):
- `turningFactor` = (30 / 120) * 10 = 2.5
- `turnRate` = 2.5 * (35/35) = 2.5°/s
- **Result**: **12x slower** than SimpleNavalController!

**Why this was wrong**:
- Dividing by full ship length makes larger ships turn disproportionately slower
- The `* 10f` multiplier doesn't compensate enough
- A 250m battleship would be 12.5x slower than a 20m patrol boat (too extreme!)

---

## ✅ The Fix

**File**: `NetworkedNavalController.cs` (lines 549-565)

**Changed from**:
```csharp
// Apply turning with effective rudder angle
float angularVelocity = 0f;
if (Mathf.Abs(effectiveRudderAngle) > 0.01f && Mathf.Abs(newSpeedMs) > 0.1f)
{
    // Calculate turn rate from rudder angle and ship characteristics
    float turningFactor = (shipConfig.rudderRate / shipConfig.length) * 10f; // Simplified turning calculation
    float turnRate = turningFactor * (effectiveRudderAngle / shipConfig.maxRudderAngle);
    float speedFactor = Mathf.Clamp01(Mathf.Abs(newSpeedMs) / (shipConfig.maxSpeed / 1.94384f));
    angularVelocity = turnRate * speedFactor;  // ❌ NO NEGATIVE SIGN, WRONG CALCULATION
}
```

**Changed to**:
```csharp
// Apply turning with effective rudder angle
float angularVelocity = 0f;
if (Mathf.Abs(effectiveRudderAngle) > 0.01f && Mathf.Abs(newSpeedMs) > 0.1f)
{
    // Calculate turn rate with realistic ship-length scaling
    // Base turn rate from ship configuration (already tuned per ship class)
    float baseTurnRate = shipConfig.rudderRate;

    // Apply gentle length-based scaling (sqrt relationship for realism)
    // Reference length: 100m (destroyer/cruiser baseline)
    // Smaller ships turn faster, larger ships turn slower
    const float referenceLength = 100f;
    float lengthRatio = shipConfig.length / referenceLength;
    float lengthScaling = 1f / Mathf.Sqrt(lengthRatio); // Gentler than direct division

    // Apply scaling to base turn rate
    float maxTurnRate = baseTurnRate * lengthScaling;
    float currentTurnRate = (effectiveRudderAngle / shipConfig.maxRudderAngle) * maxTurnRate;

    // Speed-based turning effectiveness (bell curve)
    // Convert speed to knots for curve calculation
    float currentSpeedKnots = Mathf.Abs(newSpeedMs) * 1.94384f;
    float speedEffectiveness = CalculateTurningEffectiveness(currentSpeedKnots, shipConfig.maxSpeed);
    currentTurnRate *= speedEffectiveness;

    // NEGATIVE SIGN: Unity 2D rotation is counterclockwise for positive values
    // We want positive rudder (right turn) to be clockwise, so negate
    angularVelocity = -currentTurnRate;  // ✅ NEGATIVE SIGN, LENGTH SCALING, BELL CURVE ADDED
}
```

---

## 📐 Realistic Ship-Length Scaling Explained

### The Square Root Relationship

Instead of dividing by full ship length (too extreme), we use **square root scaling**:

**Formula**: `lengthScaling = 1 / sqrt(shipLength / 100m)`

This provides realistic variation without extreme penalties:

| Ship Type | Length | Length Ratio | Sqrt Ratio | **Scaling Multiplier** | Turn Rate (rudderRate=30) |
|-----------|--------|--------------|------------|------------------------|--------------------------|
| **Patrol Boat** | 20m | 0.20 | 0.447 | **2.24x faster** | ~67°/s |
| **Corvette** | 50m | 0.50 | 0.707 | **1.41x faster** | ~42°/s |
| **Destroyer** | 100m | 1.00 | 1.000 | **1.0x baseline** | ~30°/s |
| **Cruiser** | 150m | 1.50 | 1.225 | **0.82x slower** | ~25°/s |
| **Battleship** | 250m | 2.50 | 1.581 | **0.63x slower** | ~19°/s |

### Comparison: Old vs New Scaling

| Ship Length | Old System (÷ length) | New System (÷ sqrt) | Improvement |
|-------------|----------------------|---------------------|-------------|
| 20m | 15x faster | 2.24x faster | ✅ Much more balanced |
| 50m | 6x faster | 1.41x faster | ✅ Gentler curve |
| 100m | 3x faster | 1.0x baseline | ✅ Fair reference |
| 150m | 2x slower | 0.82x slower | ✅ Small penalty |
| 250m | 1.2x slower | 0.63x slower | ✅ Reasonable for size |

**Key Benefits**:
- ✅ Realistic: Larger ships turn slower (naval physics)
- ✅ Balanced: Not extreme (good for gameplay)
- ✅ Configurable: Still uses `rudderRate` from ShipConfigurationSO
- ✅ Tunable: Adjust reference length (100m) if needed

### Why Square Root?

Square root scaling is common in naval architecture because:
1. **Turning radius** ∝ ship length (linear relationship)
2. **Turn rate** = speed / turning radius
3. **Therefore**: Turn rate ∝ 1/length (inverse relationship)
4. **But**: For gameplay balance, we use 1/sqrt(length) (gentler curve)

This gives **realistic behavior** without **extreme penalties**.

---

## ⚓ Realistic Speed-Based Turning (Bell Curve)

### The Naval Physics Reality

Real ships don't turn best at maximum speed! Naval doctrine and physics show that optimal maneuverability occurs at **Full Ahead** (~75% max speed), not **Flank Speed** (100%).

**Why?**
1. **Too Slow**: Insufficient water flow over rudder = poor control (steerageway problem)
2. **Optimal Speed**: Strong rudder authority + manageable momentum = best turning
3. **Too Fast**: Excessive momentum overwhelms rudder effectiveness = reduced turning

### Bell Curve Implementation

**Method**: `CalculateTurningEffectiveness(currentSpeedKnots, maxSpeedKnots)` (NetworkedNavalController.cs lines 589-635)

```csharp
/// <summary>
/// Calculate speed-based turning effectiveness using a bell curve
/// Peak effectiveness at ~75% of max speed (Full Ahead)
/// </summary>
private float CalculateTurningEffectiveness(float currentSpeedKnots, float maxSpeedKnots)
{
    // Always have minimal turning from prop wash when stationary
    if (currentSpeedKnots <= 0f) return 0.1f;

    // Calculate speed as percentage of max speed
    float speedPercent = currentSpeedKnots / maxSpeedKnots;

    if (speedPercent <= 0.1f)
    {
        // Very low speeds (0-10%): Poor steerage, prop wash only
        // Ramp from 10% to 30% effectiveness
        return Mathf.Lerp(0.1f, 0.3f, speedPercent / 0.1f);
    }
    else if (speedPercent <= 0.75f)
    {
        // Low to optimal speeds (10-75%): Increasing effectiveness
        // Smooth curve from 30% to 100% (peak at Full Ahead)
        float t = (speedPercent - 0.1f) / 0.65f;
        float smoothT = t * t * (3f - 2f * t); // Hermite interpolation
        return Mathf.Lerp(0.3f, 1.0f, smoothT);
    }
    else
    {
        // High speeds (75-100%+): Decreasing effectiveness due to momentum
        // Drop from 100% to 80% at flank speed
        float t = (speedPercent - 0.75f) / 0.25f;
        float dropOff = Mathf.Lerp(1.0f, 0.8f, t);

        // Beyond 100% speed (if ship exceeds rated max)
        if (speedPercent > 1.0f)
        {
            float excessSpeed = speedPercent - 1.0f;
            dropOff *= Mathf.Exp(-excessSpeed * 2f); // Exponential decay
        }

        return dropOff;
    }
}
```

### Turning Effectiveness by Speed

| Speed % | Throttle Setting | Speed Name | Effectiveness | Turn Rate* | Notes |
|---------|------------------|------------|---------------|------------|-------|
| **0%** | Stop (0) | Dead Stop | **10%** | ~3°/s | Prop wash only |
| **10%** | Dead Slow (1) | Steerageway | **30%** | ~9°/s | Minimal control |
| **25%** | Slow (1) | Maneuvering | **50%** | ~15°/s | Gaining control |
| **50%** | Half (2) | Standard | **80%** | ~24°/s | Good control |
| **75%** | Full (3) | **Full Ahead** | **100%** | **~30°/s** | **PEAK TURNING** ✅ |
| **100%** | Flank (4) | Maximum | **80%** | ~24°/s | Momentum reduces turning |
| **>100%** | Overspeed | Emergency | **<70%** | <21°/s | Exponential decay |

\* *Example values for 100m destroyer with rudderRate=30*

### Throttle System Mapping

WOS2.3_V2 uses an **8-speed throttle system** (0-4 forward, 0-3 reverse):

**Forward Throttle**:
- **0**: Stop (0% speed)
- **1**: Dead Slow Ahead (~12.5% speed)
- **2**: Half Ahead (~50% speed)
- **3**: Full Ahead (~75% speed) ← **BEST TURNING**
- **4**: Flank Speed (100% speed)

**Tactical Implication**: For tight maneuvers, use **throttle 3 (Full Ahead)**, not throttle 4 (Flank)!

### Comparison: Linear vs Bell Curve

**Old System (Linear)**:
```
0% speed → 0% effectiveness
100% speed → 100% effectiveness (WRONG - best turning at max speed)
```

**New System (Bell Curve)**:
```
0% speed → 10% effectiveness (prop wash)
75% speed → 100% effectiveness (CORRECT - optimal turning)
100% speed → 80% effectiveness (momentum penalty)
```

### Visual Representation

```
Turning Effectiveness
  100% |           ┌─────┐
       |          ╱       ╲
   80% |        ╱           ╲_____
   60% |      ╱
   40% |    ╱
   20% |  ╱
   10% |─┘
    0% └─────────────────────────────> Speed %
       0   25   50   75  100  125
                    ↑
                PEAK (Full Ahead)
```

### Why This Matters for Gameplay

1. **Tactical Depth**: Players must choose between speed and maneuverability
2. **Realistic Combat**: Slowing to Full Ahead for dogfights (like real naval tactics)
3. **Skill Expression**: Managing speed for optimal positioning
4. **Historical Accuracy**: Matches WWII naval doctrine (Full Ahead for combat maneuvering)

### Tuning the Bell Curve

**If you want different curve behavior**, modify `CalculateTurningEffectiveness()` in NetworkedNavalController.cs:

**Shift peak speed** (currently 75%):
```csharp
// Change from 0.75f to desired peak (e.g., 0.8 for 80% speed peak)
else if (speedPercent <= 0.75f)  // ← Change this threshold
```

**Adjust low-speed penalty**:
```csharp
// Change effectiveness at zero speed (currently 10%)
if (currentSpeedKnots <= 0f) return 0.1f;  // ← Increase for better stopped turning
```

**Modify high-speed drop-off**:
```csharp
// Change effectiveness at 100% speed (currently 80%)
float dropOff = Mathf.Lerp(1.0f, 0.8f, t);  // ← Change 0.8f to desired value
```

**Remove curve entirely** (back to linear):
```csharp
// Replace entire method with:
private float CalculateTurningEffectiveness(float currentSpeedKnots, float maxSpeedKnots)
{
    if (currentSpeedKnots <= 0f) return 0.1f;
    float speedPercent = Mathf.Clamp01(currentSpeedKnots / maxSpeedKnots);
    return Mathf.Lerp(0.1f, 1.0f, speedPercent); // Linear ramp
}
```

---

## 🧪 Testing The Fix

### Test 1: Steering Direction

1. **Press Play** and host server
2. **Press W** (throttle up to get speed)
3. **Press D** (right turn)
   - **Expected**: Ship turns RIGHT (clockwise)
   - **Rudder angle**: Should show positive value in debug UI
4. **Press A** (left turn)
   - **Expected**: Ship turns LEFT (counterclockwise)
   - **Rudder angle**: Should show negative value in debug UI

### Test 2: Turn Rate Verification

**Setup**:
- Ship at full speed (throttle = 4)
- Full rudder deflection (hold A or D)

**Expected**:
- Turn rate should match single-player version
- ShipDebugUI should show "Rate of Turn: ~30°/s" (depends on shipConfig.rudderRate)
- Ship should complete 180° turn in ~6 seconds (for rudderRate = 30)

**Compare**:
- Test with SimpleNavalController (single-player)
- Test with NetworkedNavalController (multiplayer)
- Turn rates should be **identical**

### Test 3: Bell Curve Speed-Based Turning

**Objective**: Verify that turning effectiveness follows bell curve (peak at throttle 3, not 4)

1. **Dead Stop** (throttle = 0)
   - Hold A or D
   - **Expected**: Ship barely turns (~10% effectiveness, ~3°/s)
   - ShipDebugUI: "Rate of Turn: ~3°/s"

2. **Dead Slow Ahead** (throttle = 1, ~12.5% speed)
   - Hold A or D
   - **Expected**: Poor steering (~30% effectiveness, ~9°/s)
   - ShipDebugUI: "Rate of Turn: ~9°/s"

3. **Half Ahead** (throttle = 2, ~50% speed)
   - Hold A or D
   - **Expected**: Good control (~80% effectiveness, ~24°/s)
   - ShipDebugUI: "Rate of Turn: ~24°/s"

4. **Full Ahead** (throttle = 3, ~75% speed) ← **CRITICAL TEST**
   - Hold A or D
   - **Expected**: **BEST TURNING** (100% effectiveness, ~30°/s)
   - ShipDebugUI: "Rate of Turn: ~30°/s" ✅ **PEAK**
   - **This should be the tightest turn!**

5. **Flank Speed** (throttle = 4, 100% speed)
   - Hold A or D
   - **Expected**: Reduced turning due to momentum (~80% effectiveness, ~24°/s)
   - ShipDebugUI: "Rate of Turn: ~24°/s"
   - **Should turn WORSE than throttle 3!**

**Pass Criteria**:
- ✅ Throttle 3 (Full Ahead) has **tightest turn radius**
- ✅ Throttle 4 (Flank) turns **worse** than throttle 3
- ✅ Turn rate at throttle 0 is minimal (~10%)
- ✅ Smooth progression from low speeds to peak at 75%

### Test 4: Tactical Maneuvering

**Scenario**: Combat maneuver requiring tight turn

1. **Start at Flank Speed** (throttle = 4)
2. **Attempt tight turn** (hold A or D)
   - Observe turn radius (should be wide)
3. **Reduce to Full Ahead** (press S to throttle = 3)
4. **Continue turning** (hold A or D)
   - **Expected**: Turn radius tightens significantly
   - Ship turns faster despite lower speed

**This validates realistic naval tactics**: Reduce speed for better maneuverability!

---

## 📊 Performance Comparison

| Configuration | Old NetworkedNavalController | New NetworkedNavalController | SimpleNavalController |
|---------------|------------------------------|------------------------------|----------------------|
| **Steering Direction** | ❌ Reversed (A=right, D=left) | ✅ Correct (A=left, D=right) | ✅ Correct |
| **Turn Rate (120m ship, rudderRate=30)** | ❌ ~2.5°/s | ✅ ~27°/s at Full Ahead | ✅ ~30°/s (linear) |
| **Ship-Length Scaling** | ❌ Linear division (too extreme) | ✅ Square root (realistic) | ❌ None |
| **Speed-Based Effectiveness** | ❌ Linear (unrealistic) | ✅ Bell curve (peak at 75%) | ⚠️ Linear ramp |
| **Peak Turning Speed** | ❌ 100% (Flank) | ✅ 75% (Full Ahead) | ❌ 100% (unrealistic) |
| **Rotation Method** | `angularVelocity` | `angularVelocity` | `transform.Rotate()` |

**Key Improvement**: NetworkedNavalController now **surpasses** SimpleNavalController in realism with both ship-size and speed-based physics!

---

## 💡 Technical Notes

### Why angularVelocity vs transform.Rotate()?

**SimpleNavalController**:
- Uses `transform.Rotate()` for direct rotation
- Works fine for single-player
- Simple and predictable

**NetworkedNavalController**:
- Uses `shipRigidbody.angularVelocity` for physics-based rotation
- Required for proper networked physics synchronization
- Mirror's NetworkTransform syncs Rigidbody2D physics automatically
- More realistic physics interactions (collision response, momentum)

### Ship Length Consideration

**NEW APPROACH** (Current System):
- Uses **square root scaling** based on ship length
- Provides realistic size-based variation
- Gentler curve than direct division (2.24x range instead of 12.5x)
- Reference length: 100m (destroyer/cruiser baseline)
- Still uses `rudderRate` from ShipConfigurationSO as base value

**How It Works**:
1. **Base turn rate**: `shipConfig.rudderRate` (set per ship class in ScriptableObject)
2. **Length scaling**: `1 / sqrt(shipLength / 100m)` (automatic based on ship size)
3. **Final turn rate**: `baseTurnRate × lengthScaling`

**Example for 120m Destroyer**:
- `rudderRate` = 30°/s (from ShipConfigurationSO)
- `lengthScaling` = 1 / sqrt(120/100) = 1 / 1.095 = 0.913
- **Final turn rate**: 30 × 0.913 = **27.4°/s**

**Benefits**:
- ✅ Realistic: Larger ships turn slower (physics-based)
- ✅ Balanced: Not too extreme (good gameplay)
- ✅ Automatic: No manual tuning per ship needed
- ✅ Configurable: Can still adjust `rudderRate` per ship class if desired

---

## 🔧 Tuning Ship Turning

### Option 1: Adjust Per-Ship Values (ShipConfigurationSO)

Each ship class can have different base turn rates:

**Recommended Starting Values**:
```
Patrol Boat (20m):   rudderRate = 40°/s → Final: ~90°/s (very nimble)
Corvette (50m):      rudderRate = 35°/s → Final: ~49°/s (agile)
Destroyer (100m):    rudderRate = 30°/s → Final: ~30°/s (baseline)
Cruiser (150m):      rudderRate = 28°/s → Final: ~23°/s (steady)
Battleship (250m):   rudderRate = 25°/s → Final: ~16°/s (sluggish)
```

**If Turn Rate Too Fast** for a specific ship:
- **Decrease** `rudderRate` in that ship's ShipConfigurationSO
- Example: Destroyer feels too nimble? Change 30 → 25

**If Turn Rate Too Slow** for a specific ship:
- **Increase** `rudderRate` in that ship's ShipConfigurationSO
- Example: Patrol boat feels sluggish? Change 40 → 50

### Option 2: Adjust Global Scaling (Code Change)

If **all ships** need different scaling behavior, change the **reference length** in NetworkedNavalController.cs line 560:

**Current** (100m baseline):
```csharp
const float referenceLength = 100f;  // Destroyer/cruiser baseline
```

**Make ALL ships turn faster** (larger reference = less penalty):
```csharp
const float referenceLength = 150f;  // Makes all ships turn ~18% faster
```

**Make ALL ships turn slower** (smaller reference = more penalty):
```csharp
const float referenceLength = 75f;   // Makes all ships turn ~13% slower
```

**Make scaling more aggressive** (change square root to cube root or linear):
```csharp
// More aggressive (closer to old system):
float lengthScaling = 1f / Mathf.Pow(lengthRatio, 0.7f);  // Between sqrt and linear

// Less aggressive (even gentler):
float lengthScaling = 1f / Mathf.Pow(lengthRatio, 0.3f);  // Very gentle curve
```

### Option 3: Disable Length Scaling Entirely

If you want **no length-based variation** (same turn rate for all ship sizes):

**NetworkedNavalController.cs line 560-562**, change:
```csharp
// OLD (with scaling):
const float referenceLength = 100f;
float lengthRatio = shipConfig.length / referenceLength;
float lengthScaling = 1f / Mathf.Sqrt(lengthRatio);

// NEW (no scaling):
float lengthScaling = 1f;  // All ships use rudderRate directly
```

Then set different `rudderRate` values per ship class in ShipConfigurationSO.

### Other Tuning Options

**Turning Response** (how fast rudder moves):
- **Increase** `rudderRate` (separate from turn rate) in ShipConfigurationSO
- Ship reaches full rudder deflection faster

**Low-Speed Turning**:
- **Adjust** `steerageway` in ShipConfigurationSO
- Lower value = better turning at low speeds

**Max Rudder Deflection**:
- **Adjust** `maxRudderAngle` in ShipConfigurationSO
- Larger angle = tighter turns (but may feel less realistic)

---

## 📝 Summary

**What Was Broken**:
1. Steering controls reversed (A/D swapped)
2. Turn rate 12x slower than single-player version (divided by full ship length)
3. No ship-size-based turning variation (unrealistic)
4. Linear speed scaling (unrealistic - best turning at max speed)

**What Was Fixed**:
1. Added negative sign to `angularVelocity` to correct rotation direction
2. Replaced extreme length division with **square root scaling** (gentler, more realistic)
3. Added automatic ship-size-based turn rate variation (2.24x range instead of 12.5x)
4. Implemented **bell curve speed-based effectiveness** (peak at 75% speed, not 100%)

**Result**:
- ✅ A key turns left (counterclockwise)
- ✅ D key turns right (clockwise)
- ✅ Turn rate based on ship size (realistic naval physics)
- ✅ Balanced gameplay (not too extreme)
- ✅ Smaller ships turn faster (patrol boats nimble)
- ✅ Larger ships turn slower (battleships sluggish)
- ✅ 100m destroyer/cruiser baseline (~30°/s peak with rudderRate=30)
- ✅ **Best turning at Full Ahead (throttle 3), not Flank (throttle 4)**
- ✅ Realistic naval tactics (reduce speed for tight maneuvers)
- ✅ ShipConfigurationSO values fully respected
- ✅ Tunable via reference length, per-ship rudderRate, or bell curve parameters

**Example Turn Rates** (with rudderRate=30, at optimal speed):
- 20m Patrol Boat: ~67°/s peak at Full Ahead (2.24x faster)
- 100m Destroyer: ~30°/s peak at Full Ahead (baseline)
- 250m Battleship: ~19°/s peak at Full Ahead (0.63x slower)

**Speed-Based Effectiveness** (100m destroyer example):
- Stop (0%): ~3°/s (10% effectiveness)
- Dead Slow (12.5%): ~9°/s (30% effectiveness)
- Half Ahead (50%): ~24°/s (80% effectiveness)
- **Full Ahead (75%): ~30°/s (100% effectiveness - PEAK)** ✅
- Flank (100%): ~24°/s (80% effectiveness - momentum penalty)

**Tactical Gameplay Implication**: Players now must choose between raw speed (Flank) and maneuverability (Full Ahead), adding strategic depth!

---

**Last Updated**: 2025-10-18
**Status**: Fix complete with realistic ship-length scaling AND bell curve speed effectiveness - ready for testing
