# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

WOS2.3_V2 is a Unity-based naval MMO game focused on realistic ship physics and economy systems. The project uses Unity 6000.0.55f1 with the Universal Render Pipeline (URP) for 2D rendering.

## Core Architecture

### ScriptableObject System
The game uses ScriptableObjects for configuration-driven development:
- **ShipConfigurationSO**: Naval ship physics parameters (displacement, turning radius, stopping distance)
- **CameraSettingsSO**: Camera behavior configuration with speed-based effects
- **CargoConfigurationSO**: Tetris-style spatial inventory system with weight/balance physics
- **UIConfigurationSO**: Professional UI theming and maritime-specific context

### Input System Architecture
Uses Unity's Input System with naval-specific action maps:
- **Naval Controls**: Steering (A/D), Throttle (W/S with 8-speed system), Emergency Stop (Space)
- **Navigation**: Waypoint placement (Right-click), Autopilot (Z), Clear waypoints (X)
- **Camera**: Zoom (Mouse wheel), Pan (Middle mouse + drag)

### Performance Optimization
- **Unity Job System**: Used in CameraController and SimpleNavalController for parallel processing
- **NativeCollections**: Managing waypoints and navigation data
- **Target**: 60+ FPS with <100MB memory usage

## Development Commands

### Unity Editor Operations
- Open Unity Hub and launch with Unity 6000.0.55f1
- Ensure URP 2D template is active
- Use Package Manager to verify all required packages are installed

### Testing Workflow
1. Create test scene in `Assets/Scenes/`
2. Add ship prefab with SimpleNavalController component
3. Assign ShipConfigurationSO asset to ship
4. Add camera with CameraController component
5. Assign CameraSettingsSO asset to camera
6. Enter Play Mode to test naval physics

### ScriptableObject Creation
Create via Unity's Asset menu:
- `Create > WOS > Ship > Ship Configuration`
- `Create > WOS > Camera > Camera Settings`
- `Create > WOS > Economy > Cargo Configuration`
- `Create > WOS > UI > UI Configuration`

## Code Standards

### Namespace Structure
All scripts use the `WOS` namespace with subsystem organization:
- `WOS.Player` - Ship controllers and player systems
- `WOS.Camera` - Camera controllers and effects
- `WOS.Economy` - Trading and inventory systems
- `WOS.ScriptableObjects` - Configuration assets

### Unity Version Compatibility
Code uses Unity 2023+ APIs:
- `linearVelocity` instead of deprecated `velocity` for Rigidbody2D
- `FindFirstObjectByType<T>()` instead of deprecated `FindObjectOfType<T>()`
- Input System package for controls (not legacy Input Manager)

### Performance Guidelines
- Use Job System for computationally intensive operations
- Implement object pooling for frequently spawned objects (waypoints, UI elements)
- Cache component references in Awake() or Start()
- Use NativeCollections for data that needs Job System processing

## Current Implementation Status

**Phase 2 Complete**: Foundation systems fully implemented
- ✅ Core ScriptableObject configurations
- ✅ Naval physics with 8-speed throttle system
- ✅ Advanced camera with look-ahead and speed effects
- ✅ Input System with naval-specific controls
- ✅ Job System optimization

**Next Phase (Phase 3)**: Economy Foundation
- Ready to implement inventory system
- Trading mechanics preparation
- Port interaction systems

## Testing Strategy

**4-Tier Testing System** (See `TESTING_STRATEGY.md` for full details):

### Tier 1: Editor Play Mode (Primary for most work)
**Use for**: UI, camera, audio, graphics, local scripts, client-only changes

```
1. Open MainMenu scene in Unity
2. Click Play
3. Click "Host (Server + Client)"
4. Test changes
5. Iterate rapidly
```

**Time**: 30 seconds per iteration
**Perfect for**: In-game menu, HUD, camera tweaks, visual feedback

### Tier 2: Local Build Testing
**Use for**: Validating Tier 1 changes, performance testing, multi-client on same PC

