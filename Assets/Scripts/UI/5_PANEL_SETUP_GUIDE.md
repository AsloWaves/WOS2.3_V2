# 5-Panel Menu System Setup Guide

Complete setup guide for the WOS2.3 5-panel menu system with MUIP layout.

## Architecture Overview

```
MenuManager (Singleton) controls 5 panels:
├── Main Panel (Start, Options, Exit)
├── Connection Panel (Host, Join, Back)
├── Host Panel (Start Host, Back)
├── Join Panel (IP Input, Connect, Back)
└── Options Panel (Coming Soon, Back)
```

## Panel Flow Diagram

```
Main Panel
├── Start → Connection Panel
│            ├── Host → Host Panel → Start Host → Game
│            └── Join → Join Panel → Connect → Game
├── Options → Options Panel → Back → Main Panel
└── Exit → Quit Game
```

---

## Scripts Overview

### **1. MenuManager.cs** (Attach to MenuManager GameObject)
- Manages all 5 panels (shows one, hides others)
- Singleton for easy access from any script
- Handles Main, Options, Exit buttons

### **2. ConnectionMenuController.cs** (Attach to Connection Panel)
- Handles Host and Join buttons on Connection panel
- Navigates to Host Panel or Join Panel

### **3. HostMenuController.cs** (Attach to Host Panel)
- Starts local server (host mode)
- "Start Host" button → calls `networkManager.StartHost()`

### **4. JoinMenuController.cs** (Attach to Join Panel)
- Handles IP input and server connection
- "Connect" button → calls `networkManager.StartClient()`
- Saves last used IP using NetworkAddressManager

### **5. OptionsMenuController.cs** (Attach to Options Panel)
- Placeholder for future settings
- Currently just "Coming Soon" message

---

## Unity Scene Setup

### Step 1: Create Scene Hierarchy

```
MainMenu Scene
├── Canvas (Screen Space - Overlay)
│   ├── MenuManager (Empty GameObject)
│   │   └── MenuManager.cs component
│   │
│   ├── Panel_Main (MUIP Panel) ← Assign to MenuManager
│   │   ├── Button_Start
│   │   ├── Button_Options
│   │   └── Button_Exit
│   │
│   ├── Panel_Connection (MUIP Panel) ← Assign to MenuManager
│   │   ├── ConnectionMenuController.cs component
│   │   ├── Text_Status (optional)
│   │   ├── Button_Host
│   │   ├── Button_Join
│   │   └── Button_Back
│   │
│   ├── Panel_Host (MUIP Panel) ← Assign to MenuManager
│   │   ├── HostMenuController.cs component
│   │   ├── Text_Status
│   │   ├── Button_StartHost
│   │   └── Button_Back
│   │
│   ├── Panel_Join (MUIP Panel) ← Assign to MenuManager
│   │   ├── JoinMenuController.cs component
│   │   ├── InputField_ServerIP (TMP_InputField)
│   │   ├── Text_Status
│   │   ├── Button_Connect
│   │   └── Button_Back
│   │
│   └── Panel_Options (MUIP Panel) ← Assign to MenuManager
│       ├── OptionsMenuController.cs component
│       ├── Text_ComingSoon
│       └── Button_Back
│
├── NetworkManager (GameObject)
│   ├── WOSNetworkManager
│   ├── TelepathyTransport
│   ├── NetworkAddressManager
│   └── ServerLauncher (optional)
│
└── EventSystem
```

---

## Step 2: Assign Panel References in MenuManager

Select the **MenuManager** GameObject and assign:

| Field | Assign |
|-------|--------|
| Main Menu Panel | `Panel_Main` |
| Connection Menu Panel | `Panel_Connection` |
| Host Panel | `Panel_Host` |
| Join Panel | `Panel_Join` |
| Options Menu Panel | `Panel_Options` |
| Starting Panel | `MainMenu` |

---

## Step 3: Wire Up Button OnClick Events

### Panel_Main (Main Menu)
| Button | OnClick Event |
|--------|---------------|
| Button_Start | `MenuManager.OnStartButtonClicked()` |
| Button_Options | `MenuManager.OnOptionsButtonClicked()` |
| Button_Exit | `MenuManager.OnExitButtonClicked()` |

**How to:**
1. Select `Button_Start`
2. In Inspector → OnClick() → Click `+`
3. Drag `MenuManager` GameObject to the object field
4. Function → `MenuManager.OnStartButtonClicked()`
5. Repeat for other buttons

---

### Panel_Connection (Connection Menu)
| Button | OnClick Event |
|--------|---------------|
| Button_Host | `ConnectionMenuController.OnHostButtonClicked()` |
| Button_Join | `ConnectionMenuController.OnJoinButtonClicked()` |
| Button_Back | `ConnectionMenuController.OnBackButtonClicked()` |

**Assign Status Text (Optional):**
- Drag `Text_Status` → `ConnectionMenuController.statusText`

---

### Panel_Host (Host Menu)
| Button | OnClick Event |
|--------|---------------|
| Button_StartHost | `HostMenuController.OnStartHostButtonClicked()` |
| Button_Back | `HostMenuController.OnBackButtonClicked()` |

