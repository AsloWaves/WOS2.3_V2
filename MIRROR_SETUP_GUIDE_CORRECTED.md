# Mirror Networking Setup Guide for WOS2.3 (CORRECTED)

## ⚠️ Corrections from Original Guide

This guide has been verified against the **latest Mirror GitHub repository** (supports Unity 2019-2022 and Unity 6).

### Key Corrections:
1. ❌ **REMOVED:** "Local Player Authority" checkbox - Does NOT exist in current Mirror NetworkIdentity
2. ✅ **CLARIFIED:** NetworkTransform variants (Unreliable, Reliable, Hybrid)
3. ✅ **CORRECTED:** Send rate configuration terminology
4. ✅ **VERIFIED:** All other APIs are current (OnServerAddPlayer, isLocalPlayer, isOwned, SyncVars, Commands)

---

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
   - ⚠️ **NOTE:** There is NO "Local Player Authority" checkbox in current Mirror
     - Client authority is controlled by NetworkTransform's `syncDirection` instead

3. **Add Network Transform (Unreliable)**
   - Click "Add Component"
   - Search for "**Network Transform Unreliable**"
     - ℹ️ Unreliable is recommended for player movement (faster, UDP-based)
     - Alternative: "Network Transform Reliable" for critical objects (TCP-like)
   - Configure settings:
     - **Sync Direction:** Client To Server
       - This gives client authority for responsive controls
     - **Sync Position:** ✅ Enabled
     - **Sync Rotation:** ✅ Enabled
     - **Sync Scale:** ❌ Disabled
     - **Interpolate Position:** ✅ Enabled (smooth movement)
     - **Interpolate Rotation:** ✅ Enabled
     - **Interpolate Scale:** ❌ Disabled
     - **Only Sync On Change:** ✅ Enabled (bandwidth optimization)
     - **Compress Rotation:** ✅ Enabled (saves bandwidth)
     - **Send Interval Multiplier:** 1 (uses global NetworkManager send rate)
     - **Coordinate Space:** Local (default)

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
   - **Send Rate:** 30 (Hz - global update frequency for all SyncVars)
     - ℹ️ NetworkTransform uses this rate by default (sendIntervalMultiplier = 1)
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
   - Naval Send Rate: 30 (should match Send Rate above)

4. **Add Transport Component**
   - With NetworkManager selected, click "Add Component"
   - Search for "KCP Transport"
   - Add it
   - ✅ **RECOMMENDED:** KCP Transport (UDP-based, reliable and fast)
   - ℹ️ Alternative: Telepathy Transport (TCP-based, more reliable but slower)

   **KCP Transport Settings:**
   - Port: 7777 (default)
   - Dual Mode: ✅ Enabled (supports IPv4 and IPv6)
   - No Delay: ✅ Enabled (reduces latency)
   - Max Retransmit: 30 (good default)

5. **Save Scene**
   - File → Save or Ctrl+S

---

### **Step 3: Create Ocean Spawn Points**

1. **Create Spawn Points Container**
   - In Main scene hierarchy, right-click → Create Empty
   - Rename to "OceanSpawnPoints"
   - Position at (0, 0, 0)

2. **Create Individual Spawn Points**
   - Right-click OceanSpawnPoints → Create Empty
   - Rename to "SpawnPoint_1"
   - Position at your desired ocean location (e.g., 100, 0, 0)
   - Repeat 2-4 more times at different ocean locations
   - Example positions:
     - SpawnPoint_1: (100, 0, 0)
     - SpawnPoint_2: (-100, 0, 0)
     - SpawnPoint_3: (0, 100, 0)
     - SpawnPoint_4: (0, -100, 0)
     - SpawnPoint_5: (100, 100, 0)

3. **Assign to NetworkManager**
   - Select NetworkManager GameObject
   - In WOS Network Manager component
   - Expand "Ocean Spawn Points" array
   - Set Size to 5 (or however many you created)
   - Drag each SpawnPoint GameObject into the array slots

4. **Save Scene**

---

### **Step 4: Testing**

#### **Host Mode Test (Single Player)**

1. **Enter Play Mode**
   - Press Play button in Unity Editor

2. **Start Host**
   - Open the NetworkManager HUD (should appear automatically)
   - Click "Host (Server + Client)" button

3. **Verify:**
   - ✅ Player ship spawns at one of your spawn points
   - ✅ Camera follows the ship
   - ✅ Ship controls work (WASD for throttle/steering)
   - ✅ Console shows: "🚢 Player returning from port - using port exit position" if returning from port

