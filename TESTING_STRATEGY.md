# Testing Strategy - WOS Naval MMO

Comprehensive testing workflow from rapid iteration to production deployment.

---

## Overview: 4-Tier Testing System

```
Tier 1: Editor Play Mode (Fastest - Minutes)
    ↓
Tier 2: Local Builds (Fast - 5-10 minutes)
    ↓
Tier 3: Docker Desktop (Moderate - 15-20 minutes)
    ↓
Tier 4: Edgegap Cloud (Slow - 30+ minutes)
```

**Rule of Thumb**: Use the fastest tier that can validate your changes.

---

## Tier 1: Editor Play Mode Testing

**When to Use**:
- ✅ Client-only changes (UI, camera, audio, graphics)
- ✅ Local scripts (no NetworkBehaviour changes)
- ✅ Visual feedback and rapid iteration
- ✅ Single-player mechanics
- ✅ **CURRENT: In-game menu testing**

**Cannot Test**:
- ❌ NetworkBehaviour changes
- ❌ Server-specific logic
- ❌ Build-specific issues
- ❌ Multi-client interactions (limited)
- ❌ Performance in builds

### How to Use

**Method 1: Host in Editor (Recommended)**

1. Open **MainMenu** scene in Unity Editor
2. Click **Play** button
3. Click **"Host (Server + Client)"** in UI
4. **Test your changes** (menu, UI, camera, etc.)
5. **Stop** when done

**What Happens**:
- Unity acts as both server AND client
- Mirror spawns your ship locally
- All local scripts work normally
- **Perfect for UI testing** (your current menu work)

**Method 2: ParrelSync for Multi-Client Testing** (Optional)

If you need to test multiple clients in Editor:

1. Install **ParrelSync** package (free on Unity Asset Store)
2. Clone your project
3. Run Host in main Editor
4. Run Client in cloned Editor
5. Both connect to same local server

**Pros**: Fastest iteration (no builds needed)
**Cons**: Limited multiplayer testing

---

## Tier 2: Local Build Testing

**When to Use**:
- ✅ Testing actual build behavior
- ✅ Client-only changes that need build validation
- ✅ Performance testing on target hardware
- ✅ Multiple clients on same PC
- ✅ Input system verification

**Cannot Test**:
- ❌ Dedicated server behavior
- ❌ Linux server issues
- ❌ Network latency (localhost only)
- ❌ Geographic distribution

### How to Use

**Step 1: Build Windows Client**

1. Unity → **File → Build Settings**
2. Platform: **Windows**
3. Target: **Windows x64**
4. Build to: `D:\Updater\WOS_Builds\Test_Local\`
5. Click **Build**

**Step 2: Test Locally**

**Option A: Single Instance (Host + Client)**
```powershell
# Run the build
.\WavesOfSteel.exe

# In game:
# 1. Click "Host (Server + Client)"
# 2. Test your changes
# 3. Works like Editor Play Mode but in actual build
```

**Option B: Multiple Instances (Host + Multiple Clients)**

```powershell
# Terminal 1 - Start Host
.\WavesOfSteel.exe
# In game: Click "Host (Server + Client)"

# Terminal 2 - Start Client 1
.\WavesOfSteel.exe
# In game: Click "Join Game"

# Terminal 3 - Start Client 2 (optional)
.\WavesOfSteel.exe
# In game: Click "Join Game"
```

**Pro Tip**: Run in windowed mode (1280x720) for easier multi-window testing.

**Pros**:
- Tests actual build
- Multiple clients possible
- No Docker needed

**Cons**:
- Slower than Editor
- All on localhost (no real network testing)

---

## Tier 3: Docker Desktop Testing

**When to Use**:
- ✅ NetworkBehaviour changes (SyncVars, Commands, RPCs)
- ✅ Server-specific logic testing
- ✅ Linux server compatibility verification
- ✅ Container behavior before Edgegap
- ✅ Headless server testing

**Cannot Test**:
- ❌ Geographic latency
- ❌ Edgegap-specific deployment issues
- ❌ Public internet connectivity

### Prerequisites

**Install Docker Desktop** (if not already):
```powershell
# Download from: https://www.docker.com/products/docker-desktop
# Or use winget:
winget install Docker.DockerDesktop

# After install, verify:
docker --version
docker-compose --version
```

**Ensure WSL2 enabled**:
```powershell
wsl --set-default-version 2
wsl --list --verbose
```

### How to Use

**Step 1: Build Linux Server**

1. Unity → **File → Build Settings**
2. Platform: **Linux**
3. Target: **Linux x64**
4. ✅ Enable **"Server Build"**
5. ✅ Enable **"Headless Mode"**
6. Build to: `EdgegapServer/`
7. Click **Build**

**Step 2: Create Dockerfile** (if doesn't exist)

Create `EdgegapServer/Dockerfile`:
```dockerfile
FROM ubuntu:20.04

