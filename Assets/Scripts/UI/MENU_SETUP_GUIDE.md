# Menu System Setup Guide

This guide explains how to set up the WOS2.3 menu system with your MUIP layout.

## Architecture Overview

```
MenuManager (Singleton)
├── MainMenu Panel (Start, Options, Exit)
├── ConnectionMenu Panel (Host, Join Server, Back)
│   └── JoinServerPanel (IP Input, Connect, Back)
└── OptionsMenu Panel (Coming Soon)
```

## Scripts Created

1. **MenuManager.cs** - Handles panel switching and menu navigation
2. **ConnectionMenuController.cs** - Handles Host/Join functionality
3. **OptionsMenuController.cs** - Placeholder for options (future)

## Unity Scene Setup (MainMenu.unity)

### Step 1: Create Menu Structure

```
MainMenu Scene
├── Canvas (Screen Space - Overlay)
│   ├── MenuManager (Empty GameObject)
│   ├── Panel_MainMenu (MUIP Panel)
│   │   ├── Button_Start (MUIP Button)
│   │   ├── Button_Options (MUIP Button)
│   │   └── Button_Exit (MUIP Button)
│   │
│   ├── Panel_ConnectionMenu (MUIP Panel)
│   │   ├── Panel_MainConnection (MUIP Panel)
│   │   │   ├── Text_Title "Connect to Game" (TextMeshPro)
│   │   │   ├── Button_Host (MUIP Button)
│   │   │   ├── Button_Join (MUIP Button)
│   │   │   ├── Button_Back (MUIP Button)
│   │   │   └── Text_Status (TextMeshProUGUI)
│   │   │
│   │   └── Panel_JoinServer (MUIP Panel)
│   │       ├── Text_Title "Enter Server IP" (TextMeshPro)
│   │       ├── InputField_ServerIP (TMP_InputField)
│   │       ├── Button_Connect (MUIP Button)
│   │       └── Button_Back (MUIP Button)
│   │
│   └── Panel_OptionsMenu (MUIP Panel)
│       ├── Text_Title "Options" (TextMeshPro)
│       ├── Text_ComingSoon "Coming Soon!" (TextMeshPro)
│       └── Button_Back (MUIP Button)
│
└── EventSystem
```

### Step 2: Add Components

**MenuManager GameObject:**
- Add Component → `MenuManager.cs`
- Assign references in Inspector:
  - Main Menu Panel → `Panel_MainMenu`
  - Connection Menu Panel → `Panel_ConnectionMenu`
  - Options Menu Panel → `Panel_OptionsMenu`
  - Starting Panel → `MainMenu`

**Panel_ConnectionMenu GameObject:**
- Add Component → `ConnectionMenuController.cs`
- Assign references in Inspector:
  - Server IP Input Field → `InputField_ServerIP`
  - Status Text → `Text_Status`
  - Main Connection Panel → `Panel_MainConnection`
  - Join Server Panel → `Panel_JoinServer`
  - Game Scene Name → `"Main"`

**Panel_OptionsMenu GameObject:**
- Add Component → `OptionsMenuController.cs`
- Assign references as needed (optional for now)

### Step 3: Wire Up Button OnClick() Events

**Panel_MainMenu:**
- `Button_Start` → `MenuManager.OnStartButtonClicked()`
- `Button_Options` → `MenuManager.OnOptionsButtonClicked()`
- `Button_Exit` → `MenuManager.OnExitButtonClicked()`

**Panel_MainConnection:**
- `Button_Host` → `ConnectionMenuController.OnHostButtonClicked()`
- `Button_Join` → `ConnectionMenuController.OnJoinButtonClicked()`
- `Button_Back` → `ConnectionMenuController.OnBackToMainMenu()`

**Panel_JoinServer:**
- `Button_Connect` → `ConnectionMenuController.OnConnectButtonClicked()`
- `Button_Back` → `ConnectionMenuController.OnBackToConnectionMenu()`

**Panel_OptionsMenu:**
- `Button_Back` → `OptionsMenuController.OnBackButtonClicked()`

