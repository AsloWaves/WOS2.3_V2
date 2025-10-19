# JoinMenu Inspector Setup Guide

**Fix: "Server Status" placeholder not updating**

---

## Problem

The serverStatusText field is not assigned in Unity Inspector, so the status updates in code don't appear in the UI.

---

## Solution: Assign UI References

### Step 1: Find JoinPanel in Hierarchy

1. Open **MainMenu** scene (or your current scene)
2. In **Hierarchy** window, find **JoinPanel**
3. **Click** on JoinPanel to select it

### Step 2: Locate JoinMenuController Component

1. With JoinPanel selected, look in **Inspector** window
2. Scroll down to find **JoinMenuController** component
3. You should see these fields:

```
JoinMenuController (Script)
├─ UI References
│  ├─ Server Status Text (None - TextMeshProUGUI)  ⚠️ NOT ASSIGNED
│  ├─ Connect Button (None - ButtonManager)        ⚠️ NOT ASSIGNED
│  └─ Back Button (None - ButtonManager)           ⚠️ NOT ASSIGNED
├─ Configuration
│  ├─ Server Config (None - ServerConfig)          ⚠️ NOT ASSIGNED
│  ├─ Game Scene Name: "Main"
│  ├─ Status Check Interval: 30
│  └─ Status Check Timeout: 5
```

### Step 3: Find UI Elements in JoinPanel

**Expand JoinPanel in Hierarchy** to see its children:

```
JoinPanel
├─ Background
├─ Title (probably says "Join Server" or similar)
├─ ServerStatusText (TextMeshPro - Text (UI))  👈 FIND THIS
├─ ConnectButton (has ButtonManager component)  👈 FIND THIS
├─ BackButton (has ButtonManager component)     👈 FIND THIS
└─ Other UI elements...
```

**Look for**:
- A **TextMeshPro - Text (UI)** component showing "Server Status" placeholder
- A **button** with MUIP styling (Connect button)
- A **button** with MUIP styling (Back button)

### Step 4: Assign Server Status Text

1. In JoinPanel hierarchy, find the **TextMeshPro text** that currently shows "Server Status"
2. **Drag and drop** that text element into the **Server Status Text** field in JoinMenuController
3. OR click the **circle icon** next to Server Status Text → select your text element from popup

**Verify**:
- Field should now show: `Server Status Text: ServerStatusText (TextMeshProUGUI)`
- The text component should be the one displaying "Server Status" placeholder

### Step 5: Assign Connect Button

1. In JoinPanel hierarchy, find your **Connect button**
2. **IMPORTANT**: Click on the button and verify it has **ButtonManager** component (not Unity Button)
3. **Drag and drop** the button into the **Connect Button** field in JoinMenuController

**Verify**:
- Field should show: `Connect Button: ConnectButton (ButtonManager)`
- Must be ButtonManager component (from Michsky.MUIP namespace)

### Step 6: Assign Back Button

1. In JoinPanel hierarchy, find your **Back button**
2. **IMPORTANT**: Verify it has **ButtonManager** component
3. **Drag and drop** the button into the **Back Button** field in JoinMenuController

**Verify**:
- Field should show: `Back Button: BackButton (ButtonManager)`

### Step 7: Create and Assign ServerConfig

1. In **Project** window, navigate to `Assets/Resources/` folder
2. **Right-click** → **Create** → **WOS** → **Networking** → **Server Configuration**
3. **Name** it: `ServerConfig`
4. **Click** on ServerConfig asset to edit in Inspector
5. Set values:
   - **Server Address**: `172.234.24.224:31139`
   - **Server Location**: `Chicago, Illinois`
   - **Show Server Info**: ✓ (checked)
6. **Drag** ServerConfig asset into the **Server Config** field in JoinMenuController

---

## Testing the Fix

### Run in Unity Editor

**Before Fix** (current):
```
UI shows: "Server Status" (placeholder, never changes)
Console logs: [JoinMenu] Server status: Up (code works, UI doesn't update)
```

