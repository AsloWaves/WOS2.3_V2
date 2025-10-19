# In-Game Menu Setup Guide

Complete guide for setting up the ESC/Pause menu in WOS Naval MMO.

---

## Features

✅ **ESC Key Toggle** - Press ESC to open/close menu
✅ **Cursor Management** - Auto-locks during gameplay, unlocks for menu
✅ **Network Disconnect** - Properly disconnects Mirror networking
✅ **MUIP Integration** - Uses Modern UI Pack for consistent styling
✅ **Multiplayer-Safe** - Doesn't pause game time (other players continue)

---

## Menu Structure

```
In-Game Menu Panel
├── Resume Button       → Close menu, return to gameplay
├── Settings Button     → Open settings (future implementation)
├── Exit to Menu Button → Disconnect & load MainMenu scene
└── Quit Game Button    → Disconnect & close application
```

---

## Unity Setup Steps

### Step 1: Create In-Game Menu Panel in Main Scene

1. **Open Main Scene** (`Assets/Scenes/Main.unity`)
2. **Find or Create Canvas** in Hierarchy
   - If no Canvas exists: Right-click Hierarchy → UI → Canvas
   - Set Canvas to **Screen Space - Overlay**
   - Add **Canvas Scaler** component (UI Scale Mode: Scale With Screen Size, Reference Resolution: 1920x1080)

3. **Create Menu Panel**:
   - Right-click Canvas → UI → Panel
   - Rename to: `InGameMenuPanel`
   - **RectTransform**: Stretch to full screen (Anchor: stretch/stretch, Offsets: all 0)
   - **Image Color**: Semi-transparent black (R:0, G:0, B:0, A:200)

4. **Create Background Blur Panel** (optional for polish):
   - Child of InGameMenuPanel
   - Name: `BlurBackground`
   - Add blur effect or darker overlay

5. **Create Menu Content Container**:
   - Right-click InGameMenuPanel → Create Empty
   - Rename to: `MenuContent`
   - **RectTransform**: Anchor center, Width: 600, Height: 800
   - This will hold all buttons and text

---

### Step 2: Add MUIP Buttons

For each button, create using MUIP prefabs:

#### **Resume Button**
1. **Add MUIP Button**: `Assets/Modern UI Pack/Prefabs/Button/` → Drag button prefab into MenuContent
2. **Rename**: `ResumeButton`
3. **Position**: Top of menu (Anchor: Top-Center, Y: -100)
4. **Button Text**: "Resume"
5. **ButtonManager Component**:
   - Button Text: "Resume"
   - Icon: None or Play icon

#### **Settings Button**
1. **Add MUIP Button** to MenuContent
2. **Rename**: `SettingsButton`
3. **Position**: Below Resume (Y: -200)
4. **Button Text**: "Settings"
5. **Icon**: Gear/Settings icon

#### **Exit to Menu Button**
1. **Add MUIP Button** to MenuContent
2. **Rename**: `ExitToMenuButton`
3. **Position**: Below Settings (Y: -300)
4. **Button Text**: "Exit to Main Menu"
5. **Icon**: Door/Exit icon

#### **Quit Game Button**
1. **Add MUIP Button** to MenuContent
2. **Rename**: `QuitButton`
3. **Position**: Bottom (Y: -400)
4. **Button Text**: "Quit Game"
5. **Icon**: Power/Close icon
6. **Color Theme**: Red tint for warning

---

### Step 3: Add Title Text (Optional)

1. **Right-click MenuContent** → UI → Text - TextMeshPro
2. **Rename**: `MenuTitle`
3. **Position**: Top center (Y: 300)
4. **Text**: "GAME PAUSED" or "MENU"
5. **Font Size**: 48
6. **Alignment**: Center
7. **Color**: White

---

### Step 4: Add InGameMenuController Script

1. **Create Empty GameObject** in Canvas (or use InGameMenuPanel itself)
   - Name: `InGameMenuController`
   - Or add script directly to InGameMenuPanel GameObject

2. **Add Script**: `Assets/Scripts/UI/InGameMenuController.cs`

