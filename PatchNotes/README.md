# Patch Notes System - Two-Product Architecture

This directory contains patch notes for **two separate products** with independent versioning:

1. **Game (WOS2.3_V2)** - The naval MMO game itself
2. **Launcher** - The game launcher application

---

## 🎯 Why Separate Directories?

**Game** and **Launcher** are distinct products that update independently:

- **Game v1.0.3** can release while **Launcher is still v1.0.5**
- **Launcher v1.0.6** can release while **Game is still v1.0.3**
- Each product has its own version history, changelog, and deployment process

**Mixing them causes confusion!** Users would see "Patch 1.0.6" and wonder why their game shows 1.0.3.

---

## 📁 Directory Structure

```
PatchNotes\
├── README.md                  # This file - explains the system
├── Game\
│   ├── template.md            # Template for game patch notes
│   ├── 1.0.0.md              # Game version 1.0.0
│   ├── 1.0.1.md              # Game version 1.0.1
│   └── 1.0.X.md              # Future game versions
└── Launcher\
    ├── template.md            # Template for launcher patch notes
    ├── legacy-versions.md     # Historical versions (1.0.0-1.0.4)
    ├── 1.0.5.md              # Launcher version 1.0.5
    └── 1.0.X.md              # Future launcher versions
```

---

## 📝 Creating New Patch Notes

### For Game Updates

```powershell
# 1. Copy template
Copy-Item "PatchNotes\Game\template.md" "PatchNotes\Game\1.0.X.md"

# 2. Edit patch notes
code "PatchNotes\Game\1.0.X.md"

# 3. Build game in Unity to D:\Updater\WOS_Builds\Version_1.0.X\

# 4. Publish with script
cd D:\Updater\Scripts
.\publish_patch.ps1 -ProductType "Game" -Version "1.0.X" `
  -PatchNotesPath "D:\GitFolder\UnityProjects\WOS2.3_V2\PatchNotes\Game\1.0.X.md" `
  -ClientUpdate -CreateGitHubRelease -PostToDiscord -UpdateChangelog
```

### For Launcher Updates

```powershell
# 1. Copy template
Copy-Item "PatchNotes\Launcher\template.md" "PatchNotes\Launcher\1.0.X.md"

# 2. Edit patch notes
code "PatchNotes\Launcher\1.0.X.md"

# 3. Build launcher in PatchManager

# 4. Publish with script
cd D:\Updater\Scripts
.\publish_patch.ps1 -ProductType "Launcher" -Version "1.0.X" `
  -PatchNotesPath "D:\GitFolder\UnityProjects\WOS2.3_V2\PatchNotes\Launcher\1.0.X.md" `
  -ClientUpdate -PostToDiscord -UpdateChangelog
```

---

## 📋 CHANGELOG Files

Two separate changelogs document each product:

- **CHANGELOG_GAME.md** - Game version history (in project root)
- **CHANGELOG_LAUNCHER.md** - Launcher version history (in project root)

---

## 🏷️ Version Numbering

Both products use **Semantic Versioning**: `MAJOR.MINOR.PATCH`

**Game and Launcher versions are completely independent!**

Example valid state:
- Game: 1.0.2
- Launcher: 1.0.5

Example valid state after updates:
- Game: 1.1.0 (new feature)
- Launcher: 1.0.5 (unchanged)

---

## 🎨 News Feed Labeling

**Game Updates:**
- Title: `GAME v1.0.2 - [Feature Name]`
- Subtitle: `GAME UPDATE`

**Launcher Updates:**
- Title: `LAUNCHER v1.0.5 - [Feature Name]`
- Subtitle: `LAUNCHER UPDATE`

**Infrastructure Updates:**
- Title: `Server Upgrade - [Description]`
- Subtitle: `INFRASTRUCTURE UPDATE`
- NO version number (or use date)

---

## 🚀 Deployment Locations

**Game:**
- Build: `D:\Updater\WOS_Builds\Version_X.X.X\`
- Patches: `D:\Updater\game-launcher-cdn\App\Release\`
- Version File: `VersionInfo.info`
- GitHub Tag: `game-v1.0.X`

**Launcher:**
- Build: `D:\Updater\Manager\Launcher Workspace\`
- Patches: `D:\Updater\game-launcher-cdn\Launcher\`
- Version File: `LauncherVersionInfo.info`
- GitHub Tag: `launcher-v1.0.X`

---

## ✅ Best Practices

**DO:**
- ✅ Use the correct template (Game vs Launcher)
- ✅ Specify `-ProductType` when running `publish_patch.ps1`
- ✅ Update the correct CHANGELOG file
- ✅ Use clear product labels in news feed
- ✅ Increment version numbers independently

**DON'T:**
- ❌ Put game changes in Launcher patch notes
- ❌ Put launcher changes in Game patch notes
- ❌ Skip the `-ProductType` parameter
- ❌ Mix version numbers between products
- ❌ Create "PATCH 1.0.X" without specifying product

---

## 🔧 Troubleshooting

**"Which version am I updating?"**
- Check `VersionInfo.info` for Game version
- Check launcher about screen for Launcher version

**"Where do I find the template?"**
- Game: `PatchNotes\Game\template.md`
- Launcher: `PatchNotes\Launcher\template.md`

**"Script says ProductType is required?"**
- Add `-ProductType "Game"` or `-ProductType "Launcher"` to your command

**"Users are confused about version numbers?"**
- Ensure news feed uses "GAME v1.0.X" and "LAUNCHER v1.0.X" labels
- Never use just "PATCH 1.0.X" without product name

---

## 📚 Additional Documentation

- `CHANGELOG_GAME.md` - Complete game version history
- `CHANGELOG_LAUNCHER.md` - Complete launcher version history
- `D:\Updater\PATCH_NOTES_SYSTEM.md` - Comprehensive patch system guide
- `D:\Updater\Scripts\README.md` - Script usage reference
- `CLAUDE.md` - Project guidance for future sessions

---

**Remember:** Game and Launcher are two separate products. Always specify which one you're updating!
