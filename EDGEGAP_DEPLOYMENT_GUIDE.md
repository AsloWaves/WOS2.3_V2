# Edgegap Headless Server Deployment Guide - WOS2.3_V2

**Complete guide for deploying WOS2.3 Naval MMO dedicated servers on Edgegap cloud platform**

**Last Updated**: 2025-10-18
**Status**: ✅ Ready for implementation

---

## 📋 Overview

This guide covers everything needed to deploy WOS2.3_V2 as a headless dedicated server on Edgegap cloud platform. Edgegap provides automatic global server distribution with their free tier offering 1.5 vCPU hosting for Mirror users.

**What You'll Deploy**:
- Linux headless Unity server build (no graphics)
- Dockerized server container
- Auto-scaling cloud deployment
- Global edge network distribution

**Deployment Time**: 15-30 minutes (first-time setup)
**Subsequent Deployments**: 2-5 minutes

---

## 🔧 Prerequisites

### 1. Edgegap Account
- **Create Account**: https://app.edgegap.com/auth/register
- **Free Tier**: 1.5 vCPU included with Mirror networking
- **Get API Token**:
  1. Login to Edgegap dashboard
  2. Navigate to **Settings** → **API**
  3. Generate new API token
  4. **Save this token** - you'll need it in Unity

### 2. Docker Desktop
- **Download**: https://www.docker.com/products/docker-desktop/
- **Install Docker Desktop** for your OS (Windows/Mac)
- **Start Docker Desktop** and ensure it's running
- **Verify Installation**:
  ```bash
  docker --version
  # Should output: Docker version 24.x.x or newer
  ```

### 3. Unity Linux Build Support
- **Open Unity Hub**
- **Installs** → Click on Unity version (6000.0.55f1)
- **Add Modules** → Check **Linux Build Support (Mono)**
- **Install/Update**

### 4. Current WOS2.3_V2 Setup Status
✅ **Already Configured**:
- Mirror Networking framework installed
- WOSNetworkManager with player spawning
- ServerLauncher with headless detection
- NetworkAddressManager for server IP management
- Edgegap plugin (bundled with Mirror)

---

## 📁 Project Structure

**Key Files**:
```
WOS2.3_V2/
├── Assets/
│   ├── Scripts/
│   │   └── Networking/
│   │       ├── WOSNetworkManager.cs          ✅ Player spawning
│   │       ├── ServerLauncher.cs              ✅ Auto-start headless
│   │       ├── NetworkAddressManager.cs       ✅ Server IP management
│   │       └── EdgegapDeployHelper.cs         ✅ Deployment helper
│   ├── Mirror/
│   │   └── Hosting/
│   │       └── Edgegap/
│   │           ├── Editor/
│   │           │   ├── Dockerfile             ✅ Server container config
│   │           │   └── EdgegapWindowV2.cs     ✅ Unity plugin UI
│   │           └── README.md                  📖 Plugin docs
│   └── edgegap-unity-plugin/
│       └── Runtime/
│           └── BootstrapTemplates/
│               └── EdgegapServerBootstrapMirrorTemp.cs.txt  ✅ Template
└── Builds/
    └── EdgegapServer/                         📁 Created by Unity
        └── ServerBuild                        🐧 Linux headless build
```

---

## 🚀 Step-by-Step Deployment

### Phase 1: Unity Editor Setup (One-Time Configuration)

#### Step 1.1: Open Edgegap Plugin
1. **In Unity Editor**: **Tools** → **Edgegap Hosting**
2. Edgegap window opens (dock it for easy access)

#### Step 1.2: Configure API Credentials
1. In Edgegap window, locate **API Token** field
2. Paste your Edgegap API token (from Prerequisites)
3. Click **Verify Token**
4. ✅ Should show "Token verified successfully"

#### Step 1.3: Create Application Version
1. In Edgegap window:
   - **Application Name**: `wos23-server` (lowercase, no special chars)
   - **Version Tag**: `v0.3.0` (increment for each deployment)
   - **Server Build Path**: `Builds/EdgegapServer`

2. **Port Configuration**:
   - **Port Name**: `game-port`
   - **Internal Port**: `7777` (default Mirror Telepathy port)
   - **Protocol**: `TCP`
   - **TLS Upgrade**: ❌ Off (not needed for Mirror Telepathy)

3. Click **Save Application Settings**

