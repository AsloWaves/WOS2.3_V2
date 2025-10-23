# Ship Debug UI Manager Setup Guide

Complete guide to set up the 6-panel ship debug information system at the bottom of the screen.

---

## Overview

This system replaces the single ShipDebugUI panel with 6 separate panels, each displaying specific ship information:

1. **Panel 1**: VESSEL & SPECS (5 fields)
2. **Panel 2**: PROPULSION & OCEAN (7 fields)
3. **Panel 3**: NAVIGATION (4 fields)
4. **Panel 4**: NEAREST PORT (3 fields)
5. **Panel 5**: NETWORK (5 fields)
6. **Panel 6**: RESERVED (empty for future use)

**Total**: 24 TextMeshProUGUI fields across 6 panels

---

## Part 1: Create Panel Structure

### Step 1: Create Main Container

```
1. Hierarchy → Right-click Canvas
2. Create Empty → Rename to "ShipDebugPanels"
3. Inspector:
   - Rect Transform → Anchor: Bottom-Stretch
   - Width: Full screen width
   - Height: 120-150 (or as needed)
   - Pos Y: 0 (anchored to bottom)
```

### Step 2: Create Individual Panels

Create 6 child panels under `ShipDebugPanels`:

```
For each panel (repeat 6 times):
1. Right-click ShipDebugPanels → UI → Panel
2. Rename:
   - Panel1_VesselSpecs
   - Panel2_PropulsionOcean
   - Panel3_Navigation
   - Panel4_NearestPort
   - Panel5_Network
   - Panel6_Reserved
3. Position and size panels as desired
```

**Layout Recommendation (Horizontal)**:
```
|  Panel 1  |  Panel 2  |  Panel 3  |  Panel 4  |  Panel 5  |  Panel 6  |
|  Vessel   | Propulsion|    Nav    |   Port    | Network   | Reserved  |
```

**Layout Recommendation (Two Rows)**:
```
Row 1: |  Panel 1  |  Panel 2  |  Panel 3  |
Row 2: |  Panel 4  |  Panel 5  |  Panel 6  |
```

---

## Part 2: Create TextMeshProUGUI Fields

### Panel 1 - VESSEL & SPECS (5 fields)

```
1. Right-click Panel1_VesselSpecs → UI → Text - TextMeshPro
2. Rename to "VesselName_Text"
3. Set text to: "USS Constitution" (example)
4. Repeat for remaining fields:
   - VesselClass_Text → "Frigate"
   - VesselLength_Text → "52m"
   - VesselDisplacement_Text → "2200 tons"
   - VesselMaxRudder_Text → "±30°"
```

**Layout Example**:
```
VESSEL & SPECS
Name: USS Constitution
Class: Frigate
Length: 52m
Displacement: 2200 tons
Max Rudder: ±30°
```

### Panel 2 - PROPULSION & OCEAN (7 fields)

```
1. Right-click Panel2_PropulsionOcean → UI → Text - TextMeshPro
2. Create these fields:
   - PropCurrentSpeed_Text → "12.5 kts"
   - PropTargetSpeed_Text → "15.0 kts"
   - PropThrottle_Text → "Full Ahead (3)"
   - PropMaxSpeed_Text → "18 kts"
   - OceanDepth_Text → "45.2m"
   - OceanTileType_Text → "Deep Ocean"
   - OceanZone_Text → "Atlantic"
```

**Layout Example**:
```
PROPULSION & OCEAN
Speed: 12.5 kts
Target: 15.0 kts
Throttle: Full Ahead (3)
Max: 18 kts
Depth: 45.2m
Tile: Deep Ocean
Zone: Atlantic
```

### Panel 3 - NAVIGATION (4 fields)

```
1. Right-click Panel3_Navigation → UI → Text - TextMeshPro
2. Create these fields:
   - NavBearing_Text → "090°"
   - NavRateOfTurn_Text → "5.2°/s"
   - NavRudderAngle_Text → "15.0°"
   - NavMode_Text → "AUTO"
```

**Layout Example**:
```
NAVIGATION
Bearing: 090°
Turn Rate: 5.2°/s
Rudder: 15.0°
Mode: AUTO
```

### Panel 4 - NEAREST PORT (3 fields)

```
1. Right-click Panel4_NearestPort → UI → Text - TextMeshPro
2. Create these fields:
   - PortName_Text → "Boston Harbor"
   - PortBearing_Text → "045°"
   - PortDistance_Text → "12.3 nm"
```

