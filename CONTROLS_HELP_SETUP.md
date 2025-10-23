# Controls Help Panel Setup Guide (F1)

Quick guide to set up the F1 controls help panel with MUIP multiline TMP text field.

---

## Unity Editor Setup (5 Minutes)

### Step 1: Create Controls Panel

```
1. Hierarchy → Right-click Canvas
2. UI → Panel (creates panel)
3. Rename to "ControlsHelpPanel"
4. Inspector:
   - Rect Transform → Anchor: Center-Middle
   - Width: 600, Height: 700 (or as needed)
   - Color: Black with Alpha = 220 (dark semi-transparent)
```

### Step 2: Create MUIP Multiline Text Input

**Recommended: Use MUIP CustomInputField (Multiline)**:
```
1. Locate MUIP "Input Field (Multi-Line)" prefab in Modern UI Pack folder
2. Drag prefab into ControlsHelpPanel
3. Rename to "ControlsInputField"
4. Inspector:
   - Rect Transform:
     - Anchor: Stretch-Stretch
     - Left: 20, Right: 20, Top: 20, Bottom: 20 (padding)
   - CustomInputField component:
     - Process Submit: Unchecked (disable enter key)
   - TMP_InputField component:
     - Text: (leave empty)
     - Character Limit: 0 (no limit)
     - Line Type: Multi Line Newline
     - Font: Courier New or monospace
     - Font Size: 14-16
     - Read Only: Will be auto-set by script
```

**Alternative: Direct TextMeshProUGUI** (If not using MUIP):
```
1. Right-click ControlsHelpPanel → UI → Text - TextMeshPro
2. Rename to "ControlsText"
3. Inspector settings same as above (monospace font, 14-16 size, top-left alignment)
```

### Step 3: Create Header (Optional)

```
1. Right-click ControlsHelpPanel → UI → Text - TextMeshPro
2. Rename to "HeaderText"
3. Inspector:
   - Rect Transform:
     - Anchor: Top-Center
     - Pos Y: -30
   - Text: "GAME CONTROLS (F1 to close)"
   - Font Size: 20
   - Font Style: Bold
   - Alignment: Center
   - Color: Cyan or Yellow
```

### Step 4: Add ControlsHelpManager Component

```
1. Hierarchy → Create Empty GameObject
2. Rename to "ControlsHelpManager"
3. Add Component → ControlsHelpManager
4. Inspector → Assign References:
   - Controls Panel: Drag ControlsHelpPanel

   - Text Component (Choose ONE):

     Option 1 - MUIP CustomInputField (RECOMMENDED):
       - MUIP Input Field: Drag the ControlsInputField GameObject
       - Script will auto-extract TMP component from inputText.textComponent
       - Script will auto-set to read-only mode

     Option 2 - Direct TextMeshProUGUI:
       - Controls Text Field: Drag the ControlsText TMP component

     Option 3 - TMP_InputField:
       - TMP Input Field: Drag the TMP_InputField component
       - Script will auto-extract textComponent

   - Toggle Key: F1 (default)
   - Start Visible: Unchecked (panel hidden on start)
```

### Step 5: Configure Controls in Inspector

All controls are pre-populated with defaults. You can edit them in the Inspector:

**Ship Movement Controls**:
- Steer Left: "A / Left Arrow" - "Steer left (rudder port)"
- Steer Right: "D / Right Arrow" - "Steer right (rudder starboard)"
- Throttle Up: "W" - "Increase throttle"
- Throttle Down: "S" - "Decrease throttle"
- Emergency Stop: "SPACE" - "Emergency stop (full stop)"

**Navigation Controls**:
- Set Waypoint: "Right Mouse Click" - "Set navigation waypoint"
- Toggle Autopilot: "Z" - "Toggle autopilot"
- Clear Waypoints: "X" - "Clear all waypoints"
- Interact: "E (hold)" - "Interact with ports/objects"

**Camera Controls**:
- Zoom In: "Mouse Wheel Up" - "Zoom in"
- Zoom Out: "Mouse Wheel Down" - "Zoom out"
- Pan Camera: "Middle Mouse + Drag" - "Pan camera"

**UI Controls**:
- Open Menu: "ESC" - "Open/close menu"
- Open Inventory: "I" - "Open inventory"
- Toggle Help: "F1" - "Toggle this help panel"

### Step 6: Hide Panel by Default

```
1. Hierarchy → Select ControlsHelpPanel
2. Inspector → Uncheck the checkbox (disables GameObject)
```

---

## Output Format

The script generates formatted text like this:

```
=== GAME CONTROLS ===

[ SHIP MOVEMENT ]
A / Left Arrow            - Steer left (rudder port)
D / Right Arrow           - Steer right (rudder starboard)
W                         - Increase throttle
S                         - Decrease throttle
SPACE                     - Emergency stop (full stop)

[ NAVIGATION ]
Right Mouse Click         - Set navigation waypoint
Z                         - Toggle autopilot
X                         - Clear all waypoints
E (hold)                  - Interact with ports/objects

[ CAMERA ]
Mouse Wheel Up            - Zoom in
Mouse Wheel Down          - Zoom out
Middle Mouse + Drag       - Pan camera

[ UI ]
ESC                       - Open/close menu
I                         - Open inventory
F1                        - Toggle this help panel
```

