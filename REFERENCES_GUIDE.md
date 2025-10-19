# NetworkManager References Guide

**Your Question**: "Where should spawn points and prefab references go if NetworkManager is in MainMenu scene?"

**Answer**: Different references go in different places! Here's the complete breakdown:

---

## 📋 Reference Assignment Summary

| Reference | Where to Assign | Why |
|-----------|----------------|-----|
| **Player Prefab** | NetworkManager Inspector (MainMenu scene) ✅ | Prefab is an asset, not a scene object - cross-scene OK! |
| **Spawn Points** | Automatically discovered at runtime ✅ | Scene objects - can't assign cross-scene, so auto-discovery! |

---

## ✅ Player Prefab Assignment (Manual - In Inspector)

### Where It Goes

**NetworkManager GameObject in MainMenu.unity**:
```
MainMenu.unity
└── NetworkManager (or MenuManagers/NetworkManager)
    └── WOSNetworkManager component
        └── Player Prefab field ← ASSIGN HERE!
```

### How to Assign

1. **Open MainMenu.unity scene**
2. **Select NetworkManager GameObject** (in Hierarchy)
3. **Inspector → WOSNetworkManager component**
4. **Find "Player Prefab" field**
5. **Drag PlayerShip.prefab** from Assets/Prefabs folder → Player Prefab field
6. **Save scene**

### Why This Works

**Prefabs are ASSETS, not scene objects**:
- Stored in Assets/Prefabs/ folder
- Not tied to any specific scene
- Can be assigned from any scene
- Unity allows cross-scene asset references

**This is CORRECT and INTENDED behavior!**

---

## ✅ Spawn Points (Automatic - Runtime Discovery)

### Where They Are

**Main.unity scene** (or any gameplay scene):
```
Main.unity
└── SpawnPoints (parent GameObject)
    ├── SpawnPoint_01 (tag: SpawnPoint)
    ├── SpawnPoint_02 (tag: SpawnPoint)
    └── ... etc
```

### How to Setup

**You DON'T assign spawn points in Inspector!**

Instead:
1. **Create spawn points in Main scene** (see SPAWN_POINTS_SETUP.md)
2. **Tag them with "SpawnPoint"** OR make children of "SpawnPoints" parent
3. **WOSNetworkManager finds them automatically** when scene loads

### Why Manual Assignment Doesn't Work

**Spawn points are SCENE OBJECTS, not assets**:
- They exist in Main.unity scene
- NetworkManager is in MainMenu.unity scene
- Unity does NOT allow dragging GameObjects from one scene into another scene's component
- Attempting this will fail silently (reference stays empty)

**Solution**: Automatic discovery at runtime ✅

### How Automatic Discovery Works

**When Main scene loads**:
1. Mirror calls `OnServerSceneChanged("Main")`
2. WOSNetworkManager executes `FindSpawnPointsInScene()`
3. Searches for GameObjects with tag "SpawnPoint"
4. Populates `oceanSpawnPoints[]` array automatically
5. Logs: "🎯 Found 5 spawn points in scene"

**You see this in console**:
```
🏝️ Server changing scene to: Main
✅ Server scene loaded: Main
🎯 Found 5 spawn points in scene
```

---

## 🎯 Complete Setup Instructions

### Step 1: Setup NetworkManager (MainMenu Scene)

**File**: MainMenu.unity

1. **Select NetworkManager GameObject**
2. **WOSNetworkManager component**:
   - Offline Scene: MainMenu
   - Online Scene: Main
   - Don't Destroy On Load: ✅ **CHECKED**
   - Player Prefab: **Drag PlayerShip.prefab here** ← MANUAL ASSIGNMENT
   - Naval Send Rate: 30
   - Spawn Method: RoundRobin

3. **Leave "Ocean Spawn Points" array EMPTY**:
   - Size: 0 (or leave default)
   - Don't assign anything here
   - It will be populated automatically at runtime

4. **Save scene**

---

### Step 2: Create Spawn Points (Main Scene)

**File**: Main.unity

1. **Create "SpawnPoint" tag**:
   - Edit → Project Settings → Tags & Layers
   - Add tag: "SpawnPoint"

2. **Create spawn point GameObjects**:
   ```
   Create Empty → Name: SpawnPoint_01 → Tag: SpawnPoint → Position: (-50, 0, 0)
   Create Empty → Name: SpawnPoint_02 → Tag: SpawnPoint → Position: (50, 0, 0)
   Create Empty → Name: SpawnPoint_03 → Tag: SpawnPoint → Position: (0, 50, 0)
   Create Empty → Name: SpawnPoint_04 → Tag: SpawnPoint → Position: (0, -50, 0)
   Create Empty → Name: SpawnPoint_05 → Tag: SpawnPoint → Position: (0, 0, 0)
   ```

3. **Save scene**

4. **DO NOT try to assign these to NetworkManager!** They will be found automatically.

---

### Step 3: Create Player Prefab (Asset)

