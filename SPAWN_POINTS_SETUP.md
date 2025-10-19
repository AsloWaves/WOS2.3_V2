# Spawn Points Setup - Automatic Discovery System

**NEW SYSTEM**: Spawn points are automatically discovered at runtime - no Inspector assignment needed!

---

## 🎯 How It Works

**The Problem**:
- NetworkManager is in MainMenu scene
- Spawn points are in Main scene
- Unity doesn't allow cross-scene references in Inspector

**The Solution** ✅:
- WOSNetworkManager automatically finds spawn points when scene loads
- Just create spawn points in your scene - no manual assignment needed!
- Works with tags OR GameObject hierarchy

---

## 📍 Method 1: Using Tags (Recommended)

### Setup Steps

1. **Create "SpawnPoint" tag** (one-time setup):
   - Edit → Project Settings → Tags and Layers
   - Click "+" under Tags
   - Add new tag: **SpawnPoint**
   - Close Project Settings

2. **Create spawn points in Main scene**:
   - Open Main.unity
   - Create Empty GameObject → Name: **SpawnPoint_01**
   - Position: (-50, 0, 0)
   - Tag: **SpawnPoint** ← IMPORTANT!
   - Repeat for 4 more spawn points

3. **That's it!** WOSNetworkManager will find them automatically.

### Quick Creation Script

```
1. Open Main.unity
2. Create 5 empty GameObjects:
   - Name: SpawnPoint_01, Position: (-50, 0, 0), Tag: SpawnPoint
   - Name: SpawnPoint_02, Position: (50, 0, 0), Tag: SpawnPoint
   - Name: SpawnPoint_03, Position: (0, 50, 0), Tag: SpawnPoint
   - Name: SpawnPoint_04, Position: (0, -50, 0), Tag: SpawnPoint
   - Name: SpawnPoint_05, Position: (0, 0, 0), Tag: SpawnPoint
3. Save scene
```

### Verification

Console should show when scene loads:
```
🎯 Found 5 spawn points in scene
```

---

## 📍 Method 2: Using Hierarchy (No Tags Required)

If you don't want to use tags, use this method:

### Setup Steps

1. **Open Main.unity scene**

2. **Create parent GameObject**:
   - Create Empty GameObject
   - Name: **SpawnPoints** ← EXACT NAME REQUIRED!
   - Position: (0, 0, 0)

