# Testing Quick Start Guide

Fast reference for testing workflows.

---

## 🎯 For Your Current Work (In-Game Menu)

### Just Hit Play!

```
1. Open MainMenu scene in Unity
2. Click Play button
3. Click "Host (Server + Client)"
4. Press ESC → Test menu
5. Test all buttons
6. Stop Play Mode
```

**That's it!** No Docker, no Edgegap needed.

**Time**: 30 seconds per test
**Perfect for**: UI, camera, audio, local scripts

---

## 📊 When to Use Each Testing Tier

### Tier 1: Editor Play Mode ⚡
**Use when changing**:
- ✅ UI (menus, HUD) ← **YOU ARE HERE**
- ✅ Camera systems
- ✅ Audio
- ✅ Graphics/VFX
- ✅ Input handling
- ✅ Any client-only code

**Don't use Docker or Edgegap for these!**

### Tier 2: Local Builds 🔄
**Use when changing**:
- ✅ Validating Tier 1 changes before release
- ✅ Testing build-specific behavior
- ✅ Performance testing
- ✅ Multi-client testing (same PC)

**Still no Docker/Edgegap needed**

### Tier 3: Docker Desktop 🐋
**Use when changing**:
- ✅ NetworkBehaviour code (SyncVars, Commands, RPCs)
- ✅ Server-specific logic
- ✅ Before deploying to Edgegap

**First time you need a real server**

### Tier 4: Edgegap Cloud ☁️
**Use when**:
- ✅ Final validation before release
- ✅ Testing with real network latency
- ✅ Pre-release checklist

**Most expensive and slowest - use sparingly**

---

## 🚀 Quick Commands

### Test in Editor (Current)
```
Unity → Play button → Host → Test → Stop
```

### Test Local Build
```powershell
# Build once
Unity → Build Settings → Build to D:\Updater\WOS_Builds\Test_Local

# Run to test
cd D:\Updater\WOS_Builds\Test_Local
.\WavesOfSteel.exe
```

### Test Docker (When Needed)
```powershell
# Build server in Unity first (Linux, Server Build, Headless)
cd EdgegapServer
docker build -t wos-server:local .
docker run -d -p 7777:7777/udp --name wos-test wos-server:local
docker logs -f wos-test

# Clean up when done
docker stop wos-test && docker rm wos-test
```

---

## 💡 Pro Tips

**Rapid Iteration**:
- Tier 1 (Editor) = 30 seconds per test
- Tier 2 (Builds) = 5 minutes per test
- Tier 3 (Docker) = 15 minutes per test
- Tier 4 (Edgegap) = 30+ minutes per test

**Choose wisely!**

**Client-Only Changes** (Like your menu):
→ Editor only during dev
→ Build once before release
→ Skip Docker and Edgegap

**Server Changes**:
→ Editor for basic check
→ Builds for initial testing
→ Docker for validation
→ Edgegap before release

---

## ❓ Quick Decisions

**"Can I test this in Editor?"**
- UI? → YES
- Camera? → YES
- Audio? → YES
- Physics? → YES
- NetworkBehaviour? → BASIC TEST ONLY
- Server logic? → NO

**"Do I need Docker for this?"**
- NetworkBehaviour changes? → YES
- Server-specific code? → YES
- Client-only changes? → NO
- UI/Graphics/Audio? → NO

**"Do I need Edgegap for this?"**
- Final release validation? → YES
- Every test iteration? → NO
- Client-only changes? → NO
- Before any code change? → NO

---

## 📋 Your Workflow (In-Game Menu)

### Phase 1: Development ✏️
```
Tier 1 Editor testing only
Iterate until perfect
~15-30 minutes total
```

### Phase 2: Pre-Release ✅
```
Tier 2 Build testing
Final validation
~10 minutes
```

### Phase 3: Deploy 🚀
```
Use update_client.ps1
Push to CDN
~5 minutes
```

**Total**: ~45 minutes from start to deployed

**No Docker or Edgegap needed!**

---

**See TESTING_STRATEGY.md for full details**