#### Step 1.4: Configure Build Settings
**Unity Build Settings** (**File** → **Build Settings**):

1. **Platform**: Linux
   - If not active, click **Switch Platform**
   - **Target Platform**: Linux
   - **Architecture**: x86_64

2. **Server Build**: ✅ Check **Server Build**
   - This enables headless mode (no graphics device)
   - Reduces build size significantly

3. **Scenes in Build**:
   ```
   ✅ Assets/Scenes/MainMenu.scene  (Build Index 0)
   ✅ Assets/Scenes/Main.scene      (Build Index 1)
   ```
   - MainMenu: For menu/lobby (if using)
   - Main: Ocean gameplay scene

4. **Other Settings** (**Edit** → **Project Settings** → **Player**):
   - **Company Name**: Your name/studio
   - **Product Name**: `WOS2.3_Server`
   - **Version**: `0.3.0` (match Edgegap version)

#### Step 1.5: Verify ServerLauncher Configuration
1. **Find ServerLauncher**: Check `Assets/Scenes/MainMenu.scene`
2. **Verify Settings**:
   - ✅ **Auto Start In Headless**: `true`
   - **Default Port**: `7777`
   - **Default Max Connections**: `300`
   - **Server Start Scene**: `Main` (or your gameplay scene)
   - ✅ **Verbose Logging**: `true` (for debugging)

**Note**: ServerLauncher automatically detects headless builds and starts the server

---

### Phase 2: Create Bootstrap Script

The Edgegap plugin requires a bootstrap script to initialize the server. Let's create one:

#### Step 2.1: Create Bootstrap Script
1. **In Unity**: **Tools** → **Edgegap Hosting** → **Create Bootstrap**
2. **Select Template**: Mirror
3. **Script Name**: `WOSEdgegapBootstrap`
4. **Location**: `Assets/Scripts/Networking/`
5. Click **Generate**

**The generated script will**:
- Auto-detect transport settings (Telepathy = TCP port 7777)
- Validate port mappings match Edgegap configuration
- Handle server startup in containerized environment

#### Step 2.2: Add Bootstrap to Scene
1. **Open**: `Assets/Scenes/MainMenu.scene` (or first scene in build)
2. **Create Empty GameObject**: Right-click Hierarchy → Create Empty
3. **Rename**: `EdgegapBootstrap`
4. **Add Component**: `WOSEdgegapBootstrap` script
5. **Save Scene**

**Important**: This script only runs in server builds, not in Editor/Client

---

### Phase 3: Build and Deploy

#### Step 3.1: Build Linux Headless Server
1. **In Unity**: **File** → **Build Settings**
2. **Verify**:
   - ✅ Platform: Linux
   - ✅ Server Build: Checked
   - ✅ Scenes added
3. Click **Build**
4. **Save As**: Navigate to `Builds/EdgegapServer/`
5. **Name**: `ServerBuild` (exact name, no extension)
6. **Wait**: Build takes 2-10 minutes (first build slower)

**Expected Output**:
```
Builds/
└── EdgegapServer/
    ├── ServerBuild                  (Executable)
    ├── ServerBuild_Data/            (Game data)
    └── UnityPlayer.so               (Unity runtime)
```

#### Step 3.2: Test Build Locally (Optional but Recommended)
**Using Command Line** (not Docker yet):
```bash
cd Builds/EdgegapServer
./ServerBuild -batchmode -nographics -server -port 7777
```

**Expected Console Output**:
```
[ServerLauncher] 🖥️ Headless build detected
[ServerLauncher] 🌊 Starting WOS2.3 Dedicated Server...
[ServerLauncher] Port: 7777
[ServerLauncher] Max Players: 300
[ServerLauncher] ✅ Server started successfully!
[WOSNetworkManager] 🌊 WOS Server started!
```

**Stop**: Press `Ctrl+C`

#### Step 3.3: Deploy to Edgegap
1. **In Unity**: **Tools** → **Edgegap Hosting**
2. **Verify Settings**:
   - Application: `wos23-server`
   - Version: `v0.3.0`
   - Build Path: `Builds/EdgegapServer`
   - Port: `7777 TCP`

3. Click **Deploy to Edgegap**

**Deployment Process** (2-5 minutes):
```
⏳ Building Docker container...
⏳ Uploading container to Edgegap registry...
⏳ Creating deployment...
⏳ Starting server on edge location...
✅ Server deployed successfully!
```

