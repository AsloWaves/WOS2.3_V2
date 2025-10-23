# Quit Confirmation Panel Setup Guide

Quick guide to add "Are you sure you want to quit?" confirmation dialog.

---

## Unity Editor Setup (5 Minutes)

### Step 1: Create Quit Confirmation Panel

```
1. Hierarchy → Right-click InGameMenuPanel
2. UI → Panel (creates child panel)
3. Rename to "QuitConfirmationPanel"
4. Inspector:
   - Rect Transform → Anchor: Stretch-Stretch
   - Color: Black with Alpha = 200 (darker overlay)
```

### Step 2: Create Container

```
1. Right-click QuitConfirmationPanel → Create Empty
2. Rename to "ConfirmationContainer"
3. Inspector:
   - Rect Transform:
     - Anchor: Center-Middle
     - Width: 500, Height: 300
   - Add Component → Image
   - Color: Dark Navy or Dark Gray
```

### Step 3: Create Warning Icon (Optional)

```
1. Right-click ConfirmationContainer → UI → Image
2. Rename to "WarningIcon"
3. Inspector:
   - Rect Transform:
     - Anchor: Top-Center
     - Width: 60, Height: 60
     - Pos Y: -50
   - Image: Use ⚠️ sprite or warning icon
   - Color: Orange/Yellow
```

### Step 4: Create Confirmation Text

```
1. Right-click ConfirmationContainer → UI → Text - TextMeshPro
2. Rename to "ConfirmationText"
3. Inspector:
   - Rect Transform:
     - Anchor: Top-Center
     - Width: 450, Height: 120
     - Pos Y: -130
   - Text: "Are you sure you want to quit?\n\nYou will be disconnected from the server."
   - Font Size: 22
   - Alignment: Center
   - Color: White
```

### Step 5: Create YES Button (Red)

```
1. Drag MUIP Button prefab into ConfirmationContainer
   OR Right-click → UI → Button → Add ButtonManager
2. Rename to "YesButton"
3. Inspector:
   - Rect Transform:
     - Anchor: Bottom-Center
     - Width: 180, Height: 60
     - Pos X: -100, Pos Y: 60
   - ButtonManager:
     - Normal Color: Red (#FF4444 or similar)
     - Hover Color: Brighter Red
   - Text child:
     - Text: "YES, QUIT"
     - Font Size: 20
     - Color: White
```

### Step 6: Create NO Button (Green/Blue)

```
1. Duplicate YesButton (Ctrl+D)
   OR Create new MUIP Button
2. Rename to "NoButton"
3. Inspector:
   - Rect Transform:
     - Pos X: 100, Pos Y: 60 (right side)
   - ButtonManager:
     - Normal Color: Blue/Green (#4488FF or #44FF44)
     - Hover Color: Brighter Blue/Green
   - Text child:
     - Text: "NO, CANCEL"
     - Font Size: 20
     - Color: White
```

### Step 7: Assign References in Inspector

```
1. Hierarchy → Select InGameMenuController GameObject
2. Inspector → InGameMenuController component:
   - Quit Confirmation Panel: Drag QuitConfirmationPanel
   - Quit Yes Button: Drag YesButton's ButtonManager
   - Quit No Button: Drag NoButton's ButtonManager
```

### Step 8: Hide Panel by Default

```
1. Hierarchy → Select QuitConfirmationPanel
2. Inspector → Uncheck the checkbox (disables GameObject)
```

---

## Visual Layout

```
QuitConfirmationPanel (Full Screen Dark Overlay)
└─ ConfirmationContainer (500x300 centered box)
   ├─ WarningIcon (⚠️ optional)
   ├─ ConfirmationText
   │  └─ "Are you sure you want to quit?"
   │     "You will be disconnected from the server."
   ├─ YesButton (Red, left side)
   │  └─ "YES, QUIT"
   └─ NoButton (Blue/Green, right side)
      └─ "NO, CANCEL"
```

---

## Testing Workflow

### Test 1: Show Confirmation
```
1. Unity → Play
2. Press ESC → Main menu appears
3. Click "Quit Game" button
4. Quit confirmation panel should appear
5. Main menu should be hidden
```

### Test 2: Confirm Quit
```
1. In quit confirmation panel
2. Click "YES, QUIT" button
3. Unity Editor: Play mode should stop
4. Build: Application should close
```

### Test 3: Cancel Quit
```
1. In quit confirmation panel
2. Click "NO, CANCEL" button
3. Confirmation panel hides
4. Main menu reappears
5. Can continue playing or press ESC again
```

### Test 4: ESC Key from Confirmation
```
1. In quit confirmation panel
2. Press ESC
3. Entire menu closes (returns to game)
```

---

## Color Recommendations

**YES Button (Danger):**
- Normal: Red (#FF4444)
- Hover: Bright Red (#FF6666)
- Text: White

**NO Button (Safe):**
- Normal: Blue (#4488FF) or Green (#44CC44)
- Hover: Bright Blue (#66AAFF) or Bright Green (#66EE66)
- Text: White

**Panel Background:**
- Dark with high opacity for attention
- Black with Alpha = 220-240

---

## Optional Enhancements

### Add Fade Animation
```
1. Select QuitConfirmationPanel
2. Add Component → Canvas Group
3. Add Component → Animation
4. Create fade-in/fade-out animations
```

### Add Sound Effect
```
1. Create Audio Source on YesButton
2. Assign click sound
3. Play on button click
```

### Add Keyboard Shortcuts
```
In InGameMenuController Update():
- If confirmation open:
  - Y key → Quit (Yes)
  - N key → Cancel (No)
  - ESC → Close menu entirely
```

---

## Code Flow

```
User clicks "Quit Game" button
  ↓
OnQuitClicked() called
  ↓
Hide main menu
  ↓
Show quit confirmation panel
  ↓
User clicks YES or NO
  ↓
YES: OnQuitConfirmYes() → Disconnect → Quit
NO:  OnQuitConfirmNo() → Hide confirmation → Show menu
```

---

## Notes

- Confirmation panel appears OVER the main menu
- Both panels are children of InGameMenuPanel for proper layering
- ESC key closes entire menu (including confirmation)
- YES button disconnects from network before quitting
- NO button safely returns to menu without quitting

**Current Protection:** Network disconnect happens BEFORE quit
**User Safety:** Cannot accidentally quit without confirmation

---

**Total Setup Time:** ~5 minutes
**User Experience:** Professional, prevents accidental quits!