**Assign References:**
- Drag `Text_Status` → `HostMenuController.statusText`
- Set `Game Scene Name` to `"Main"`

---

### Panel_Join (Join Menu)
| Button | OnClick Event |
|--------|---------------|
| Button_Connect | `JoinMenuController.OnConnectButtonClicked()` |
| Button_Back | `JoinMenuController.OnBackButtonClicked()` |

**Assign References:**
- Drag `InputField_ServerIP` → `JoinMenuController.serverIPInputField`
- Drag `Text_Status` → `JoinMenuController.statusText`
- Set `Default Server IP` to `"127.0.0.1"`
- Set `Game Scene Name` to `"Main"`

---

### Panel_Options (Options Menu)
| Button | OnClick Event |
|--------|---------------|
| Button_Back | `OptionsMenuController.OnBackButtonClicked()` |

---

## Step 4: Configure NetworkManager

Create GameObject: **NetworkManager**

### Add Components:

**1. WOSNetworkManager**
- Offline Scene: `MainMenu`
- Online Scene: `Main`
- Don't Destroy On Load: ✅ **checked**
- Player Prefab: (Assign after creating PlayerShip prefab)
- Max Connections: `10`

**2. TelepathyTransport** (Auto-added by Mirror)
- Port: `7777`

**3. NetworkAddressManager**
- Default Server IP: `127.0.0.1`

**4. ServerLauncher** (Optional - for headless builds)
- Auto Start In Headless: ✅ **checked**
- Default Port: `7777`
- Server Start Scene: `Main`

---

## Testing Checklist

### ✅ Menu Navigation Test
1. Press Play in Unity Editor
2. Should see **Main Panel** only (Start, Options, Exit)
3. Click **Start** → Should show **Connection Panel** (Host, Join, Back)
4. Click **Host** → Should show **Host Panel** (Start Host, Back)
5. Click **Back** → Should return to **Connection Panel**
6. Click **Join** → Should show **Join Panel** (IP input, Connect, Back)
7. Click **Back** → Should return to **Connection Panel**
8. Click **Back** → Should return to **Main Panel**
9. Click **Options** → Should show **Options Panel**
10. Click **Back** → Should return to **Main Panel**

### ✅ Host Functionality Test
1. Press Play
2. Main → Start → Connection → Host
3. Click **Start Host**
4. Status should show "Host started! Loading game..."
5. Should load "Main" scene (will error if scene not set up yet - that's OK)

### ✅ Join Functionality Test
1. Build the game or use 2 Unity Editors (ParrelSync)
2. Instance 1: Host the server
3. Instance 2: Main → Start → Connection → Join
4. Enter IP: `127.0.0.1`
5. Click **Connect**
6. Status should show "Connecting to 127.0.0.1..."
7. Should load "Main" scene and connect

---

## MUIP Styling Tips

### Panel Appearance
- Use MUIP Panel Manager for fade in/out transitions
- Set transition duration: 0.3-0.5 seconds
- Use blur background for depth

### Button Styling
- Use MUIP Button Manager for hover animations
- Configure button states:
  - Normal: Base theme color
  - Highlighted: 20% lighter
  - Pressed: 20% darker
  - Disabled: 50% opacity

### Input Field
- Use MUIP Input Field component
- Placeholder text: "Enter server IP..."
- Text color: White
- Placeholder color: Gray (50% opacity)

### Status Text
- Font: TextMeshPro
- Size: 18-24pt
- Color: White (normal), Red (errors)
- Alignment: Center

---

## Common Issues & Solutions

### Issue: All panels visible at once
**Solution:** MenuManager hides all panels in Awake(). Make sure panels are assigned correctly in MenuManager Inspector.

### Issue: Buttons don't work
**Solution:**
- Check EventSystem exists
- Verify OnClick() events are assigned correctly
- Ensure correct GameObject is dragged to OnClick() object field

### Issue: "MenuManager not found" error
**Solution:** MenuManager GameObject must be in the scene (not disabled)

### Issue: "NetworkManager not found" error
**Solution:**
- Add NetworkManager GameObject to scene
- Add WOSNetworkManager component
- Enable "Don't Destroy On Load"

### Issue: Scene doesn't load after Host/Join
**Solution:**
- Add "Main" scene to Build Settings
- Verify scene name matches exactly in HostMenuController/JoinMenuController
- Check WOSNetworkManager "Online Scene" is set to "Main"

---

## Next Steps After Menu Setup

1. ✅ Create PlayerShip prefab with networking components
2. ✅ Setup spawn points in Main scene
3. ✅ Add scenes to Build Settings
4. ✅ Test local multiplayer
5. ✅ Deploy to Edgegap for cloud hosting

---

## File Structure

```
Assets/
├── Scenes/
│   └── MainMenu.unity (this scene)
├── Scripts/
│   └── UI/
│       ├── MenuManager.cs ✅ Created
│       ├── ConnectionMenuController.cs ✅ Created
│       ├── HostMenuController.cs ✅ Created
│       ├── JoinMenuController.cs ✅ Created
│       └── OptionsMenuController.cs ✅ Created
```

All scripts are ready! Just set up the scene hierarchy and wire up the buttons. 🎮