4. **Get Server Connection Info**:
   - **Server IP**: Displayed in Edgegap window (e.g., `123.45.67.89`)
   - **Port**: `7777` (or port shown)
   - **Status**: `Running`

#### Step 3.4: Connect from Client
1. **In Unity Editor** (or built client):
   - **Start Unity** → Play Mode
   - **Main Menu** → Join Game
   - **Server IP**: Enter Edgegap server IP (e.g., `123.45.67.89`)
   - **Port**: `7777`
   - Click **Connect**

2. **Verify Connection**:
   - ✅ Client connects successfully
   - ✅ Player ship spawns in ocean
   - ✅ Ship controls respond
   - ✅ ShipDebugUI shows "Mode: Client"

---

## 🔍 Testing Checklist

### Local Testing (Before Edgegap)
- [ ] Linux build completes without errors
- [ ] Server starts in headless mode (`-batchmode -nographics`)
- [ ] Server listens on port 7777
- [ ] Local client can connect to `127.0.0.1:7777`
- [ ] Player spawns correctly
- [ ] Ship physics work properly
- [ ] Server logs show no errors

### Edgegap Deployment Testing
- [ ] Docker build completes successfully
- [ ] Container uploads to Edgegap registry
- [ ] Deployment shows "Running" status
- [ ] Server IP is accessible (ping/telnet)
- [ ] Port 7777 is open and accessible
- [ ] Client can connect from external network
- [ ] Multiple clients can connect simultaneously
- [ ] Network stats show acceptable ping (<100ms to nearest edge)
- [ ] Players see each other (multiplayer sync works)
- [ ] Server handles disconnections gracefully

---

## 🐳 Docker Configuration

### Understanding the Dockerfile

**Location**: `Assets/Mirror/Hosting/Edgegap/Editor/Dockerfile`

```dockerfile
FROM ubuntu:22.04

ARG DEBIAN_FRONTEND=noninteractive
ARG SERVER_BUILD_PATH=Builds/EdgegapServer

# Copy Unity build to container
COPY ${SERVER_BUILD_PATH} /root/build/

WORKDIR /root/

# Make executable runnable
RUN chmod +x /root/build/ServerBuild

# Install SSL certificates (for HTTPS APIs if needed)
RUN apt-get update && \
    apt-get install -y ca-certificates && \
    apt-get clean && \
    update-ca-certificates

# Start server with Unity headless flags
CMD ["/bin/bash", "-c", "env;/root/build/ServerBuild -batchmode -nographics $UNITY_COMMANDLINE_ARGS"]
```

**Key Points**:
- **Base Image**: Ubuntu 22.04 LTS (stable, small size)
- **Build Path**: Copies entire `Builds/EdgegapServer/` directory
- **Executable**: Expects file named `ServerBuild` (exact match)
- **Server Flags**:
  - `-batchmode`: Non-interactive mode
  - `-nographics`: Headless (no GPU needed)
  - `$UNITY_COMMANDLINE_ARGS`: Edgegap passes runtime arguments

### Local Docker Testing (Advanced)

**Build Docker Image Locally**:
```bash
# In project root
docker build -t wos23-server:v0.3.0 -f Assets/Mirror/Hosting/Edgegap/Editor/Dockerfile .
```

**Run Docker Container Locally**:
```bash
docker run -p 7777:7777 wos23-server:v0.3.0
```

**Test Connection**:
```bash
# From another terminal
telnet localhost 7777
# Should connect (or use Unity client to connect to localhost:7777)
```

**Stop Container**:
```bash
docker ps                    # Get container ID
docker stop <container-id>
```

---

## ⚙️ Server Configuration

### Environment Variables (Edgegap)

Edgegap allows passing environment variables to your server. Configure in Edgegap dashboard:

**Common Variables**:
```
UNITY_COMMANDLINE_ARGS=-port 7777 -maxplayers 300
SERVER_NAME=WOS2.3 North America Server
SERVER_REGION=na-east
```

**Access in Unity** (ServerLauncher.cs already handles this):
```csharp
string[] args = System.Environment.GetCommandLineArgs();
```

### ServerLauncher Command-Line Arguments

**Supported Flags** (already implemented in ServerLauncher.cs):
```
-server              Force server mode
-port <number>       Set server port (default: 7777)
-maxplayers <number> Set max connections (default: 300)
-scene <name>        Set starting scene (default: Main)
-verbose             Enable verbose logging
```

