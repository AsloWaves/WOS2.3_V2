# WOS Update Workflows - Complete Guide

Comprehensive automation guide for Client, Server, and Launcher updates.

---

## 📋 Quick Reference

| Update Type | Build Client | Build Server | Update News | PatchManager | CDN Push | Edgegap Deploy | Downtime |
|-------------|--------------|--------------|-------------|--------------|----------|----------------|----------|
| **Client-Only** | ✅ Windows | ❌ No | ✅ Yes | ✅ App | ✅ Yes | ❌ No | None |
| **Server+Client** | ✅ Windows | ✅ Linux | ✅ Yes | ✅ App | ✅ Yes | ✅ Yes | 2-5 min |
| **Launcher Update** | ❌ No | ❌ No | ✅ Yes | ✅ Launcher | ✅ Yes | ❌ No | None |

---

## 🗂️ Directory Structure

```
D:\Updater\
├── Manager\
│   ├── PatchManager.exe                    # Creates patches for App/Launcher
│   ├── Game Launcher - News JSON Creator.exe  # Creates news updates
│   ├── App Workspace\
│   │   └── Release\
│   │       ├── App\                        # Copy Unity builds here
│   │       ├── Versions\                   # Previous versions
│   │       ├── Output\                     # Patch output
│   │       ├── SelfPatcher\                # Patcher files
│   │       └── Settings.xml
│   └── Launcher Workspace\
│       └── (Same structure for launcher)
├── WOS_Builds\
│   ├── Version_1.0.0\                     # Unity Windows builds
│   ├── Version_1.0.1\
│   └── Version_1.0.2\
└── game-launcher-cdn\                     # Git repo (GitHub Pages)
    ├── App\
    │   └── Release\                       # Final app patches (from Output)
    ├── Launcher\                          # Final launcher patches
    └── News\
        └── Release\
            └── en_US_Release_News.txt    # Auto-generated news JSON
```

---

## 🎯 Workflow 1: Client-Only Update

**When to use**: UI changes, graphics, audio, camera, local scripts (no NetworkBehaviour)

**Example**: In-game menu, HUD updates, visual polish

### **Steps:**

#### **1. Prepare Version Numbers**
```
Current Version: 1.0.2
New Version:     1.0.3
```

#### **2. Build Unity Client**
1. Open Unity project: `D:\GitFolder\UnityProjects\WOS2.3_V2`
2. File → Build Settings
3. Platform: **Windows**
4. **Uncheck** "Server Build" (client only)
5. Build Location: `D:\Updater\WOS_Builds\Version_1.0.3\`
6. Click **Build**
7. Wait for build to complete

#### **3. Copy Build to PatchManager**
```powershell
# Copy entire build folder
xcopy /E /I /Y "D:\Updater\WOS_Builds\Version_1.0.3\*" "D:\Updater\Manager\App Workspace\Release\App\"
```

#### **4. Run PatchManager**
1. Launch `D:\Updater\Manager\PatchManager.exe`
2. Select **App Workspace/Release**
3. Set Version: `1.0.3`
4. Click **Create Patch**
5. Wait for patch generation
6. Output goes to: `D:\Updater\Manager\App Workspace\Release\Output\`

#### **5. Copy Output to CDN**
```powershell
# Copy patch files to CDN
xcopy /E /I /Y "D:\Updater\Manager\App Workspace\Release\Output\*" "D:\Updater\game-launcher-cdn\App\Release\"
```

#### **6. Update News JSON**
**Option A: Use GUI Tool**
1. Run `D:\Updater\Manager\Game Launcher - News JSON Creator.exe`
2. Add news entry:
   - Title: "PATCH 1.0.3"
   - Content: "Client update: [describe changes]"
   - Previous: 1.0.2 → New: 1.0.3
3. Save to: `D:\Updater\game-launcher-cdn\News\Release\en_US_Release_News.txt`

**Option B: Manual JSON Edit** (Faster, can be automated)
```bash
# See automation script below
```

#### **7. Push to GitHub CDN**
```bash
cd "D:\Updater\game-launcher-cdn"
git add -A
git commit -m "Update to v1.0.3 - [Brief description]