```powershell
# Build to test location
Unity → Build Settings → Build to D:\Updater\WOS_Builds\Test_Local\

# Run build
.\WavesOfSteel.exe
```

**Time**: 5-10 minutes per iteration
**Required before**: Client releases (use update_client.ps1)

### Tier 3: Docker Desktop Testing
**Use for**: NetworkBehaviour changes, server-specific logic, Linux compatibility

```powershell
# Build Linux server in Unity (Server Build + Headless Mode)
cd EdgegapServer
docker build -t wos-server:local .
docker run -d -p 7777:7777/udp --name wos-test wos-server:local

# View logs
docker logs -f wos-test

# Clean up
docker stop wos-test && docker rm wos-test
```

**Time**: 15-20 minutes per iteration
**Required for**: NetworkBehaviour changes before Edgegap deployment

### Tier 4: Edgegap Cloud Testing
**Use for**: Final validation, production environment, real network latency

```
Unity → Tools → Edgegap → Server Hosting
Build Docker Image
Deploy to Seattle datacenter
Update ServerConfig.asset with new IP
Test client connection
```

**Time**: 30-60 minutes per iteration
**Required for**: Pre-release validation, server updates

### Decision Matrix

| Change Type | Tier 1 | Tier 2 | Tier 3 | Tier 4 |
|-------------|--------|--------|--------|--------|
| UI/HUD | ✅ Primary | ✅ Validation | ❌ Skip | ❌ Skip |
| Camera/Graphics | ✅ Primary | ✅ Validation | ❌ Skip | ❌ Skip |
| Audio | ✅ Primary | ✅ Validation | ❌ Skip | ❌ Skip |
| Local Scripts | ✅ Primary | ✅ Validation | ❌ Skip | ❌ Skip |
| NetworkBehaviour (minor) | ⚠️ Basic | ✅ Primary | ✅ Validation | ⚠️ Final check |
| NetworkBehaviour (major) | ⚠️ Basic | ✅ Primary | ✅ Required | ✅ Pre-release |
| Server Logic | ❌ Cannot test | ⚠️ Limited | ✅ Primary | ✅ Validation |
| Pre-Release | ❌ Not sufficient | ✅ Required | ✅ Recommended | ✅ **MUST DO** |

**Best Practice**: Always start with Tier 1 (Editor). Only move to higher tiers when needed.

**Time Saved**: Using tiered approach saves ~80% of testing time vs. always deploying to Edgegap.

**Quick Reference**: See `TESTING_QUICK_START.md` for fast decision flowchart.

## Server Connection & Beta Testing Workflow

**Status**: ✅ **FULLY IMPLEMENTED AND TESTED** - Beta testing workflow problem solved!

### The Problem (Before)
Traditional hardcoded server IP approach caused major beta testing friction:
- ❌ Redeploy server → IP/ports change
- ❌ Rebuild client with new hardcoded IP
- ❌ Redistribute 500MB build to all beta testers
- ❌ Repeat every time server is deployed
- ❌ Beta testers get update fatigue and quit

### The Solution (Now)
**Dual-Approach Server Discovery**: Primary (Backend API) + Fallback (FQDN)

**Benefits**:
- ✅ Build client **ONCE** with ServerBrowserManager configured
- ✅ Distribute to beta testers **ONCE**
- ✅ Redeploy server **as many times as needed** for development
- ✅ Backend API auto-discovers new IP and ports
- ✅ All beta testers connect automatically
- ✅ **ZERO client rebuilds for server deployments**

### Architecture

**Primary Approach: Backend API Auto-Discovery**
- Unity queries backend API on startup
- Backend discovers current Edgegap deployment
- Returns: IP, game port, health port, status
- Unity auto-updates ServerConfig and reconnects
- **Zero manual configuration on redeploy**

