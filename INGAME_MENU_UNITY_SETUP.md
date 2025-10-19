# In-Game Menu - Unity Editor Setup Guide

Step-by-step guide to implement the ESC/pause menu in Unity.

---

## Prerequisites

- ✅ Unity 6000.0.55f1 open with Main scene
- ✅ MUIP (Modern UI Pack) imported
- ✅ InGameMenuController.cs script exists in `Assets/Scripts/UI/`
- ✅ Mirror Networking configured

---

## Step 1: Create Canvas (If Doesn't Exist)

If your Main scene doesn't have a Canvas for UI:

1. **Hierarchy** → Right-click → **UI → Canvas**
2. Rename to "MainCanvas"
3. **Canvas** component settings:
   - Render Mode: **Screen Space - Overlay**
   - UI Scale Mode: **Scale With Screen Size**
   - Reference Resolution: **1920 x 1080**

4. Add **Canvas Scaler** component if not present
5. Add **Graphic Raycaster** component if not present

---

## Step 2: Create In-Game Menu Panel

### 2.1 Create Panel GameObject

1. **Hierarchy** → Right-click **MainCanvas** → **UI → Panel**
2. Rename to **"InGameMenuPanel"**
3. **RectTransform** settings:
   - **Anchor**: Stretch-Stretch (full screen)
   - **Left**: 0, **Right**: 0, **Top**: 0, **Bottom**: 0
   - **Width**: Should auto-adjust to screen
   - **Height**: Should auto-adjust to screen

### 2.2 Style the Background

1. Select **InGameMenuPanel**
2. **Image** component:
   - Color: Black with **Alpha = 180** (semi-transparent overlay)
   - Or use a blur shader for professional look

---

## Step 3: Create Menu Container

### 3.1 Create Container Panel

1. Right-click **InGameMenuPanel** → **UI → Panel**
2. Rename to **"MenuContainer"**
3. **RectTransform** settings:
   - **Anchor**: Center-Middle
   - **Pos X**: 0, **Pos Y**: 0
   - **Width**: 600, **Height**: 800

### 3.2 Style Container

1. **Image** component:
   - Color: Dark gray or navy (e.g., RGB: 20, 30, 40, Alpha: 255)
   - Optional: Add outline or shadow for depth

---

## Step 4: Create Title Text

1. Right-click **MenuContainer** → **UI → Text - TextMeshPro**
2. Rename to **"Title"**
3. **RectTransform**:
   - **Anchor**: Top-Center
   - **Pos X**: 0, **Pos Y**: -50
   - **Width**: 500, **Height**: 80
4. **TextMeshProUGUI** settings:
   - Text: **"GAME PAUSED"**
   - Font Size: **48**
   - Alignment: **Center**
   - Color: **White**

---

## Step 5: Add MUIP Buttons

You'll create 4 buttons using MUIP's ButtonManager component.

### 5.1 Resume Button

1. Right-click **MenuContainer** → **Create Empty**
2. Rename to **"ResumeButton"**
3. Add component: **Michsky.MUIP.ButtonManager**
4. **RectTransform**:
   - **Anchor**: Top-Center
   - **Pos X**: 0, **Pos Y**: -200
   - **Width**: 400, **Height**: 80

**ButtonManager Settings**:
- Button Text: **"Resume Game"**
- Button Icon: None (or play icon if available)
- Button Style: Select from MUIP presets
- Normal Color: Blue or primary color
- Highlighted Color: Lighter blue
- onClick: **(leave empty for now - will assign in code)**

### 5.2 Settings Button

1. Duplicate **ResumeButton** (Ctrl+D)
2. Rename to **"SettingsButton"**
3. **RectTransform**:
   - **Pos Y**: -300
4. **ButtonManager**:
   - Button Text: **"Settings"**
   - Button Icon: Gear icon (if available)

### 5.3 Exit to Menu Button

1. Duplicate **ResumeButton**
2. Rename to **"ExitToMenuButton"**
3. **RectTransform**:
   - **Pos Y**: -400
4. **ButtonManager**:
   - Button Text: **"Exit to Menu"**
   - Button Icon: Home or door icon
   - Normal Color: Orange or warning color

### 5.4 Quit Game Button

1. Duplicate **ResumeButton**
2. Rename to **"QuitButton"**
3. **RectTransform**:
   - **Pos Y**: -500
4. **ButtonManager**:
   - Button Text: **"Quit Game"**
   - Button Icon: Exit or X icon
   - Normal Color: Red or danger color

---

## Step 6: Add InGameMenuController Script

### 6.1 Create Controller GameObject

1. Right-click **MainCanvas** → **Create Empty**
2. Rename to **"InGameMenuController"**
3. **Add Component** → Search for **"InGameMenuController"**
4. Script should appear (from `Assets/Scripts/UI/`)

### 6.2 Assign References in Inspector

With **InGameMenuController** selected:

**Menu Panel**:
- Drag **InGameMenuPanel** to `Menu Panel` field

**MUIP Buttons** (find the ButtonManager components):
- Drag **ResumeButton** to `Resume Button` field
- Drag **SettingsButton** to `Settings Button` field
- Drag **ExitToMenuButton** to `Exit To Menu Button` field
- Drag **QuitButton** to `Quit Button` field

**Configuration**:
- Main Menu Scene Name: **"MainMenu"** (your main menu scene name)
- Lock Cursor In Game: **✓ Checked**
- Show Cursor In Menu: **✓ Checked**

