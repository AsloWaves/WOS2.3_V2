# Settings Panel Setup Guide

Quick guide to add a temporary Settings panel to your in-game menu.

---

## Unity Editor Setup (5 Minutes)

### Step 1: Create Settings Panel

```
1. Hierarchy → Right-click InGameMenuPanel
2. UI → Panel (creates child panel)
3. Rename to "SettingsPanel"
4. Inspector:
   - Rect Transform → Anchor: Stretch-Stretch
   - Color: Same as InGameMenuPanel (or slightly darker)
```

### Step 2: Create Title Text

```
1. Right-click SettingsPanel → UI → Text - TextMeshPro
2. Rename to "SettingsTitle"
3. Inspector:
   - Rect Transform:
     - Anchor: Top-Center
     - Width: 500, Height: 80
     - Pos Y: -50
   - Text: "SETTINGS"
   - Font Size: 36
   - Alignment: Center
   - Color: White
```

### Step 3: Create "Coming Soon" Message (Temporary)

```
1. Right-click SettingsPanel → UI → Text - TextMeshPro
2. Rename to "ComingSoonText"
3. Inspector:
   - Rect Transform:
     - Anchor: Center-Middle
     - Width: 600, Height: 100
     - Pos Y: 0
   - Text: "Settings panel coming soon!\nVolume, Graphics, Controls, etc."
   - Font Size: 24
   - Alignment: Center
   - Color: Light Gray
```

### Step 4: Create Back Button

```
1. Right-click SettingsPanel → UI → Button - TextMeshPro
2. Rename to "BackButton"
3. Inspector:
   - Rect Transform:
     - Anchor: Bottom-Center
     - Width: 200, Height: 60
     - Pos Y: 100
   - Add Component → ButtonManager (MUIP)
   - Text child:
     - Text: "Back"
     - Font Size: 24
     - Color: White
```

**OR Use MUIP Styled Button:**
```
1. Assets/Modern UI Pack/Prefabs → Drag "Button" prefab into SettingsPanel
2. Rename to "BackButton"
3. Position at bottom-center (Y: 100)
4. Update text to "Back"
```

### Step 5: Assign References in Inspector

```
1. Hierarchy → Select InGameMenuController GameObject
2. Inspector → InGameMenuController component:
   - Settings Panel: Drag SettingsPanel here
   - Settings Back Button: Drag BackButton's ButtonManager here
```

### Step 6: Hide Settings Panel by Default

```
1. Hierarchy → Select SettingsPanel
2. Inspector → Uncheck the checkbox next to the name (disables GameObject)
```

---

## Testing Workflow

### Test 1: Open Settings
```
1. Unity → Play
2. Press ESC → Main menu appears
3. Click "Settings" button
4. Settings panel should appear, main menu hidden
```

### Test 2: Back to Menu
```
1. While in Settings panel
2. Click "Back" button
3. Settings panel hides, main menu reappears
```

### Test 3: Close from Settings
```
1. In Settings panel
2. Press ESC
3. Both panels close, return to game
```

---

## Quick Visual Layout

```
SettingsPanel (Full Screen Panel)
├─ SettingsTitle (Top Center)
│  └─ "SETTINGS" text
│
├─ ComingSoonText (Center)
│  └─ "Settings panel coming soon..." message
│
└─ BackButton (Bottom Center)
   └─ "Back" button (MUIP styled)
```

---

## Optional Enhancements (Later)

When you're ready to add real settings:

### Volume Controls
- Master Volume slider
- Music Volume slider
- SFX Volume slider

### Graphics Settings
- Quality dropdown (Low, Medium, High, Ultra)
- Resolution dropdown
- Fullscreen toggle
- VSync toggle

### Game Settings
- Mouse Sensitivity slider
- Invert Y-Axis toggle
- Camera Shake toggle

---

## Notes

- Settings panel uses same MUIP styling as main menu
- ESC key works from settings panel (closes entire menu)
- Settings panel automatically hidden on game start
- Back button uses ButtonManager for MUIP consistency

**Current Implementation:** Temporary "Coming Soon" message
**Future:** Add actual settings controls when needed

---

**Total Setup Time:** ~5 minutes for temporary panel