**Layout Example**:
```
NEAREST PORT
Port: Boston Harbor
Bearing: 045°
Distance: 12.3 nm
```

### Panel 5 - NETWORK (5 fields)

```
1. Right-click Panel5_Network → UI → Text - TextMeshPro
2. Create these fields:
   - NetStatus_Text → "Connected"
   - NetMode_Text → "Host"
   - NetPing_Text → "25ms"
   - NetRTT_Text → "50ms"
   - NetQuality_Text → "Excellent"
```

**Layout Example**:
```
NETWORK
Status: Connected
Mode: Host
Ping: 25ms
RTT: 50ms
Quality: Excellent
```

### Panel 6 - RESERVED (Empty)

```
Leave this panel empty for now.
User will add custom fields later.
```

---

## Part 3: Add ShipDebugUIManager Component

### Step 1: Create GameObject

```
1. Hierarchy → Create Empty GameObject
2. Rename to "ShipDebugUIManager"
3. Add Component → ShipDebugUIManager
```

### Step 2: Assign All References

**Panel 1 - VESSEL & SPECS** (5 assignments):
```
Inspector → ShipDebugUIManager component:
- Vessel Name: Drag VesselName_Text
- Vessel Class: Drag VesselClass_Text
- Vessel Length: Drag VesselLength_Text
- Vessel Displacement: Drag VesselDisplacement_Text
- Vessel Max Rudder: Drag VesselMaxRudder_Text
```

**Panel 2 - PROPULSION & OCEAN** (7 assignments):
```
- Prop Current Speed: Drag PropCurrentSpeed_Text
- Prop Target Speed: Drag PropTargetSpeed_Text
- Prop Throttle: Drag PropThrottle_Text
- Prop Max Speed: Drag PropMaxSpeed_Text
- Ocean Depth: Drag OceanDepth_Text
- Ocean Tile Type: Drag OceanTileType_Text
- Ocean Zone: Drag OceanZone_Text
```

**Panel 3 - NAVIGATION** (4 assignments):
```
- Nav Bearing: Drag NavBearing_Text
- Nav Rate Of Turn: Drag NavRateOfTurn_Text
- Nav Rudder Angle: Drag NavRudderAngle_Text
- Nav Mode: Drag NavMode_Text
```

**Panel 4 - NEAREST PORT** (3 assignments):
```
- Port Name: Drag PortName_Text
- Port Bearing: Drag PortBearing_Text
- Port Distance: Drag PortDistance_Text
```

**Panel 5 - NETWORK** (5 assignments):
```
- Net Status: Drag NetStatus_Text
- Net Mode: Drag NetMode_Text
- Net Ping: Drag NetPing_Text
- Net RTT: Drag NetRTT_Text
- Net Quality: Drag NetQuality_Text
```

**Panel 6 - RESERVED**:
```
(No assignments needed - empty for now)
```

### Step 3: Configure Update Settings

```
Inspector → ShipDebugUIManager component:
- Update Rate: 10 (updates per second)
- Speed Precision: 1 (decimal places)
- Angle Precision: 1 (decimal places)
```

### Step 4: Optional References

```
Ship Reference:
- Leave empty → Auto-detects NetworkedNavalController or SimpleNavalController

Ocean Biome Reference:
- Leave empty → Auto-detects OceanChunkManager
```

---

## Part 4: Styling Recommendations

### TextMeshProUGUI Settings

**Font Settings**:
```
- Font: Courier New or monospace font (for alignment)
- Font Size: 12-16 (readable but compact)
- Color: White or light cyan (good contrast)
- Alignment: Left (for labels and values)
```

**Rect Transform**:
```
- Anchor: Top-Left (within panel)
- Pivot: 0, 1 (top-left)
- Spacing: 2-3 pixels between lines
```

### Panel Styling

**Background Color**:
```
- Dark semi-transparent (e.g., Black with Alpha = 150)
- Or match existing MUIP theme
```

**Border/Outline** (Optional):
```
- Add Outline component to panels
- Color: Cyan or Navy blue
- Thickness: 1-2 pixels
```

---

## Part 5: Testing Workflow

### Test 1: Component Setup
```
1. Unity → Select ShipDebugUIManager GameObject
2. Inspector → Verify all 24 TMP references assigned
3. Console → Check for "ShipDebugUIManager initialization complete" message
```

