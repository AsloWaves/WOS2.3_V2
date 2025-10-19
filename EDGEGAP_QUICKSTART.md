# Edgegap Quick Start - WOS2.3_V2

**Fast-track guide to deploy your first WOS2.3 server on Edgegap in 15-20 minutes**

---

## ⚡ Prerequisites (5 minutes)

### 1. Create Edgegap Account
- [ ] Visit: https://app.edgegap.com/auth/register
- [ ] Complete registration (free tier: 1.5 vCPU)
- [ ] Login to dashboard
- [ ] Navigate: **Settings** → **API**
- [ ] Click **Generate API Token**
- [ ] **Copy token** (save somewhere safe)

### 2. Install Docker Desktop
- [ ] Download: https://www.docker.com/products/docker-desktop/
- [ ] Install Docker Desktop
- [ ] **Start Docker Desktop** (must be running!)
- [ ] Verify: Open terminal → `docker --version`
  - Should show: `Docker version 24.x.x` or newer

### 3. Install Unity Linux Build Support
- [ ] Open **Unity Hub**
- [ ] Click **Installs** → Find Unity 6000.0.55f1
- [ ] Click ⚙️ (Settings) → **Add Modules**
- [ ] Check ✅ **Linux Build Support (Mono)**
- [ ] Click **Install** → Wait for completion

---

## 🎮 Unity Project Setup (5 minutes)

### 4. Configure Edgegap Plugin
- [ ] **In Unity**: **Tools** → **Edgegap Hosting**
- [ ] **API Token**: Paste your Edgegap token
- [ ] Click **Verify Token**
- [ ] Should show: "✅ Token verified successfully"

### 5. Create Application in Edgegap
**In Edgegap window**:
- [ ] **Application Name**: `wos23-server`
- [ ] **Version Tag**: `v0.3.0`
- [ ] **Server Build Path**: `Builds/EdgegapServer`

**Port Configuration**:
- [ ] Click **Add Port**
- [ ] **Port Name**: `game-port`
- [ ] **Internal Port**: `7777`
- [ ] **Protocol**: `UDP` (WOS2.3 uses KCP transport)
- [ ] Click **Save Application Settings**

### 6. Add Bootstrap Script (OPTIONAL - for validation)
**Note**: This step is optional but recommended for configuration validation.

**Add to Scene**:
- [ ] Open scene: `Assets/Scenes/MainMenu.unity`
- [ ] Right-click Hierarchy → **Create Empty**
- [ ] Rename to: `EdgegapBootstrap`
- [ ] **Add Component**: `WOSEdgegapBootstrap` (already exists in project)
- [ ] **Save scene** (Ctrl+S)

**What it does**:
- Validates NetworkManager configuration
- Checks port settings match Edgegap (7777 UDP)
- Verifies ServerLauncher is present
- Logs helpful deployment checklist

**Can I skip this?** Yes - server will work without it. It's just for validation.

### 7. Configure Build Settings
- [ ] **File** → **Build Settings**
- [ ] **Platform**: Linux (click **Switch Platform** if needed)
- [ ] ✅ Check **Server Build** (IMPORTANT!)
- [ ] **Scenes in Build**:
  - ✅ MainMenu.scene
  - ✅ Main.scene