- [Change 1]
- [Change 2]
- [Change 3]"
git push origin gh-pages
```

#### **8. Verify Deployment**
1. Open game launcher
2. Check for update notification
3. Download and install update
4. Test changes in game

**⏱️ Total Time**: 10-15 minutes
**🚫 Downtime**: NONE

---

## 🎯 Workflow 2: Server + Client Update

**When to use**: Network code changes, gameplay mechanics, physics, SyncVars, Commands/RPCs

**Example**: Ship physics update, new networked features

### **Steps:**

#### **1. Prepare Version Numbers**
```
Current Client: 1.0.2
New Client:     1.0.3
Current Server: 1.0.A
New Server:     1.0.B
```

#### **2. Build Unity Client (Windows)**
1. Open Unity project
2. File → Build Settings
3. Platform: **Windows**
4. **Uncheck** "Server Build"
5. Build Location: `D:\Updater\WOS_Builds\Version_1.0.3\`
6. Click **Build**

#### **3. Build Unity Server (Linux)**
1. File → Build Settings
2. Platform: **Linux**
3. **Check** "Server Build" ✓
4. **Check** "Headless Mode" ✓ (if available)
5. Build Location: `D:\GitFolder\UnityProjects\WOS2.3_V2\EdgegapServer\`
6. Click **Build**
7. Wait for Linux server build

#### **4. Process Client Build (Same as Workflow 1)**
```powershell
# Copy to PatchManager
xcopy /E /I /Y "D:\Updater\WOS_Builds\Version_1.0.3\*" "D:\Updater\Manager\App Workspace\Release\App\"
```

Run PatchManager → Copy output to CDN (see Workflow 1)

#### **5. Deploy Server to Edgegap**

**Unity Edgegap Plugin Method**:
1. In Unity: **Tools → Edgegap → Server Hosting**
2. Edgegap window opens
3. **Server Build Settings**:
   - Build Path: `EdgegapServer/` (already built)
   - Dockerfile: Auto-generated or use existing
4. Click **"Build Docker Image"**
5. Wait for Docker build (~3-5 minutes)
6. Click **"Push to Registry"** (uploads to Edgegap)
7. Wait for upload (~2-5 minutes)
8. **Deploy Options**:
   - **Option A: Update Existing Deployment**
     - Select active deployment
     - Click "Stop Deployment"
     - Update app version to new build
     - Click "Start Deployment"
   - **Option B: Create New Version** (Recommended)
     - Create new app version (e.g., `wos-v1.0.3`)
     - Deploy new version
     - Test on new deployment
     - Switch traffic from old to new

**Important**: You do **NOT** create a new application each time. You either:
- Update the existing app version, OR
- Create a new version under the same application

**Edgegap Application Structure**:
```
Application: "wos-naval-mmo" (Created once)
├─ Version 1.0.A (Old)
├─ Version 1.0.B (New) ← Create this
└─ Version 1.0.C (Future)
```

#### **6. Update ServerConfig in Unity**
1. Open Unity
2. Find: `Assets/Resources/ServerConfigs/ServerConfig.asset`
3. Update:
   - Server Address: `[New Edgegap IP]:[Port]`
   - Server Location: `[Server Region]`
4. Save
5. **Rebuild client** with new IP (repeat Step 2)
6. **Re-run PatchManager** (repeat Step 4)

**Note**: If IP/port didn't change, skip this step.

#### **7. Update News & Push to CDN**
Same as Workflow 1, but mention both client AND server updates:
```json
{
  "title": "PATCH 1.0.3 - Server Update",
  "content": "Server and client update:\n- [Server change 1]\n- [Client change 1]"
}
```

**⏱️ Total Time**: 25-35 minutes
**🚫 Downtime**: 2-5 minutes (during server switch)

---

## 🎯 Workflow 3: Launcher-Only Update

**When to use**: Game launcher improvements, UI changes, updater fixes

### **Steps:**

#### **1. Build New Launcher**
(Assuming you have the launcher project source)
1. Build launcher project
2. Output: `GameLauncher.exe` + `SelfUpdater` folder

#### **2. Copy to Launcher Workspace**
```powershell
# Copy launcher files
xcopy /E /I /Y "D:\YourLauncherBuild\GameLauncher.exe" "D:\Updater\Manager\Launcher Workspace\Launcher\"
xcopy /E /I /Y "D:\YourLauncherBuild\SelfUpdater\" "D:\Updater\Manager\Launcher Workspace\Launcher\SelfUpdater\"
```

#### **3. Run PatchManager for Launcher**
1. Launch `PatchManager.exe`
2. Select **Launcher Workspace**
3. Set new version (e.g., 1.0.5)
4. Create patch
5. Output to: `D:\Updater\Manager\Launcher Workspace\Output\`

#### **4. Copy to CDN**
```powershell
# Launcher patches go to CDN/Launcher
xcopy /E /I /Y "D:\Updater\Manager\Launcher Workspace\Output\*" "D:\Updater\game-launcher-cdn\Launcher\"
```

#### **5. Create Launcher Zip**
```powershell
# Create manual download zip
cd "D:\Updater\Manager\Launcher Workspace\Launcher"
Compress-Archive -Path * -DestinationPath "D:\Updater\game-launcher-cdn\WOS_Launcher_v1.0.5.zip" -Force
```

#### **6. Update News**
```json
{
  "title": "LAUNCHER UPDATE 1.0.5",
  "content": "Game launcher improvements:\n- [Feature 1]\n- [Bug fix 1]"
}
```

#### **7. Push to CDN**
Same git push process as Workflow 1.

**⏱️ Total Time**: 8-12 minutes
**🚫 Downtime**: NONE

---

## 🤖 Automation Scripts

### **Script 1: Update News JSON**

Create: `D:\Updater\Scripts\update_news.ps1`

```powershell
param(
    [string]$VersionFrom,
    [string]$VersionTo,
    [string]$UpdateType,  # "Client", "Server", or "Launcher"
    [string]$Description
)