**Fallback Approach: Static FQDN**
- Uses Edgegap FQDN: `530c50ac1da5.pr.edgegap.net`
- DNS resolves to current IP automatically
- Works when backend API is unavailable
- Provides reliable backup connection method

### Current Configuration

**Active Edgegap Deployment**:
- Request ID: `530c50ac1da5`
- FQDN: `530c50ac1da5.pr.edgegap.net` (static for this deployment)
- IP: `172.232.162.171` (dynamic - auto-discovered)
- Game Port: `31058` (External UDP → Internal 7777)
- Health Port: `31269` (External TCP → Internal 8080)
- Location: Seattle, Washington
- Transport: KCP (UDP)

**Backend API**:
- URL: `https://wos-edgegap-proxy.onrender.com/api/servers`
- Status: ✅ **FULLY OPERATIONAL**
- Response Time: ~15-19ms
- Auto-discovers server IP and ports on each query

**ServerConfig.asset** (Fallback):
```yaml
serverAddress: 530c50ac1da5.pr.edgegap.net:31058
serverLocation: Seattle, Washington
useLocalhostInEditor: false
```

### How It Works

**Normal Operation** (Primary - Backend API):
```
Unity Startup
   ↓
ServerBrowserManager.Start()
   ↓
Query: https://wos-edgegap-proxy.onrender.com/api/servers
   ↓
Backend queries Edgegap API
   ↓
Returns: { ip: "172.232.162.171", port: 31058, healthPort: 31269 }
   ↓
Auto-update ServerConfig with current IP/ports
   ↓
Restart health check with updated config
   ↓
Connect to server ✅
```

**Fallback Operation** (when backend unavailable):
```
Unity Startup
   ↓
ServerBrowserManager.Start()
   ↓
Query Backend API
   ↓
❌ Backend timeout or error
   ↓
Load ServerConfig.asset (FQDN)
   ↓
DNS resolves FQDN to current IP
   ↓
Connect using FQDN ✅
```

### What Changes on Redeploy?

**NEVER Changes (Static)**:
- ✅ FQDN: `530c50ac1da5.pr.edgegap.net` (for this deployment)
- ✅ Request ID: `530c50ac1da5`
- ✅ Internal Ports: 7777 (game), 8080 (health)

**MIGHT Change (Auto-Discovered by Backend)**:
- ⚠️ IP Address: Backend API auto-discovers
- ⚠️ External Game Port: Backend API auto-discovers
- ⚠️ External Health Port: Backend API auto-discovers

**Result**: Client works across all redeployments with zero updates needed!

### Implementation Files

**Core Components**:
- `Assets/Scripts/Networking/ServerBrowserManager.cs` - Backend API integration
- `Assets/Scripts/UI/JoinMenuController.cs` - Health check and connection
- `Assets/Resources/ServerConfigs/ServerConfig.asset` - Fallback configuration
- `Assets/Scenes/MainMenu.unity` - ServerBrowserManager GameObject configured

**Unity Editor Setup**:
- ServerBrowserManager GameObject in MainMenu scene
- Backend API URL: `https://wos-edgegap-proxy.onrender.com`
- Fallback Config: ServerConfig.asset assigned
- Auto-select best server: Enabled
- Connected to JoinMenuController

### When to Update ServerConfig

**Update ServerConfig.asset ONLY if**:
1. Creating a NEW Edgegap deployment (new Request ID)
2. Backend API is down AND ports changed
3. Switching to different server location

**DO NOT update ServerConfig.asset for**:
- ❌ Normal server redeployments (backend handles it)
- ❌ IP changes (DNS resolves automatically)
- ❌ Port changes (backend discovers automatically)

### Detailed Documentation

For complete setup and troubleshooting guides, see:
- **`SERVER_BROWSER_SETUP.md`** - Complete Unity Editor setup guide
- **`UNITY_FIXES.md`** - Dual-approach implementation details
- **`SECURE_SERVER_INTEGRATION_GUIDE.md`** - Backend API architecture