3. **Assign References in Inspector**:
   ```
   InGameMenuController (Script)
   ├─ Menu Panel
   │  └─ [Drag InGameMenuPanel here]
   ├─ MUIP Buttons
   │  ├─ Resume Button    → [Drag ResumeButton]
   │  ├─ Settings Button  → [Drag SettingsButton]
   │  ├─ Exit To Menu Button → [Drag ExitToMenuButton]
   │  └─ Quit Button      → [Drag QuitButton]
   └─ Configuration
      ├─ Main Menu Scene Name: "MainMenu"
      ├─ Lock Cursor In Game: ✓ (checked)
      └─ Show Cursor In Menu: ✓ (checked)
   ```

4. **Disable Menu Panel** by default:
   - Select `InGameMenuPanel` in Hierarchy
   - **Uncheck** the checkbox at top of Inspector
   - Menu will be hidden on game start

---

### Step 5: Configure NetworkManager Reference

The script will automatically find `NetworkManager` at runtime using `FindFirstObjectByType<NetworkManager>()`.

**Verify**:
- Ensure you have a `NetworkManager` or `WOSNetworkManager` in your Main scene
- Usually on a GameObject named "NetworkManager"

---

## Testing Checklist

### Local Testing (Unity Editor)

1. **Open Main Scene**
2. **Enter Play Mode**
3. **Start as Host/Client** (connect to server)
4. **Wait for ship to spawn**

**Test Menu**:
- [ ] Press **ESC** → Menu opens
- [ ] Cursor becomes visible
- [ ] Press **ESC** again → Menu closes
- [ ] Cursor locks again
- [ ] Click **Resume** → Menu closes
- [ ] Click **Settings** → (Shows "not implemented" log)
- [ ] Click **Exit to Main Menu** → Disconnects, loads MainMenu scene
- [ ] Verify MainMenu loads correctly

**Test Quit**:
- [ ] Open menu, click **Quit Game**
- [ ] In Editor: Play mode stops
- [ ] In Build: Application closes

---

### Multiplayer Testing

**Scenario**: Two players connected to server

**Player 1**:
1. Press ESC to open menu
2. Verify Player 1's ship stops responding to input
3. Verify Player 2's ship continues moving normally

**Player 2**:
1. Observe Player 1's ship keeps its last velocity (doesn't freeze in place)
2. Verify no lag or stuttering
3. Player 2 can interact with game normally

**Player 1**:
1. Click "Exit to Main Menu"
2. Verify clean disconnect
3. Verify MainMenu loads

**Player 2**:
1. Verify Player 1's ship disappears from scene
2. Verify no errors in console
3. Verify game continues normally

---

## Expected Behavior

### Cursor States

| State | Cursor Lock | Cursor Visible | Input |
|-------|-------------|----------------|-------|
| **Gameplay** | Locked | Hidden | Ship controls active |
| **Menu Open** | Unlocked | Visible | Menu interaction |
| **MainMenu Scene** | Unlocked | Visible | Menu interaction |

### Network States

| Action | Client | Host | Server (Dedicated) |
|--------|--------|------|-------------------|
| **Exit to Menu** | StopClient() | StopHost() | StopServer() |
| **Quit Game** | Disconnect + Quit | Disconnect + Quit | Disconnect + Quit |

---

## Troubleshooting

### "Menu doesn't open when pressing ESC"

**Check**:
1. InGameMenuPanel is assigned in Inspector
2. Script is enabled and active
3. Console for errors during Start()
4. Input System isn't blocking ESC key

**Fix**: Check Inspector assignments and console logs

---

### "Cursor stays locked when menu opens"

**Check**:
1. `Show Cursor In Menu` is checked in Inspector
2. No other script is locking cursor

**Fix**: Set `showCursorInMenu = true` in Inspector

---

### "NetworkManager not found error"

**Problem**: Script can't find NetworkManager at runtime

**Fix**:
1. Ensure NetworkManager exists in Main scene
2. Check it's active in Hierarchy
3. Try manually assigning if using custom NetworkManager

---

### "Menu doesn't close on Resume"

**Check**:
1. Resume button is assigned in Inspector
2. Resume button has ButtonManager component
3. Console for click event logs

**Fix**: Verify button assignments and MUIP setup