$newsFile = "D:\Updater\game-launcher-cdn\News\Release\en_US_Release_News.txt"
$timestamp = Get-Date -Format "yyyy-MM-ddTHH:mm:ss.fffffffzzz"

# Read existing news
$newsData = Get-Content $newsFile -Raw | ConvertFrom-Json

# Create new news entry
$newEntry = @{
    header = "LATEST UPDATE"
    title = "PATCH $VersionTo"
    subtitle = "NOW AVAILABLE"
    subtitleColor = "#FFFFFF"
    date = $timestamp
    imagesURL = @()
    videoURL = ""
    interactionURL = ""
    content = "$UpdateType Update ($VersionFrom → $VersionTo):`n$Description"
    buttonContent = "More info..."
    showHeader = $true
    showTitle = $true
    showSubTitle = $true
    subtitleCustomColor = $false
    showContent = $true
    showDate = $true
    showButton = $false
    showVideo = $false
}

# Add to top of news array
$newsData.News = @($newEntry) + $newsData.News

# Keep only last 5 news entries
if ($newsData.News.Count > 5) {
    $newsData.News = $newsData.News[0..4]
}

# Save updated news
$newsData | ConvertTo-Json -Depth 10 | Set-Content $newsFile -Encoding UTF8

Write-Host "✅ News updated: $VersionFrom → $VersionTo" -ForegroundColor Green
```

**Usage**:
```powershell
.\update_news.ps1 -VersionFrom "1.0.2" -VersionTo "1.0.3" -UpdateType "Client" -Description "In-game menu added`nUI improvements`nBug fixes"
```

---

### **Script 2: Client-Only Update Automation**

Create: `D:\Updater\Scripts\update_client.ps1`

```powershell
param(
    [string]$NewVersion,
    [string]$Description
)

Write-Host "🚀 Starting Client-Only Update to v$NewVersion..." -ForegroundColor Cyan

# 1. Copy Unity build to PatchManager
Write-Host "📦 Copying Unity build..." -ForegroundColor Yellow
xcopy /E /I /Y "D:\Updater\WOS_Builds\Version_$NewVersion\*" "D:\Updater\Manager\App Workspace\Release\App\"

Write-Host "⚠️ MANUAL STEP REQUIRED:" -ForegroundColor Red
Write-Host "   1. Run PatchManager.exe" -ForegroundColor Yellow
Write-Host "   2. Select App Workspace/Release" -ForegroundColor Yellow
Write-Host "   3. Set version to: $NewVersion" -ForegroundColor Yellow
Write-Host "   4. Click Create Patch" -ForegroundColor Yellow
Write-Host "   5. Wait for completion" -ForegroundColor Yellow
Read-Host "Press Enter when PatchManager is COMPLETE"

# 2. Copy output to CDN
Write-Host "📂 Copying patches to CDN..." -ForegroundColor Yellow
xcopy /E /I /Y "D:\Updater\Manager\App Workspace\Release\Output\*" "D:\Updater\game-launcher-cdn\App\Release\"

# 3. Update news
Write-Host "📰 Updating news..." -ForegroundColor Yellow
$prevVersion = (Get-Content "D:\Updater\game-launcher-cdn\App\Release\VersionInfo.info" | Select-String -Pattern "<Version>(.*)</Version>").Matches.Groups[1].Value
& "D:\Updater\Scripts\update_news.ps1" -VersionFrom $prevVersion -VersionTo $NewVersion -UpdateType "Client" -Description $Description

# 4. Git push
Write-Host "🔄 Pushing to GitHub CDN..." -ForegroundColor Yellow
cd "D:\Updater\game-launcher-cdn"
git add -A
git commit -m "Update to v$NewVersion - Client update

$Description"
git push origin gh-pages