### Testing Workflow with Server Discovery

**Development Cycle**:
1. Make server changes in Unity
2. Build Linux server (Tier 3 or 4)
3. Deploy to Edgegap
4. **That's it!** - All clients auto-discover new deployment
5. No client rebuild needed ✅

**Beta Testing Workflow**:
1. Build client with ServerBrowserManager configured
2. Distribute to beta testers
3. Develop and redeploy server as needed
4. Beta testers automatically connect to latest deployment
5. Update client only for gameplay/UI changes

## Key Implementation Files

### Core Systems
- `Assets/Scripts/Player/SimpleNavalController.cs` - Naval physics and movement
- `Assets/Scripts/Camera/CameraController.cs` - Advanced camera system
- `Assets/InputSystem_Actions.inputactions` - Input configuration

### Configuration Assets
- `Assets/ScriptableObjects/Configs/ShipConfigurationSO.cs`
- `Assets/ScriptableObjects/Configs/CameraSettingsSO.cs`
- `Assets/ScriptableObjects/Configs/CargoConfigurationSO.cs`
- `Assets/ScriptableObjects/Configs/UIConfigurationSO.cs`

## Implementation Guide References
- Main guide: `WOS2.3_V2_IMPLEMENTATION_GUIDE.md`
- Current status: `IMPLEMENTATION_STATUS.md`
- Setup instructions: `SETUP_TEST_GUIDE.md`

## Update Workflows

Complete deployment workflows for Client, Server, and Launcher updates. See `UPDATE_WORKFLOWS.md` for full details.

### Quick Decision: Client-Only vs Server+Client

**Client-Only Update** (UI, graphics, audio, camera, local scripts):
- ✅ Build Windows client only
- ✅ Run `D:\Updater\Scripts\update_client.ps1`
- ✅ No server rebuild needed
- ✅ No downtime

**Server+Client Update** (NetworkBehaviour, SyncVars, gameplay mechanics):
- ✅ Build Windows client
- ✅ Build Linux server
- ✅ Deploy to Edgegap
- ⚠️ 2-5 minute downtime

### Automated Client Update

When user requests a client-only update:

```powershell
# 1. User builds in Unity to: D:\Updater\WOS_Builds\Version_X.X.X\

# 2. Run automation script:
D:\Updater\Scripts\update_client.ps1 -NewVersion "X.X.X" -Description "Brief changelog"

# 3. Follow PatchManager manual step (opens automatically)

# 4. Script completes:
#    - Copies patches to CDN
#    - Updates news JSON
#    - Pushes to GitHub
```

### Manual Server+Client Update

When NetworkBehaviour or gameplay code changes:

1. **Build Windows Client**:
   - Unity → Build Settings → Windows
   - Build to: `D:\Updater\WOS_Builds\Version_X.X.X\`

2. **Build Linux Server**:
   - Unity → Build Settings → Linux
   - Enable: "Server Build" + "Headless Mode"
   - Build to: `EdgegapServer/`

3. **Update Client** (use automation script above)

4. **Deploy Server to Edgegap**:
   - Unity → Tools → Edgegap → Server Hosting
   - Build Docker Image
   - Push to Registry
   - Update existing app version (NOT create new application)
   - Deploy updated version

5. **Update ServerConfig** (if IP changed):
   - Edit: `Assets/Resources/ServerConfigs/ServerConfig.asset`
   - Update IP and port
   - Rebuild client and re-run automation

### Edgegap Deployment Notes

**IMPORTANT**: When deploying server updates:
- ❌ DO NOT create a new application each time
- ✅ UPDATE existing app version OR create new version
- Application: "wos-naval-mmo" (created once)
- Versions: 1.0.A → 1.0.B → 1.0.C (increment)

**Current Server**:
- IP: 172.232.162.171
- Port: 30509 (external) → 7777 (internal)
- Location: Seattle, Washington
- Transport: KCP (UDP)

### News Update Automation

Update news JSON manually or via script:

```powershell
D:\Updater\Scripts\update_news.ps1 `
  -VersionFrom "1.0.2" `
  -VersionTo "1.0.3" `
  -UpdateType "Client" `
  -Description "In-game menu added`nUI improvements`nBug fixes"