3. **Create child spawn points**:
   - Right-click SpawnPoints → Create Empty
   - Name: SpawnPoint_01 (name doesn't matter for children)
   - Position: (-50, 0, 0)
   - NO TAG NEEDED!
   - Repeat for 4 more children

4. **Scene hierarchy should look like**:
```
Main.unity
└── SpawnPoints (parent - must have this exact name)
    ├── SpawnPoint_01 (-50, 0, 0)
    ├── SpawnPoint_02 (50, 0, 0)
    ├── SpawnPoint_03 (0, 50, 0)
    ├── SpawnPoint_04 (0, -50, 0)
    └── SpawnPoint_05 (0, 0, 0)
```

### Verification

Console should show:
```
🎯 Found 5 spawn points in scene
```

---

## 🔍 How WOSNetworkManager Finds Spawn Points

**Code Flow** (happens automatically):

1. **Scene loads**: Main.unity loads after hosting
2. **Callback triggered**: `OnServerSceneChanged()` is called
3. **Search Method 1**: Look for GameObjects with tag "SpawnPoint"
4. **Search Method 2** (fallback): If no tags found, look for children of "SpawnPoints" GameObject
5. **Populate array**: `oceanSpawnPoints[]` is filled automatically
6. **Log result**: Console shows how many spawn points were found

**What you see in console**:
```
🏝️ Server changing scene to: Main
✅ Server scene loaded: Main
🎯 Found 5 spawn points in scene
```

---

## ⚠️ Common Mistakes

### Mistake 1: Wrong Tag Name
❌ Tag: "Spawn Point" (with space)
✅ Tag: "SpawnPoint" (no space, exact match)

### Mistake 2: Wrong Parent Name
❌ GameObject: "Spawn Points" or "SpawnPoint"
✅ GameObject: "SpawnPoints" (exact match)

### Mistake 3: Trying to Assign in Inspector
❌ Dragging spawn points to NetworkManager Inspector (cross-scene reference - won't work!)
✅ Just create spawn points in scene - automatic discovery handles it!

### Mistake 4: Forgetting to Tag Individual Spawn Points
If using Method 1 (tags), EACH spawn point needs the tag, not just the parent.

---

## 🧪 Testing Spawn Point Discovery

### Test 1: Verify Console Output

1. Clear Unity Console
2. Press Play
3. Host server (transitions to Main scene)
4. Check Console:

**Expected**:
```
🏝️ Server changing scene to: Main
✅ Server scene loaded: Main
🎯 Found 5 spawn points in scene
✅ Spawned player ship for connection 0 at (-50.00, 0.00, 0.00)
```

**If you see**:
```
⚠️ No spawn points found in scene! Players will spawn at origin (0,0,0)
💡 Create GameObjects with tag 'SpawnPoint' or children of GameObject named 'SpawnPoints'
```
→ Check your spawn point setup (tag or hierarchy)

### Test 2: Verify Player Spawns at Spawn Point

1. Host server
2. Check player position in Console
3. Should show position matching one of your spawn points
4. NOT (0, 0, 0) unless you have a spawn point at origin

### Test 3: Test Multiple Players (Round-Robin)

1. Set Spawn Method = RoundRobin in NetworkManager
2. Connect 3 clients
3. Each should spawn at different spawn point
4. Player 1: SpawnPoint_01
5. Player 2: SpawnPoint_02
6. Player 3: SpawnPoint_03

---

## 🎨 Advanced: Spawn Point Visualization

Add this to spawn points for visibility in Scene view:

1. Select SpawnPoint GameObject
2. Add Component → **Gizmo Icon** (optional)
3. Or create a simple script:

```csharp
using UnityEngine;

public class SpawnPointVisualizer : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 2f);
        Gizmos.DrawLine(transform.position, transform.position + transform.right * 5f);
    }
}
```

Attach to each spawn point to see them in Scene view.

---

## 📊 Comparison: Old vs New System

### OLD WAY (Didn't Work)
```
❌ Create spawn points in Main scene
❌ Try to assign them to NetworkManager in MainMenu scene
❌ Unity error: Can't assign cross-scene references
❌ Spawn points stay empty
❌ Players spawn at (0,0,0)
```

### NEW WAY (Automatic Discovery) ✅
```
✅ Create spawn points in Main scene (with tag or under "SpawnPoints" parent)
✅ WOSNetworkManager finds them automatically when scene loads
✅ No Inspector assignment needed
✅ Works across all scenes (Main, PortHarbor, etc.)
✅ Players spawn at correct positions
```

---

## 🔄 Multi-Scene Support

**This system works for ALL scenes**:

**Main Scene**:
```
SpawnPoints parent with ocean spawn locations
```

**PortHarbor Scene**:
```
SpawnPoints parent with dock spawn location
```

WOSNetworkManager automatically finds spawn points in whichever scene is loaded!

---

## ✅ Quick Setup Checklist

- [ ] Create "SpawnPoint" tag (Edit → Project Settings → Tags & Layers)
- [ ] Open Main.unity scene
- [ ] Create 5 empty GameObjects as spawn points
- [ ] Assign tag "SpawnPoint" to each OR make them children of "SpawnPoints" parent
- [ ] Position them at different locations in your ocean
- [ ] Save scene
- [ ] Test: Console shows "🎯 Found 5 spawn points in scene"
- [ ] Test: Player spawns at spawn point (not 0,0,0)

---

## 💡 Pro Tips

**Tip 1**: Use descriptive names for debugging
```
SpawnPoint_North_01
SpawnPoint_South_02
SpawnPoint_Harbor_01
```

**Tip 2**: Spread spawn points across your map
- Avoid clustering (players won't collide)
- Consider strategic locations (near islands, trade routes)
- Face spawn points toward interesting areas

**Tip 3**: Use rotation for ship orientation
- Set spawn point rotation
- Ships will spawn facing that direction
- Example: Face dock spawn points away from shore

---

**Last Updated**: 2025-10-18
**Status**: Automatic spawn point discovery implemented in WOSNetworkManager