**Example Usage**:
```bash
./ServerBuild -server -port 7777 -maxplayers 100 -scene Main -verbose
```

### WOSNetworkManager Configuration

**Inspector Settings** (already configured):
- **Player Prefab**: NetworkedPlayerShip (with NetworkedNavalController)
- **Spawn Points**: Auto-detected from scene (tag: "SpawnPoint")
- **Naval Send Rate**: 30Hz (sufficient for naval movement)
- **Max Connections**: 300 (Edgegap default)

**Ocean Spawn Points Setup**:
1. **In Main scene**: Create empty GameObjects
2. **Tag**: "SpawnPoint" (or children of GameObject named "SpawnPoints")
3. **Position**: Place at ocean locations (spread out)
4. **Rotation**: Ship starting rotation

**Auto-Detection**: WOSNetworkManager automatically finds spawn points on scene load

---

## 🌐 Networking Architecture

### How It Works

**Client → Edgegap → Server**:
```
Unity Client (Your PC)
    ↓ TCP connection
Edgegap Edge Server (Nearest location)
    ↓ Routing
Docker Container (Ubuntu + Unity Server)
    ↓ Mirror Networking
WOSNetworkManager → NetworkServer
    ↓ Spawn
PlayerShip with NetworkedNavalController
```

### Port Configuration

**Default Setup** (Mirror Telepathy):
- **Transport**: Telepathy (TCP)
- **Port**: 7777
- **Protocol**: TCP
- **Public Access**: Yes

**Edgegap Port Mapping**:
```
External Port: 7777 (client connects here)
    ↓ NAT
Internal Port: 7777 (Docker container)
    ↓ Unity
Server listens: 0.0.0.0:7777
```

### Network Address Management

**Server-Side** (automatic):
- ServerLauncher detects headless mode
- Starts NetworkServer on 0.0.0.0:7777
- WOSNetworkManager handles player spawning

**Client-Side** (manual connection):
- User enters Edgegap server IP in UI
- NetworkAddressManager saves recent servers
- Mirror NetworkClient connects to IP:port

---

## 📊 Monitoring and Debugging

### Edgegap Dashboard

**Access**: https://app.edgegap.com/deployments

**Deployment Info**:
- **Status**: Running, Stopped, Failed
- **Server IP**: Public IP address
- **Port**: Exposed port(s)
- **Location**: Edge server location (e.g., us-east-1)
- **Uptime**: Time since deployment
- **Players**: Current connections (if using Edgegap analytics)

### Server Logs

**View Logs in Edgegap**:
1. Dashboard → Deployments
2. Click on your deployment
3. **Logs** tab → Real-time server console output

**Expected Logs**:
```
[ServerLauncher] 🖥️ Headless build detected
[ServerLauncher] 🌊 Starting WOS2.3 Dedicated Server...
[ServerLauncher] Port: 7777
[ServerLauncher] Max Players: 300
[ServerLauncher] Start Scene: Main
[ServerLauncher] ✅ Server started successfully!
[WOSNetworkManager] 🌊 WOS Server started!
[WOSNetworkManager] 🎯 Found 8 spawn points in scene
```

**Connection Logs**:
```
[WOSNetworkManager] ✅ Spawned player ship for connection 1 at (152.3, 45.7, 0)
NetworkServer: connection ready: 1
```

### Debugging Connection Issues

**Problem**: Client can't connect to Edgegap server

**Checklist**:
1. ✅ Server status shows "Running" in Edgegap dashboard
2. ✅ Server IP is correct (check dashboard)
3. ✅ Port 7777 is open (not blocked by firewall)
4. ✅ Client using correct IP:port format (no "http://")
5. ✅ Server logs show no startup errors

**Test Server Connectivity**:
```bash
# Windows PowerShell
Test-NetConnection -ComputerName <server-ip> -Port 7777

# Mac/Linux
telnet <server-ip> 7777
# Or
nc -zv <server-ip> 7777
```

**Expected Result**: Connection succeeds (port is open)

**Common Issues**:
- ❌ **Connection refused**: Server not running (check logs)
- ❌ **Connection timeout**: Firewall blocking port (check Edgegap port mapping)
- ❌ **Invalid IP**: Using old/wrong server IP (get latest from dashboard)

---

## 🔄 Updates and Versioning

### Updating Your Server

**When to Deploy New Version**:
- Bug fixes
- New features
- Performance improvements
- Configuration changes

