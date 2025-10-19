# Edgegap Plugin Installation Guide - WOS2.3_V2

**How to properly set up the Edgegap Unity plugin for server deployment**

**Status**: Mirror already includes Edgegap plugin - no additional installation needed!

---

## 🎯 Good News!

**The Edgegap plugin is ALREADY installed** in your project because it comes bundled with Mirror Networking!

You don't need to install anything else. Just configure it and you're ready to deploy.

---

## 📁 Edgegap Plugin Location

The plugin is already at:
```
Assets/
├── Mirror/
│   └── Hosting/
│       └── Edgegap/            ✅ Already installed!
│           ├── Editor/
│           │   ├── EdgegapWindowV2.cs
│           │   ├── EdgegapBuildUtils.cs
│           │   ├── Dockerfile
│           │   └── Api/
│           ├── Models/
│           └── README.md
└── edgegap-unity-plugin/       ✅ Also present (standalone version)
    └── Runtime/
        └── BootstrapTemplates/
```

**You have TWO copies**:
1. **Mirror-bundled version** (Assets/Mirror/Hosting/Edgegap/) - Use this one
2. **Standalone version** (Assets/edgegap-unity-plugin/) - Package manager version

**Which to use?** Use the Mirror-bundled version (it's already integrated).

---

## ✅ Verify Plugin Is Available

### Check Unity Menu
1. **In Unity Editor**: Check top menu
2. **Tools** → **Edgegap Hosting**
3. **Should see**: Edgegap window opens ✅

**If you see the menu**: Plugin is ready! Skip to "Configure API Token" below.

**If menu is missing**: Check troubleshooting section below.

---

## 🔧 Configure Edgegap Plugin (Required)

### Step 1: Get API Token

1. **Visit**: https://app.edgegap.com/auth/register
2. **Create account** (free tier available)
3. **Login** → Dashboard
4. **Navigate**: Settings → API
5. **Click**: Generate API Token
6. **Copy token** (long string like `abc123def456...`)

### Step 2: Configure in Unity

1. **In Unity**: **Tools** → **Edgegap Hosting**
2. **Edgegap window opens**
3. **Paste API Token** in "API Token" field
4. **Click**: Verify Token
5. **Should show**: "✅ Token verified successfully"

**That's it!** Plugin is now configured.

---

## 🚀 Create Application (One-Time Setup)

### In Edgegap Window

**Application Settings**:
- **Application Name**: `wos23-server` (lowercase, no spaces)
- **Version Tag**: `v0.3.0`
- **Server Build Path**: `Builds/EdgegapServer`

**Port Configuration**:
- Click **Add Port**
- **Port Name**: `game-port`
- **Internal Port**: `7777`
- **Protocol**: `TCP`

**Click**: Save Application Settings

---

## 📋 Bootstrap Script (OPTIONAL)

### WOSEdgegapBootstrap.cs ✅ Already Created

**Good news**: I already created a standalone bootstrap script that:
- ✅ Does NOT require Edgegap plugin classes
- ✅ Works in both Editor and Runtime
- ✅ Validates server configuration
- ✅ Provides helpful warnings

**Location**: `Assets/Scripts/Networking/WOSEdgegapBootstrap.cs`

**To use it**:
1. Open scene: `Assets/Scenes/MainMenu.unity`
2. Right-click Hierarchy → Create Empty
3. Rename to: `EdgegapBootstrap`
4. Add Component: `WOSEdgegapBootstrap`
5. Save scene

**Is it required?** No, it's optional for validation.

---

## 🛠️ Troubleshooting

### Problem: "Tools → Edgegap Hosting" menu missing

**Possible Causes**:
1. Unity didn't detect the plugin
2. Plugin scripts need recompilation
3. Namespace/assembly issues

**Solutions**:

**Solution 1: Force Recompile**
```
Assets → Reimport All
```
- Wait for Unity to recompile all scripts
- Check **Tools** menu again

**Solution 2: Check for Errors**
- Open **Console** window
- Check for compilation errors
- Fix any errors related to Edgegap scripts

**Solution 3: Verify Plugin Files Exist**
- Check: `Assets/Mirror/Hosting/Edgegap/Editor/EdgegapWindowV2.cs`
- If missing: Reinstall Mirror from Unity Asset Store

**Solution 4: Restart Unity**
- Close Unity completely
- Reopen project
- Check **Tools** menu

---

### Problem: "API token verification failed"

**Solutions**:
1. **Regenerate token** in Edgegap dashboard
2. **Copy ENTIRE token** (long string, don't truncate)
3. **Paste in Unity** without extra spaces
4. **Click Verify** again

---

### Problem: "Compiler error about EdgegapServerBootstrap"

**Solution**: This has been fixed!

The `WOSEdgegapBootstrap.cs` script is now standalone and doesn't depend on Edgegap plugin classes.

If you still see errors:
1. Check `Assets/Scripts/Networking/WOSEdgegapBootstrap.cs`
2. Verify it inherits from `MonoBehaviour` (line 29)
3. Should NOT have any `using Edgegap.Editor;` or `using Edgegap.Bootstrap;`

---

### Problem: "Cannot find Dockerfile"

**Solution**:
The Dockerfile is at two locations:
1. **Mirror version**: `Assets/Mirror/Hosting/Edgegap/Editor/Dockerfile`
2. **WOS custom version**: `Dockerfile` (project root)

The Edgegap plugin will use one of these automatically during deployment.

---

## 📖 Alternative: Manual Plugin Installation (If Needed)

**Only do this if the Mirror-bundled version isn't working**

### Option 1: Unity Asset Store
1. **Open**: Unity Asset Store
2. **Search**: "Edgegap Game Server Hosting"
3. **Download and Import**

### Option 2: Git URL (Package Manager)
1. **Window** → **Package Manager**
2. **+** icon → **Add package from git URL**
3. **URL**: `https://github.com/edgegap/edgegap-unity-plugin.git`
4. **Click**: Add

### Option 3: Download ZIP
1. **Visit**: https://github.com/edgegap/edgegap-unity-plugin
2. **Code** → **Download ZIP**
3. **Extract** to `Assets/` folder

---

## ✅ Verification Checklist

**After setup, verify**:
- [ ] **Tools** → **Edgegap Hosting** menu exists
- [ ] Edgegap window opens successfully
- [ ] API token is configured and verified
- [ ] Application settings saved (app name, version, port)
- [ ] WOSEdgegapBootstrap script compiles without errors
- [ ] No console errors related to Edgegap

**If all checked**: You're ready to deploy! ✅

---

## 🎯 Next Steps

**Now that plugin is configured**:
1. **Read**: `EDGEGAP_QUICKSTART.md` (15-min deployment guide)
2. **Build**: Linux server (File → Build Settings → Server Build ✅)
3. **Deploy**: Tools → Edgegap Hosting → Deploy to Edgegap

---

## 📚 Additional Resources

**Edgegap Documentation**:
- Plugin Guide: https://docs.edgegap.com/docs/tools-and-integrations/unity-plugin-guide
- Mirror Integration: https://docs.edgegap.com/docs/sample-projects/unity-netcodes/mirror-on-edgegap

**Mirror Documentation**:
- Edgegap Hosting: https://mirror-networking.gitbook.io/docs/hosting/edgegap-hosting-plugin

**GitHub**:
- Plugin Repository: https://github.com/edgegap/edgegap-unity-plugin

---

**Summary**: Plugin is already installed with Mirror! Just configure API token and you're ready to deploy.

**Next**: Follow `EDGEGAP_QUICKSTART.md` for deployment.
