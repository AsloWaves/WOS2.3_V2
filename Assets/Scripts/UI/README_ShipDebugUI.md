# Ship Debug UI Setup Guide

## Overview
The **ShipDebugUI** script provides real-time display of ship telemetry data including speed, throttle, navigation, and ship specifications.

## Setup Instructions

### 1. Create UI Panel
1. **Right-click in Hierarchy** → UI → Panel
2. **Rename** to "ShipDebugPanel"
3. **Position** where you want the debug info (e.g., top-left corner)

### 2. Add Text Component
**Option A: TextMeshPro (Recommended)**
1. **Right-click ShipDebugPanel** → UI → Text - TextMeshPro
2. **Rename** to "ShipInfoText"
3. **Configure text properties:**
   - Font size: 12-14
   - Color: White or cyan for readability
   - Alignment: Top-Left
   - Enable word wrap if needed

**Option B: Legacy UI Text**
1. **Right-click ShipDebugPanel** → UI → Text
2. **Follow same configuration as above**

### 3. Add ShipDebugUI Script
1. **Select ShipDebugPanel**
2. **Add Component** → WOS.UI → Ship Debug UI
3. **Assign the text component** to either:
   - `Ship Info Text` (if using TextMeshPro)
   - `Legacy Info Text` (if using UI Text)

### 4. Configure Settings
- **Update Rate**: 10 Hz (default) - adjust for performance
- **Speed Precision**: 1 decimal place
- **Angle Precision**: 1 decimal place
- **Ship Controller**: Auto-found if not assigned

## MUIP Integration

For **Multi-line UI Panel** compatibility with InputField components:

### Setup with MUIP InputField (Recommended)
1. **Import MUIP** package to your project
2. **Create UI Panel** for the debug display
3. **Add TMP_InputField component** instead of regular text
4. **Configure ShipDebugUI script:**
   - Check **"Use Muip Input Field"** option
   - Assign the InputField to **"Muip Input Field"** slot
   - Keep **"Muip Read Only"** checked for display-only
   - Adjust **"Muip Line Spacing"** (default 1.2)

### MUIP InputField Configuration
The script automatically configures the InputField:
- **Multi-line support** with newline handling
- **Read-only mode** (non-interactive display)
- **Optimized line spacing** for readability
- **Word wrapping** for long text
- **Overflow handling** for large content

### Alternative MUIP Setup
1. **Standard TextMeshPro** with MUIP panel styling
2. **Leave "Use Muip Input Field" unchecked**
3. **Assign TextMeshPro component** to "Ship Info Text"
4. **Configure MUIP panel background and styling**

### MUIP vs Standard Text
| Feature | MUIP InputField | Standard Text |
|---------|----------------|---------------|
| **Multi-line** | ✅ Native support | ✅ Rich text |
| **Scrolling** | ✅ Auto-scroll | ❌ Overflow only |
| **Selection** | ✅ Text selection | ❌ No selection |
| **Formatting** | ✅ Clean formatting | ✅ Rich text bold |
| **Performance** | ✅ Optimized | ✅ Standard |

## Display Information

### Real-time Data
- **Vessel Name & Class**
- **Current Speed** (knots)
- **Target Speed** (knots)
- **Throttle Setting** (with naval terminology)
- **Bearing** (degrees)
- **Rate of Turn** (degrees/second)
- **Rudder Angle** (degrees)
- **Navigation Mode** (Manual/Auto)

### Ship Specifications
- **Maximum Speed**
- **Length**
- **Displacement**
- **Maximum Rudder Angle**

## Customization

### Update Rate
```csharp
GetComponent<ShipDebugUI>().SetUpdateRate(5f); // 5 Hz
```

### Toggle Visibility
```csharp
GetComponent<ShipDebugUI>().ToggleVisibility();
```

### Get Rate of Turn
```csharp
float turnRate = GetComponent<ShipDebugUI>().GetRateOfTurn();
```

## Performance Notes

- **Default 10 Hz update** provides smooth data without performance impact
- **Lower update rates** (5 Hz) suitable for less critical displays
- **Higher update rates** (30 Hz) for precision monitoring
- **Automatic caching** prevents redundant calculations

## Troubleshooting

### No Data Displayed
1. **Check ship controller assignment**
2. **Verify text component assignment**
3. **Ensure ship has ShipConfigurationSO assigned**

### Performance Issues
1. **Reduce update rate** to 5 Hz or lower
2. **Check for multiple debug panels** updating simultaneously
3. **Reduce decimal precision** in settings

### Text Formatting Issues
1. **Enable Rich Text** in text component
2. **Use TextMeshPro** for better formatting support
3. **Check font and material** assignments

## Example Usage

```csharp
// Get reference to debug UI
ShipDebugUI debugUI = FindFirstObjectByType<ShipDebugUI>();

// Configure for MUIP InputField
debugUI.GetComponent<ShipDebugUI>().useMuipInputField = true;

// Set custom update rate
debugUI.SetUpdateRate(5f);

// Toggle visibility with key press (F3 is built-in)
if (Input.GetKeyDown(KeyCode.F3))
{
    debugUI.ToggleVisibility();
}
```

## MUIP-Specific Features

### Automatic Configuration
When using MUIP InputField, the script automatically:
- Sets `lineType` to `MultiLineNewline`
- Configures `readOnly` mode for display
- Optimizes `lineSpacing` for readability
- Enables `wordWrapping` for long lines
- Sets `interactable` to false for display-only

### Debug Output Example (MUIP Format)
```
=== SHIP TELEMETRY ===
VESSEL: Frigate
CLASS: Frigate

PROPULSION
Current Speed: 12.5 kts
Target Speed: 14.0 kts
Throttle: Half Ahead (2)

NAVIGATION
Bearing: 045.0°
Rate of Turn: 2.3°/s
Rudder Angle: 15.0°
Mode: MANUAL

SPECIFICATIONS
Max Speed: 28 knots
Length: 85m
Displacement: 1200 tons
Max Rudder: ±35°
```

## Naval Terminology Reference

| Term | Meaning |
|------|---------|
| **Knots** | Nautical miles per hour |
| **Bearing** | Direction ship is facing (0-360°) |
| **Rudder Angle** | Current rudder position |
| **Rate of Turn** | How fast ship is turning |
| **Full Astern** | Maximum reverse power |
| **Flank Speed** | Maximum forward power |