# Install dependencies
RUN apt-get update && apt-get install -y \
    libgdiplus \
    && rm -rf /var/lib/apt/lists/*

# Copy server files
WORKDIR /app
COPY . .

# Make server executable
RUN chmod +x WavesOfSteel.x86_64

# Expose Mirror port (internal Docker port)
EXPOSE 7777/udp

# Run server
CMD ["./WavesOfSteel.x86_64", "-batchmode", "-nographics"]
```

**Step 3: Build Docker Image**

```powershell
cd D:\GitFolder\UnityProjects\WOS2.3_V2\EdgegapServer

docker build -t wos-server:local .
```

**Step 4: Run Server Container**

```powershell
# Run server on localhost:7777
docker run -d -p 7777:7777/udp --name wos-test-server wos-server:local

# Check server logs
docker logs wos-test-server

# Follow logs in real-time
docker logs -f wos-test-server
```

**Step 5: Connect Client to Local Docker Server**

1. Open **ServerConfig.asset** in Unity
2. Temporarily change:
   - Server Address: **"localhost:7777"** (or **"127.0.0.1:7777"**)
3. Build Windows client (or test in Editor)
4. Click **"Join Game"** in client
5. Should connect to Docker container

**Step 6: Test and Iterate**

```powershell
# Stop container
docker stop wos-test-server

# Remove container
docker rm wos-test-server

# Rebuild server after changes
# (Repeat Steps 1-5)
```

**Pros**:
- Tests Linux server locally
- Container behavior verification
- Faster than Edgegap deployment
- No internet needed

**Cons**:
- Slower than Editor/Local builds
- Still localhost (no real latency)
- Docker overhead

---

## Tier 4: Edgegap Cloud Testing

**When to Use**:
- ✅ Final validation before release
- ✅ Geographic latency testing
- ✅ Public internet connectivity
- ✅ Multi-region testing
- ✅ Production environment verification

**Step 1: Build & Deploy**

1. Build Linux server (same as Tier 3, Step 1)
2. Unity → **Tools → Edgegap → Server Hosting**
3. **Build Docker Image** (creates and pushes to registry)
4. **Deploy** to Seattle datacenter
5. Wait for deployment (~5 minutes)

**Step 2: Update Client**

1. Get new server IP from Edgegap dashboard
2. Update **ServerConfig.asset**:
   - Server Address: `[NEW_IP]:[PORT]`
3. Build client or test in Editor

**Step 3: Test Over Internet**

- Test from your PC (same network as deployment)
- Test from different network (mobile hotspot, friend's house)
- Test from different geographic region (if possible)
- Monitor Edgegap logs and metrics

**Pros**:
- Production environment
- Real latency testing
- Geographic distribution

**Cons**:
- Slowest iteration cycle
- Costs money (per deployment)
- Internet required

---

## Decision Matrix: Which Tier to Use?

| Change Type | Tier 1 (Editor) | Tier 2 (Builds) | Tier 3 (Docker) | Tier 4 (Edgegap) |
|-------------|----------------|----------------|-----------------|------------------|
| **UI Changes** (Menu, HUD) | ✅ Primary | ✅ Validation | ❌ Not needed | ❌ Not needed |
| **Camera/Graphics** | ✅ Primary | ✅ Validation | ❌ Not needed | ❌ Not needed |
| **Audio** | ✅ Primary | ✅ Validation | ❌ Not needed | ❌ Not needed |
| **Input System** | ✅ Primary | ✅ Validation | ❌ Not needed | ❌ Not needed |
| **Local Scripts** (no network) | ✅ Primary | ✅ Validation | ❌ Not needed | ❌ Not needed |
| **NetworkBehaviour** (minor) | ⚠️ Basic test | ✅ Primary | ✅ Validation | ⚠️ Final check |
| **NetworkBehaviour** (major) | ⚠️ Basic test | ✅ Primary | ✅ Required | ✅ Final validation |
| **Server Logic** | ❌ Cannot test | ⚠️ Limited | ✅ Primary | ✅ Validation |
| **Performance** | ⚠️ Approximate | ✅ Primary | ✅ Validation | ✅ Final check |
| **Pre-Release** | ❌ Not sufficient | ✅ Required | ✅ Recommended | ✅ **MUST DO** |

---

## Testing Workflow Examples

### Example 1: In-Game Menu (Current Work)

**Tier 1 Only**:
```
1. Open MainMenu scene in Unity Editor
2. Click Play
3. Click "Host (Server + Client)"
4. Press ESC → Test menu
5. Click all buttons → Verify functionality
6. Test cursor lock/unlock
7. Stop Play Mode
8. Iterate and repeat
```

**Time**: 30 seconds per iteration
**Good Enough?**: YES for client-only UI

**Optional Tier 2** (before release):
```
1. Build Windows client
2. Run build
3. Host game
4. Test menu one more time
5. Verify no build-specific issues
```

**Time**: 5 minutes
**Recommended?**: YES before deploying to players

---

### Example 2: Ship Physics Tweak (Client-Only)

**Tier 1 Primary**:
```
1. Editor Play Mode
2. Adjust physics values
3. Test ship movement
4. Iterate rapidly
```

**Tier 2 Validation**:
```
1. Build Windows client
2. Test actual performance
3. Verify physics consistency
```

**Skip Tier 3/4**: Not needed (client-only change)

---

### Example 3: New NetworkBehaviour SyncVar

**Tier 1 Basic Test**:
```
1. Editor Play Mode (Host)
2. Verify SyncVar works locally
3. Basic functionality check
```

**Tier 2 Primary Testing**:
```
1. Build Windows client
2. Run 2 instances (Host + Client)
3. Verify SyncVar synchronizes
4. Test on localhost
```

**Tier 3 Required**:
```
1. Build Linux server
2. Run in Docker Desktop
3. Connect client to Docker server
4. Verify server-client sync works
5. Test headless server behavior
```

**Tier 4 Final Validation**:
```
1. Deploy to Edgegap
2. Test with real latency
3. Verify production behavior
```

---

### Example 4: Major Server Update (Economy System)

**All Tiers Required**:

**Tier 1** (5% of testing time):
- Basic logic verification
- UI hookup testing

**Tier 2** (25% of testing time):
- Client-server interaction
- Multiple clients on localhost
- Data synchronization

**Tier 3** (40% of testing time):
- Linux server compatibility
- Headless server testing
- Container behavior
- Server logic validation

**Tier 4** (30% of testing time):
- Production environment
- Real network conditions
- Load testing
- Final validation

---

## Recommended Workflow for Current Session

Since you're working on **in-game menu (client-only)**:

### Phase 1: Development (Tier 1)
```
1. Open MainMenu scene
2. Set up UI in Editor
3. Click Play
4. Test ESC menu
5. Iterate until perfect
```

**Time**: 15-30 minutes
**Iterations**: As many as needed

### Phase 2: Validation (Tier 2)
```
1. Build Windows client
2. Test in actual build
3. Verify no Editor-specific issues
4. Test cursor behavior
5. Test all buttons
```

**Time**: 5-10 minutes
**When**: Once menu works in Editor

### Phase 3: Skip Tier 3 & 4
- Not needed for client-only changes
- Save time and focus on next features

### Phase 4: Pre-Release (Tier 2 again)
```
1. Final build before deploying
2. Full playthrough
3. Verify no regressions
4. Deploy via update_client.ps1
```

**Time**: 10 minutes
**When**: Ready to release to players

---

## Performance Comparison

| Tier | Iteration Speed | Setup Time | Fidelity | Cost |
|------|----------------|------------|----------|------|
| Tier 1: Editor | ⚡ 30 seconds | None | 85% accurate | $0 |
| Tier 2: Builds | 🔄 5-10 minutes | 2 minutes | 95% accurate | $0 |
| Tier 3: Docker | 🐢 15-20 minutes | 10 minutes first time | 98% accurate | $0 |
| Tier 4: Edgegap | 🐌 30-60 minutes | 5-10 minutes | 100% production | $$$ |

---

## Quick Reference: Testing Commands

### Editor Testing
```
Unity → Click Play button
Test in Editor
Stop Play Mode
```

### Local Build Testing
```powershell
# Build location
cd D:\Updater\WOS_Builds\Test_Local

# Run instance 1 (Host)
.\WavesOfSteel.exe

# Run instance 2 (Client) - new terminal
.\WavesOfSteel.exe
```

### Docker Testing
```powershell
# Build image
cd EdgegapServer
docker build -t wos-server:local .

# Run server
docker run -d -p 7777:7777/udp --name wos-test wos-server:local

# View logs
docker logs -f wos-test

# Stop and remove
docker stop wos-test
docker rm wos-test
```

### Edgegap Testing
```
Unity → Tools → Edgegap → Server Hosting
Build Docker Image
Deploy to Seattle
Wait for deployment
Update ServerConfig.asset with new IP
Test client connection
```

---

## Common Questions

**Q: Can I skip Docker and go straight to Edgegap?**
A: Yes, but Docker Desktop testing catches Linux-specific issues faster and cheaper.

**Q: Do I need to test in Edgegap every time?**
A: No. Only for:
- Final validation before release
- NetworkBehaviour changes
- Server logic changes

**Q: Can I test multiplayer in Editor?**
A: Limited. Use ParrelSync for multi-client Editor testing, or use Tier 2 (builds).

**Q: Should I always start with Tier 1?**
A: Yes, unless the change literally cannot be tested in Editor (rare).

**Q: How do I know when to move to the next tier?**
A: When current tier passes and you need higher fidelity or production validation.

---

## Best Practices

1. **Start Small**: Always begin with Tier 1 (Editor)
2. **Iterate Fast**: Don't jump to builds too early
3. **Validate Before Deploy**: Use Tier 2 before Edgegap
4. **Use Docker Locally**: Test server changes before cloud deployment
5. **Edgegap Last**: Only for final validation and pre-release

**Time Saved**: Using this tiered approach saves ~80% of testing time compared to always deploying to Edgegap.

---

**For your in-game menu**: Stick to Tier 1 (Editor Play Mode) during development, then Tier 2 (Local Build) before release. Skip Docker and Edgegap entirely for this feature.