**Update Process**:
1. **Increment Version**: Change `versionTag` (e.g., `v0.3.0` → `v0.3.1`)
2. **Rebuild**: Create new Linux build
3. **Re-deploy**: Use Edgegap plugin to deploy new version
4. **Test**: Verify new version works
5. **Update Clients**: Inform players to use new server IP (if changed)

### Version Management

**Semantic Versioning** (recommended):
```
v0.3.0 → v0.3.1   (Bug fix)
v0.3.1 → v0.4.0   (New feature)
v0.4.0 → v1.0.0   (Major release)
```

**Multiple Versions**:
- Edgegap allows multiple app versions simultaneously
- Useful for testing (production + staging servers)
- Free tier: Limited to 1.5 vCPU total (consider stopping old deployments)

---

## 💰 Cost and Free Tier

### Edgegap Free Tier (Mirror Users)

**Included**:
- **1.5 vCPU** of compute time (always free)
- **Global edge network** (90+ locations)
- **Automatic scaling**
- **Basic monitoring**

**Usage Estimation**:
```
1 vCPU = ~20-50 concurrent players (depends on game)
1.5 vCPU ≈ 30-75 players (WOS2.3 naval physics)
```

**Monitoring Usage**:
- Edgegap Dashboard → Billing
- Track vCPU hours used
- Set up alerts for quota limits

### Scaling Beyond Free Tier

**Paid Plans**:
- **Pay-as-you-go**: $0.50-2.00/vCPU-hour (location-dependent)
- **Enterprise**: Custom pricing for large deployments