```

### File Locations Reference

```
D:\Updater\
├── WOS_Builds\Version_X.X.X\      # Unity Windows builds
├── Manager\
│   ├── PatchManager.exe           # Creates patches
│   ├── App Workspace\Release\     # For game updates
│   └── Launcher Workspace\        # For launcher updates
├── Scripts\
│   ├── update_client.ps1          # Client update automation
│   └── update_news.ps1            # News JSON automation
└── game-launcher-cdn\             # Git repo (GitHub Pages)
    ├── App\Release\               # Final game patches
    ├── Launcher\                  # Final launcher patches
    └── News\Release\              # News JSON files

EdgegapServer\                     # Linux server builds (in Unity project)
```

### Common Issues

**"PatchManager fails"**:
- Verify Unity build is in `WOS_Builds\Version_X.X.X\`
- Check version number is incremented
- Ensure previous version exists in `Versions/` folder

**"News not showing in launcher"**:
- Verify JSON syntax is valid
- Wait 1-2 minutes for GitHub Pages deployment
- Check GitHub Actions tab for deployment status

**"Server deployment fails"**:
- Verify Docker is running
- Check Edgegap API key in Unity
- Review container logs in Edgegap dashboard

**"Players can't connect after server update"**:
- Verify ServerConfig.asset has new IP
- Rebuild client with updated IP
- Re-run update automation
- Check Edgegap port mapping (7777 → external)

## Patch Notes Management System

**Two-Product Architecture**: Separate tracking for **Game** (WOS2.3_V2) and **Launcher** with independent versioning.

### 🎯 Key Concept: Two Separate Products

**Game** and **Launcher** are distinct products with independent versions:
- Game v1.0.3 can release while Launcher is still v1.0.5
- Launcher v1.0.6 can release while Game is still v1.0.3
- Each has its own CHANGELOG, templates, and patch notes directory

### Quick Start: Publishing a Game Patch

```powershell
# 1. Create patch notes from Game template
Copy-Item "D:\GitFolder\UnityProjects\WOS2.3_V2\PatchNotes\Game\template.md" `
          "D:\GitFolder\UnityProjects\WOS2.3_V2\PatchNotes\Game\1.0.X.md"

# 2. Edit patch notes (write features, fixes, improvements)
code "D:\GitFolder\UnityProjects\WOS2.3_V2\PatchNotes\Game\1.0.X.md"

# 3. Build game in Unity to D:\Updater\WOS_Builds\Version_1.0.X\

# 4. Publish with ProductType parameter
cd D:\Updater\Scripts
.\publish_patch.ps1 -ProductType "Game" -Version "1.0.X" `
  -PatchNotesPath "D:\GitFolder\UnityProjects\WOS2.3_V2\PatchNotes\Game\1.0.X.md" `
  -ClientUpdate `
  -CreateGitHubRelease `
  -PostToDiscord `
  -UpdateChangelog `
  -UpdateType "Client"
```

### Quick Start: Publishing a Launcher Patch

```powershell
# 1. Create patch notes from Launcher template
Copy-Item "D:\GitFolder\UnityProjects\WOS2.3_V2\PatchNotes\Launcher\template.md" `
          "D:\GitFolder\UnityProjects\WOS2.3_V2\PatchNotes\Launcher\1.0.X.md"

# 2. Edit patch notes
code "D:\GitFolder\UnityProjects\WOS2.3_V2\PatchNotes\Launcher\1.0.X.md"

# 3. Build launcher in PatchManager

