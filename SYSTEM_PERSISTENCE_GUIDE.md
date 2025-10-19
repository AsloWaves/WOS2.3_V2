# System Persistence Guide

How the patch notes and workflow systems are preserved across Claude sessions.

---

## ✅ What's Now Permanent

### 1. **CLAUDE.md** - Primary Knowledge Source
**Location**: `D:\GitFolder\UnityProjects\WOS2.3_V2\CLAUDE.md`

**Every Claude session reads this file at startup**. It now contains:
- Complete patch notes management system documentation
- `publish_patch.ps1` workflow with all flags and examples
- Script reference (all 7 automation scripts)
- Platform distribution matrix (GitHub, Launcher, Discord, CHANGELOG)
- Version numbering strategy (semantic versioning)
- Patch notes writing guidelines and template structure
- Workflow integration with existing systems
- Troubleshooting guide for all platforms
- **"Future Claude Sessions"** section with explicit instructions

**Result**: Future Claude will automatically know about the patch notes system and suggest it when appropriate.

---

### 2. **Documentation Files** (Git Committed)
All committed to repository and pushed to GitHub:

**Core Documentation**:
- ✅ `CLAUDE.md` - Main knowledge base for Claude sessions
- ✅ `CHANGELOG.md` - Master version history
- ✅ `UPDATE_WORKFLOWS.md` - Deployment workflows
- ✅ `INGAME_MENU_SETUP.md` - Unity setup guide
- ✅ `PatchNotes/template.md` - Standardized patch notes template

**In-Game Menu Code**:
- ✅ `Assets/Scripts/UI/InGameMenuController.cs`
- ✅ `Assets/Scripts/UI/InGameMenuController.cs.meta`

**Git Status**: All pushed to `origin/main` branch

---