### Test 2: Data Display
```
1. Unity → Play (Editor Host mode: press H in Main scene)
2. Verify all 24 fields populate with ship data
3. Move ship → Verify fields update in real-time (10Hz)
```

### Test 3: Rate Calculations
```
1. Turn ship left/right
2. Verify "Rate of Turn" updates correctly
3. Speed up/slow down
4. Verify "Current Speed" and "Target Speed" update
```

### Test 4: Network Info
```
1. Test as Host → Should show "Host" in Network Mode
2. Test as Client → Should show "Client" in Network Mode
3. Verify ping/RTT display correctly
```

### Test 5: Null Safety
```
1. Leave some TMP references unassigned
2. Unity → Play
3. Console → Verify no errors (null-safe implementation)
```

---

## Part 6: Disable Old ShipDebugUI

### Migration Steps

```
1. Hierarchy → Find old ShipDebugUI component
2. Inspector → Uncheck component (disable, don't delete)
3. Keep for reference in case rollback needed
```

**Rollback Plan** (if needed):
```
1. Re-enable old ShipDebugUI component
2. Disable ShipDebugUIManager component
3. Original functionality restored immediately
```

---

## Part 7: Future Expansion (Panel 6)

When ready to add custom fields to Panel 6:

### Step 1: Add Fields to Script
```csharp
[Header("Panel 6 - CUSTOM INFO")]
public TextMeshProUGUI customField1;
public TextMeshProUGUI customField2;
// ... add more as needed
```

### Step 2: Create Update Method
```csharp
private void UpdatePanel6()
{
    SafeSetText(customField1, "Custom Value 1");
    SafeSetText(customField2, "Custom Value 2");
}
```

### Step 3: Call from UpdateAllPanels()
```csharp
UpdatePanel5();
UpdatePanel6(); // Add this line
```

### Step 4: Create UI Elements
```
1. Right-click Panel6_Reserved → UI → Text - TextMeshPro
2. Create custom fields
3. Assign in Inspector
```

---

## Troubleshooting

### Issue: Fields Not Updating
**Solution**:
- Check console for "ShipDebugUIManager initialization complete"
- Verify ship controller auto-detected (or assign manually)
- Ensure you're testing as local player in multiplayer

### Issue: Network Panel Shows "Not Connected"
**Solution**:
- Only works in multiplayer mode (NetworkedNavalController)
- Use Editor Host mode (press H) for testing
- Single-player mode shows "Single Player / Local"

### Issue: Ocean Panel Shows "No Ocean Manager"
**Solution**:
- Verify OceanChunkManager exists in scene
- Assign oceanManager reference manually if auto-detect fails

### Issue: Port Panel Shows "None Detected"
**Solution**:
- This is normal if no ports in scene yet
- Ports need PortConfigurationSO component to be detected
- Future: PortManager system will handle this better

---

## Performance Notes

**Update Load**:
- 24 fields × 10 updates/second = 240 updates/sec
- Minimal performance impact (<0.1ms per update)
- Rate of turn calculated separately at 100ms intervals

**Optimization**:
- Null-safe updates prevent unnecessary processing
- Ship controller cached (not re-found each frame)
- Network stats only calculated when networked

---

## Quick Reference Checklist

### Setup Checklist
- [ ] Create ShipDebugPanels container
- [ ] Create 6 panel GameObjects
- [ ] Create 24 TextMeshProUGUI fields across panels
- [ ] Add ShipDebugUIManager component to scene
- [ ] Assign all 24 TMP references in Inspector
- [ ] Configure update settings (10Hz, precision)
- [ ] Test in Editor Play mode
- [ ] Verify all fields update correctly
- [ ] Disable old ShipDebugUI component

### Field Assignment Checklist (24 total)
- [ ] Panel 1: 5 fields (Vessel & Specs)
- [ ] Panel 2: 7 fields (Propulsion & Ocean)
- [ ] Panel 3: 4 fields (Navigation)
- [ ] Panel 4: 3 fields (Nearest Port)
- [ ] Panel 5: 5 fields (Network)
- [ ] Panel 6: 0 fields (Reserved for future)

---

**Total Setup Time**: ~15-20 minutes
**Complexity**: Moderate (UI layout + component assignment)
**Result**: Professional 6-panel ship debug system with real-time updates!