# 4. Publish with ProductType parameter
cd D:\Updater\Scripts
.\publish_patch.ps1 -ProductType "Launcher" -Version "1.0.X" `
  -PatchNotesPath "D:\GitFolder\UnityProjects\WOS2.3_V2\PatchNotes\Launcher\1.0.X.md" `
  -ClientUpdate `
  -PostToDiscord `
  -UpdateChangelog
```

**This automates**:
- ✅ Game Launcher news feed update (with clear product labels)
- ✅ GitHub release creation with product-specific tag (game-v1.0.X or launcher-v1.0.X)
- ✅ Discord announcement
- ✅ Correct CHANGELOG update (CHANGELOG_GAME.md or CHANGELOG_LAUNCHER.md)
- ✅ CDN deployment to GitHub Pages

**Time**: ~5 minutes (mostly PatchManager manual step)

### Patch Notes File Structure

```
D:\GitFolder\UnityProjects\WOS2.3_V2\
├── CHANGELOG_GAME.md         # Game version history (1.0.0 → current)
├── CHANGELOG_LAUNCHER.md     # Launcher version history (1.0.5 → current)
├── PatchNotes\
│   ├── README.md             # Explains two-product system
│   ├── Game\
│   │   ├── template.md       # Game-specific template
│   │   ├── 1.0.0.md         # Game versions
│   │   ├── 1.0.1.md
│   │   ├── 1.0.2.md         # Current game version
│   │   └── 1.0.X.md         # Future game patches
│   └── Launcher\
│       ├── template.md       # Launcher-specific template
│       ├── legacy-versions.md # Historical documentation (1.0.0-1.0.4)
│       ├── 1.0.5.md         # Current launcher version
│       └── 1.0.X.md         # Future launcher patches
└── CLAUDE.md                 # This file

D:\Updater\
├── PATCH_NOTES_SYSTEM.md     # Comprehensive system documentation
└── Scripts\
    ├── README.md             # Script reference guide
    ├── publish_patch.ps1     # ⭐ Main orchestration script (UPDATED with -ProductType)
    ├── create_release.ps1    # GitHub release automation
    ├── post_discord.ps1      # Discord webhook poster
    ├── update_news.ps1       # Game launcher news (existing)
    └── update_client.ps1     # Client workflow (existing)
```

### Script Reference

**Primary Script**: `publish_patch.ps1` - Use this for all releases

**Required Parameters**:
- `-ProductType` - **"Game" | "Launcher"** (REQUIRED - specifies which product)
- `-Version` - Version number (e.g., "1.0.3")
- `-PatchNotesPath` - Path to patch notes file

**Action Flags**:
- `-ClientUpdate` - Update game launcher news feed and push to CDN
- `-CreateGitHubRelease` - Create GitHub release with product-specific tag
- `-PostToDiscord` - Post announcement to Discord webhook
- `-UpdateChangelog` - Append to correct CHANGELOG (CHANGELOG_GAME.md or CHANGELOG_LAUNCHER.md)

**Optional Parameters**:
- `-UpdateType` - "Client" | "Server" | "Hotfix" | "Launcher" (default: "Client")
- `-VideoURL` - YouTube trailer/showcase video
- `-ImageURL` - Banner image for launcher
- `-InteractionURL` - Link to patch notes website

**Product-Specific Behavior**:
- **Game**: Uses `PatchNotes/Game/`, updates `CHANGELOG_GAME.md`, creates tag `game-v1.0.X`
- **Launcher**: Uses `PatchNotes/Launcher/`, updates `CHANGELOG_LAUNCHER.md`, creates tag `launcher-v1.0.X`

### Platform Distribution

| Platform | Updated By | Format | Character Limits |
|----------|-----------|--------|------------------|
| **Game Launcher** | `publish_patch.ps1 -ClientUpdate` | JSON (2 main news + 4 SubNews) | ~500 chars recommended |
| **GitHub** | `publish_patch.ps1 -CreateGitHubRelease` | Markdown with tag | No limit |
| **Discord** | `publish_patch.ps1 -PostToDiscord` | Rich embed | Title: 256, Desc: 4096 |
| **CHANGELOG** | `publish_patch.ps1 -UpdateChangelog` | Markdown | No limit |

### Prerequisites (Already Configured)

**GitHub CLI** (for release automation):
- ✅ Installed: `gh --version`
- ✅ Authenticated: `gh auth status`

**Discord Webhook** (for announcements):
- ✅ Environment variable: `$env:WOS_DISCORD_WEBHOOK`
- ✅ Webhook URL stored securely

**Note**: User has Discord Claude Bot in another session - can be used for additional automation.

### Example Workflows

**Game Update (Client-Only)**:
```powershell
.\publish_patch.ps1 -ProductType "Game" -Version "1.0.3" `
  -PatchNotesPath "D:\GitFolder\UnityProjects\WOS2.3_V2\PatchNotes\Game\1.0.3.md" `
  -ClientUpdate -PostToDiscord -UpdateChangelog `
  -UpdateType "Client"
```