**Cost Optimization**:
- Stop servers when not needed
- Use lower tick rates (naval games don't need 60Hz)
- Optimize player count per server
- Monitor and reduce bandwidth usage

---

## 🛠️ Troubleshooting

### Build Issues

**Problem**: "Linux build support not installed"

**Solution**:
1. Unity Hub → Installs → Unity 6000.0.55f1 → Add Modules
2. Check "Linux Build Support (Mono)"
3. Install and restart Unity

---

**Problem**: "Build failed - missing scenes"

**Solution**:
1. File → Build Settings
2. Add all required scenes (MainMenu, Main)
3. Verify scenes are enabled (checkbox checked)

---

**Problem**: "Server Build option missing"

**Solution**:
1. Update Unity to 2021.2+ (Server Build added in 2021.2)
2. Verify Platform is Linux (not Windows/Mac)

---

### Docker Issues

**Problem**: "Docker daemon not running"

**Solution**:
1. Start Docker Desktop
2. Wait for Docker to fully initialize (status shows "Running")
3. Verify: `docker --version`

---

**Problem**: "Permission denied when building Docker image"

**Solution** (Linux/Mac):
```bash
sudo usermod -aG docker $USER
# Then log out and back in
```

---

**Problem**: "Dockerfile not found"

**Solution**:
1. Verify Dockerfile exists at `Assets/Mirror/Hosting/Edgegap/Editor/Dockerfile`
2. Edgegap plugin handles Docker build automatically (don't build manually unless testing)

---

### Deployment Issues

**Problem**: "API token invalid"

**Solution**:
1. Edgegap Dashboard → Settings → API
2. Generate new token
3. Copy ENTIRE token (long string)
4. Paste in Unity Edgegap window
5. Click "Verify Token"

---

**Problem**: "Build upload failed - file too large"

**Solution**:
1. Check build size: `du -sh Builds/EdgegapServer/`
2. Typical size: 150-500 MB (acceptable)
3. If >1 GB: Remove unused assets, optimize textures
4. Edgegap limit: 5 GB (shouldn't hit with headless build)

---

**Problem**: "Deployment shows 'Failed' status"

**Solution**:
1. Edgegap Dashboard → Deployments → Click deployment
2. Check **Logs** tab for errors
3. Common errors:
   - **Executable not found**: Verify build name is `ServerBuild`
   - **Port binding failed**: Check port configuration (7777 TCP)
   - **Crash on startup**: Check Unity logs for errors

---

### Server Runtime Issues

**Problem**: "Server starts but clients can't connect"

**Solution**:
1. **Verify IP**: Use EXACT IP from Edgegap dashboard
2. **Test Port**: `telnet <ip> 7777` (should connect)
3. **Check Logs**: Edgegap dashboard → Logs (look for startup errors)
4. **Firewall**: Ensure client firewall allows outgoing TCP on 7777

---

**Problem**: "Server crashes after player connects"

**Solution**:
1. **Check Logs**: Edgegap dashboard → Logs
2. **Common Causes**:
   - Missing ScriptableObjects (ship configs)
   - Scene setup issues (missing spawn points)
   - NetworkIdentity configuration errors
3. **Test Locally**: Run headless build locally to reproduce

---

**Problem**: "High latency (ping >200ms)"

**Solution**:
1. **Check Location**: Edgegap deploys to nearest edge
2. **Manual Location**: In Edgegap settings, select specific region
3. **Network Stats**: Use ShipDebugUI network stats to monitor
4. **Expected**: <50ms local, <100ms regional, <200ms global

---

## 📚 Additional Resources

### Documentation
- **Edgegap Plugin Guide**: https://docs.edgegap.com/docs/tools-and-integrations/unity-plugin-guide
- **Mirror Networking Docs**: https://mirror-networking.gitbook.io/
- **Mirror Edgegap Guide**: https://docs.edgegap.com/docs/sample-projects/unity-netcodes/mirror-on-edgegap
- **Docker Getting Started**: https://docs.docker.com/get-started/

### Edgegap Dashboard
- **Main Dashboard**: https://app.edgegap.com/
- **Deployments**: https://app.edgegap.com/deployments
- **Applications**: https://app.edgegap.com/applications
- **Billing**: https://app.edgegap.com/billing

### Mirror Resources
- **Mirror Discord**: https://discord.gg/mirror
- **Mirror GitHub**: https://github.com/MirrorNetworking/Mirror
- **Mirror Forum**: https://forum.unity.com/forums/multiplayer.26/

---

## 🎯 Quick Reference

### Essential Commands

**Build Server** (Unity):
```
File → Build Settings → Platform: Linux → Server Build ✓ → Build
```

**Deploy to Edgegap** (Unity):
```
Tools → Edgegap Hosting → Deploy to Edgegap
```

**Test Locally**:
```bash
cd Builds/EdgegapServer
./ServerBuild -batchmode -nographics -server
```

**Test Docker** (advanced):
```bash
docker build -t wos23-server:test -f Assets/Mirror/Hosting/Edgegap/Editor/Dockerfile .
docker run -p 7777:7777 wos23-server:test
```

### File Checklist

**Before First Deployment**:
- ✅ Edgegap API token configured
- ✅ Docker Desktop installed and running
- ✅ Linux Build Support installed
- ✅ ServerLauncher in MainMenu scene
- ✅ WOSEdgegapBootstrap created and added to scene
- ✅ Build Settings configured (Linux, Server Build)
- ✅ Spawn points set up in Main scene

**Before Each Deployment**:
- ✅ Increment version tag (e.g., v0.3.1)
- ✅ Test locally if possible
- ✅ Check Edgegap quota usage
- ✅ Build succeeds without errors

---

## 📝 Summary

**What We Have**:
1. ✅ **WOSNetworkManager**: Handles player spawning and scene management
2. ✅ **ServerLauncher**: Auto-detects headless mode and starts server
3. ✅ **NetworkAddressManager**: Manages server IP addresses
4. ✅ **Edgegap Plugin**: Bundled with Mirror for one-click deployment
5. ✅ **Dockerfile**: Pre-configured for Unity Linux servers

**What You Need to Do**:
1. 📝 Set up Edgegap account and get API token
2. 🐳 Install Docker Desktop
3. 🐧 Install Linux Build Support in Unity Hub
4. 📝 Create WOSEdgegapBootstrap script (Tools → Edgegap → Create Bootstrap)
5. 🏗️ Build Linux headless server (File → Build Settings)
6. 🚀 Deploy to Edgegap (Tools → Edgegap Hosting → Deploy)
7. 🎮 Connect from client using Edgegap server IP

**Expected Results**:
- ✅ Server deploys in 2-5 minutes
- ✅ Players can connect from anywhere globally
- ✅ Low latency (<100ms to nearest edge)
- ✅ Handles 30-75 concurrent players (free tier)
- ✅ Automatic server management and scaling

---

**Ready to Deploy?** Follow Phase 1 → Phase 2 → Phase 3 above!

**Questions?** Check Troubleshooting section or Edgegap docs.

---

**Last Updated**: 2025-10-18
**Next Steps**: See Phase 1 for Unity Editor setup