**After Fix** (expected):
```
UI shows: "🔍 Checking Server Status..." → "✅ Server Up - 172.234.24.224:31139 (Chicago, Illinois)"
Console logs: [JoinMenu] Server status: Up (same logs, but UI updates too!)
```

### Expected Console Output (After Fix)

```
[JoinMenu] Checking server status...
[JoinMenu] Server status: Checking
[JoinMenu] Server status: Up
[MenuManager] Showing panel: JoinMenu
```

**No error messages!** If you see these errors, fields are still not assigned:
```
[JoinMenu] ⚠️ Server Status Text is NOT assigned in Inspector!
[JoinMenu] ⚠️ Connect Button is NOT assigned in Inspector!
[JoinMenu] ⚠️ Back Button is NOT assigned in Inspector!
```

---

## Visual Confirmation

### Inspector After All Fields Assigned

```
JoinMenuController (Script)
├─ UI References
│  ├─ Server Status Text: ServerStatusText (TextMeshProUGUI)    ✅
│  ├─ Connect Button: ConnectButton (ButtonManager)             ✅
│  └─ Back Button: BackButton (ButtonManager)                   ✅
├─ Configuration
│  ├─ Server Config: ServerConfig (ServerConfig)                ✅
│  ├─ Game Scene Name: "Main"
│  ├─ Status Check Interval: 30
│  └─ Status Check Timeout: 5
```

### UI Behavior After Fix

**Panel Opens**:
- Text changes from "Server Status" to "🔍 Checking Server Status..." (yellow)
- After 0.5s: "✅ Server Up - 172.234.24.224:31139 (Chicago, Illinois)" (green)
- Connect button becomes enabled (clickable)

---

## Common Issues

### "I can't find the text element showing 'Server Status'"

**Solution**:
1. Look for **any TextMeshPro - Text (UI)** in JoinPanel
2. Check its **Text Input** field in Inspector for "Server Status" placeholder
3. If it doesn't exist, create one:
   - Right-click JoinPanel → **UI** → **Text - TextMeshPro**
   - Name it: `ServerStatusText`
   - Set font size to 20-24
   - Assign to JoinMenuController

### "My button is Unity Button, not ButtonManager"

**Solution**:
1. You need MUIP buttons, not standard Unity buttons
2. Options:
   - **Option A**: Convert existing button to MUIP (complex)
   - **Option B**: Use MUIP prefab from `Assets/Modern UI Pack/Prefabs/Button/`
   - **Option C**: Create new MUIP button in JoinPanel

### "Server Config creation menu doesn't appear"

**Solution**:
1. Ensure `ServerConfig.cs` exists in `Assets/Scripts/Networking/`
2. Check that `[CreateAssetMenu]` attribute is present in ServerConfig.cs
3. Restart Unity Editor if menu doesn't appear
4. Try: Right-click in Project → Create → Other menus

### "Fields keep resetting to None"

**Solution**:
1. Make sure JoinPanel is a **prefab** or in the scene (not a missing reference)
2. Check that assigned objects are **children of JoinPanel** or in the scene
3. Don't drag from prefab preview - drag from scene Hierarchy
4. Save scene after assigning fields (Ctrl+S)

---

## Checklist

- [ ] JoinPanel found in Hierarchy
- [ ] JoinMenuController component visible in Inspector
- [ ] Server Status Text assigned (TextMeshProUGUI)
- [ ] Connect Button assigned (MUIP ButtonManager)
- [ ] Back Button assigned (MUIP ButtonManager)
- [ ] ServerConfig asset created in Assets/Resources/
- [ ] ServerConfig assigned to JoinMenuController
- [ ] Server address set to: 172.234.24.224:31139
- [ ] Scene saved (Ctrl+S)
- [ ] Tested in Play Mode
- [ ] Status text updates from placeholder
- [ ] Connect button enables when server up

---

**Once all fields are assigned, the status text will update automatically!** 🎉