**Game Hotfix Release**:
```powershell
.\publish_patch.ps1 -ProductType "Game" -Version "1.0.3" `
  -PatchNotesPath "D:\GitFolder\UnityProjects\WOS2.3_V2\PatchNotes\Game\1.0.3.md" `
  -ClientUpdate -CreateGitHubRelease -PostToDiscord `
  -UpdateType "Hotfix"
```

**Game Major Release (Full Distribution)**:
```powershell
.\publish_patch.ps1 -ProductType "Game" -Version "1.1.0" `
  -PatchNotesPath "D:\GitFolder\UnityProjects\WOS2.3_V2\PatchNotes\Game\1.1.0.md" `
  -ClientUpdate -CreateGitHubRelease -PostToDiscord -UpdateChangelog `
  -UpdateType "Client" `
  -VideoURL "https://youtu.be/VIDEO_ID" `
  -InteractionURL "https://wavesofsteel.com/patch-notes/1.1.0"
```

**Launcher Update**:
```powershell
.\publish_patch.ps1 -ProductType "Launcher" -Version "1.0.6" `
  -PatchNotesPath "D:\GitFolder\UnityProjects\WOS2.3_V2\PatchNotes\Launcher\1.0.6.md" `
  -ClientUpdate -PostToDiscord -UpdateChangelog
```

**Manual News Update Only** (when patch notes not needed):
```powershell
.\update_news.ps1 -VersionFrom "1.0.2" -VersionTo "1.0.3" `
  -UpdateType "Client" `
  -Description "Quick fixes`n- Fixed crash`n- Improved performance"
```

### Version Numbering Strategy

**Semantic Versioning**: `MAJOR.MINOR.PATCH`

- **MAJOR** (1.x.x): Breaking changes, major features, complete overhauls
- **MINOR** (x.1.x): New features, non-breaking additions
- **PATCH** (x.x.1): Bug fixes, hotfixes, minor improvements

**Examples**:
- `1.0.0` → `1.0.1`: Hotfix (crash fix, minor bug)
- `1.0.1` → `1.1.0`: New feature (in-game menu system)
- `1.1.0` → `2.0.0`: Major overhaul (new economy system)

### Patch Notes Writing Guidelines

**Template Structure** (see `PatchNotes/template.md`):
```markdown
# PATCH X.X.X - [Title]
**Release Date**: YYYY-MM-DD
**Type**: Client Update | Server Update | Hotfix

## Summary
1-2 sentence overview

## New Features
- Feature 1
- Feature 2

## Improvements
- Improvement 1

## Bug Fixes
- Fixed issue X
- Resolved problem Y

## Known Issues
- Issue 1 (workaround if available)

## Technical Details
Build, compatibility, requirements

## Media
Videos, screenshots, documentation links
```

