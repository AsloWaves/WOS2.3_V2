# Mirror Networking Setup Guide for WOS2.3

## ✅ Completed (Code)
- ✅ WOSNetworkManager created
- ✅ NetworkedNavalController created
- ✅ SimpleCameraController updated for multiplayer
- ✅ All networking scripts ready

## 🎯 Unity Editor Setup (Do This Now)

### **Step 1: Configure PlayerShip Prefab**

1. **Open PlayerShip Prefab**
   - Navigate to `Assets/Prefabs/Player/PlayerShip.prefab`
   - Double-click to open in Prefab mode

2. **Add Network Identity**
   - Click "Add Component"
   - Search for "Network Identity"
   - Add it to the prefab root
   - ✅ **CRITICAL:** Check "Local Player Authority" checkbox
     - This allows client-side prediction for smooth movement

3. **Add Network Transform**
   - Click "Add Component"
   - Search for "Network Transform"
   - Configure settings:
     - **Sync Direction:** Client To Server
     - **Sync Position:** ✅ Enabled
     - **Sync Rotation:** ✅ Enabled (2D rotation only)
     - **Sync Scale:** ❌ Disabled
     - **Interpolate Position:** ✅ Enabled (smooth movement)
     - **Interpolate Rotation:** ✅ Enabled
     - **Send Rate:** 30 (matches WOSNetworkManager)

4. **Replace Ship Controller**
   - **Remove** the old `SimpleNavalController` component
   - Click "Add Component"
   - Search for "Networked Naval Controller"
   - Add the new `NetworkedNavalController`
   - Assign same ShipConfigurationSO as before
   - Configure all serialized fields (waypoint prefab, line renderer, etc.)