---

### "Exit to Menu crashes or errors"

**Check Console Logs**:
- NetworkManager disconnect errors
- Scene loading errors
- Missing MainMenu scene

**Fix**:
1. Verify `Main Menu Scene Name` is set to "MainMenu" in Inspector
2. Ensure MainMenu scene is in Build Settings (File → Build Settings)
3. Check NetworkManager properly disconnects (see logs)

---

### "Other players see lag when I open menu"

**Expected**: No lag should occur (menu doesn't pause game)

**If lag occurs**:
1. Check no Time.timeScale changes
2. Verify no blocking operations in menu code
3. Check network performance separately

---

## Advanced Customization

### Add Settings Panel

1. **Create SettingsPanel** in MenuContent
2. **Hide by default**
3. **Update OnSettingsClicked()**:
   ```csharp
   public void OnSettingsClicked()
   {
       settingsPanel.SetActive(true);
       menuPanel.SetActive(false);
   }
   ```

### Add Confirmation Dialog for Quit

1. **Create ConfirmQuitPanel**
2. **Add "Are you sure?" text**
3. **Add Yes/No buttons**
4. **Update OnQuitClicked()** to show confirmation first

### Add Animation

1. **Add Animator** component to InGameMenuPanel
2. **Create animation** for menu slide-in/fade-in
3. **Update OpenMenu()** to play animation:
   ```csharp
   animator.SetTrigger("Open");
   ```

### Add Sound Effects

1. **Add AudioSource** component to InGameMenuController
2. **Assign sound clips** for menu open/close/button click
3. **Play sounds** in appropriate methods

---

## Code Architecture

### Key Components

**InGameMenuController.cs**:
- Single responsibility: In-game menu management
- Network-aware: Properly disconnects Mirror networking
- Input handling: ESC key toggle
- Cursor management: Lock/unlock based on state
- MUIP integration: Uses ButtonManager components

**Key Methods**:
- `ToggleMenu()` - Opens/closes menu
- `OpenMenu()` - Shows menu, unlocks cursor
- `CloseMenu()` - Hides menu, locks cursor
- `DisconnectFromNetwork()` - Handles Mirror disconnect
- `LoadMainMenu()` - Loads MainMenu scene

---

## Integration with Existing Systems

### MenuManager (MainMenu Scene)

- **Separate systems**: InGameMenuController is for in-game, MenuManager is for MainMenu
- **No conflict**: Different scenes, different purposes
- **Scene transition**: InGameMenu → MainMenu uses `SceneManager.LoadScene()`

### NetworkManager

- **Auto-detection**: Finds NetworkManager via `FindFirstObjectByType<>()`
- **Smart disconnect**: Detects if client, host, or server and calls appropriate stop method
- **Clean shutdown**: Prevents network errors on scene transition

### Input System

- **Keyboard-based**: Uses `Input.GetKeyDown(KeyCode.Escape)`
- **Compatible**: Works alongside ship controls (they check IsMenuOpen())
- **Future**: Can integrate with new Input System if needed

---

## Performance Considerations

### Multiplayer Performance

✅ **No Time.timeScale changes** - Game time continues normally
✅ **Local operation** - Menu only affects local player
✅ **Efficient** - No continuous Update() logic when menu closed
✅ **Clean disconnect** - Properly notifies server of disconnect

### Memory

- Menu panel stays in memory (minimal overhead)
- MUIP buttons are pooled and efficient
- No dynamic allocation during gameplay

---

## Security Considerations

### Multiplayer Security

✅ **Client-side only** - Menu doesn't affect server state
✅ **No cheat risk** - Can't pause game time for advantage
✅ **Clean disconnect** - Server properly removes player
✅ **No exploits** - Standard Mirror disconnect procedures

---

## Future Enhancements

**Planned Features**:
- [ ] Settings panel integration
- [ ] Keybind customization
- [ ] Save/Load options
- [ ] Statistics display
- [ ] Social features (friends list, party invite)
- [ ] Report player functionality
- [ ] Help/Tutorial access

---

**In-Game Menu Setup Complete!** 🎮

The menu is now ready for testing. Press ESC during gameplay to access menu options.