4. **Test Port System:**
   - Sail to port (blue circle)
   - Press E to enter harbor
   - Verify scene transition
   - Exit harbor (if implemented)
   - Verify return to correct position facing away from port

#### **Multiplayer Test (2 Clients)**

**Requires building an executable:**

1. **Build Settings**
   - File → Build Settings
   - Add "Main" scene to build
   - Add "PortHarbor" scene to build (if using)
   - Platform: Windows/Mac/Linux
   - Click "Build" and save executable

2. **Run Host in Editor:**
   - Unity Editor Play Mode
   - Click "Host (Server + Client)"

3. **Run Client in Build:**
   - Launch the built executable
   - Click "Client" button
   - Enter "localhost" as address
   - Click "Connect"

4. **Verify Multiplayer:**
   - ✅ Both players see each other's ships
   - ✅ Both ships move independently
   - ✅ Camera follows only local player
   - ✅ Each player can enter ports independently
   - ✅ Other player is unaffected by port entry
   - ✅ No ghost ships or duplicate spawns
   - ✅ Smooth movement synchronization

---

## 🔍 Troubleshooting

### **Player Not Spawning**
- ❌ Check NetworkManager has Player Prefab assigned
- ❌ Verify prefab has NetworkIdentity component
- ❌ Check Ocean Spawn Points array is populated
- ❌ Look for errors in Console

### **Controls Not Working**
- ❌ Verify InputSystem_Actions is in Resources folder
- ❌ Check NetworkedNavalController is attached (not SimpleNavalController)
- ❌ Verify Ship Configuration SO is assigned
- ❌ Check Console for "Input system re-initialized" message

### **Camera Not Following**
- ❌ Verify SimpleCameraController is in scene
- ❌ Check Console for "🎥 Camera assigned to LOCAL player" message
- ❌ Verify NetworkIdentity.isLocalPlayer is working (check logs)

### **Multiplayer Desyncing**
- ❌ Verify NetworkTransform Unreliable is added
- ❌ Check Sync Direction is "Client To Server"
- ❌ Verify Send Rate matches between NetworkManager and WOSNetworkManager
- ❌ Check network latency (ping)
- ❌ Consider using NetworkTransformReliable for testing

### **Port System Not Working**
- ❌ Verify SimplePortTest inherits from NetworkBehaviour
- ❌ Check local player detection (see Console logs)
- ❌ Verify ScenePortManager finds local networked player
- ❌ Check PlayerPrefs port data is saved/loaded

---

## 📊 Mirror API Verification

**Verified against Mirror GitHub repository (latest):**

✅ **NetworkManager.OnServerAddPlayer(NetworkConnectionToClient conn)** - Current API
✅ **NetworkIdentity.isLocalPlayer** - Recommended for local player checks
✅ **NetworkBehaviour.isOwned** - Current authority check
✅ **[SyncVar]** - Current synchronization attribute
✅ **[Command]** - Current client-to-server RPC
✅ **[ClientRpc]** - Current server-to-client RPC
✅ **NetworkTransform variants** - Unreliable, Reliable, Hybrid available
✅ **syncDirection** - ClientToServer or ServerToClient
✅ **NetworkServer.AddPlayerForConnection()** - Current spawn API

❌ **OUTDATED:** "Local Player Authority" checkbox (does not exist in current Mirror)
❌ **OUTDATED:** Single "NetworkTransform" component (now has variants)

---

## 📚 Official Documentation

- **Mirror Documentation:** https://mirror-networking.gitbook.io/docs/
- **GitHub Repository:** https://github.com/MirrorNetworking/Mirror
- **NetworkManager:** https://mirror-networking.gitbook.io/docs/components/network-manager
- **NetworkTransform:** https://mirror-networking.gitbook.io/docs/components/network-transform
- **SyncVars:** https://mirror-networking.gitbook.io/docs/guides/synchronization/syncvars

---

## ✅ Status Check

After completing these steps, you should have:
- ✅ Fully networked player ships with client-side prediction
- ✅ Multiplayer-ready controls with Commands
- ✅ Smooth network synchronization via NetworkTransform Unreliable
- ✅ Camera following local player only (NetworkIdentity.isLocalPlayer)
- ✅ Port system fully networked (see PORT_NETWORKING_CHANGES.md)
- ✅ Foundation for inventory/trading systems with NetworkBehaviour

**All Mirror integration complete! APIs verified against latest GitHub repository!**
**Ready for multiplayer testing and Phase 3 (Inventory)!**
