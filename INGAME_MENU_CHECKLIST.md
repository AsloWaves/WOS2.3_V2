# In-Game Menu Setup Checklist

Quick reference checklist for Unity Editor setup.

---

## 📋 Pre-Setup

- [ ] Unity 6000.0.55f1 opened
- [ ] Main scene loaded
- [ ] MUIP package imported
- [ ] InGameMenuController.cs exists in `Assets/Scripts/UI/`

---

## 🎨 UI Creation

### Canvas Setup
- [ ] Canvas exists (or create: UI → Canvas)
- [ ] Canvas set to "Screen Space - Overlay"
- [ ] UI Scale Mode: "Scale With Screen Size" (1920x1080)

### Menu Panel
- [ ] Created InGameMenuPanel (UI → Panel)
- [ ] Anchor set to Stretch-Stretch (full screen)
- [ ] Background color: Black, Alpha = 180

### Menu Container
- [ ] Created MenuContainer (child of InGameMenuPanel)
- [ ] Size: 600 x 800, anchored center-middle
- [ ] Background: Dark gray/navy

### Title Text
- [ ] Created Title (TextMeshPro)
- [ ] Text: "GAME PAUSED"
- [ ] Font size: 48, centered, white

### Buttons (MUIP ButtonManager)
- [ ] ResumeButton created (Y: -200)
  - Text: "Resume Game", color: Blue
- [ ] SettingsButton created (Y: -300)
  - Text: "Settings", color: Blue
- [ ] ExitToMenuButton created (Y: -400)
  - Text: "Exit to Menu", color: Orange
- [ ] QuitButton created (Y: -500)
  - Text: "Quit Game", color: Red

---

## 🔧 Script Setup

### Controller GameObject
- [ ] Created empty GameObject "InGameMenuController"
- [ ] Added InGameMenuController component
- [ ] Parent to MainCanvas

### Inspector References
- [ ] Menu Panel → InGameMenuPanel assigned
- [ ] Resume Button → ResumeButton ButtonManager assigned
- [ ] Settings Button → SettingsButton ButtonManager assigned
- [ ] Exit To Menu Button → ExitToMenuButton ButtonManager assigned
- [ ] Quit Button → QuitButton ButtonManager assigned

### Configuration
- [ ] Main Menu Scene Name: "MainMenu" (or correct name)
- [ ] Lock Cursor In Game: ✓
- [ ] Show Cursor In Menu: ✓

### Initial State
- [ ] InGameMenuPanel GameObject **disabled** by default

---

## ✅ Testing Checklist

### Basic Functionality
- [ ] Press ESC → Menu appears
- [ ] Cursor visible when menu open
- [ ] Press ESC again → Menu closes
- [ ] Cursor locks when menu closed
- [ ] No errors in Console

### Button Functions
- [ ] Resume → Closes menu, game continues
- [ ] Settings → (Shows placeholder or opens settings)
- [ ] Exit to Menu → Disconnects and loads MainMenu
- [ ] Quit → Exits game (or stops Play Mode in Editor)

### Multiplayer Testing
- [ ] Start as Host → ESC menu works
- [ ] Join as Client → ESC menu works
- [ ] Client exits → Disconnects cleanly
- [ ] Other players unaffected when one opens menu
- [ ] No network errors in Console

---

## 🐛 Quick Troubleshooting

**Menu doesn't appear?**
→ Check InGameMenuPanel is disabled by default
→ Verify menuPanel reference assigned

**ESC not working?**
→ Check InGameMenuController enabled
→ Look for Input conflicts

**Buttons not clickable?**
→ Verify EventSystem exists
→ Check Graphic Raycaster on Canvas

**Cursor issues?**
→ Check Lock/Show cursor settings
→ Verify no other cursor scripts

**Exit crashes?**
→ Check scene name spelling
→ Verify scene in Build Settings

---

## 📊 Final Verification

- [ ] All references assigned (no "None" in Inspector)
- [ ] Menu hidden by default
- [ ] ESC key toggles menu
- [ ] All 4 buttons work correctly
- [ ] Multiplayer disconnect works
- [ ] No Console errors
- [ ] Cursor locks/unlocks properly

---

## 🎯 Optional Enhancements

- [ ] Add fade animations
- [ ] Add button sound effects
- [ ] Add blur effect to background
- [ ] Add confirmation dialogs
- [ ] Add statistics display
- [ ] Implement settings panel

---

**When everything works, build and test!**