---

## Testing Workflow

### Test 1: Component Setup
```
1. Unity → Play
2. Console → Check for "[ControlsHelp] Controls help panel initialized"
3. Verify panel is hidden on start
```

### Test 2: Toggle Functionality
```
1. Press F1 → Panel should appear
2. Verify formatted controls text is displayed
3. Press F1 again → Panel should hide
```

### Test 3: Text Formatting
```
1. Open panel with F1
2. Verify:
   - All sections visible (Ship, Navigation, Camera, UI)
   - Text aligned properly with monospace font
   - Keys and descriptions separated correctly
```

### Test 4: Inspector Customization
```
1. Unity → Stop Play mode
2. Select ControlsHelpManager GameObject
3. Inspector → Change any control binding
   Example: Change "W" to "W / Up Arrow"
4. Play → Press F1
5. Verify updated text appears in panel
```

### Test 5: Runtime Refresh
```
1. Unity → Play
2. Hierarchy → Select ControlsHelpManager
3. Inspector → Right-click component header
4. Click "Refresh Controls Text"
5. Console → Check for "[ControlsHelp] Controls text refreshed!"
```

---

## Styling Recommendations

### Panel Background
```
- Dark semi-transparent: Black with Alpha = 200-230
- Or match existing MUIP theme
- Add subtle border with Outline component (optional)
```

### Text Styling
```
- Font: Courier New, Consolas, or any monospace font
- Font Size: 14-16 (readable but compact)
- Color: White, Cyan, or Yellow (good contrast)
- Line Spacing: 1.0-1.2 (comfortable reading)
```

### Layout Options

**Option 1: Center Modal** (Recommended)
```
- Panel centered on screen
- Semi-transparent overlay behind
- Click outside to close (optional)
```

**Option 2: Sidebar**
```
- Panel anchored to left/right side
- Full height, narrow width (300-400px)
- Quick reference while playing
```

**Option 3: Bottom Overlay**
```
- Panel at bottom of screen
- Above ship debug panels
- Horizontal layout with categories side-by-side
```

---

## Optional Enhancements

### Close Button
```
1. Right-click ControlsHelpPanel → UI → Button
2. Rename to "CloseButton"
3. Position at top-right corner
4. Text: "X" or "Close"
5. OnClick() → ControlsHelpManager.HidePanel()
```

### Background Blur
```
1. Add UI blur effect to panel background
2. Makes text more readable
3. Professional appearance
```

### Category Icons
```
1. Add small icons next to each category header
2. Ship icon for Ship Movement
3. Compass for Navigation
4. Camera icon for Camera
5. Gear icon for UI
```

### Scroll Support
```
If controls list gets too long:
1. Add Scroll View component to panel
2. Put ControlsText inside Scroll View content
3. Enable vertical scrolling
```

---

## Troubleshooting

### Issue: Panel Doesn't Appear
**Solution**:
- Check controlsPanel is assigned in Inspector
- Verify panel GameObject exists in scene
- Check panel is child of active Canvas

### Issue: Text Field Empty
**Solution**:
- Check controlsTextField is assigned in Inspector
- Verify TMP component reference (not GameObject)
- Right-click ControlsHelpManager → Refresh Controls Text

### Issue: F1 Doesn't Toggle
**Solution**:
- Verify ControlsHelpManager is in scene (not disabled)
- Check toggleKey is set to F1 in Inspector
- Console should show initialization message

### Issue: Text Not Aligned
**Solution**:
- Use monospace font (Courier New, Consolas)
- Check TMP alignment is Top-Left
- Verify word wrapping is enabled

### Issue: Panel Shows on Start
**Solution**:
- Uncheck startVisible in ControlsHelpManager Inspector
- Or disable ControlsHelpPanel GameObject in Hierarchy

---

## API Reference

### Public Methods

```csharp
// Toggle panel visibility
ControlsHelpManager.TogglePanel();

// Show panel
ControlsHelpManager.ShowPanel();

// Hide panel
ControlsHelpManager.HidePanel();

// Refresh controls text (after changing bindings at runtime)
ControlsHelpManager.RefreshControlsText();
```

### Context Menu Commands

```
Inspector → Right-click ControlsHelpManager component:
- "Refresh Controls Text" → Updates displayed text
```

---

## Notes

- All controls are editable in Inspector without code changes
- Text auto-generates on Start() from configured bindings
- F1 toggle works while playing (doesn't pause game)
- Panel can be opened/closed via code for tutorials
- Monospace font ensures proper alignment
- Categories clearly separated for readability

**Total Setup Time**: ~5 minutes
**Complexity**: Easy (mostly UI layout)
**Result**: Professional controls reference panel with F1 toggle!