Write-Host "✅ Client update complete! Version $NewVersion live on CDN." -ForegroundColor Green
```

**Usage**:
```powershell
.\update_client.ps1 -NewVersion "1.0.3" -Description "In-game menu added`nUI improvements`nBug fixes"
```

---

### **Script 3: Full Server+Client Update** (Coming soon)

---

## 📝 CLAUDE.md Integration

Add to `D:\GitFolder\UnityProjects\WOS2.3_V2\CLAUDE.md`:

```markdown
## Update Workflows

When the user requests an update deployment:

### Client-Only Update
1. Verify changes are client-only (no NetworkBehaviour changes)
2. User builds in Unity to `D:\Updater\WOS_Builds\Version_X.X.X\`
3. Run: `D:\Updater\Scripts\update_client.ps1 -NewVersion "X.X.X" -Description "[changes]"`
4. Follow PatchManager manual step
5. Verify CDN push successful
6. Test with game launcher

### Server+Client Update
1. User builds Windows client to `D:\Updater\WOS_Builds\Version_X.X.X\`
2. User builds Linux server to `EdgegapServer/`
3. Run client update script (as above)
4. Help user deploy to Edgegap via Unity plugin
5. Update ServerConfig.asset with new IP (if changed)
6. Rebuild client if IP changed
7. Push to CDN

### Edgegap Deployment
- **DO NOT** create new application each time
- **UPDATE** existing app version OR create new version
- Application name stays the same: "wos-naval-mmo"
- Only version number increments: 1.0.A → 1.0.B
```

---

## ✅ Deployment Checklist

### Pre-Deployment
- [ ] All code changes committed to git
- [ ] Unity console has no errors
- [ ] Version numbers prepared
- [ ] Backup current live version

### Client Build
- [ ] Unity builds successfully
- [ ] Build copied to `WOS_Builds\Version_X.X.X\`
- [ ] PatchManager runs without errors
- [ ] Patches copied to CDN

### Server Build (if applicable)
- [ ] Linux server builds successfully
- [ ] EdgegapServer folder contains new build
- [ ] Docker image builds successfully
- [ ] Pushed to Edgegap registry

### CDN Update
- [ ] News JSON updated
- [ ] Patch files in correct CDN location
- [ ] Git commit with clear message
- [ ] Pushed to GitHub (gh-pages branch)
- [ ] GitHub Pages deployed (check Actions tab)

### Edgegap Deployment (if applicable)
- [ ] New version created (NOT new application)
- [ ] Deployment successful
- [ ] Server logs show successful start
- [ ] Port mapping correct (7777 → external port)
- [ ] IP address noted
- [ ] ServerConfig updated (if needed)

### Post-Deployment Verification
- [ ] Game launcher shows update
- [ ] Update downloads successfully
- [ ] Game launches without errors
- [ ] Multiplayer connection works
- [ ] Changes visible in-game
- [ ] No console errors
- [ ] Performance acceptable

---

## 🐛 Troubleshooting

### "PatchManager fails to create patch"
- Check version numbers are incremented
- Verify `App/` folder has complete build
- Ensure `Versions/` folder has previous version
- Check Settings.xml configuration

### "News JSON not showing in launcher"
- Verify JSON syntax is valid
- Check timestamp format
- Ensure file pushed to GitHub
- Wait for GitHub Pages deployment (~1-2 minutes)
- Clear launcher cache

### "Edgegap deployment fails"
- Check Docker is running
- Verify Edgegap API key
- Check container logs for errors
- Ensure port 7777 is configured
- Verify Linux build is headless

### "Server IP changed but clients can't connect"
- Update ServerConfig.asset
- Rebuild Windows client
- Re-run PatchManager
- Push new patch to CDN
- Verify IP:Port in Edgegap dashboard

### "Git push rejected"
- Pull latest: `git pull origin gh-pages`
- Resolve conflicts
- Push again: `git push origin gh-pages`

---

## 🎯 Best Practices

1. **Version Numbering**:
   - Client: Semantic versioning (1.0.0, 1.0.1, 1.0.2)
   - Server: Letter suffix (1.0.A, 1.0.B, 1.0.C)
   - Launcher: Separate versioning (1.0.4, 1.0.5)

2. **Testing Before Deployment**:
   - Test locally first (host + client)
   - Test on Edgegap staging (if available)
   - Then deploy to production

3. **Incremental Updates**:
   - Small, frequent updates > large, rare updates
   - Easier to debug issues
   - Faster rollback if needed

4. **Communication**:
   - Announce server updates in advance
   - Use News JSON for update details
   - Provide estimated downtime

5. **Backup Strategy**:
   - Keep previous version builds
   - Can rollback quickly if needed
   - Edgegap keeps previous images

---

**Update Workflows Complete!** 🚀

Use this guide for all Client, Server, and Launcher deployments.