**Best Practices**:
- ✅ Use clear, user-friendly language (not technical jargon)
- ✅ Organize by category (Features, Fixes, Improvements)
- ✅ Include video for major features
- ✅ Keep launcher version short (5-7 bullet points max)
- ✅ Use dashes (-) for bullet points, not Unicode bullets (encoding issues)
- ✅ Mention breaking changes prominently
- ❌ Don't skip version numbers
- ❌ Don't use inconsistent formatting

### Workflow Integration

The patch notes system integrates seamlessly with existing update workflows:

**Combined Client Update + Patch Notes**:
1. User builds in Unity → `D:\Updater\WOS_Builds\Version_X.X.X\`
2. Create patch notes → `PatchNotes/X.X.X.md`
3. Run `publish_patch.ps1` → Calls `update_client.ps1` internally
4. PatchManager manual step (user confirms)
5. Automatic distribution to all platforms

**Flow**:
```
Unity Build → Patch Notes → publish_patch.ps1 → [
  ├─ update_client.ps1 (CDN + News)
  ├─ create_release.ps1 (GitHub)
  ├─ post_discord.ps1 (Discord)
  └─ CHANGELOG.md update
] → Live on all platforms
```

### Detailed Documentation

For comprehensive details, see:
- **`D:\Updater\PATCH_NOTES_SYSTEM.md`** - Complete system overview
- **`D:\Updater\Scripts\README.md`** - Script usage reference
- **`D:\GitFolder\UnityProjects\WOS2.3_V2\CHANGELOG.md`** - Version history
- **`UPDATE_WORKFLOWS.md`** - Deployment workflows

### Troubleshooting Patch System

**GitHub Release Fails**:
- Check: `gh auth status`
- Verify repository access
- Ensure tag doesn't already exist: `git tag -l`

**Discord Post Fails**:
- Verify webhook URL: `echo $env:WOS_DISCORD_WEBHOOK`
- Check character limits (title: 256, description: 4096)
- Test webhook separately with curl

**Patch Notes Not Found**:
- Verify path: `D:\GitFolder\UnityProjects\WOS2.3_V2\PatchNotes\X.X.X.md`
- Check filename matches version number exactly
- Ensure .md extension

**News Feed Not Updating**:
- Wait 1-2 minutes for GitHub Pages deployment
- Check: https://github.com/AsloWaves/GameLuncher/actions
- Verify `gh-pages` branch updated
- Clear launcher cache

### Future Claude Sessions

**When user asks about patch notes or publishing updates**:
1. **Determine product first**: Is this a Game update or Launcher update?
2. Reference this section for quick commands
3. Check `PatchNotes/README.md` for two-product system overview
4. Check `PATCH_NOTES_SYSTEM.md` for comprehensive details
5. Use `publish_patch.ps1` with `-ProductType` parameter
6. Follow semantic versioning (MAJOR.MINOR.PATCH) independently for each product
7. Always create patch notes in correct subdirectory (`PatchNotes/Game/` or `PatchNotes/Launcher/`)
8. Verify prerequisites (GitHub CLI, Discord webhook) before running

**Key Files to Check**:
- **System overview**: `PatchNotes/README.md` (explains two-product architecture)
- **Game template**: `PatchNotes/Game/template.md`
- **Launcher template**: `PatchNotes/Launcher/template.md`
- **Game version history**: `CHANGELOG_GAME.md`
- **Launcher version history**: `CHANGELOG_LAUNCHER.md`
- **Scripts location**: `D:\Updater\Scripts\`
- **Current game version**: Check `game-launcher-cdn/App/Release/VersionInfo.info`
- **Current launcher version**: Check launcher about screen or build

**Common Mistake to Avoid**:
- ❌ DON'T use `publish_patch.ps1` without `-ProductType`
- ❌ DON'T put Game patch notes in `PatchNotes/Launcher/` or vice versa
- ❌ DON'T update `CHANGELOG.md` (it's now `CHANGELOG_GAME.md` and `CHANGELOG_LAUNCHER.md`)
- ❌ DON'T assume Game and Launcher have the same version number