### 3. **Automation Scripts** (File System)
**Location**: `D:\Updater\Scripts\`

These scripts are permanent on your file system:

**Primary Scripts**:
- ✅ `publish_patch.ps1` - Unified publishing workflow (orchestrator)
- ✅ `create_release.ps1` - GitHub release automation
- ✅ `post_discord.ps1` - Discord webhook announcements
- ✅ `update_news.ps1` - Game launcher news automation
- ✅ `update_client.ps1` - Client update workflow

**Utility Scripts**:
- ✅ `test_news.ps1` - News automation testing
- ✅ `cleanup_test_news.ps1` - Test data cleanup
- ✅ `reorganize_news.ps1` - News structure management
- ✅ `fix_encoding.ps1` - UTF-8 encoding fixes

**Documentation**:
- ✅ `README.md` - Script reference guide
- ✅ `PATCH_NOTES_SYSTEM.md` - Comprehensive system overview

**Note**: Scripts directory is not a Git repository. Scripts are preserved on your file system and documented in CLAUDE.md.

---

### 4. **Environment Configuration** (System Level)
**Already configured** (user confirmed):

**GitHub CLI**:
- ✅ Installed and authenticated
- Command: `gh --version` to verify
- Used by: `create_release.ps1`

**Discord Webhook**:
- ✅ Environment variable set: `$env:WOS_DISCORD_WEBHOOK`
- Stored securely at user level
- Used by: `post_discord.ps1`

**Discord Claude Bot**:
- ✅ Running in separate session
- Can be integrated for additional automation

---

## 🔄 How Future Claude Sessions Access This

### Session Startup Process
1. **Claude reads CLAUDE.md** automatically at session start
2. CLAUDE.md contains complete patch notes system documentation
3. Claude now "knows" about:
   - `publish_patch.ps1` unified workflow
   - All 7 automation scripts and their purposes
   - File locations and structure
   - Best practices and troubleshooting
   - Integration with existing workflows

### When User Requests Updates
**User says**: "I want to publish patch 1.0.5"

**Future Claude will**:
1. Check CLAUDE.md → See "Patch Notes Management System" section
2. Suggest using `publish_patch.ps1` workflow
3. Guide user to create patch notes from template
4. Provide exact command with all flags
5. Reference troubleshooting if issues arise

### Knowledge Retention
**Permanent Documentation**:
- ✅ CLAUDE.md (read every session)
- ✅ PATCH_NOTES_SYSTEM.md (detailed reference)
- ✅ Scripts/README.md (script usage)
- ✅ CHANGELOG.md (version history)

**File-Based Knowledge**:
- ✅ Template: `PatchNotes/template.md`
- ✅ Scripts: `D:\Updater\Scripts/*.ps1`
- ✅ Workflows: `UPDATE_WORKFLOWS.md`

---

## 📋 Quick Reference for Future Sessions

### User Says: "Publish an update"
**Claude Response**:
1. Check CLAUDE.md → Patch Notes Management System section
2. Guide: Create patch notes from template
3. Provide: `publish_patch.ps1` command with flags
4. Monitor: Script execution and troubleshoot if needed

### User Says: "How do I publish patch notes?"
**Claude Response**:
1. Reference: CLAUDE.md lines 236-488
2. Show: Quick start workflow (4 steps)
3. Explain: Platform distribution (GitHub, Launcher, Discord)
4. Provide: Example commands for different update types

### User Says: "Update game launcher news"
**Claude Response**:
1. Reference: CLAUDE.md update workflows section
2. Use: `update_news.ps1` or `publish_patch.ps1 -ClientUpdate`
3. Show: Example with parameters
4. Verify: News JSON structure and encoding

---

## 🧪 Testing Persistence

### Test 1: New Claude Session
```
User: "How do I publish a patch?"
Expected: Claude references CLAUDE.md and provides publish_patch.ps1 workflow
```

### Test 2: Script Location
```
User: "Where are the update scripts?"
Expected: Claude knows D:\Updater\Scripts\ and lists all 7 scripts
```

### Test 3: Workflow Integration
```
User: "Update to version 1.0.5"
Expected: Claude suggests creating patch notes first, then running publish_patch.ps1
```

---

## 🔑 Key Success Factors

### Why This Approach Works
1. **CLAUDE.md Read Every Session** - Guaranteed knowledge persistence
2. **Comprehensive Documentation** - All information in one place
3. **Explicit "Future Sessions" Section** - Direct instructions for future Claude
4. **Git Version Control** - CLAUDE.md backed up and tracked
5. **File System Scripts** - Permanent automation tools
6. **Environment Variables** - System-level configuration preserved

### What Future Claude Will Know
✅ Patch notes system exists
✅ How to use `publish_patch.ps1`
✅ File locations and structure
✅ Prerequisites (GitHub CLI, Discord webhook)
✅ Troubleshooting for common issues
✅ Integration with existing workflows
✅ Best practices and guidelines

### What Future Claude Won't Know (Unless User Mentions)
❌ This specific conversation context
❌ Discord Claude Bot session details
❌ User preferences not documented
❌ Temporary decisions or discussions

**Solution**: Document important decisions in CLAUDE.md, CHANGELOG.md, or relevant .md files.

---

## 📝 Maintenance

### When to Update CLAUDE.md
- New automation scripts created
- Workflow changes or improvements
- New prerequisites or dependencies
- Common issues encountered
- Integration with new systems

### How to Update
```powershell
# 1. Edit CLAUDE.md
code "D:\GitFolder\UnityProjects\WOS2.3_V2\CLAUDE.md"

# 2. Commit changes
git add CLAUDE.md
git commit -m "Update workflow documentation"
git push origin main
```

### Recommended Additions
- API endpoints for web-based patch notes
- Steam integration (if applicable)
- A/B testing workflows
- Rollback procedures
- Automated testing integration

---

## ✅ Verification Checklist

**Documentation Committed**:
- [x] CLAUDE.md updated with patch notes system
- [x] CHANGELOG.md created with version history
- [x] PatchNotes/template.md created
- [x] UPDATE_WORKFLOWS.md committed
- [x] INGAME_MENU_SETUP.md committed
- [x] All changes pushed to GitHub

**Scripts Created**:
- [x] publish_patch.ps1 (primary orchestrator)
- [x] create_release.ps1 (GitHub automation)
- [x] post_discord.ps1 (Discord announcements)
- [x] update_news.ps1 (enhanced with UTF-8 fix)
- [x] Scripts/README.md (reference guide)
- [x] PATCH_NOTES_SYSTEM.md (comprehensive docs)

**Environment Configured**:
- [x] GitHub CLI installed and authenticated
- [x] Discord webhook environment variable set
- [x] Scripts directory structure created

**Knowledge Preserved**:
- [x] CLAUDE.md contains complete system documentation
- [x] Future Claude sessions have access via CLAUDE.md
- [x] Troubleshooting documented
- [x] Examples provided for all update types

---

## 🎯 Summary

**Your workflows are now permanent** because:

1. **CLAUDE.md is the source of truth** - Read by every Claude session
2. **Git version control** - All documentation backed up
3. **File system scripts** - Automation tools ready to use
4. **Comprehensive documentation** - Nothing left undocumented
5. **Explicit future guidance** - Claude knows how to help

**Next time you start a new Claude session**, just mention "patch notes" or "publish update" and Claude will reference CLAUDE.md and guide you through the `publish_patch.ps1` workflow automatically.

**No manual reminders needed** - The system is self-documenting and permanent.
