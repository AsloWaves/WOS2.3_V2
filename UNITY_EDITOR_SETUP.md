# Unity Editor Setup Guide - WOS2.3 Main Menu & Networking

Complete step-by-step guide for setting up the Main Menu, Network Manager, and multiplayer server deployment.

---

## 📋 Table of Contents
1. [Create MainMenu Scene](#1-create-mainmenu-scene)
2. [Setup Network Manager in PortHarbor](#2-setup-network-manager-in-portharbor)
3. [Create Player Ship Prefab](#3-create-player-ship-prefab)
4. [Configure Build Settings](#4-configure-build-settings)
5. [Optional: Edgegap Cloud Setup](#5-optional-edgegap-cloud-setup)
6. [Testing Procedures](#6-testing-procedures)

---

## 1. Create MainMenu Scene

### Step 1.1: Create New Scene
1. **File → New Scene**
2. **Save as**: `Assets/Scenes/MainMenu.unity`
3. **Delete** default Main Camera and Directional Light

### Step 1.2: Add Canvas & Event System
1. **GameObject → UI → Canvas** (creates Canvas + EventSystem automatically)
2. **Select Canvas** in Hierarchy
3. **Configure Canvas**:
   - Render Mode: `Screen Space - Overlay`
   - Canvas Scaler: `Scale With Screen Size`
   - Reference Resolution: `1920 × 1080`
   - Screen Match Mode: `Match Width Or Height`
   - Match: `0.5`

### Step 1.3: Add Background Panel
1. **Right-click Canvas → UI → Panel**
2. **Rename**: `Background`
3. **Configure Image component**:
   - Source Image: (None) - solid color
   - Color: `#0A1929FF` (dark blue, naval theme)
4. **RectTransform**:
   - Anchor: Stretch all (full screen)
   - Left: `0`, Right: `0`, Top: `0`, Bottom: `0`

### Step 1.4: Add Title Text
1. **Right-click Canvas → UI → Text - TextMeshPro**
2. **Rename**: `TitleText`
3. **Configure TextMeshProUGUI**:
   - Text: `WOS 2.3 - World of Ships`
   - Font Size: `72`
   - Alignment: Center, Top
   - Color: White
   - Font Style: Bold
4. **RectTransform**:
   - Anchor: Top Center
   - Pos Y: `-100`
   - Width: `1200`, Height: `100`

### Step 1.5: Add Version Text
1. **Right-click Canvas → UI → Text - TextMeshPro**
2. **Rename**: `VersionText`
3. **Configure TextMeshProUGUI**:
   - Text: `v0.3.0-alpha`
   - Font Size: `18`
   - Alignment: Right, Bottom
   - Color: Gray (#CCCCCC)
4. **RectTransform**:
   - Anchor: Bottom Right
   - Pos X: `-20`, Pos Y: `20`
   - Width: `200`, Height: `30`

### Step 1.6: Add Server IP Input Field
1. **Drag prefab**: `Assets/Modern UI Pack/Prefabs/Input Field/Input Field - Standard.prefab`
2. **Parent to Canvas**
3. **Rename**: `ServerIPInput`
4. **Configure**:
   - Placeholder Text: `Enter Server IP (e.g., 127.0.0.1)`
   - Text: (empty)
5. **RectTransform**:
   - Anchor: Middle Center
   - Pos Y: `100`
   - Width: `600`, Height: `60`

### Step 1.7: Add Connect Button
1. **Drag prefab**: `Assets/Modern UI Pack/Prefabs/Button/Rounded/Blue.prefab`
2. **Parent to Canvas**
3. **Rename**: `ConnectButton`
4. **Configure Button Manager**:
   - Button Text: `Connect to Server`
   - Icon: (none)
   - Enable Text: ✓
5. **RectTransform**:
   - Anchor: Middle Center
   - Pos Y: `0`
   - Width: `400`, Height: `80`

### Step 1.8: Add Settings Button
1. **Duplicate ConnectButton** (Ctrl+D)
2. **Rename**: `SettingsButton`
3. **Configure**:
   - Button Text: `Settings`
4. **RectTransform**:
   - Pos Y: `-100`

### Step 1.9: Add Exit Button
1. **Duplicate ConnectButton** (Ctrl+D)
2. **Rename**: `ExitButton`
3. **Configure**:
   - Button Text: `Exit Game`
4. **RectTransform**:
   - Pos Y: `-200`

### Step 1.10: Add Status Text
1. **Right-click Canvas → UI → Text - TextMeshPro**
2. **Rename**: `StatusText`
3. **Configure**:
   - Text: `Ready to connect`
   - Font Size: `24`
   - Alignment: Center, Middle
   - Color: White
4. **RectTransform**:
   - Anchor: Bottom Center
   - Pos Y: `100`
   - Width: `800`, Height: `40`

### Step 1.11: Add MainMenuManager GameObject
1. **Create Empty GameObject in scene root**: `MainMenuManager`
2. **Add Component**: `Main Menu Controller` script
3. **Assign References in Inspector**:
   - **Server IP Input Field**: Drag `ServerIPInput` → find TMP_InputField component
   - **Status Text**: Drag `StatusText` TextMeshProUGUI
   - **Default Server IP**: `127.0.0.1`
   - **Client Scene**: `PortHarbor`

### Step 1.12: Hook Up Button Events
1. **Select ConnectButton**
2. **Scroll to Button Manager component → On Click event**
3. **Click `+`** to add event
4. **Drag MainMenuManager** into the object field
5. **Select Function**: `MainMenuController → ConnectToServer()`

Repeat for other buttons:
- **SettingsButton**: `MainMenuController → OpenSettings()`
- **ExitButton**: `MainMenuController → QuitGame()`

### Step 1.13: Add NetworkAddressManager (Optional but Recommended)
1. **Create Empty GameObject**: `NetworkAddressManager`
2. **Add Component**: `Network Address Manager` script
3. **Configure**:
   - Max Recent Servers: `10`
   - Default Server IP: `127.0.0.1`

### Step 1.14: Save MainMenu Scene
- **File → Save** (Ctrl+S)

---

## 2. Setup Network Manager in PortHarbor

### Step 2.1: Open PortHarbor Scene
1. **Double-click**: `Assets/Scenes/PortHarbor.unity`

### Step 2.2: Create Network Manager GameObject
1. **Create Empty GameObject in scene root**: `NetworkManager`
2. **Add Component**: `WOS Network Manager` script
3. **Add Component**: `Network Manager HUD` (for testing only)
4. **Add Component**: `Server Launcher` script

### Step 2.3: Configure WOS Network Manager
**Transport Settings**:
- Transport: `Telepathy Transport` (should auto-add)
- Port: `7777`

**Network Info**:
- Network Address: `127.0.0.1`
- Max Connections: `300`
- Server Tick Rate: `30` Hz
- Offline Scene: `MainMenu`
- Online Scene: `PortHarbor`

**WOS Naval Configuration**:
- Ocean Spawn Points: (will assign after creating spawn point)
- Spawn Method: `Specific` (for port spawning)
- Naval Send Rate: `30` Hz

**Player Prefab**: (will assign after creating prefab)

### Step 2.4: Create Spawn Point
1. **Create Empty GameObject**: `SpawnPoints` (parent container)
2. **Position**: `(0, 0, 0)` or wherever your port dock is located
3. **Create Child GameObject**: `PortSpawn_01`
4. **Position PortSpawn_01**: Inside harbor area at docking location
   - Example: `(5, 10, 0)` - adjust to your port layout
5. **Rotation**: Face outward from dock (ship should spawn facing open water)
   - Example: `(0, 0, -90)` if dock is on left side

### Step 2.5: Assign Spawn Point to Network Manager
1. **Select NetworkManager GameObject**
2. **WOS Network Manager → Ocean Spawn Points**:
   - Size: `1`
   - Element 0: **Drag PortSpawn_01** here

### Step 2.6: Configure Server Launcher
1. **Select NetworkManager GameObject**
2. **Server Launcher Component**:
   - Auto Start In Headless: ✓
   - Default Port: `7777`
   - Default Max Connections: `300`
   - Server Start Scene: `PortHarbor`
   - Verbose Logging: ✓

### Step 2.7: Save PortHarbor Scene
- **File → Save** (Ctrl+S)

---

## 3. Create Player Ship Prefab

### Step 3.1: Create Ship GameObject in Scene
1. **In PortHarbor scene**
2. **GameObject → 2D Object → Sprites → Square**
3. **Rename**: `PlayerShipPrefab`
4. **Configure**:
   - Sprite: (use ship sprite if available, or default square for testing)
   - Color: Blue (to distinguish from environment)
   - Scale: `(2, 4, 1)` to look like a ship

### Step 3.2: Add Rigidbody2D
1. **Add Component**: `Rigidbody 2D`
2. **Configure**:
   - Body Type: `Dynamic`
   - Mass: `10`
   - Linear Drag: `0.5`
   - Angular Drag: `2`
   - Gravity Scale: `0`
   - Interpolation: `Interpolate`
   - Collision Detection: `Continuous`

### Step 3.3: Add Collider
1. **Add Component**: `Box Collider 2D`
2. **Configure**:
   - Size: `(1.8, 3.8)` (slightly smaller than sprite)

### Step 3.4: Add Network Components
1. **Add Component**: `Network Identity`
   - Local Player Authority: ✓ CHECKED

2. **Add Component**: `Network Transform`
   - Sync Position: ✓
   - Sync Rotation: ✓
   - Sync Scale: ✗
   - Interpolate Position: ✓
   - Interpolate Rotation: ✓
   - Send Interval: `0.1`

3. **Add Component**: `Networked Naval Controller`

### Step 3.5: Create/Assign Ship Configuration ScriptableObject
1. **Create SO**: Right-click in Assets → `Create > WOS > Ship > Ship Configuration`
2. **Name**: `DefaultShipConfig`
3. **Configure basic values**:
   - Max Speed: `30` knots
   - Acceleration: `0.5`
   - Deceleration: `0.8`
   - Max Rudder Angle: `35`
   - Turning Radius: `200`

4. **Assign to PlayerShipPrefab**:
   - Select PlayerShipPrefab
   - Networked Naval Controller → Ship Config: **Drag DefaultShipConfig**

### Step 3.6: Add Camera Target Tag
1. **Select PlayerShipPrefab**
2. **Tag**: Create new tag `Player` if doesn't exist
3. **Assign Tag**: `Player`
4. **Layer**: Create layer `Player` if doesn't exist

### Step 3.7: Create Prefab
1. **Create folder** if doesn't exist: `Assets/Prefabs/`
2. **Drag PlayerShipPrefab** from Hierarchy → `Assets/Prefabs/` folder
3. **Prefab created!**
4. **Delete PlayerShipPrefab from scene** (we only need the prefab)

### Step 3.8: Assign Prefab to Network Manager
1. **Select NetworkManager in scene**
2. **WOS Network Manager → Spawn Info**:
   - Player Prefab: **Drag PlayerShipPrefab** from Assets/Prefabs/

### Step 3.9: Save Scene
- **File → Save**

---

## 4. Configure Build Settings

### Step 4.1: Open Build Settings
- **File → Build Settings** (Ctrl+Shift+B)

### Step 4.2: Add Scenes in Order
1. **Click "Add Open Scenes"** or drag scenes manually
2. **Scene order** (IMPORTANT - order matters!):
   - **Scene 0**: `MainMenu`
   - **Scene 1**: `PortHarbor`
   - **Scene 2**: `Main` (ocean gameplay scene)

3. **Verify order** by checking index numbers

### Step 4.3: Configure Platform
- **Platform**: `PC, Mac & Linux Standalone`
- **Target Platform**: `Windows`
- **Architecture**: `x86_64`

### Step 4.4: Configure Player Settings
1. **Click "Player Settings..."**
2. **Company Name**: Your name/studio
3. **Product Name**: `WOS2.3`
4. **Default Icon**: (optional)

### Step 4.5: Configure for Server Builds
1. **Player Settings → Other Settings**
2. **Scripting Backend**: `Mono` (or IL2CPP for production)
3. **API Compatibility Level**: `.NET Standard 2.1`

---

## 5. Optional: Edgegap Cloud Setup

### Step 5.1: Create Edgegap Account
1. Go to https://edgegap.com
2. Click "Sign Up"
3. Create free account (no credit card required)
4. Verify email

### Step 5.2: Install Docker Desktop
1. Download from https://www.docker.com/products/docker-desktop
2. Install and restart computer
3. Launch Docker Desktop
4. Ensure it's running (whale icon in system tray)

### Step 5.3: Install Linux Build Support
1. **Open Unity Hub**
2. **Installs tab**
3. **Find Unity 6000.0.55f1** → **Click gear icon** → **Add Modules**
4. **Check**: `Linux Build Support (Mono)` AND `Linux Build Support (IL2CPP)`
5. **Install**

### Step 5.4: Configure Edgegap Plugin (If Available)
1. **Tools → Edgegap → Hosting Plugin** (if menu exists)
2. **API Token**: Paste from Edgegap dashboard
3. **Application Name**: `WOS2.3_Server`
4. **Version**: `v0.3.0`

### Step 5.5: Add EdgegapDeployHelper to MainMenu
1. **Open MainMenu scene**
2. **Create Empty GameObject**: `EdgegapDeployHelper`
3. **Add Component**: `Edgegap Deploy Helper` script
4. **Configure**:
   - Enable Edgegap Deploy: ✓
   - Application Name: `WOS2.3_Server`
   - Version Tag: `v0.3.0`

5. **Add "Quick Deploy" Button** (optional):
   - Duplicate ConnectButton
   - Rename: `EdgegapDeployButton`
   - Text: `Quick Deploy to Cloud`
   - Pos Y: `200`
   - On Click: `EdgegapDeployHelper → DeployServer()`

---

## 6. Testing Procedures

### Test 1: Local Host (Simplest)
1. **Play in Editor**
2. **MainMenu appears**
3. **Click "Connect to Server"** with `127.0.0.1`
4. **Should fail** (no server running yet)
5. **Click "Host Local Server"** button (if you added it)
6. **Ship spawns in PortHarbor**

### Test 2: Dedicated Server + Client
#### Build Server:
1. **File → Build Settings**
2. **Build** (not Build and Run)
3. **Name**: `WOS2.3_Server.exe`
4. **Save to**: `Builds/Server/`
5. **Launch server.exe**
6. **Console should show**: "Server started" message

#### Connect Client:
1. **Play in Unity Editor**
2. **Enter**: `127.0.0.1` in server IP
3. **Click "Connect to Server"**
4. **Ship spawns in PortHarbor**
5. **You can move ship**

### Test 3: Multi-Client with ParrelSync
1. **Install ParrelSync** from Package Manager
2. **ParrelSync → Create Clone**
3. **Run server build** (from Test 2)
4. **Unity Editor → Play** (Client 1)
5. **ParrelSync Clone → Play** (Client 2)
6. **Both clients connect to** `127.0.0.1`
7. **Verify both players see each other**

### Test 4: Edgegap Cloud Deploy
1. **Ensure Docker is running**
2. **Open MainMenu scene in Editor**
3. **Click "Quick Deploy to Cloud"** (if button added)
4. **Wait 2-5 minutes** for deployment
5. **Server IP will auto-fill**
6. **Click "Connect to Server"**
7. **Ship spawns in PortHarbor**

---

## Common Issues & Solutions

### Issue: "NetworkManager not found"
**Solution**: Ensure NetworkManager is in the PortHarbor scene, not MainMenu.

### Issue: "Player prefab missing NetworkIdentity"
**Solution**: Add NetworkIdentity component to player prefab, check "Local Player Authority".

### Issue: Ship spawns at (0,0,0) instead of spawn point
**Solution**: Verify spawn point is assigned in NetworkManager → Ocean Spawn Points.

### Issue: Input doesn't work on ship
**Solution**: Verify Input System actions asset is in Resources folder.

### Issue: Mirror errors in console
**Solution**: Update Mirror to latest version via Package Manager.

### Issue: Edgegap deployment fails
**Solution**:
- Verify Docker Desktop is running
- Check Linux Build Support is installed
- Verify Edgegap API credentials

---

## Next Steps
1. ✅ Complete this setup guide
2. Test local multiplayer (Test 1-3)
3. Test Edgegap deployment (Test 4)
4. Create ship art assets
5. Add more spawn points for balanced player distribution
6. Implement port → ocean scene transition
7. Add player name/team selection
8. Implement proper camera follow for networked ships

---

**Last Updated**: January 2025
**Unity Version**: 6000.0.55f1
**Mirror Version**: Latest from Package Manager
