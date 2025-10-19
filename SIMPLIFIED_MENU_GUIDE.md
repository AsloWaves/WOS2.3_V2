# Simplified Menu System - Setup Guide

**Streamlined for Edgegap dedicated server deployment**

---

## What Changed

✅ **Removed**: Host panel (dedicated server only)
✅ **Removed**: Connection menu (intermediate step)
✅ **Simplified**: Main Menu → Join (auto-populated IP)
✅ **Centralized**: Server IP managed in one place

---

## Quick Setup (2 minutes)

### Step 1: Create ServerConfig Asset

**In Unity**:
1. **Right-click** in Project window → `Assets/Resources/` folder
2. **Create** → **WOS** → **Networking** → **Server Configuration**
3. **Rename** to: `ServerConfig`
4. **Click** the asset to edit in Inspector

### Step 2: Configure Current Server

**In ServerConfig Inspector**:
```
Server Address: 172.234.24.224:31139
Server Location: Chicago, Illinois
Show Server Info: ✓ (checked)
```

### Step 3: Assign to JoinMenu

**In Unity**:
1. Open **MainMenu** scene
2. Find **JoinPanel** in Hierarchy
3. Select **JoinPanel** → Find `JoinMenuController` component
4. **Drag** `ServerConfig` asset into **Server Config** field

### Step 4: Update Main Menu Button

**In Unity**:
1. Find **MainMenuPanel** in Hierarchy
2. Find the **"Start"** or **"Play"** button
3. In Inspector → **Button component** → **OnClick()**
4. **Change** from `ConnectionMenuController.OnStartButtonClicked` to:
   - **Object**: MainMenuController
   - **Function**: `MainMenuController.OnPlayButtonClicked`

### Step 5: Assign MUIP Buttons to JoinMenu

**In Unity**:
1. Find **JoinPanel** in Hierarchy
2. Select **JoinPanel** → Find `JoinMenuController` component
3. Assign UI references:
   - **Server Status Text**: TextMeshPro text showing status
   - **Connect Button**: MUIP ButtonManager (not standard Unity Button)
   - **Back Button**: MUIP ButtonManager to return to main menu

---

## How It Works Now

### User Flow
```
Main Menu
   ↓ (Click "Play")
Join Panel (IP auto-populated: 172.234.24.224:31139)
   ↓ (Click "Connect")
Game
```

**No more**:
- ❌ Entering IP manually
- ❌ Host/Join selection
- ❌ Extra menu navigation

---

## Updating Server IP (When Redeploying)

**When you redeploy to Edgegap** and get a new IP:

1. **Find new server IP** in Edgegap dashboard or Unity plugin
2. **Open** `ServerConfig` asset
3. **Update** `Server Address` field
   - Example: `172.234.24.224:31139` → `162.45.89.123:30456`
4. **Update** `Server Location` if changed
   - Example: `Chicago, Illinois` → `New York, New York`
5. **Done!** All clients will use new IP automatically

---

## Files Created/Modified

**New Files**:
- `Assets/Scripts/Networking/ServerConfig.cs` - Server configuration ScriptableObject
- `Assets/Resources/ServerConfig.asset` - Your server IP (create this)

**Modified Files**:
- `Assets/Scripts/UI/MainMenuController.cs` - Simplified to skip Connection menu
- `Assets/Scripts/UI/JoinMenuController.cs` - Auto-populates from ServerConfig

**Unchanged** (still works):
- `ConnectionMenuController.cs` - Can remove if not needed
- `HostMenuController.cs` - Can remove if not needed

---

## Optional: Remove Unused Panels

**If you want to clean up** (optional):

1. **In MainMenu scene**:
   - Find **ConnectionMenuPanel** → Delete or disable
   - Find **HostPanel** → Delete or disable

2. **In MenuManager**:
   - Remove references to deleted panels
   - Or leave them for future flexibility

---

## Current Server Info

**Your Edgegap Deployment**:
- **IP**: 172.234.24.224:31139
- **Location**: Chicago, Illinois
- **Status**: ✅ Running
- **Dashboard**: https://app.edgegap.com/deployments

**Free Tier Limits**:
- 24-hour max runtime (then auto-stops)
- 1 deployment at a time
- Need to redeploy for new IP when stopped

---

## Testing the New Flow

1. **Play** in Unity Editor
2. **Click "Play"** on Main Menu
3. **Verify** IP field shows: `172.234.24.224:31139`
4. **Verify** Status shows: `Connect to: 172.234.24.224:31139 (Chicago, Illinois)`
5. **Click "Connect"**
6. **Should connect** automatically!

---

## Troubleshooting

**IP field is empty**:
- Assign ServerConfig asset to JoinMenuController
- Create ServerConfig asset if missing

**Still showing old IP**:
- Update ServerConfig asset
- Restart Play mode

**"Back" button not working**:
- It should go to Main Menu (not Connection Menu anymore)

---

**All set!** Your menu system is now streamlined for dedicated Edgegap servers. 🚀