### Step 4: Add NetworkManager to Scene

Create a GameObject called "NetworkManager" in the scene:

1. Add Component → `WOSNetworkManager`
2. Configure in Inspector:
   - Network Address: `localhost`
   - Max Connections: `10`
   - Offline Scene: `MainMenu`
   - Online Scene: `Main`
   - Don't Destroy On Load: ✅ **checked**

3. Add Component → `TelepathyTransport` (if not auto-added)
   - Port: `7777`

4. Add Component → `NetworkAddressManager`

5. Add Component → `ServerLauncher` (optional, for headless builds)
   - Auto Start In Headless: ✅ **checked**
   - Default Port: `7777`
   - Default Max Connections: `10`
   - Server Start Scene: `Main`

## Testing in Unity Editor

### Test 1: Menu Navigation
1. Play the scene
2. Click "Start" → Should show Connection Menu
3. Click "Back" → Should return to Main Menu
4. Click "Options" → Should show Options Menu
5. Click "Exit" → Should stop Play Mode (Editor) or quit (Build)

### Test 2: Host Mode
1. Play the scene
2. Click "Start" → Click "Host"
3. Should see "Host started! Loading game..."
4. Should load "Main" scene with your ship spawned

### Test 3: Join Mode (2 Unity Instances)
1. Build the game (`File > Build and Run`)
2. In Unity Editor: Click "Start" → Click "Host"
3. In Build: Click "Start" → Click "Join Server"
4. Enter IP: `127.0.0.1` → Click "Connect"
5. Both should see each other's ships in Main scene

## MUIP Customization Tips

### Button Styling
- Use MUIP Button Manager component for animations
- Configure button colors in MUIP Button component:
  - Normal Color: Your theme color
  - Highlighted Color: Lighter variant
  - Pressed Color: Darker variant

### Panel Transitions
- Add MUIP Panel Manager for fade in/out animations
- Configure transition speed in Panel Manager

### Input Field Styling
- Use MUIP Input Field component
- Set placeholder text: "Enter server IP..."
- Configure colors to match theme

### Text Styling
- Use TextMeshPro for all text elements
- Main Title: 48-72pt, Bold
- Button Text: 24-32pt, Medium
- Status Text: 18-24pt, Regular

## Network Flow Diagram

```
User Clicks "Start"
    ↓
Connection Menu Appears
    ↓
User Selects "Host" OR "Join Server"
    ↓                           ↓
Host Mode                  Join Mode
    ↓                           ↓
Start Local Server         Show IP Input Panel
    ↓                           ↓
Load "Main" Scene          User Enters IP → Connect
    ↓                           ↓
Spawn Host Ship            Load "Main" Scene
                                ↓
                           Spawn Client Ship
```

## Next Steps

After menu setup:
1. ✅ Create PlayerShip prefab (see PLAYER_PREFAB_GUIDE.md)
2. ✅ Setup spawn points in Main scene
3. ✅ Configure Build Settings
4. ✅ Test local multiplayer
5. ✅ Deploy to Edgegap for cloud hosting

## Troubleshooting

### Issue: Buttons don't respond
- Check EventSystem exists in scene
- Verify button OnClick() events are assigned
- Ensure MenuManager/ConnectionMenuController scripts are attached

### Issue: "NetworkManager not found" error
- Add NetworkManager GameObject to MainMenu scene
- Assign WOSNetworkManager component
- Enable "Don't Destroy On Load"

### Issue: Scene doesn't load after Host/Join
- Check "Main" scene is added to Build Settings
- Verify scene name in ConnectionMenuController matches exactly
- Ensure WOSNetworkManager has correct Online Scene set

### Issue: Can't connect to server
- Verify server IP is correct (use `127.0.0.1` for local testing)
- Check firewall allows port 7777 (TCP + UDP)
- Ensure server is running (Host mode started first)

## Support

For additional help, check:
- Mirror Documentation: https://mirror-networking.gitbook.io/
- MUIP Documentation: Modern UI Pack asset documentation
- Unity Forums: Search for "Mirror networking"