**File**: Assets/Prefabs/PlayerShip.prefab

1. **Create prefab** with these components:
   - NetworkIdentity (Client Authority: ✅)
   - NetworkTransformReliable (Client Authority: ✅)
   - NetworkedNavalController
   - Rigidbody2D
   - CircleCollider2D
   - SpriteRenderer

2. **Save prefab**

3. **Assign to NetworkManager**:
   - Open MainMenu.unity
   - NetworkManager → Player Prefab = PlayerShip.prefab

---

## 🔍 Verification Checklist

### MainMenu Scene Check

- [ ] NetworkManager GameObject exists
- [ ] WOSNetworkManager component attached
- [ ] Player Prefab field = PlayerShip.prefab (from Assets/Prefabs)
- [ ] Ocean Spawn Points array = EMPTY (size 0 or not assigned)
- [ ] Don't Destroy On Load = ✅ CHECKED

### Main Scene Check

- [ ] NO NetworkManager GameObject (should be deleted)
- [ ] SpawnPoints parent GameObject exists (or individual spawn points with tag)
- [ ] 5+ spawn point children (each tagged "SpawnPoint")
- [ ] Spawn points positioned at different locations
- [ ] Scene saved

### Runtime Check (After Hosting)

- [ ] Console shows: "🎯 Found 5 spawn points in scene"
- [ ] Player spawns at spawn point position (NOT 0,0,0)
- [ ] No "Multiple NetworkManagers" warning
- [ ] Ship moves correctly

---

## ❌ Common Mistakes

### Mistake 1: Trying to Assign Spawn Points in Inspector
```
❌ Dragging spawn points from Main scene → NetworkManager in MainMenu scene
   Result: Reference stays empty (cross-scene not allowed)

✅ Just create spawn points with tag in Main scene
   Result: Automatically discovered at runtime
```

### Mistake 2: Not Assigning Player Prefab
```
❌ Leaving Player Prefab field empty
   Result: "Player prefab not set" error, no spawning

✅ Assign PlayerShip.prefab in NetworkManager
   Result: Players spawn correctly
```

### Mistake 3: Creating NetworkManager in Main Scene
```
❌ Main scene has its own NetworkManager
   Result: "Multiple NetworkManagers detected" error

✅ ONLY MainMenu has NetworkManager (persists via DontDestroyOnLoad)
   Result: Single NetworkManager across all scenes
```

### Mistake 4: Wrong Tag Name
```
❌ Tag: "Spawn Point" (with space)
❌ Tag: "spawn point" (lowercase)
❌ Tag: "SpawnPoints" (plural)

✅ Tag: "SpawnPoint" (exact match, no space, capital S and P)
```

---

## 📊 Why This Architecture?

**Design Principle**: "Assets can be assigned anywhere, Scene objects must be discovered at runtime"

### Assets (Can Assign in Inspector)
- ✅ Prefabs (PlayerShip.prefab)
- ✅ ScriptableObjects (ShipConfigurationSO)
- ✅ Materials, Textures, Audio Clips
- ✅ Any file in Assets folder

### Scene Objects (Must Find at Runtime)
- ❌ GameObjects in current scene
- ❌ GameObjects in other scenes
- ❌ Transform references across scenes
- ✅ Use FindGameObjectsWithTag(), GameObject.Find(), etc.

**Why NetworkManager Uses Both**:
- Player Prefab: Asset (manual assignment OK)
- Spawn Points: Scene objects (automatic discovery required)

---

## 🎓 Understanding Unity Limitations

**Unity Rule**: You cannot assign a GameObject from one scene to a component in another scene.

**Example That DOESN'T Work**:
```
Main.unity has: SpawnPoint_01 GameObject
MainMenu.unity has: NetworkManager GameObject

Trying to drag SpawnPoint_01 → NetworkManager component = FAILS
Unity silently keeps the reference empty
```

**Why**:
- Each scene is a separate file
- GameObjects only exist when their scene is loaded
- Cross-scene references would break when scenes aren't loaded simultaneously
- Unity prevents this to avoid null references

**Workaround**:
- Find GameObjects at runtime using tags, names, or types
- WOSNetworkManager does this automatically in OnServerSceneChanged()

---

## 💡 Pro Tips

**Tip 1**: Always check console for spawn point discovery
```
Console should show:
🎯 Found 5 spawn points in scene

If you see:
⚠️ No spawn points found in scene!
→ Check your tag spelling or parent GameObject name
```

**Tip 2**: Player Prefab can be assigned in any scene
```
You could assign it in Main scene too (but we use MainMenu for organization)
Prefabs are assets, not scene-specific
```

**Tip 3**: Each scene can have different spawn points
```
Main.unity: Ocean spawn points (spread across water)
PortHarbor.unity: Dock spawn point (near port)
→ WOSNetworkManager finds correct ones for each scene automatically
```

---

**Last Updated**: 2025-10-18
**Key Takeaway**: Player Prefab = Manual assignment ✅ | Spawn Points = Automatic discovery ✅