5. **Move InputSystem_Actions to Resources**
   - Find `InputSystem_Actions.inputactions` file
   - Create folder: `Assets/Resources/` (if doesn't exist)
   - **Move** (not copy) InputSystem_Actions into Resources folder
   - This allows NetworkedNavalController to load it dynamically

6. **Remove PlayerInput Component**
   - The NetworkedNavalController will add this dynamically for local player only
   - Remove any existing PlayerInput component from the prefab

7. **Save Prefab**
   - File → Save or Ctrl+S
   - Exit Prefab mode

---

### **Step 2: Set Up Main Scene**

1. **Create NetworkManager GameObject**
   - In Main scene hierarchy, right-click → Create Empty
   - Rename to "NetworkManager"
   - Position at (0, 0, 0)

2. **Add WOS Network Manager Component**
   - Select NetworkManager GameObject
   - Click "Add Component"
   - Search for "WOS Network Manager"
   - Add it

3. **Configure WOS Network Manager**

   **Configuration Section:**
   - ✅ Don't Destroy On Load: Checked
   - ✅ Run In Background: Checked

   **Auto-Start Options:**
   - Headless Start Mode: Do Nothing (change to Auto Start Server for dedicated server)
   - Editor Auto Start: Unchecked (manual start for testing)

   **Network Settings:**
   - Send Rate: 30 (good for naval game)
   - Network Address: "localhost" (for local testing)
   - Max Connections: 100

   **Scene Management:**
   - Offline Scene: Leave empty (or set to main menu if you have one)
   - Online Scene: "Main" (your ocean scene name)

   **Player Object:**
   - Player Prefab: Drag `Assets/Prefabs/Player/PlayerShip.prefab` here

   **WOS Naval Configuration:**
   - Ocean Spawn Points: (see Step 3 below)
   - Spawn Method: Random (or Round Robin)
   - Naval Send Rate: 30

4. **Add Transport Component**
   - Select NetworkManager GameObject
   - Click "Add Component"
   - Search for "KCP Transport" (recommended for games)
   - **Alternative:** Use "Telepathy Transport" (simpler, TCP-based)

   **KCP Transport Settings (Recommended):**
   - Port: 7777 (default)
   - Dual Mode: ✅ Enabled (allows IPv4 and IPv6)
   - No Delay: ✅ Enabled (reduces latency)
   - Interval: 10 (default)
   - Fast Resend: 2 (for reliability)
   - Max Retransmits: 10 (default)

5. **Assign Transport to Network Manager**
   - Select NetworkManager GameObject
   - Find "Transport" field in WOSNetworkManager component
   - Drag the KCP Transport (or Telepathy) component into this field

6. **Add Network Manager HUD (Optional - for testing)**
   - Select NetworkManager GameObject
   - Click "Add Component"
   - Search for "Network Manager HUD"
   - This adds an on-screen UI to start/stop server/client
   - Remove this in production builds

---

### **Step 3: Create Spawn Points**

1. **Create Spawn Points Container**
   - Right-click in hierarchy → Create Empty
   - Rename to "OceanSpawnPoints"
   - Position at (0, 0, 0)

2. **Add Individual Spawn Points**
   - Right-click OceanSpawnPoints → Create Empty
   - Rename to "SpawnPoint_1"
   - Position somewhere in ocean (e.g., -100, 0, 0)
   - Rotation: (0, 0, 0)

   - Create 3-5 more spawn points at different locations:
     - SpawnPoint_2: (100, 0, 0)
     - SpawnPoint_3: (0, 100, 0)
     - SpawnPoint_4: (0, -100, 0)
     - SpawnPoint_5: (-100, -100, 0)

3. **Assign Spawn Points to Network Manager**
   - Select NetworkManager GameObject
   - Find "Ocean Spawn Points" array in WOS Network Manager
   - Set Size to 5 (or however many you created)
   - Drag each SpawnPoint GameObject into the array slots

---

### **Step 4: Update ScenePortManager (If Using)**

If you're using the ScenePortManager for port returns:

1. **Find ScenePortManager** in Main scene
2. **Update Player Ship Prefab field**
   - Assign the SAME PlayerShip prefab (now networked)
3. **Leave everything else as-is**
   - ScenePortManager will work with Mirror automatically

---

### **Step 5: Configure Camera**

1. **Main Camera GameObject**
   - Should have SimpleCameraController component
   - No changes needed - it auto-finds local player now

2. **If Camera Doesn't Find Player:**
   - The camera waits 0.5 seconds for network spawn
   - If still issues, manually assign target after play

---

## 🧪 Testing Your Setup

### **Test 1: Host Mode (Single Player)**

1. **Start Unity Editor**
   - Press Play
   - You should see the NetworkManagerHUD in top-left corner

2. **Click "Host" Button**
   - This starts both server and client
   - Your PlayerShip should spawn at a random spawn point
   - Camera should follow your ship
   - Controls should work (WASD for throttle, AD for steering)

3. **Verify:**
   - ✅ Ship spawns
   - ✅ Ship responds to input
   - ✅ Camera follows smoothly
   - ✅ No console errors

### **Test 2: Multiplayer (2 Clients)**

1. **Build the Game**
   - File → Build Settings
   - Add "Main" scene to build
   - Click "Build And Run"
   - This creates a standalone executable

2. **Start Host in Unity Editor**
   - Press Play in Unity
   - Click "Host" button
   - Your ship spawns

3. **Start Client in Build**
   - Run the built executable
   - Click "Client" button
   - Second ship should spawn at different location

4. **Verify:**
   - ✅ Both ships visible
   - ✅ Each player controls their own ship
   - ✅ Ships move smoothly (no jitter)
   - ✅ Each camera follows their own ship only
   - ✅ No console errors

### **Expected Behavior:**
- You should see the other player's ship moving in real-time
- Your camera should ONLY follow YOUR ship
- Movement should be smooth (NetworkTransform interpolation)
- Controls should feel responsive (client-side prediction)

---

## 🐛 Troubleshooting

### **"Player doesn't spawn"**
- ✅ Check PlayerShip prefab has NetworkIdentity
- ✅ Check NetworkManager has Player Prefab assigned
- ✅ Check Transport component is assigned
- ✅ Check spawn points are set up

### **"Ship spawns but doesn't respond to input"**
- ✅ Check NetworkIdentity has "Local Player Authority" checked
- ✅ Check InputSystem_Actions is in Resources folder
- ✅ Check NetworkedNavalController has ShipConfigurationSO assigned

### **"Camera doesn't follow my ship"**
- ✅ Wait 1 second after spawn (camera searches for local player)
- ✅ Check console for "Camera assigned to LOCAL player" message
- ✅ Verify NetworkIdentity.isLocalPlayer is true for your ship

### **"I see other player's ship but it's jittery"**
- ✅ Check NetworkTransform has Interpolation enabled
- ✅ Check Send Rate is 30 or higher
- ✅ Check network latency (LAN should be < 50ms)

### **"Controls feel laggy"**
- ✅ Verify "Local Player Authority" is checked
- ✅ Check Rigidbody2D Interpolation is enabled
- ✅ Lower Send Rate if CPU-bound

---

## 📋 Quick Checklist

Before testing, verify:

**PlayerShip Prefab:**
- [ ] Has NetworkIdentity component
- [ ] NetworkIdentity → Local Player Authority ✅ CHECKED
- [ ] Has NetworkTransform component
- [ ] NetworkTransform → Send Rate: 30
- [ ] NetworkTransform → Interpolate Position: ✅
- [ ] NetworkTransform → Interpolate Rotation: ✅
- [ ] Has NetworkedNavalController (not SimpleNavalController)
- [ ] NetworkedNavalController → ShipConfigurationSO assigned
- [ ] PlayerInput component removed from prefab
- [ ] InputSystem_Actions moved to Resources folder

**Main Scene - NetworkManager:**
- [ ] Has WOSNetworkManager component
- [ ] Has Transport (KCP or Telepathy)
- [ ] Transport assigned in WOSNetworkManager
- [ ] Player Prefab field = PlayerShip prefab
- [ ] Ocean Spawn Points array filled (3-5 points)
- [ ] Send Rate = 30

**Main Scene - Camera:**
- [ ] Has SimpleCameraController
- [ ] Target can be empty (auto-finds)

**Build Settings:**
- [ ] Main scene added to build
- [ ] Development Build ✅ (for testing)

---

## 🎮 Next Steps After Setup

Once multiplayer is working:

1. **Port System Integration** (Tasks 6-7)
   - Update port entry/exit for network spawning
   - Handle scene transitions in multiplayer

2. **Inventory System** (Phase 3)
   - Build with NetworkBehaviour from start
   - Use SyncList for item collections
   - Use [Command] for item operations

3. **Trading System**
   - Server-authoritative trading
   - Atomic transactions
   - Validation

---

## 🔍 Mirror Documentation References

- **NetworkManager:** https://mirror-networking.gitbook.io/docs/components/network-manager
- **NetworkIdentity:** https://mirror-networking.gitbook.io/docs/components/network-identity
- **NetworkTransform:** https://mirror-networking.gitbook.io/docs/components/network-transform
- **NetworkBehaviour:** https://mirror-networking.gitbook.io/docs/guides/networkbehaviour
- **Commands & ClientRpc:** https://mirror-networking.gitbook.io/docs/guides/communications/remote-actions
- **SyncVars:** https://mirror-networking.gitbook.io/docs/guides/synchronization/syncvars

---

## ✅ Status Check

After completing these steps, you should have:
- ✅ Fully networked player ships
- ✅ Multiplayer-ready controls
- ✅ Smooth network synchronization
- ✅ Camera following local player only
- ✅ **Port system fully networked** (see PORT_NETWORKING_CHANGES.md)
- ✅ Foundation for inventory/trading systems

**All Mirror integration complete! Ready for multiplayer testing and Phase 3 (Inventory)!**