- [ ] **Close** (don't build yet)

---

## 🚀 Build and Deploy (10 minutes)

### 8. Build Linux Server
- [ ] **File** → **Build Settings**
- [ ] Click **Build**
- [ ] **Navigate to**: `Builds/EdgegapServer/`
- [ ] **File name**: `ServerBuild` (exactly this name, no extension)
- [ ] Click **Save**
- [ ] ⏳ Wait for build (2-10 minutes first time)
- [ ] Verify build created:
  ```
  Builds/
  └── EdgegapServer/
      ├── ServerBuild ✅
      ├── ServerBuild_Data/ ✅
      └── UnityPlayer.so ✅
  ```

### 9. Test Build Locally (Optional but Recommended)
**In terminal** (from project root):
```bash
cd Builds/EdgegapServer
./ServerBuild -batchmode -nographics -server
```

**Expected output**:
```
[ServerLauncher] 🖥️ Headless build detected
[ServerLauncher] 🌊 Starting WOS2.3 Dedicated Server...
[ServerLauncher] ✅ Server started successfully!
```

**Stop server**: Press `Ctrl+C`

### 10. Deploy to Edgegap
- [ ] **In Unity**: **Tools** → **Edgegap Hosting**
- [ ] Verify all settings (app name, version, build path, port)
- [ ] Click **🚀 Deploy to Edgegap**
- [ ] ⏳ Wait for deployment (2-5 minutes)

**Deployment steps**:
```
⏳ Building Docker container...
⏳ Uploading to Edgegap...
⏳ Creating deployment...
⏳ Starting server...
✅ Server deployed successfully!
```

- [ ] **Copy Server IP** (shown in Edgegap window)
  - Example: `123.45.67.89`

---

## 🎯 Test Connection (2 minutes)

### 11. Connect from Client
**In Unity Editor** (Play Mode):
- [ ] Press **Play**
- [ ] **Main Menu** → **Join Game**
- [ ] **Server IP**: Paste Edgegap server IP
- [ ] **Port**: `7777`
- [ ] Click **Connect**

**Verify**:
- [ ] ✅ Connected to server
- [ ] ✅ Player ship spawns
- [ ] ✅ Ship controls work
- [ ] ✅ ShipDebugUI shows "Mode: Client"
- [ ] ✅ Network stats show ping/RTT

---

## ✅ Success Checklist

**You've successfully deployed if**:
- ✅ Edgegap dashboard shows deployment "Running"
- ✅ Client connects to server IP
- ✅ Player spawns and can control ship
- ✅ Network ping is reasonable (<150ms)
- ✅ No errors in Unity console

**Server IP**: `_____________` (write it down!)

---

## 🆘 Quick Troubleshooting

### Problem: "Token verification failed"
**Fix**:
- Regenerate token in Edgegap dashboard
- Copy ENTIRE token (long string)
- Paste in Unity → Verify again

### Problem: "Linux Build Support not found"
**Fix**:
- Unity Hub → Installs → Unity 6000.0.55f1 → Settings → Add Modules
- Install "Linux Build Support (Mono)"
- Restart Unity Editor

### Problem: "Docker daemon not running"
**Fix**:
- Open Docker Desktop application
- Wait until status shows "Running"
- Try deployment again

### Problem: "Build failed - no scenes"
**Fix**:
- File → Build Settings
- Add MainMenu.scene and Main.scene
- Ensure both are ✅ checked
- Build again

### Problem: "Deployment failed"
**Fix**:
- Check Edgegap dashboard → Logs tab
- Common issues:
  - Build name must be exactly `ServerBuild`
  - Port must be 7777 UDP (KCP transport)
  - Server Build checkbox must be checked

### Problem: "Can't connect to server"
**Fix**:
- Verify server IP is correct (check Edgegap dashboard)
- Test port: Use Unity client to connect (telnet doesn't work with UDP)
- Check firewall isn't blocking outgoing UDP 7777
- Verify deployment status is "Running" not "Failed"

---

## 📚 Next Steps

**You're all set!** For more details:
- 📖 Full guide: `EDGEGAP_DEPLOYMENT_GUIDE.md`
- 🔧 Advanced configs: `EDGEGAP_DEPLOYMENT_GUIDE.md` → Configuration
- 🐞 Debugging: `EDGEGAP_DEPLOYMENT_GUIDE.md` → Troubleshooting
- 📊 Monitoring: https://app.edgegap.com/deployments

**To update server**:
1. Change version tag (e.g., `v0.3.0` → `v0.3.1`)
2. Rebuild Linux server
3. Click "Deploy to Edgegap" again

---

**Deployment Time**: 15-20 minutes (first time)
**Subsequent Deploys**: 2-5 minutes

**🎉 Congratulations!** You're now running a global dedicated server!