---

## Step 7: Initial State Setup

The menu should be hidden by default when the game starts.

1. Select **InGameMenuPanel**
2. In Inspector, **uncheck the checkbox** next to the name (disables GameObject)
3. This ensures menu is hidden on game start

---

## Step 8: Test in Play Mode

### 8.1 Basic Functionality Test

1. **Play** the game (single-player test first)
2. Press **ESC** → Menu should appear
3. Cursor should become visible
4. Press **ESC** again → Menu should close
5. Cursor should lock again

### 8.2 Button Tests

**Resume Button**:
- Click → Menu closes, game resumes

**Settings Button**:
- Click → Shows "Settings not implemented" (if you haven't added settings yet)
- Or opens your settings panel if available

**Exit to Menu Button**:
- Click → Should disconnect from network and load MainMenu scene
- Verify no errors in Console

**Quit Button**:
- In Editor: Stops Play Mode
- In Build: Closes application

---

## Step 9: Multiplayer Testing

### 9.1 Test Network Disconnect

1. **Start Host** from MainMenu
2. Join game as client
3. Press **ESC** → Menu appears
4. Click **"Exit to Menu"**
5. **Verify**:
   - Client disconnects cleanly
   - No errors in Console
   - Returns to MainMenu scene
   - Server continues running (if testing as host)

### 9.2 Test Multiple Players

1. Host starts game
2. Two clients join
3. **Client 1** opens menu (ESC)
4. **Verify**:
   - Only Client 1 sees menu
   - Client 2 continues playing normally
   - Server/Host unaffected

---

## Step 10: Polish & Refinements

### 10.1 Add Animations (Optional)

Use MUIP's animation features:
1. Select **InGameMenuPanel**
2. Add **Animator** component
3. Create fade-in/fade-out animation
4. Trigger animations from InGameMenuController

### 10.2 Add Sound Effects (Optional)

1. Select **ButtonManager** components
2. In **Sound** section:
   - Hover Sound: UI hover sfx
   - Click Sound: UI click sfx

### 10.3 Add Blur Effect (Optional)

For professional look:
1. Add **UI Blur** shader to InGameMenuPanel background
2. Or use post-processing blur when menu opens

---

## Troubleshooting

### Menu Doesn't Appear

**Check**:
- InGameMenuPanel is disabled by default? (Should be ✓)
- InGameMenuController has menuPanel reference assigned?
- Console for errors?

**Fix**:
- Verify all references in Inspector
- Check InGameMenuController script is on a GameObject in scene

### ESC Key Not Working

**Check**:
- Is InGameMenuController enabled?
- Any other scripts capturing ESC input?
- Input System conflicts?

**Fix**:
- Make sure no other Input.GetKeyDown(KeyCode.Escape) in code
- Check Unity's Input Manager settings

### Buttons Don't Respond

**Check**:
- EventSystem exists in scene? (Should be auto-created with Canvas)
- Graphic Raycaster on Canvas?
- Buttons have ButtonManager component?

**Fix**:
- Add EventSystem: GameObject → UI → Event System
- Ensure Canvas has Graphic Raycaster component

### Cursor Doesn't Lock/Unlock

**Check**:
- InGameMenuController settings:
  - Lock Cursor In Game = true
  - Show Cursor In Menu = true

**Fix**:
- Verify settings in Inspector
- Check no other scripts controlling cursor

### Exit to Menu Causes Errors

**Check**:
- MainMenu scene name matches "mainMenuSceneName" field?
- MainMenu scene included in Build Settings?

**Fix**:
1. File → Build Settings
2. Add MainMenu scene to list
3. Verify scene name spelling

### Network Disconnect Issues

**Check**:
- NetworkManager exists in scene?
- Mirror components configured correctly?

**Fix**:
- Verify InGameMenuController.DisconnectFromNetwork() logic
- Check Console for Mirror errors
- Ensure proper cleanup (StopHost/StopClient/StopServer)

---

## Hierarchy Structure (Final)

```
MainCanvas (Canvas)
├── InGameMenuPanel (Panel - starts disabled)
│   └── MenuContainer (Panel)
│       ├── Title (TextMeshPro)
│       ├── ResumeButton (ButtonManager)
│       ├── SettingsButton (ButtonManager)
│       ├── ExitToMenuButton (ButtonManager)
│       └── QuitButton (ButtonManager)
└── InGameMenuController (Script)
```

---

## Configuration Summary

| Setting | Value |
|---------|-------|
| Canvas Render Mode | Screen Space - Overlay |
| Canvas Resolution | 1920 x 1080 |
| Menu Panel Size | Full screen (stretch) |
| Menu Container Size | 600 x 800 |
| Buttons Width x Height | 400 x 80 |
| Main Menu Scene Name | "MainMenu" |
| Lock Cursor | ✓ Enabled |

---

## Next Steps

Once basic menu works:

1. **Add Settings Panel**
   - Audio volume sliders
   - Graphics quality dropdown
   - Key binding settings

2. **Add Confirmation Dialogs**
   - "Are you sure you want to quit?" popup
   - "Exit to menu will disconnect you" warning

3. **Add Statistics Display**
   - Playtime
   - Score/stats
   - Network ping

4. **Implement Save/Load**
   - Save game state before exiting
   - Load state on resume

---

**Setup Complete!**

Test thoroughly in both single-player and multiplayer before building.
